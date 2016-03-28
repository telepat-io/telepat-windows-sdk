using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace TelepatSDK.Utils
{
    public static class Utilites
    {
        public static string Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        public static object Deserialize(string xml, Type toType)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(toType);
                return deserializer.ReadObject(stream);
            }
        }

        public static string HashSha256(string input)
        {
            IBuffer buffUtf8 = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);

            // Create a HashAlgorithmProvider object.
            var provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            IBuffer buffHash = provider.HashData(buffUtf8);

            var buffer = buffHash.ToArray();
            string hashString = string.Empty;
            foreach (byte x in buffer)
            {
                hashString += String.Format("{0:x2}", x);
            }

            return hashString;
        }

        /// <summary>
        /// Application's version code from the <code>Package.Current.ID</code>.
        /// </summary>
        /// <returns></returns>
        public static string GetAppName()
        {
            try {
                var packageInfo = Package.Current.Id;
                return packageInfo.Name;
            }
            catch (Exception e) {
                // should never happen
                throw new Exception("Could not get package name: " + e);
            }
        }

        /// <summary>
        /// Application's version code from the <code>Package.Current.ID</code>.
        /// </summary>
        /// <returns></returns>
        public static int GetAppVersion()
        {
            try {
                var packageInfo = Package.Current.Id;
                return packageInfo.Version.Build;
            }
            catch (Exception e) {
                // should never happen
                throw new Exception("Could not get package name: " + e);
            }
        }

        public static int GetErrorCode(string errorBody)
        {
            int code = 0;
            try {
                var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(errorBody);
                code = obj.ContainsKey("status") ? Convert.ToInt32(obj["status"]) : 0;
            } catch { }
            return code;
        }

        public static string GetErrorMessage(string errorBody)
        {
            string message = "";
            try {
                var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(errorBody);
                message = obj.ContainsKey("message") ? obj["message"] : "";
            } catch { }
            return message;
        }

        public static int GetStatusCode(Dictionary<string, object> dictionary)
        {
            var status = dictionary.Get("status");
            if (status != null)  return Convert.ToInt32(status.ToString());
            return -1;
        }
    }
}
