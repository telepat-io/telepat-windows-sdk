using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Created by Dorin Damaschin on 22.01.2016
// Container for transmitting notifications received from different transport types to
// the corresponding channel.

namespace TelepatSDK.Models
{
    public class TransportNotification
    {
        /// <summary>
        /// Notification type based on the <code>Channel.NotificationType</code> enum
        /// </summary>
        Channel.NotificationType notificationType;

        /// <summary>
        /// The value the notification adds or changes
        /// </summary>
        JObject notificationValue;

        /// <summary>
        /// The object path the notification affects
        /// </summary>
        JObject notificationPath;

        public TransportNotification(JObject notificationObject, Channel.NotificationType notificationType)
        {
            if (notificationObject.GetValue("value") != null)
                notificationValue = notificationObject.GetValue("value").Value<JObject>();
            if (notificationObject.GetValue("path") != null)
                notificationPath = notificationObject.GetValue("path").Value<JObject>();
            this.notificationType = notificationType;
        }

        public TransportNotification(JObject createdObject)
        {
            this.notificationType = Channel.NotificationType.ObjectAdded;
            this.notificationValue = createdObject;
        }

        public Channel.NotificationType GetNotificationType()
        {
            return notificationType;
        }

        public void SetNotificationType(Channel.NotificationType notificationType)
        {
            this.notificationType = notificationType;
        }

        public JObject GetNotificationValue()
        {
            return notificationValue;
        }

        public void SetNotificationValue(JObject notificationValue)
        {
            this.notificationValue = notificationValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> Returns true if the notificationValue field is not null </returns>
        public bool HasValue()
        {
            return notificationValue != null;
        }

        public JObject GetNotificationPath()
        {
            return notificationPath;
        }

        public void SetNotificationPath(JObject notificationPath)
        {
            this.notificationPath = notificationPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> returns ture if the notificationPath field is not null </returns>
        public bool HasPath()
        {
            return notificationPath != null;
        }
    }
}
