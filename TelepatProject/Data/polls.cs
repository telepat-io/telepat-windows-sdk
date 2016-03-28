using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelepatSDK.Models;

namespace TelepatProject
{
    public class polls : TelepatBaseObject
    {
        string _question;
        public string question
        {
            get { return _question; }
            set { _question = value; OnPropertyChanged("question"); }
        }

        List<option> _option;
        public List<option> option
        {
            get { return _option; }
            set { _option = value; OnPropertyChanged("option"); }
        }
    }
}
