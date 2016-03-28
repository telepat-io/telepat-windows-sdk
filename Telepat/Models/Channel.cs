using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelepatSDK.Data;
using Newtonsoft;
using TelepatSDK.Utils;
using Newtonsoft.Json;
using System.Reflection;
using TelepatSDK.Networking;
using Newtonsoft.Json.Linq;
using System.Net;

// Created by Dorin Damaschin on 22.01.2016
// Telepat Channel model

namespace TelepatSDK.Models
{
    public class ObjectAddedEventArgs
    {
        public TelepatBaseObject ToAdd;
    }

    public class ObjectCreateSuccessEventArgs
    {
        public TelepatBaseObject ToAdd;
    }

    public class ObjectRemovedEventArgs
    {
        /// <summary>
        /// the deleted object. This value is null if the object isn't stored locally
        /// </summary>
        public TelepatBaseObject ToRemove;
        /// <summary>
        /// the ID of the deleted object
        /// </summary>
        public string ObjectId;
    }

    public class ObjectModifiedEventArgs
    {
        /// <summary>
        /// the updated object
        /// </summary>
        public TelepatBaseObject Target;
        /// <summary>
        /// the updated property name
        /// </summary>
        public string PropertyName;
        /// <summary>
        /// the new value of the updated property
        /// </summary>
        public string NewValue;
    }

    public class ErrorEventArgs
    {
        /// <summary>
        /// the code of the error
        /// </summary>
        public int StatusCode;
        /// <summary>
        /// a message associated with the error
        /// </summary>
        public string Message;
    }
        
    public class Channel : TelepatBaseObject
    {
        private string mModelName;
        private string mUserId;
        private string mParentModelName;
        private string mParentId;
        private string mSingleObjectId;
        private Dictionary<string, object> mFilters;
        private TelepatContext mTelepatContext;
        private Dictionary<string, TelepatBaseObject> waitingForCreation = new Dictionary<string, TelepatBaseObject>();
        private TelepatInternalDB dbInstance;
        private RestApi apiInstance;
        private ChannelEventListener eventListener = new ChannelEventListener();

        /// <summary>
        /// Possible notification message types arriving from the Telepat cloud
        /// </summary>
        public enum NotificationType
        {
            ObjectAdded,
            ObjectUpdated,
            ObjectDeleted
        }

        public ChannelEventListener EventListener { get { return eventListener; } }

        /// <summary>
        /// Builder pattern implementation for the Channel class
        /// </summary>
        public class Builder
        {
            internal string mModelName, mUserId, mParentModelName, mParentId, mSingleObjectId;
            internal Dictionary<string, object> mFilters;
            internal TelepatContext mTelepatContext;

            public Builder SetModelName(string modelName) { this.mModelName = modelName; return this; }
            public Builder SetContext(TelepatContext context) { this.mTelepatContext = context; return this; }
            public Builder SetUserFilter(string userId) { this.mUserId = userId; return this; }
            public Builder SetParentFilter(string parentModelName, String parentId) { this.mParentModelName = parentModelName; this.mParentId = parentId; return this; }
            public Builder SetSingleObjectIdFilter(string singleObjectId) { this.mSingleObjectId = singleObjectId; return this; }
            public Builder SetFilters(Dictionary<string, object> filters) { this.mFilters = filters; return this; }
            public Channel Build()
            {
                return new Channel(this);
            }
        }

        public Channel(string identifier)
        {
            String[] identifierSegments = identifier.Split(new[] { ':' });
            String contextId = identifierSegments[1];
            this.mTelepatContext = Telepat.GetInstance().GetContexts().ContainsKey(contextId) ? Telepat.GetInstance().GetContexts()[contextId] : null;
            this.mModelName = identifierSegments[2];
            linkExternalDependencies();
        }

        public Channel(Builder builder)
        {
            this.mModelName = builder.mModelName;
            this.mTelepatContext = builder.mTelepatContext;
            this.mFilters = builder.mFilters;
            this.mUserId = builder.mUserId;
            this.mParentModelName = builder.mParentModelName;
            this.mParentId = builder.mParentId;
            this.mSingleObjectId = builder.mSingleObjectId;
            this.dbInstance = Telepat.GetInstance().GetDBInstance();
            linkExternalDependencies();
            // this.notifyStoredObjects();
        }

        private void linkExternalDependencies()
        {
            this.dbInstance = Telepat.GetInstance().GetDBInstance();
            this.apiInstance = Telepat.GetInstance().GetAPIInstance();
        }

        /// <summary>
        /// Create a new subscription with the Telepat Cloud instance.
        /// If the device is already registered, the stored objects will be notified again.
        /// </summary>
        public async Task Subscribe<ObjectType>(ChannelEventListener listener) where ObjectType : TelepatBaseObject
        {
            eventListener = listener;

            var response = await Telepat.GetInstance()
                                        .GetAPIInstance()
                                        .Subscribe(GetSubscribingRequestBody());

            if (response.IsSuccessStatusCode) {

                if (response.StatusCode == 200) {
                    Telepat.GetInstance().RegisterSubscription(this);

                    foreach (var entry in response.Content) {
                        // TODO: Concurrency optimisation
                        await ProcessNotification<ObjectType>(new TransportNotification(entry));
                    }
                    EventHelper.FireEvent(EventListener.SubscribeComplete, this);
                }
                else {
                    EventHelper.FireEvent(EventListener.Error, this, new ErrorEventArgs() { StatusCode = response.StatusCode, Message = response.ErrorMessage });
                }
            }
            else {
                switch (response.HttpStatusCode)
                {
                    case HttpStatusCode.Conflict:
                        DebugLog.log("There is an already active subscription for this channel.");
                        break;
                    case HttpStatusCode.Unauthorized:
                        DebugLog.log("Not logged in.");
                        break;
                    default:
                        DebugLog.log("Error subscribing: "  + response.ErrorMessage);
                        break;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <returns> Get a HashMap containing the relevant POST field parameters for a 
        /// Subscribe Sync API request
        /// </returns>
        public Dictionary<string, object> GetSubscribingRequestBody()
        {
            var requestBody = new Dictionary<string, object>();
            var channel = new Dictionary<string, object>();
            channel.Add("context", mTelepatContext.Id);
            channel.Add("model", mModelName);
            if (mSingleObjectId != null) channel.Add("id", mSingleObjectId);
            if (mParentId != null && mParentModelName != null)
            {
                var parent = new Dictionary<string, object>();
                parent.Add("id", mParentId);
                parent.Add("model", mParentModelName);
                requestBody.Add("parent", parent);
            }
            if (mUserId != null) channel.Add("user", mUserId);
            requestBody.Add("channel", channel);
            if (mFilters != null) requestBody.Add("filters", mFilters);
            return requestBody;
        }

        /// <summary>
        /// </summary>
        /// <returns> Get a HashMap containing the relevant POST field parameters for a 
        /// item creating request
        /// </returns>
        public Dictionary<string, object> GetCreateRequestBody(TelepatBaseObject obj)
        {
            var requestBody = new Dictionary<string, object>();
            requestBody.Add("context", mTelepatContext.Id);
            requestBody.Add("model", mModelName);
            requestBody.Add("content", obj);
            return requestBody;
        }

        /// <summary>
        /// </summary>
        /// <returns> Get a HashMap containing the relevant POST field parameters for a 
        /// item update request
        /// </returns>
        public Dictionary<string, object> GetUpdateRequestBody(PendingPatch pendingPatch)
        {
            var body = new Dictionary<string, object>();
            body.Add("model", this.mModelName);
            body.Add("context", this.mTelepatContext.Id);
            body.Add("id", pendingPatch.getObjectId());
            var pendingPatches = new List<Dictionary<string, object>>();
            pendingPatches.Add(pendingPatch.ToMap());

            body.Add("patches", pendingPatches);
            return body;
        }

        /// <summary>
        /// </summary>
        /// <returns> Get a HashMap containing the relevant POST field parameters for a 
        /// item delete request
        /// </returns>
        public Dictionary<string, object> GetDeleteRequestBody(TelepatBaseObject obj)
        {
            var body = new Dictionary<string, object>();
            body.Add("model", this.mModelName);
            body.Add("context", this.mTelepatContext.Id);
            body.Add("id", obj.ID);
            return body;
        }

        /// <summary>
        /// Unsubscribes this device from receiving further updates from this channel
        /// </summary>
        public async Task Unsubscribe()
        {
            var response = await Telepat.GetInstance()
                                        .GetAPIInstance()
                                        .Unsubscribe(GetSubscribingRequestBody());

            if (response.IsSuccessStatusCode)
            {
                DebugLog.log("Unsubscribed");
                await dbInstance.DeleteChannelObjects(GetSubscriptionIdentifier());
            }
            else
            {
                DebugLog.log("Error unsubscribing: " + response.ErrorMessage);
            }
        }

        /// <summary>
        /// Create a new object on this Telepat channel
        /// </summary>
        /// <param name="obj"> an object of a class extending <code>TelepatBaseObject</code> </param>
        /// <returns> An UUID used for detecting the creation success </returns>
        public async Task<string> Add(TelepatBaseObject obj)
        {
            if (obj == null) return null;
            //TODO add a proper uuid
            var ts = TimeSpan.FromTicks(DateTime.Now.Ticks);
            obj.UUID = "" + ts.TotalMilliseconds;
            waitingForCreation.Add(obj.UUID, obj);
            CrudOperations.Resolve("Create", await apiInstance.Create(GetCreateRequestBody(obj)));
            return obj.UUID;
        }

        /// <summary>
        /// Deletes an object from this Telepat channel
        /// </summary>
        /// <param name="obj"> an object of a class extending <code>TelepatBaseObject</code> </param>
        public async Task Remove(TelepatBaseObject obj)
        {
            if (obj == null) return;
            CrudOperations.Resolve("Delete", await apiInstance.Delete(GetDeleteRequestBody(obj)));
        }

        /// <summary>
        /// Get a string representation of this channels characteristics
        /// </summary>
        /// <returns></returns>
        public string GetSubscriptionIdentifier()
        {
            /*
            4:  "blg:{appId}:{model}",                                 //channel
            used for built-in models (users, contexts)
            5:  "blg:{appId}:context:{context}:{model}",
            //the Channel of all objects from a context
            7:  "blg:{appId}:context:{context}:users:{user_id}:{model}",
            //the Channel of all objects from a context from an user
            12: "blg:{appId}:{parent_model}:{parent_id}:{model}",            //the
            Channel of all objects belong to a parent
            14: "blg:{appId}:users:{user_id}:{parent_model}:{parent_id}:{model}",//the
            Channel of all comments from event 1 from user 16
            20: "blg:{appId}:{model}:{id}",                            //the
            Channel of one item
            var key = 'blg:'+API.appId;
            if (!options.channel.id && options.channel.context) {
              key += ':context:'+options.channel.context;
            }
            if (options.channel.user) {
              key += ':users:'+options.channel.user;
            }
            key += ':'+options.channel.model;
            if (options.channel.id) {
              key += ':'+options.channel.id;
            }
            return key;
            */

            if (mModelName == null) return null;

            string identifier = "blg:" + Telepat.GetInstance().GetAppId();
            if (mSingleObjectId == null && mTelepatContext != null) {
                identifier += ":context:" + mTelepatContext.Id;
            }

            if (mUserId != null) {
                identifier += ":users:" + mUserId;
            }

            if (mParentModelName != null && mParentId != null) {
                identifier += ":" + mParentModelName + ":" + mParentId;
            }

            identifier += ":" + mModelName;

            if (mSingleObjectId != null) {
                identifier += ":" + mSingleObjectId;
            }

            if (mFilters != null) {
                string jsonFilters = JsonConvert.SerializeObject(mFilters);
                byte[] data = new byte[0];
                try {
                    data = Encoding.UTF8.GetBytes(jsonFilters);
                }
                catch (Exception e) {
                    DebugLog.catchLogAndThrow(e);
                }
                string base64Filters = Convert.ToBase64String(data);
                identifier += ":filter:" + base64Filters;
            }
            //		return "blg:"+Telepat.getInstance().getAppId()+":context:"+mTelepatContext.getId()+':'+mModelName;
            return identifier;
            //TODO add support for more channel params
        }

        /// <summary>
        /// Send created notifications for currently locally stored objects in this channel.
        /// </summary>
        public async Task NotifyStoredObjects()
        {
            var objects = await Telepat.GetInstance().
                    GetDBInstance().GetChannelObjects<Channel>(GetSubscriptionIdentifier());
            foreach (var dataObject in objects) {
                dataObject.PropertyChanged += OnPropertyChanged;
                EventHelper.FireEvent(EventListener.ObjectAdded, 
                                      this, 
                                      new ObjectAddedEventArgs() { ToAdd = dataObject });
            }
        }

        /// <summary>
        /// Save an object to the internal DB
        /// </summary>
        /// <param name="obj"> An object of a class extending TelepatBaseObject </param>
        private void PersistObject(TelepatBaseObject obj)
        {
            dbInstance.
                    PersistObject(GetSubscriptionIdentifier(),
                            obj
                    );
        }

        /// <summary>
        /// Process an incoming notification from a network transport
        /// </summary>
        /// <param name="notification"> the received notification </param>
        /// <returns></returns>
        public async Task ProcessNotification<ObjectType>(TransportNotification notification) where ObjectType : TelepatBaseObject
        {
            string[] pathSegments;
            string modelName;
            string objectId;

            switch (notification.GetNotificationType())
            {
                case NotificationType.ObjectAdded:
                    TelepatBaseObject dataObject = JsonConvert.DeserializeObject<ObjectType>(notification.GetNotificationValue().ToString());
                    if (dataObject.UUID != null && waitingForCreation.ContainsKey(dataObject.UUID))
                    {
                        waitingForCreation[dataObject.UUID].ID = dataObject.ID;
                        waitingForCreation[dataObject.UUID].PropertyChanged += OnPropertyChanged;
                        EventHelper.FireEvent(EventListener.ObjectCreateSuccess, 
                                              this, 
                                              new ObjectCreateSuccessEventArgs() { ToAdd = waitingForCreation[dataObject.UUID] });
                        if (EventListener.ObjectAdded != null)
                        {
                            waitingForCreation.Remove(dataObject.UUID);
                        }
                        PersistObject(dataObject);
                        return;
                    }
                    if (await dbInstance.ObjectExists(GetSubscriptionIdentifier(), dataObject.ID))
                    {
                        return;
                    }
                    EventHelper.FireEvent(EventListener.ObjectAdded,
                                          this,
                                          new ObjectAddedEventArgs() { ToAdd = dataObject });
                    PersistObject(dataObject);
                    break;
                case NotificationType.ObjectUpdated:
                    DebugLog.log("Object updated: " +
                            notification.GetNotificationValue().ToString() +
                            " with path: " + notification.GetNotificationPath().ToString());

                    if (!notification.HasValue())
                    {
                        DebugLog.log("Notification object has no associated field value");
                        return;
                    }

                    pathSegments = notification.GetNotificationPath().ToString().Split(new[] { '/' });
                    modelName = pathSegments[0];
                    if (!modelName.Equals(this.mModelName)) return;

                    objectId = pathSegments[1];
                    string propertyName = pathSegments[2];

                    if (await dbInstance.ObjectExists(this.GetSubscriptionIdentifier(), objectId))
                    {
                        TelepatBaseObject updatedObject = await dbInstance.GetObject<TelepatBaseObject>(GetSubscriptionIdentifier(),
                                                                              objectId);
                        updatedObject.GetType().GetRuntimeProperty(propertyName).SetValue(updatedObject,
                                                  notification.GetNotificationValue().ToString());

                        EventHelper.FireEvent(EventListener.ObjectModified,
                                              this,
                                              new ObjectModifiedEventArgs() { Target = updatedObject,
                                                                              PropertyName = propertyName,
                                                                              NewValue = notification.GetNotificationValue().ToString() });

                        await dbInstance.PersistObject(GetSubscriptionIdentifier(), updatedObject);
                    }
                    break;
                case NotificationType.ObjectDeleted:
                    DebugLog.log("Object deleted " +
                            " with path: " + notification.GetNotificationPath().ToString());
                    pathSegments = notification.GetNotificationPath().ToString().Split(new[] { '/' });
                    modelName = pathSegments[0];
                    if (!modelName.Equals(this.mModelName)) return;

                    objectId = pathSegments[1];
                    TelepatBaseObject deletedObject = null;
                    if (await dbInstance.ObjectExists(this.GetSubscriptionIdentifier(), objectId))
                    {
                        deletedObject = await dbInstance.GetObject<TelepatBaseObject>(GetSubscriptionIdentifier(),
                                                                                     objectId);
                        await dbInstance.DeleteObject(GetSubscriptionIdentifier(), deletedObject);
                    }
                    EventHelper.FireEvent(EventListener.ObjectRemoved,
                                          this,
                                          new ObjectRemovedEventArgs() { ToRemove = deletedObject, ObjectId = objectId });
                    break;
            }
        }

        public void Listen(TelepatBaseObject obj)
        {
            obj.PropertyChanged += OnPropertyChanged;
        }

        public void StopListening(TelepatBaseObject obj)
        {
            obj.PropertyChanged -= OnPropertyChanged;
        }

        /// <summary>
        /// Listener for modified objects (previously emitted by this channel)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected async void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var obj = (TelepatBaseObject)sender;
            DebugLog.log("Channel " + GetSubscriptionIdentifier() + ": Object with ID " + obj.ID + " changed");
            PendingPatch patch = new PendingPatch(PendingPatch.PatchType.Replace,
                this.mModelName + "/" + obj.ID + "/" + args.PropertyName,
                sender.GetType().GetRuntimeProperty(args.PropertyName).GetValue(sender, null),
                obj.ID);
            CrudOperations.Resolve("Update", await apiInstance.Update(GetUpdateRequestBody(patch)));
        }
    }
}
