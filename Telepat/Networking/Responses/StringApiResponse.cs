using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

// Created by Dorin Damaschin on 28.01.2016

namespace TelepatSDK.Networking.Responses
{
    public class StringApiResponse : ApiResponse<Dictionary<string, string>>
    {
        public StringApiResponse(bool isSuccessStatusCode, HttpStatusCode httpStatusCode, int statusCode, object content, string errorMessage) :
            base(isSuccessStatusCode, httpStatusCode, statusCode, content, errorMessage)
        { }
    }
}
