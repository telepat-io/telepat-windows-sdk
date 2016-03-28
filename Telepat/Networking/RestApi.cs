using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelepatSDK.Networking.Responses;
using System.Net.Http;
using TelepatSDK.Utils;
using Newtonsoft.Json;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using TelepatSDK.Models;

namespace TelepatSDK.Networking
{
    public class RestApi
    {
        private static string BASE_ENDPOINT;

        /// <summary>
        /// The Telepat application ID
        /// </summary>
        private string appId;

        /// <summary>
        /// The Telepat API key
        /// </summary>
        private string clientApiKey;

        /// <summary>
        /// The Telepat device identifier
        /// </summary>
        private string udid;

        /// <summary>
        /// The Telepat JWT auth token
        /// </summary>
        private string authorizationToken;

        public string UDID
        {
            get { return udid; }
            set { udid = value; }
        }

        public string AuthorizationToken
        {
            get { return authorizationToken; }
            set { authorizationToken = value; }
        }

        public async Task Initialize(string telepatEndpoint,
                                     string clientApiKey,
                                     string clientAppId)
        {
            this.clientApiKey = clientApiKey;

            appId = clientAppId;

            BASE_ENDPOINT = telepatEndpoint;
            if (BASE_ENDPOINT.EndsWith("/")) {
                BASE_ENDPOINT = BASE_ENDPOINT.Remove(BASE_ENDPOINT.Length - 1);
            }

            udid = await Telepat.GetInstance()
                                .GetDBInstance()
                                .GetOperationsData(TelepatConstants.UDID_KEY, "");
        }

        /// <summary>
        /// Method for sending a device registration request
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Task<GenericApiResponse> RegisterDevice(Dictionary<string, object> body)
        {
            return post<GenericApiResponse, Dictionary<string, object>>("/device/register", body);
        }

        /// <summary>
        /// Method for retrieving all active contexts
        /// </summary>
        /// <returns></returns>
        public Task<ContextApiResponse> UpdateContexts()
        {
           return get<ContextApiResponse>("/context/all");
        }

        /// <summary>
        /// Method for sending an async register request
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Task<StringApiResponse> Register(Dictionary<string, object> body)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method for sending an async register request
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Task<MessageApiResponse> CreateUserWithEmailAndPassword(Dictionary<string, string> body)
        {
            return post<MessageApiResponse, Dictionary<string, string>>("/user/register-username", body);
        }

        /// <summary>
        /// Method for sending an async login request
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Task<GenericApiResponse> Login(Dictionary<string, object> body)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method for sending an async login request
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Task<GenericApiResponse> LoginWithEmailAndPassword(Dictionary<string, string> body)
        {
            return post<GenericApiResponse, Dictionary<string, string>>("/user/login_password", body);
        }

        /// <summary>
        /// Method for sending a logout request
        /// </summary>
        /// <returns></returns>
        public Task<MessageApiResponse> Logout()
        {
            return get<MessageApiResponse>("/user/logout");
        }

        /// <summary>
        /// Method for requesting a password reset email
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Task<MessageApiResponse> RequestPasswordReset(Dictionary<string, string> body)
        {
            return post<MessageApiResponse, Dictionary<string, string>>("/user/request_password_reset", body);
        }

        /// <summary>
        /// Method for changing a user authentication password
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Task<MessageApiResponse> ResetPassword(Dictionary<string, string> body)
        {
            return post<MessageApiResponse, Dictionary<string, string>>("/user/password_reset", body);
        }

        public Task<StringApiResponse> UpdateUser(Dictionary<string, object> body)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method for sending a subscribe request
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Task<JSonApiResponse> Subscribe(Dictionary<string, object> body)
        {
            return post<JSonApiResponse, Dictionary<string, object>>("/object/subscribe", body);
        }

        /// <summary>
        /// Method for sending an unsubscribe request
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Task<MessageApiResponse> Unsubscribe(Dictionary<string, object> body)
        {
            return post<MessageApiResponse, Dictionary<string, object>>("/object/unsubscribe", body);
        }

        /// <summary>
        /// Method for sending an object creation request
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Task<MessageApiResponse> Create(Dictionary<string, object> body)
        {
            return post<MessageApiResponse, Dictionary<string, object>>("/object/create", body);
        }

        /// <summary>
        /// Method for sending an object update request
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Task<MessageApiResponse> Update(Dictionary<string, object> body)
        {
            return post<MessageApiResponse, Dictionary<string, object>>("/object/update", body);
        }

        /// <summary>
        /// Method for sending an object delete request
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Task<MessageApiResponse> Delete(Dictionary<string, object> body)
        {
            return post<MessageApiResponse, Dictionary<string, object>>("/object/delete", body);
        }

        #region Low level HTTP interface

        private async Task<TResponse> get<TResponse>(string url) 
            where TResponse : class
        {
            var httpClient = new HttpClient();
            url = BASE_ENDPOINT + url;

            try {
                applyHeaders(httpClient);
                var response = await httpClient.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) {
                    var responseHashMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);

                    Int32 status = Utilites.GetStatusCode(responseHashMap);
                    return (TResponse)ApiResponseFactory.Create(response.IsSuccessStatusCode,
                                                                response.StatusCode,
                                                                status,
                                                                responseHashMap.Get("content"),
                                                                typeof(TResponse));
                }
                else {
                    return (TResponse)ApiResponseFactory.Create(response.IsSuccessStatusCode,
                                                                response.StatusCode,
                                                                Utilites.GetErrorCode(responseString),
                                                                null,
                                                                typeof(TResponse),
                                                                Utilites.GetErrorMessage(responseString));
                }
            }
            catch (Exception e) {
                DebugLog.catchLog(e);
            }
            return null;
        }

        private async Task<TResponse> post<TResponse, TBody>(string url, TBody body)
            where TResponse : class
            where TBody : class
        {
            var httpClient = new HttpClient();

            try {
                var json = JsonConvert.SerializeObject(body);
                var requestContent = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                url = BASE_ENDPOINT + url;

                applyHeaders(httpClient, requestContent);
                var response = await httpClient.PostAsync(url, requestContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) {
                    var responseHashMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);

                    Int32 status = Utilites.GetStatusCode(responseHashMap);
                    return (TResponse)ApiResponseFactory.Create(response.IsSuccessStatusCode,
                                                                response.StatusCode,
                                                                status,
                                                                responseHashMap.Get("content"),
                                                                typeof(TResponse));
                }
                else {
                    return (TResponse)ApiResponseFactory.Create(response.IsSuccessStatusCode,
                                                                response.StatusCode,
                                                                Utilites.GetErrorCode(responseString),
                                                                null,
                                                                typeof(TResponse),
                                                                Utilites.GetErrorMessage(responseString));
                }
            }
            catch (Exception e) {
                DebugLog.catchLog(e);
            }
            return (TResponse)ApiResponseFactory.Create(false,
                                                        System.Net.HttpStatusCode.BadRequest,
                                                        400,
                                                        null,
                                                        typeof(TResponse),
                                                        "Bad request");
        }

        private void applyHeaders(HttpClient client)
        {
            if (appId != null) client.DefaultRequestHeaders.Add("X-BLGREQ-APPID", appId);
            if (udid != null) client.DefaultRequestHeaders.Add("X-BLGREQ-UDID", udid != "" ? udid : "TP_EMPTY_UDID");
            if (udid != null) client.DefaultRequestHeaders.Add("X-BLGREQ-SIGN", Utilites.HashSha256(clientApiKey));
            if (authorizationToken != null) client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationToken);
        }

        private void applyHeaders(HttpClient client, HttpContent content)
        {
            if (appId != null) content.Headers.Add("X-BLGREQ-APPID", appId);
            if (udid != null) content.Headers.Add("X-BLGREQ-UDID", udid != "" ? udid : "TP_EMPTY_UDID");
            if (udid != null) content.Headers.Add("X-BLGREQ-SIGN", Utilites.HashSha256(clientApiKey));
            if (authorizationToken != null) client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationToken);
        }

        #endregion
    }
}
