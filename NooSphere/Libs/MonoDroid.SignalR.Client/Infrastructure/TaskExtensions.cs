using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;

namespace MonoSingalRLib.Infrastructure
{
    #region .net
    //public static class TaskExtensions
    //{
    //    public static Task Unwrap(this Task<Task> task)
    //    {
    //        bool result;
    //        if (task == null)
    //        {
    //            throw new ArgumentNullException("task");
    //        }
    //        TaskCompletionSource<Task> tcs = new TaskCompletionSource<Task>(task.CreationOptions & TaskCreationOptions.AttachedToParent);
    //        task.ContinueWith(delegate
    //        {
    //            Action<Task> continuationAction = null;
    //            Action<Task> action2 = null;
    //            switch (task.Status)
    //            {
    //                case TaskStatus.RanToCompletion:
    //                    if (task.Result != null)
    //                    {
    //                        if (action2 == null)
    //                        {
    //                            action2 = _ => result = tcs.TrySetFromTask<Task>(task.Result);
    //                        }
    //                        if (continuationAction == null)
    //                        {
    //                            continuationAction = antecedent => tcs.TrySetException(antecedent.Exception);
    //                        }
    //                        task.Result.ContinueWith(action2, TaskContinuationOptions.ExecuteSynchronously).ContinueWith(continuationAction, TaskContinuationOptions.OnlyOnFaulted);
    //                        return;
    //                    }
    //                    tcs.TrySetCanceled();
    //                    return;

    //                case TaskStatus.Canceled:
    //                case TaskStatus.Faulted:
    //                    result = tcs.TrySetFromTask<Task>(task);
    //                    return;
    //            }
    //        }, TaskContinuationOptions.ExecuteSynchronously).ContinueWith(delegate(Task antecedent)
    //        {
    //            tcs.TrySetException(antecedent.Exception);
    //        }, TaskContinuationOptions.OnlyOnFaulted);
    //        return tcs.Task;
    //    }

    //    private static bool TrySetFromTask<TResult>(this TaskCompletionSource<TResult> me, Task source)
    //    {
    //        switch (source.Status)
    //        {
    //            case TaskStatus.RanToCompletion:
    //                if (source is Task<TResult>)
    //                {
    //                    return me.TrySetResult(((Task<TResult>)source).Result);
    //                }
    //                return me.TrySetResult(default(TResult));

    //            case TaskStatus.Canceled:
    //                return me.TrySetCanceled();

    //            case TaskStatus.Faulted:
    //                return me.TrySetException(source.Exception.InnerExceptions);
    //        }
    //        return false;
    //    }

    //    public static Task<TResult> Unwrap<TResult>(this Task<Task<TResult>> task)
    //    {
    //        bool result;
    //        if (task == null)
    //        {
    //            throw new ArgumentNullException("task");
    //        }
    //        TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>(task.CreationOptions & TaskCreationOptions.AttachedToParent);
    //        task.ContinueWith(delegate
    //        {
    //            Action<Task> continuationAction = null;
    //            Action<Task<TResult>> action2 = null;
    //            switch (task.Status)
    //            {
    //                case TaskStatus.RanToCompletion:
    //                    if (task.Result != null)
    //                    {
    //                        if (action2 == null)
    //                        {
    //                            action2 = _ => result = tcs.TrySetFromTask<TResult>(task.Result);
    //                        }
    //                        if (continuationAction == null)
    //                        {
    //                            continuationAction = antecedent => tcs.TrySetException(antecedent.Exception);
    //                        }
    //                        task.Result.ContinueWith(action2, TaskContinuationOptions.ExecuteSynchronously).ContinueWith(continuationAction, TaskContinuationOptions.OnlyOnFaulted);
    //                        return;
    //                    }
    //                    tcs.TrySetCanceled();
    //                    return;

    //                case TaskStatus.Canceled:
    //                case TaskStatus.Faulted:
    //                    result = tcs.TrySetFromTask<TResult>(task);
    //                    return;
    //            }
    //        }, TaskContinuationOptions.ExecuteSynchronously).ContinueWith(delegate(Task antecedent)
    //        {
    //            tcs.TrySetException(antecedent.Exception);
    //        }, TaskContinuationOptions.OnlyOnFaulted);
    //        return tcs.Task;
    //    }


    //}
    #endregion

    public static class TaskExtensions
    {
        const TaskContinuationOptions opt = TaskContinuationOptions.ExecuteSynchronously;

        public static Task<TResult> Unwrap<TResult>(this Task<Task<TResult>> task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            TaskCompletionSource<TResult> src = new TaskCompletionSource<TResult>();

            task.ContinueWith(t1 => CopyCat(t1, src, () => t1.Result.ContinueWith(t2 => CopyCat(t2, src, () => src.SetResult(t2.Result)), opt)), opt);

            return src.Task;
        }

        public static Task Unwrap(this Task<Task> task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            TaskCompletionSource<object> src = new TaskCompletionSource<object>();

            task.ContinueWith(t1 => CopyCat(t1, src, () => t1.Result.ContinueWith(t2 => CopyCat(t2, src, () => src.SetResult(null)), opt)), opt);

            return src.Task;
        }

        static void CopyCat<TResult>(Task source,
        TaskCompletionSource<TResult> dest,
        Action normalAction)
        {
            if (source.IsCanceled)
                dest.SetCanceled();
            else if (source.IsFaulted)
                dest.SetException(source.Exception.InnerExceptions);
            else
                normalAction();
        }
    }
}