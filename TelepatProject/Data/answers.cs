using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelepatSDK.Models;

namespace TelepatProject.Data
{
    public class answers : TelepatBaseObject
    {
        string _polls_id;
        public string polls_id
        {
            get { return _polls_id; }
            set { _polls_id = value; OnPropertyChanged("polls_id");  }
        }

        int _option_index = -1;
        public int option_index
        {
            get { return _option_index; }
            set { _option_index = value; OnPropertyChanged("option_index"); }
        }
    }
}
