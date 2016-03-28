using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelepatSDK.Models;

/// Created by Dorin Damaschin on 22.01.2016
/// Interface for DB providers

namespace TelepatSDK.Data
{
    public interface TelepatInternalDB
    {
        /// <summary>
        /// Telepat internal metadata - get a stored value
        /// </summary>
        /// <typeparam name="T"> the type the return value should be casted to </typeparam>
        /// <param name="key"> the key the value is stored on </param>
        /// <param name="defaultValue"> the default value to return if the key does not exist </param>
        /// <returns> the metadata value </returns>
        Task<T> GetOperationsData<T>(string key, T defaultValue);

        /// <summary>
        /// Telepat internal metadata - sets a value on a specific key
        /// </summary>
        /// <param name="key"> the metadata key </param>
        /// <param name="value"> the metadata value </param>
        Task SetOperationsData(string key, object value);

        /// <summary>
        /// Checks if an object exists in the internal DB
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        /// <param name="id"> the Telepat object ID </param>
        /// <returns> true if the object exists, false otherwise </returns>
        Task<bool> ObjectExists(string channelIdentifier, string id);

        /// <summary>
        /// Retrieve a stored object
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        /// <param name="id"> the Telepat object ID </param>
        /// <param name="type"> the class the object should be casted to </param>
        /// <returns> the stored object </returns>
        Task<T> GetObject<T>(string channelIdentifier, string id);

        /// <summary>
        /// Retrieve a list of all stored objects for a channel
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        /// <param name="type"> the class the objects should be casted to </param>
        /// <returns></returns>
        Task<List<T>> GetChannelObjects<T>(string channelIdentifier);

        /// <summary>
        /// Save an objects to the internal DB
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        /// <param name="obj"> the object to store </param>
        Task PersistObject(string channelIdentifier, TelepatBaseObject obj);

        /// <summary>
        /// Save an array of objects to the internal DB
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        /// <param name="obj"> an array of objects to store </param>
        Task PersistObjects(string channelIdentifier, IEnumerable<TelepatBaseObject> obj);

        /// <summary>
        /// Delete an object from the internal DB
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        /// <param name="obj"> the object to delete </param>
        Task DeleteObject(string channelIdentifier, TelepatBaseObject obj);

        /// <summary>
        /// Delete all objects stored in a specific channel
        /// </summary>
        /// <param name="channelIdentifier"> the identifier of the channel the object is stored in </param>
        Task DeleteChannelObjects(string channelIdentifier);

        /// <summary>
        /// Empty the internal DB
        /// </summary>
        Task Empty();

        /// <summary>
        /// Close the connection with the internal DB
        /// </summary>
        Task Close();
    }
}
