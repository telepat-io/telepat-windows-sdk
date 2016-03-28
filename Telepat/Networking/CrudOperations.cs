using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TelepatSDK.Networking.Responses;
using TelepatSDK.Utils;

namespace TelepatSDK.Networking
{
    public static class CrudOperations
    {
        public static void Resolve(string opType, MessageApiResponse apiResponse)
        {
            if (apiResponse.IsSuccessStatusCode)
            {
                if (apiResponse.StatusCode == 202)
                    DebugLog.log(opType + " successful: " + apiResponse.Content);
                else
                    DebugLog.log(opType + " failed: " + apiResponse.Content);
            }
            else
                DebugLog.log(opType + " failed: " + apiResponse.Content);
        }
    }
}
