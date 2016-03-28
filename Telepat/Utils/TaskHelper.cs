using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelepatSDK.Utils
{
    public static class TaskHelper
    {
        public static void FireTask(Task task)
        {
            task.ConfigureAwait(false);
        }

        public static void FireTask<TResult>(Task<TResult> task)
        {
            task.ConfigureAwait(false);
        }

        public static void RunInBackground(Action action)
        {
            var fireTask = Task.Run(action);
        }

        public static void RunInBackground<TResult>(Func<TResult> func)
        {
            var fireTask = Task.Run(func);
        }

        public static async Task DoInBackground(Action action)
        {
            await Task.Run(action).ConfigureAwait(false);
        }

        public static async Task<TResult> DoInBackground<TResult>(Func<TResult> func)
        {
            return await Task.Run(func).ConfigureAwait(false);
        }

        public static async Task<TResult> DoInBackground<TResult>(Func<Task<TResult>> func)
        {
            return await Task.Run(func).ConfigureAwait(false);
        }

        /// <summary>
        /// Apply ConfigureAwait(false) to task
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task DiscartContext(Task action)
        {
            action.ConfigureAwait(false);
            return action;
        }

        /// <summary>
        /// Apply ConfigureAwait(false) to task
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task<TResult> DiscartContext<TResult>(Task<TResult> action)
        {
            action.ConfigureAwait(false);
            return action;
        }

        /// <summary>
        /// Set a TaskCompletionSource's result in a safe manner
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="tcs"></param>
        /// <param name="result"></param>
        public static void SetResult<TResult>(TaskCompletionSource<TResult> tcs, TResult result)
        {
            if (tcs != null && tcs.Task != null && !tcs.Task.IsCompleted) tcs.TrySetResult(result);
            tcs = null;
        }

        /// <summary>
        /// Set a TaskCompletionSource's exception in a safe manner
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="tcs"></param>
        /// <param name="result"></param>
        public static void SetException<TResult>(TaskCompletionSource<TResult> tcs, Exception result)
        {
            if (tcs != null && tcs.Task != null && !tcs.Task.IsCompleted) tcs.TrySetException(result);
            tcs = null;
        }
    }
}
