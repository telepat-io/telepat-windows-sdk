using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace TelepatSDK.Models
{
    public class TelepatBaseObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _id;
        /// <summary>
        /// The Telepat object ID
        /// </summary>
        public string ID
        {
            get { return _id; }
            set
            {
                var oldValue = _id;
                _id = value;
                if (oldValue != null) OnPropertyChanged("ID");
            }
        }

        private string _uuid;
        /// <summary>
        /// Object creation UUID
        /// </summary>
        public string UUID
        {
            get { return _uuid; }
            set
            {
                var oldValue = _uuid;
                _uuid = value;
                if (oldValue != null) OnPropertyChanged("UUID");
            }
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
