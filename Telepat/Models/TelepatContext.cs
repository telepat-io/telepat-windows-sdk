using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// Created by Dorin Damaschin on 22.01.2016
/// Model class for Telepat Contexts

namespace TelepatSDK.Models
{
    public class TelepatContext
    {
        private string type;
        private long startTime;
        private long endTime;
        private int state;
        private string application_id;
        private string id;
        private string name;

        private Dictionary<string, object> meta;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public long StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        public long EndTime
        {
            get { return endTime; }
            set { endTime = value; }
        }

        public int State
        {
            get { return state; }
            set { state = value; }
        }

        public Dictionary<string, object> Meta
        {
            get { return meta; }
            set { meta = value; }
        }

        public string ApplicationId
        {
            get { return application_id; }
            set { application_id = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
