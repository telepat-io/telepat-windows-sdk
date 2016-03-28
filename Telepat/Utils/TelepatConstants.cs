using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelepatSDK.Utils
{
    public static class TelepatConstants
    {
        public static string TAG = "TelepatSDK";
	    public static bool RETROFIT_DEBUG_ENABLED  = true;

	    public static string UDID_KEY = "udid";
	    public static string JWT_KEY = "authentication-token";
	    public static string JWT_TIMESTAMP_KEY = "authentication-token-timestamp";
	    public static string CURRENT_USER_DATA = "current-user-data";
	    public static string FB_TOKEN_KEY = "fb-token";
	    public static string LOCAL_UDID_KEY = "local-udid";
	    public static int JWT_MAX_AGE = 60 * 60 * 1000;
    }
}
