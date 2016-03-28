using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TelepatSDK.Models;

namespace TelepatSDK.Networking.Responses
{
    public static class ApiResponseFactory
    {
        public static object Create(bool isSuccessStatusCode, HttpStatusCode httpStatusCode, int statusCode, object content, Type type, string errorMessage = "")
        {
            if (type == typeof(GenericApiResponse)) {
                return new GenericApiResponse(isSuccessStatusCode, 
                                              httpStatusCode, 
                                              statusCode,
                                              content != null ? JsonConvert.DeserializeObject<Dictionary<string, object>>(content.ToString()) : null,
                                              errorMessage);
            }
            if (type == typeof(StringApiResponse)) {
                return new StringApiResponse(isSuccessStatusCode,
                                             httpStatusCode,
                                             statusCode,
                                             content != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(content.ToString()) : null,
                                             errorMessage);
            }
            if (type == typeof(JSonApiResponse)) {
                return new JSonApiResponse(isSuccessStatusCode,
                                           httpStatusCode,
                                           statusCode,
                                           content != null ? JsonConvert.DeserializeObject<List<JObject>>(content.ToString()) : null,
                                           errorMessage);
            }
            if (type == typeof(ContextApiResponse)) {
                return new ContextApiResponse(isSuccessStatusCode,
                                              httpStatusCode,
                                              statusCode,
                                              content != null ? JsonConvert.DeserializeObject<List<TelepatContext>>(content.ToString()) : null,
                                              errorMessage);
            }
            if (type == typeof(MessageApiResponse)) {
                return new MessageApiResponse(isSuccessStatusCode,
                                              httpStatusCode,
                                              statusCode,
                                              content is string ? content : "",
                                              errorMessage);
            }
            return null;
        }
    }

    public abstract class ApiResponse<T>
        where T : class
    {
        public bool           IsSuccessStatusCode { get; }
        public HttpStatusCode HttpStatusCode { get; }
        public int            StatusCode { get; }

        public T              Content { get; }

        public string         ErrorMessage { get; }

        public ApiResponse(bool isSuccessStatusCode, HttpStatusCode httpStatusCode, int statusCode, object content, string errorMessage = "")
        {
            IsSuccessStatusCode = isSuccessStatusCode;
            HttpStatusCode = httpStatusCode;
            StatusCode = statusCode;
            Content = content is T ? content as T : null;
            ErrorMessage = errorMessage;
        }
    }
}
