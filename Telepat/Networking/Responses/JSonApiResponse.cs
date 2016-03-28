using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TelepatSDK.Networking.Responses
{
    public class JSonApiResponse : ApiResponse<List<JObject>>
    {
        public JSonApiResponse(bool isSuccessStatusCode, HttpStatusCode httpStatusCode, int statusCode, object content, string errorMessage) :
            base(isSuccessStatusCode, httpStatusCode, statusCode, content, errorMessage)
        { }
    }
}
