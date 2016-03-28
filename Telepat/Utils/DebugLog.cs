using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelepatSDK.Utils
{
    internal class DebugLog
    {
        static String tag = ((Type)typeof(DebugLog)).Namespace.Split('.')[0];
        static bool justShow = false;
        internal static int debugLevel = 2;
        internal static bool debugMode = false;

        static DebugLog()
        {
#if DEBUG
            if(debugMode)
            System.Diagnostics.Debug.WriteLine(tag + " : " + "DebugLevel: " + debugLevel + " justShow: " + justShow);
#endif
        }
        public static void log(string text)
        {
#if DEBUG
            if (debugMode)
                System.Diagnostics.Debug.WriteLine(tag + " : " + text);
#endif
        }

        public static void log(string secondTag, string text)
        {
            log(secondTag + " : " + text);
        }

        public static void log(object sender, string text)
        {
            log(sender.GetType().Name, text);
        }

        public static void log(int inputDebugLevel, string text)
        {
#if DEBUG
            if (debugMode && inputDebugLevel <= debugLevel)
                System.Diagnostics.Debug.WriteLine(tag + " : " + text);
#endif
        }

        public static void log(int inputDebugLevel, string secondTag, string text)
        {
            log(inputDebugLevel, secondTag + " : " + text);
        }

        public static void log(int inputDebugLevel, object sender, string text)
        {
            log(inputDebugLevel, sender.GetType().Name, text);
        }

        public static void catchLogAndThrow(int inputDebugLevel, Exception e)
        {
            DebugLog.catchLogAndThrow(inputDebugLevel, "", e);
        }

        public static void catchLogAndThrow(int inputDebugLevel, object sender, Exception e)
        {
            DebugLog.catchLogAndThrow(inputDebugLevel, sender.GetType().Name, e);
        }

        public static void catchLogAndThrow(int inputDebugLevel, string className, Exception e)
        {
            if (inputDebugLevel <= debugLevel)
                DebugLog.catchLogAndThrow(className, e);
        }

        public static void catchLogAndThrow(Exception e)
        {
            DebugLog.catchLogAndThrow("", e);
        }

        public static void catchLog(Exception e)
        {
            DebugLog.catchLog("", e);
        }

        public static void catchLogAndThrow(object sender, Exception e)
        {
            catchLogAndThrow(sender.GetType().Name, e);
        }

        public static void catchLogAndThrow(string className, Exception e)
        {
            if (e.InnerException != null)
            {
                DebugLog.log(className, " Exception: " + e + "\nMesage: " + e.Message + "\nInnerException: " + e.InnerException + " \nData: " + e.Data);
                if (!justShow)
                    throw new Exception(className + " Exception: " + e + " InnerException: " + e.InnerException);
            }
            else
            {
                DebugLog.log(className, " Exception" + e);
                if (!justShow)
                    throw new Exception(className + " Exception: " + e);
            }
        }

        public static void catchLog(string className, Exception e)
        {
            if (e.InnerException != null) {
                DebugLog.log(className, " Exception: " + e + "\nMesage: " + e.Message + "\nInnerException: " + e.InnerException + " \nData: " + e.Data);
            }
            else {
                DebugLog.log(className, " Exception" + e);
            }
        }
    }
}
