using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace TelepatProject.Utils
{
    public static class TaskHelper
    {
        public static void RunOnUiThread(CoreDispatcher dispatcher, Action action)
        {
            var task = dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(action));
        }
    }
}
