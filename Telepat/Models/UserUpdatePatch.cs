using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelepatSDK.Models
{
    public class UserUpdatePatch
    {
        public string FieldName { get; set; }

        public string FieldValue { get; set; }
    }
}
