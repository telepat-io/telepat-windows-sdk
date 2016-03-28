using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelepatSDK.Models
{
    public class ChannelEventListener
    {
        /// <summary>
        /// Fired when an object was added to the channel (by a 3rd party device, locally initiated adds
	    /// are fired through<code> onObjectCreateSuccess</code>
        /// </summary>
        public EventHandler<ObjectAddedEventArgs> ObjectAdded;
        /// <summary>
        /// Fired when an object was created successfully. The same object as the one submitted for
	    /// creation is returned in this callback.The object ID is transparently added, even if this
	    ///  event is not handled.
        /// </summary>
        public EventHandler<ObjectCreateSuccessEventArgs> ObjectCreateSuccess;
        /// <summary>
        /// Fired when an object was deleted
        /// </summary>
        public EventHandler<ObjectRemovedEventArgs> ObjectRemoved;
        /// <summary>
        /// Fired when an object was updated
        /// </summary>
        public EventHandler<ObjectModifiedEventArgs> ObjectModified;
        /// <summary>
        /// Fired when an error was detected by the SDK while subscribing
        /// </summary>
        public EventHandler<ErrorEventArgs> Error;
        /// <summary>
        /// Fired when subscription completes
        /// </summary>
        public EventHandler SubscribeComplete;
    }
}
