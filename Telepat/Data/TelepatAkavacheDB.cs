using Akavache;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelepatSDK.Utils;
using TelepatSDK.Models;

namespace TelepatSDK.Data
{
    public class TelepatAkavacheDB : TelepatInternalDB
    {
        /// <summary>
        /// SnappyDB database name
        /// </summary>
        private static string DB_NAME = typeof(Telepat).Name + "_OPERATIONS";

        /// <summary>
        /// Prefix for Telepat internal metadata
        /// </summary>
        private static string OPERATIONS_PREFIX = "TP_OPERATIONS_";

        /// <summary>
        /// Prefix for stored objects
        /// </summary>
        private static string OBJECTS_PREFIX = "TP_OBJECTS_";


        public TelepatAkavacheDB(string applicationName)
        {
            try {
                BlobCache.ApplicationName = applicationName;
                BlobCache.EnsureInitialized();
            }
            catch (Exception e) {
                DebugLog.catchLog(e);
            }
        }

        /// <summary>
        /// Telepat internal metadata - get a stored value
        /// </summary>
        /// <typeparam name="T"> the type the return value should be casted to </typeparam>
        /// <param name="key"> the key the value is stored on </param>
        /// <param name="defaultValue"> the default value to return if the key does not exist </param>
        /// <returns> the metadata value </returns>
        public Task<T> GetOperationsData<T>(string key, T defaultValue)
        {
            return getData(OPERATIONS_PREFIX + key, defaultValue);
        }

        /// <summary>
        /// Telepat internal metadata - sets a value on a specific key
        /// </summary>
        /// <param name="key"> the metadata key </param>
        /// <param name="value"> the metadata value </param>
        public Task SetOperationsData(string key, object value)
        {
            return setData(OPERATIONS_PREFIX + key, value);
        }

        /// <summary>
        /// Checks if an object exists in the internal DB
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        /// <param name="id"> the Telepat object ID </param>
        /// <returns> true if the object exists, false otherwise </returns>
        public async Task<bool> ObjectExists(string channelIdentifier, string id)
        {
            try {
                await BlobCache.LocalMachine.Get(getObjectKey(channelIdentifier, id));
                return true;
            }
            catch (KeyNotFoundException) {
                return false;
            }
        }

        /// <summary>
        /// Retrieve a stored object
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        /// <param name="id"> the Telepat object ID </param>
        /// <param name="type"> the class the object should be casted to </param>
        /// <returns> the stored object </returns>
        public Task<T> GetObject<T>(string channelIdentifier, string id)
        {
            return getData(getObjectKey(channelIdentifier, id), default(T));
        }

        /// <summary>
        /// Retrieve a list of all stored objects for a channel
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        /// <param name="type"> the class the objects should be casted to </param>
        /// <returns></returns>
        public async Task<List<T>> GetChannelObjects<T>(string channelIdentifier)
        {
            var keys = await ChannelKeys(channelIdentifier);
            var objects = new List<T>();
            foreach (var key in keys)
            {
                try {
                    objects.Add((await BlobCache.LocalMachine.GetObject<T>(key)));
                }
                catch (Exception) { }
            }
            objects.Sort(delegate (T x, T y)
            {
                var first = x as TelepatBaseObject;
                var second = y as TelepatBaseObject;
                return first.ID.CompareTo(second.ID);
            });

            DebugLog.log("Retrieved " + channelIdentifier + " objects. Size: " + objects.Count);
            return objects;
        }

        /// <summary>
        /// Save an objects to the internal DB
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        /// <param name="obj"> the object to store </param>
        public Task PersistObject(string channelIdentifier, TelepatBaseObject obj)
        {
            string objectKey = getObjectKey(channelIdentifier, obj.ID);
            return setData(objectKey, obj);
        }

        /// <summary>
        /// Save an array of objects to the internal DB
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        /// <param name="obj"> an array of objects to store </param>
        public async Task PersistObjects(string channelIdentifier, IEnumerable<TelepatBaseObject> objects)
        {
            // TODO: Concurrency optimisations
            foreach (var obj in objects)
            {
                await PersistObject(channelIdentifier, obj);
            }
        }

        /// <summary>
        /// Delete an object from the internal DB
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        /// <param name="obj"> the object to delete </param>
        public async Task DeleteObject(string channelIdentifier, TelepatBaseObject obj)
        {
            try {
                await BlobCache.LocalMachine.InvalidateObject<TelepatBaseObject>(obj.ID);
            }
            catch (Exception e) {
                DebugLog.catchLog(e);
            }
        }

        /// <summary>
        /// Delete all objects stored in a specific channel
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        public async Task DeleteChannelObjects(string channelIdentifier)
        {
            var keys = await ChannelKeys(channelIdentifier);
            try {
                await BlobCache.LocalMachine.Invalidate(keys);
            }
            catch(Exception) { }
        }

        /// <summary>
        /// Get all the keys stored for a specific channel
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        /// <returns> An array of keys </returns>
        public async Task<IEnumerable<string>> ChannelKeys(string channelIdentifier)
        {
            try
            {
                var keys = await BlobCache.LocalMachine.GetAllKeys();
                return keys.Where(k => k.StartsWith(getChannelPrefix(channelIdentifier)));
            }
            catch (KeyNotFoundException) {
                return null;
            }
        }

        /// <summary>
        /// Empty the internal DB
        /// </summary>
        public async Task Empty()
        {
            try {
                await BlobCache.LocalMachine.InvalidateAll();
            }
            catch (Exception e) {
                DebugLog.catchLog(e);
            }
        }

        /// <summary>
        /// Close the connection with the internal DB
        /// </summary>
        public async Task Close()
        {
            try {
                await BlobCache.Shutdown();
            }
            catch (Exception e) {
                DebugLog.catchLog(e);
            }
        }

        /// <summary>
        /// Retrieve data from the internal DB
        /// </summary>
        /// <param name="key"> the key the data is stored under </param>
        /// <param name="defaultValue"> the default value to return if the key does not exist </param>
        /// <param name="type"> the class the return value should be casted to </param>
        /// <returns> the stored data </returns>
        private async Task<T> getData<T>(string key, T defaultValue)
        {
            try {
                T obj = await BlobCache.LocalMachine.GetObject<T>(key);
                return obj;
            }
            catch (KeyNotFoundException) {
                DebugLog.log("Internal DB object with key " + key + " not found");
            }
            catch (Exception e) {
                DebugLog.catchLog(e);
            }
            return defaultValue;
        }

        /// <summary>
        /// Save data to internal DB
        /// </summary>
        /// <param name="key"> the key to store under </param>
        /// <param name="value"> the data to store </param>
        private async Task setData(string key, object value)
        {
            try {
                await BlobCache.LocalMachine.InsertObject(key, value);
            }
            catch (Exception e) {
                DebugLog.catchLog(e);
            }
        }

        private string getObjectKey(string channelIdentifier, string id)
        {
            return getChannelPrefix(channelIdentifier) + ":" + id;
        }

        private string getChannelPrefix(string channelIdentifier)
        {
            return OBJECTS_PREFIX + channelIdentifier;
        }
    }
}
