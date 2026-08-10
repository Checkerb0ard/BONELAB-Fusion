using System.Collections.Concurrent;

namespace LabFusion.Utilities;

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
