using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace TelepatSDK.Networking.Requests
{
    public class RegisterDeviceRequest
    {
        /// <summary>
        /// Type of connection used
        /// </summary>
        private TransportConnectionType type;

        /// <summary>
        /// The transport token of the device
        /// </summary>
        private string token;

        public RegisterDeviceRequest(TransportConnectionType type, string token)
        {
            this.type = type;
            this.token = token;
        }

        public async Task<Dictionary<string, object>> GetParams()
        {
            var param = new Dictionary<string, object>();
            var persistentData = new Dictionary<string, object>();
            var volatileData = new Dictionary<string, object>();
            var deviceInfo = new Dictionary<string, object>();

            EasClientDeviceInformation device = new EasClientDeviceInformation();

            switch (type)
            {
                case TransportConnectionType.SocketIO:
                    persistentData.Add("type", "windows");
                    persistentData.Add("token", "");
                    persistentData.Add("active", 0);

                    volatileData.Add("type", "sockets");
                    volatileData.Add("token", token);
                    volatileData.Add("active", 1);
                    break;

                default:
                    persistentData.Add("type", "windows");
                    persistentData.Add("token", token);
                    persistentData.Add("active", 1);

                    volatileData.Add("type", "sockets");
                    volatileData.Add("token", "");
                    volatileData.Add("active", 0);
                    break;
            }

            deviceInfo.Add("os", device.OperatingSystem);
            deviceInfo.Add("udid", await Telepat.GetInstance().GetDeviceLocalIdentifier());
            deviceInfo.Add("manufacturer", device.SystemManufacturer);
            deviceInfo.Add("model", device.SystemProductName);

            param.Add("volatile", volatileData);
            param.Add("persistent", persistentData);
            param.Add("info", deviceInfo);

            return param;
        }
    }
}
