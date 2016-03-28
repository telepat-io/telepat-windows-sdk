using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

// Created by Dorin Damaschin on 28.01.2016
// Response class for User/Login

namespace TelepatSDK.Networking.Responses
{
    public class GenericApiResponse : ApiResponse<Dictionary<string, object>>
    {
        public GenericApiResponse(bool isSuccessStatusCode, HttpStatusCode httpStatusCode, int statusCode, object content, string errorMessage) :
            base(isSuccessStatusCode, httpStatusCode, statusCode, content, errorMessage)
        { }
    }
}
