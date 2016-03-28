using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TelepatSDK.Networking.Responses
{
    public class MessageApiResponse : ApiResponse<string>
    {
        public MessageApiResponse(bool isSuccessStatusCode, HttpStatusCode httpStatusCode, int statusCode, object content, string errorMessage) :
            base(isSuccessStatusCode, httpStatusCode, statusCode, content, errorMessage)
        { }
    }
}
