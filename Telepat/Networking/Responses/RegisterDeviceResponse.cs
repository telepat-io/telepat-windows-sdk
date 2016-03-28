using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelepatSDK.Networking.Responses
{
    public class RegisterDeviceResponse
    {
        /// <summary>
        /// The API status code
        /// </summary>
        public int Status;

        /// <summary>
        /// The Telepat device identifier
        /// </summary>
        public string Identifier;
    }
}
