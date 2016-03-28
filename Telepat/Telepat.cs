using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelepatSDK.Utils;
using TelepatSDK.Data;
using TelepatSDK.Models;
using TelepatSDK.Networking;
using TelepatSDK.Networking.Requests;
using TelepatSDK.Networking.Transports.SocketIo;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Newtonsoft.Json;

namespace TelepatSDK
{
    public enum TransportConnectionType
    {
        SocketIO
    }

    public class UserUpdateEventArgs
    {
        public List<UserUpdatePatch> Patch;
        public string ErrorMessage;
    }

    public class Telepat
    {
        private static readonly Telepat INSTANCE = new Telepat();

        /// <summary>
        /// References to the currently available Telepat contexts
        /// </summary>
        private Dictionary<string, TelepatContext> mServerContexts;

        /// <summary>
        /// Reference to a Telepat Sync API client
        /// </summary>
        private RestApi apiClient;

        /// <summary>
        /// Locally registered Channel instances
        /// </summary>
        private Dictionary<string, Channel> subscriptions = new Dictionary<string, Channel>();

        /// <summary>
        /// Unique device identifier
        /// </summary>
        private string localUdid;

        /// <summary>
        /// Configured Telepat Application ID
        /// </summary>
        private string appId;

        /// <summary>
        /// Internal storage reference
        /// </summary>
        private TelepatInternalDB internalDB;

        /// <summary>
        /// True when a transport registration in succesfull
        /// </summary>
        private bool isDeviceRegistered = false;

        public EventHandler DeviceRegistered;

        public EventHandler<string> CreateUserSuccess;
        public EventHandler<string> CreateUserFailure;

        public EventHandler<string> LoginUserSuccess;
        public EventHandler<string> LoginUserFailure;

        public EventHandler LogoutSuccess;
        public EventHandler LogoutFailure;

        public EventHandler RequestPasswordResetSuccess;
        public EventHandler RequestPasswordResetFailure;

        public EventHandler ResetPasswordSuccess;
        public EventHandler ResetPasswordFailure;

        public EventHandler<List<UserUpdatePatch>> UserUpdateSuccess;
        public EventHandler<UserUpdateEventArgs>   UserUpdateFailure;

        public bool IsDeviceRegistered { get { return isDeviceRegistered; } }

        private Telepat()
        { }

        public static Telepat GetInstance()
        {
            return INSTANCE;
        }

        /// <summary>
        /// Get access to an instance controlling the internal storage DB
        /// </summary>
        /// <returns> An instance of a class implementing <code>TelepatInternalDB</code> </returns>
        public TelepatInternalDB GetDBInstance()
        {
            return internalDB;
        }

        /// <summary>
        /// Get access to an Retrofit instance that is able to communicate with the Telepat Sync API
        /// </summary>
        /// <returns> An <code>RestApi</code> instance </returns>
        public RestApi GetAPIInstance() { return apiClient; }


        public async Task Initialize(string telepatEndpoint, 
                                     string clientApiKey,
                                     string clientAppId,
                                     bool   useSocketIo = false,
                                     string socketIoUrl = "")
        {
            appId = clientAppId;

            internalDB = new TelepatAkavacheDB(Utilites.GetAppName());

            //await internalDB.Empty();

            apiClient = new RestApi();
            await apiClient.Initialize(telepatEndpoint, clientApiKey, clientAppId);

            await updateContexts();

            if (useSocketIo)
            {
                TaskHelper.FireTask( SocketIoTransport.GetInstance().Initialize(socketIoUrl) );
            }
        }

        /// <summary>
        /// Close the current Telepat instance. You should reinitialize the Telepat SDK before doing
        /// additional work.
        /// </summary>
        public void Destroy()
        {
            internalDB.Close();

            isDeviceRegistered = false;
        }

        // TODO: Register with facebook

        /// <summary>
        /// Submit a set of user credentials for creation
        /// </summary>
        /// <param name="email"> The username of the Telepat user </param>
        /// <param name="password"> A cleartext password to be associated </param>
        /// <param name="name"> The displayable name of the user </param>
        public void CreateUser(string email, string password, string name) {

            if (email != null && password != null && name != null) {
                TaskHelper.RunInBackground(async () =>
                {
                    var userHash = new Dictionary<string, string>();
                    userHash["username"] = email;
                    userHash["email"] = email;
                    userHash["password"] = password;
                    userHash["name"] = name;
                    var response = await apiClient.CreateUserWithEmailAndPassword(userHash);
                    if (response.IsSuccessStatusCode) {
                        EventHelper.FireEvent(CreateUserSuccess, this, response.Content);
                    }
                    else {
                        EventHelper.FireEvent(CreateUserFailure, this, response.ErrorMessage);
                    }
                });
            }
        }

        public void LoginWithUsername(string email, string password) {

            if (email != null && password != null) {
                TaskHelper.RunInBackground(async () =>
                {
                    var userHash = new Dictionary<string, string>();
                    userHash["username"] = email;
                    userHash["password"] = password;
                    var response = await apiClient.LoginWithEmailAndPassword(userHash);
                    if (response.IsSuccessStatusCode) {
                        DebugLog.log("Login successful");
                        await internalDB.SetOperationsData(TelepatConstants.CURRENT_USER_DATA, response.Content.Get("user"));
                        apiClient.AuthorizationToken = response.Content.Get("token") as string;
                        EventHelper.FireEvent(LoginUserSuccess, this, email);
                    }
                    else {
                        DebugLog.log("Login failed");
                        EventHelper.FireEvent(LoginUserFailure, this, response.ErrorMessage);
                    }
                });
		    }
	    }

        // TODO: MORE TESTING - LOGOUT FAILS
        /// <summary>
        /// Send a Telepat Sync API call for logging out the current user.
        /// </summary>
        public void Logout() {
            TaskHelper.RunInBackground(async () =>
            {
                var response = await apiClient.Logout();
                if (response.IsSuccessStatusCode) {

                    DebugLog.log("Logout successful");
                    apiClient.AuthorizationToken = null;
                    EventHelper.FireEvent(LogoutSuccess, this);
                }
                else {
                    DebugLog.log("user logout failed - " + response.ErrorMessage);
                    EventHelper.FireEvent(LogoutFailure, this);
                }
            });
	    }

        /// <summary>
        /// Get information about the currently logged in user
        /// </summary>
        /// <returns> a HashMap of the user data. </returns>
        public async Task<Dictionary<string, object>> GetLoggedInUserInfo()
        {
            var userData = await internalDB.GetOperationsData<Dictionary<string, object>>(TelepatConstants.CURRENT_USER_DATA, null);
		    if(userData == null) {
                //noinspection unchecked
                DebugLog.log("Not a hashmap");
            }
            return userData;
        }

        /// <summary>
        /// Request a password reset email
        /// </summary>
        /// <param name="username"></param>
        public void RequestPasswordResetEmail(string username) {
            TaskHelper.RunInBackground(async () =>
            {
                var requestBody = new Dictionary<string, string>();
                requestBody.Put("username", username);
                requestBody.Put("type", "app");
                var response = await apiClient.RequestPasswordReset(requestBody);
                if (response.IsSuccessStatusCode) {

                    DebugLog.log("Reset email sent");
                    apiClient.AuthorizationToken = null;
                    EventHelper.FireEvent(RequestPasswordResetSuccess, this);
                }
                else {
                    DebugLog.log("Reset request failed");
                    EventHelper.FireEvent(RequestPasswordResetFailure, this);
                }
            });
        }

        /// <summary>
        /// Commit password change request
        /// </summary>
        /// <param name="userId"> userId the user ID </param>
        /// <param name="token"> the token received via the reset email / deep-link </param>
        /// <param name="newPassword"> newPassword </param>
        public void ResetPassword(string userId, string token, string newPassword) {
            TaskHelper.RunInBackground(async () =>
            {
                var requestBody = new Dictionary<string, string>();
                requestBody.Put("user_id", userId);
                requestBody.Put("token", token);
                requestBody.Put("password", newPassword);
                var response = await apiClient.ResetPassword(requestBody);
                if (response.IsSuccessStatusCode) {

                    DebugLog.log("Password was reset");
                    EventHelper.FireEvent(ResetPasswordSuccess, this);
                }
                else {
                    DebugLog.log("Password reset failed");
                    EventHelper.FireEvent(ResetPasswordFailure, this);
                }
            });
        }

        public async Task<Channel> Subscribe<ObjectType>(TelepatContext context,
                                                         string modelName,
                                                         ChannelEventListener listener = null
                                                         ) where ObjectType : TelepatBaseObject
        {
            Channel channel = new Channel.Builder()
                    .SetContext(context)
                    .SetModelName(modelName)
                    .Build();
            //subscriptions.Put(channel.GetSubscriptionIdentifier(), channel);

            await channel.Subscribe<ObjectType>(listener);
            return channel;
        }

        public async Task<Channel> Subscribe<ObjectType>(TelepatContext context,
                                                         string modelName,
                                                         string objectId,
                                                         string userId,
                                                         string parentModelName,
                                                         string parentId,
                                                         Dictionary<string, object> filters,
                                                         ChannelEventListener listener
                                                         ) where ObjectType : TelepatBaseObject
        {
            Channel channel = new Channel.Builder()
                    .SetContext(context)
                    .SetModelName(modelName)
                    .SetUserFilter(userId)
                    .SetSingleObjectIdFilter(objectId)
                    .SetParentFilter(parentModelName, parentId)
                    .SetFilters(filters)
                    .Build();

            await channel.Subscribe<ObjectType>(listener);
            return channel;
        }

        /// <summary>
        /// Get a Map of all curently active contexts for the Telepat Application
        /// </summary>
        /// <returns> A Map instance containing TelepatContext objects stored by their ID </returns>
        public Dictionary<string, TelepatContext> GetContexts() { return mServerContexts; }

        /// <summary>
        /// Send the Telepat Sync API a device registration request
        /// </summary>
        /// <param name="regId"> A WNS token for the current device </param>
        /// <param name="shouldUpdateBackend"> If true, an update should be sent to the Telepat cloud instance
        ///                                    regardless of the state of the token (new/already sent)
        /// </param>
        public Task RegisterDevice(string regId, bool shouldUpdateBackend)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send the Telepat Sync API a device registration request using socket.io
        /// </summary>
        /// <param name="url"> socket.io server url </param>
        /// <param name="shouldUpdateBackend"> If true, an update should be sent to the Telepat cloud instance
        ///                                    regardless of the state of the token (new/already sent)
        /// </param>
        public async Task RegisterDeviceWithSocketIo(string token, bool shouldUpdateBackend)
        {
            string udid = await internalDB.GetOperationsData<string>(TelepatConstants.UDID_KEY, null);

            if (udid != null && !shouldUpdateBackend) {
                isDeviceRegistered = false;
                EventHelper.FireEvent(DeviceRegistered, this);
                return;
            }
            if (udid == null) {
                var request = new RegisterDeviceRequest(TransportConnectionType.SocketIO, token);
                var response = await apiClient.RegisterDevice(await request.GetParams());

                if (response.IsSuccessStatusCode) {
                    DebugLog.log("Register device success");
                    if (response.StatusCode == 200 && response.Content.Get("identifier") != null && response.Content["identifier"] is string) {

                        apiClient.UDID = response.Content["identifier"] as string;
                        await internalDB.SetOperationsData(TelepatConstants.UDID_KEY, response.Content["identifier"]);
                        DebugLog.log("Received Telepat UDID: " + response.Content["identifier"]);
                        isDeviceRegistered = false;
                        EventHelper.FireEvent(DeviceRegistered, this);
                    }
                }
                else {
                    DebugLog.log("Register device failure : " + response.ErrorMessage);
                }
            }
            else {
                EventHelper.FireEvent(DeviceRegistered, this);
            }
        }

        /// <summary>
        /// Retrieve the currently active contexts for the current Telepat application
        /// </summary>
        public async Task updateContexts()
        {
            var response = await apiClient.UpdateContexts();

            if (response.IsSuccessStatusCode) {
                DebugLog.log("Register device success");
                if (response.Content == null) return;
                if (mServerContexts == null) mServerContexts = new Dictionary<string, TelepatContext>();
                foreach (var ctx in response.Content)
                    mServerContexts.Put(ctx.Id, ctx);
                DebugLog.log("Retrieved " + response.Content.Count + "contexts");
            }
            else {
                DebugLog.log("Failed to get contexts : " + response.ErrorMessage);
            }
        }

        public async Task RegisterFacebookUser(string fbToken)
        {

        }

        /// <summary>
        /// Locally register an active subscription to a Telepat Channel with the Telepat SDK instance
        /// (new channel objects register themselves automatically)
        /// </summary>
        /// <param name="mChannel"> The channel object to be registered </param>
        public void RegisterSubscription(Channel mChannel)
        {
            subscriptions.Put(mChannel.GetSubscriptionIdentifier(), mChannel);
        }

        /// <summary>
        /// Remove a locally registered subscription of a Telepat Channel object (this does not send any
        /// notifications to the Telepat Sync API
        /// </summary>
        /// <param name="mChannel"> The channel instance </param>
        public async Task RemoveSubscription(Channel mChannel)
        {
            if (mChannel == null) return;
            subscriptions.Remove(mChannel.GetSubscriptionIdentifier());
            await mChannel.Unsubscribe();
        }

        /// <summary>
        /// Get the <code>Channel</code> instance of a locally registered channel.
        /// </summary>
        /// <param name="channelIdentifier"> A properly formatted string of the channel identifier. </param>
        /// <returns></returns>
        public Channel GetSubscribedChannel(string channelIdentifier)
        {
            return subscriptions.ContainsKey(channelIdentifier) ? subscriptions[channelIdentifier] : null;
        }

        /// <summary>
        /// Get a unique device identifier. Used internally for detecting already registered devices
        /// </summary>
        /// <returns> A String containing the UDID </returns>
        public async Task<string> GetDeviceLocalIdentifier()
        {
            if (localUdid != null) return localUdid;
            var token = HardwareIdentification.GetPackageSpecificToken(null);
            var hardwareId = token.Id;
            var dataReader = DataReader.FromBuffer(hardwareId);
            byte[] bytes = new byte[hardwareId.Length];
            dataReader.ReadBytes(bytes);

            localUdid = await internalDB.GetOperationsData(TelepatConstants.LOCAL_UDID_KEY,
                                                           BitConverter.ToString(bytes));
		    return localUdid;
	    }

        /// <summary>
        /// Set the unique device identifier sent to the Telepat cloud. This method should be used as
        /// early as possible, before registering the device with the Sync API.
        /// </summary>
        /// <param name="udid"> udid the desired UDID </param>
        public void SetDeviceLocalIdentifier(string udid) {
            internalDB.SetOperationsData(TelepatConstants.LOCAL_UDID_KEY, udid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userChanges"></param>
        /// <param name="userId"></param>
        public async Task UpdateUser(List<UserUpdatePatch> userChanges, string userId)
        {
            var requestBody = new Dictionary<string, object>();
            var jsonPatches = new List<Dictionary<string, string>>();
            foreach (var patch in userChanges)
            {
                var jsonPatch = new Dictionary<string, string>();
                jsonPatch.Put("op", "replace");
                jsonPatch.Put("path", "user/" + userId + "/" + patch.FieldName);
                jsonPatch.Put("value", patch.FieldValue);
                jsonPatches.Add(jsonPatch);
            }
            requestBody.Put("patches", jsonPatches);
            var response = await apiClient.UpdateUser(requestBody);

            if (response.IsSuccessStatusCode) {
                DebugLog.log("User update successful");
                var loggedInUserInfo = await GetLoggedInUserInfo();

                if (loggedInUserInfo != null) {
                    var userData = new Dictionary<string, object>(loggedInUserInfo);
                    foreach (var patch in userChanges) {
                        userData.Put(patch.FieldName, patch.FieldValue);
                    }
                    await internalDB.SetOperationsData(TelepatConstants.CURRENT_USER_DATA, userData);
                }
                EventHelper.FireEvent(UserUpdateSuccess, this, userChanges);
            }
            else {
                DebugLog.log("User update failed");
                EventHelper.FireEvent(UserUpdateFailure, this, new UserUpdateEventArgs() { Patch = userChanges, ErrorMessage = response.ErrorMessage });
            }
        }

        public string GetAppId()
        {
            return appId;
        }
    }
}
