using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelepatSDK.Utils
{
    public static class EventHelper
    {
        public static void FireEvent<T>(EventHandler<T> ev, object context, T args)
        {
            if (ev != null) ev(context, args);
        }

        public static void FireEvent(EventHandler ev, object context)
        {
            if (ev != null) ev(context, null);
        }
    }
}
