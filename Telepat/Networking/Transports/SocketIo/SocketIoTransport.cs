using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quobject.SocketIoClientDotNet.Client;
using TelepatSDK.Utils;
using Newtonsoft.Json;

namespace TelepatSDK.Networking.Transports.SocketIo
{
    public class SocketIoTransport
    {
        public static string PROPERTY_SESSION_ID   = "session_id";

        private static string PROPERTY_APP_VERSION = "appVersion";
        private static int MAX_ATTEMPTS = 3;

        private static readonly SocketIoTransport INSTANCE = new SocketIoTransport();

        public static SocketIoTransport GetInstance() { return INSTANCE; }

        private SocketIoTransport() { }

        private Socket socket = null;
        private bool connected = false;

        private string sessionId = "";

        public EventHandler            Connected;
        public EventHandler            Disconnected;
        public EventHandler<Exception> Error;
        
        public bool IsConnected { get { return connected; } }

        public async Task Initialize(string url)
        {
            if (sessionId == "")
            {
                TaskHelper.FireTask(initAsync(url));
            }
            else
            {
                TaskHelper.FireTask(Telepat.GetInstance().RegisterDeviceWithSocketIo(sessionId, false));
            }
        }

        private Task<object> initAsync(string url)
        {
            return TaskHelper.DoInBackground<object>(async () => {

                int backoff = 2000 + new Random().Next();
                for (int i = 0; i < MAX_ATTEMPTS; i++)
                {
                    try {
                        sessionId = await TaskHelper.DiscartContext(Connect(url));
                        DebugLog.log(sessionId);

                        await Telepat.GetInstance().RegisterDeviceWithSocketIo(sessionId, true);
                        return sessionId;
                    }
                    catch (Exception e) {
                        // TODO: print stack trace
                        DebugLog.catchLog(e);
                        if (i == MAX_ATTEMPTS - 1) break;
                    }

                    try {
                        DebugLog.log("Sleeping for " + backoff + " ms before retry");
                        await Task.Delay(backoff);
                    }
                    catch (TaskCanceledException) {
                        // App finished before we complete - exit.
                        DebugLog.log("Thread interrupted: abort remaining retries!");
                        return "";
                    }
                    // increase backoff exponentially
                    backoff *= 2;
                }

                return null;
            });
        }

        public Task<string> Connect(string url)
        {
            var tcs = new TaskCompletionSource<string>();
            
            var socket = IO.Socket(url);
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                DebugLog.log("socket.io : connected to " + url);
            });

            socket.On("welcome", (args) =>
            {
                var message = JsonConvert.DeserializeObject<Dictionary<string, object>>(args.ToString());
                var sesionID = message.ContainsKey("sessionId") ? message["sessionId"] as string : "error";
                DebugLog.log("Welcomed with sessionID: " + sesionID);

                string deviceId = Telepat.GetInstance().GetAppId();
                string applicationId = Telepat.GetInstance().GetAppId();
                var obj = new Dictionary<string, string>();
                obj["deviceId"] = deviceId;
                obj["application_id"] = applicationId;

                socket.Emit("bindDevice", JsonConvert.SerializeObject(obj));

                connected = true;
                EventHelper.FireEvent(Connected, this);
                TaskHelper.SetResult(tcs, sesionID);
            });

            socket.On(Socket.EVENT_MESSAGE, (message) =>
            {
                int x = 0;
            });

            socket.On(Socket.EVENT_ERROR, (e) =>
            {
                DebugLog.log("socket.io : " + e.ToString());
                var exception = new Exception("Socket.io error : " + e.ToString());
                EventHelper.FireEvent(Error, this, exception);
                TaskHelper.SetException(tcs, exception);
            });

            socket.On(Socket.EVENT_CONNECT_ERROR, (e) =>
            {
                DebugLog.log("socket.io : " + e.ToString());
                var exception = new Exception("Socket.io connect error : " + e.ToString());
                EventHelper.FireEvent(Error, this, exception);
                TaskHelper.SetException(tcs, exception);
            });

            socket.On(Socket.EVENT_DISCONNECT, () =>
            {
                DebugLog.log("socket.io : disconnected from " + url);
                EventHelper.FireEvent(Disconnected, this);
                TaskHelper.SetException(tcs, new Exception("disconnected"));
            });

            return tcs.Task;
        }

        public void Disconnect()
        {
            if (socket != null) socket.Disconnect();
        }
    }
}
