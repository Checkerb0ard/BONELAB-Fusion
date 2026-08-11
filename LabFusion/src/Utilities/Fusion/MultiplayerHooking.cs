using LabFusion.Player;
using LabFusion.Extensions;

namespace LabFusion.Utilities;

public delegate bool UserAccessEvent(PlayerID playerId, out string reason);
public delegate void ServerEvent();
public delegate void UpdateEvent();
public delegate void PlayerUpdate(PlayerID playerId);

/// <summary>
/// Hooks for getting events from the server, players, etc.
/// <para> All hooks are events. You cannot invoke them yourself. </para>
/// </summary>
public static class MultiplayerHooking
{
    // Server hooks
    public static event PlayerUpdate OnPlayerJoined, OnPlayerLeft;

    internal static void InvokeOnPlayerJoined(PlayerID id) => OnPlayerJoined.InvokeSafe(id, "executing OnPlayerJoined hook");

    internal static void InvokeOnPlayerLeft(PlayerID id) => OnPlayerLeft.InvokeSafe(id, "executing OnPlayerLeft hook");

    // Unity hooks
    /// <summary>
    /// A hook for frame updates. Errors are not caught for performance reasons, please use carefully!
    /// </summary>
    public static event UpdateEvent OnUpdate, OnFixedUpdate, OnLateUpdate;

    public static event UpdateEvent OnMainSceneInitialized, OnLoadingBegin, OnTargetLevelLoaded;

    internal static void InvokeOnUpdate() => OnUpdate?.Invoke();
    internal static void InvokeOnFixedUpdate() => OnFixedUpdate?.Invoke();
    internal static void InvokeOnLateUpdate() => OnLateUpdate?.Invoke();
    internal static void InvokeOnMainSceneInitialized() => OnMainSceneInitialized.InvokeSafe("executing OnMainSceneInitialized hook");
    internal static void InvokeOnLoadingBegin() => OnLoadingBegin.InvokeSafe("executing OnLoadingBegin hook");
    internal static void InvokeTargetLevelLoaded() => OnTargetLevelLoaded.InvokeSafe("executing OnTargetLevelLoaded hook");
}