using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelepatSDK.Models;
using System.Net;

// Created by Dorin Damaschin on 28.01.2016

namespace TelepatSDK.Networking.Responses
{
    public class ContextApiResponse : ApiResponse<List<TelepatContext>>
    {
        public ContextApiResponse(bool isSuccessStatusCode, HttpStatusCode httpStatusCode, int statusCode, object content, string errorMessage) :
            base(isSuccessStatusCode, httpStatusCode, statusCode, content, errorMessage)
        { }
    }
}
