using System.Collections.Concurrent;

namespace LabFusion.Utilities;

/// <summary>
/// Helper for dealing with running functions on different threads.
/// </summary>
public static class ThreadHelper
{
    private static readonly ConcurrentQueue<Action> _actionQueue = new();

    /// <summary>
    /// Runs an action on the main thread.
    /// If called on the main thread, this will cause the action to be delayed by a frame.
    /// </summary>
    /// <param name="action"></param>
    public static void RunOnMainThread(Action action)
    {
        _actionQueue.Enqueue(action);
    }

    /// <summary>
    /// Runs an action on the main thread returning a task that can be awaited.
    /// If called on the main thread, this will cause the action to be delayed by a frame.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Task RunOnMainThreadAsTask(Action action)
    {
        var completionSource = new TaskCompletionSource();

        RunOnMainThread(ExecuteAction);

        return completionSource.Task;

        void ExecuteAction()
        {
            try
            {
                action.Invoke();

                completionSource.SetResult();
            }
            catch (Exception ex)
            {
                completionSource.SetException(ex);
            }
        }
    }

    /// <summary>
    /// Runs a function on the main thread returning a task that can be awaited with a generic return type.
    /// If called on the main thread, this will cause the function to be delayed by a frame.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="function"></param>
    /// <returns></returns>
    public static Task<TResult> RunOnMainThreadAsTask<TResult>(Func<TResult> function)
    {
        var completionSource = new TaskCompletionSource<TResult>();

        RunOnMainThread(ExecuteFunction);

        return completionSource.Task;

        void ExecuteFunction()
        {
            try
            {
                var result = function.Invoke();

                completionSource.SetResult(result);
            }
            catch (Exception ex)
            {
                completionSource.SetException(ex);
            }
        }
    }

    internal static void Tick()
    {
        while (!_actionQueue.IsEmpty)
        {
            try
            {
                if (!_actionQueue.TryDequeue(out var action))
                {
                    break;
                }

                action();
            }
            catch (Exception ex)
            {
                FusionLogger.LogException("running action on main thread", ex);
            }
        }
    }
}
