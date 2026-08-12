using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Preferences;

using Il2CppSLZ.Marrow.SceneStreaming;

namespace LabFusion.Network;

/// <summary>
/// Internal class used for cleaning up servers, executing events on disconnect, etc.
/// </summary>
public static class InternalServerHelpers
{
    /// <summary>
    /// Initializes information about the server, such as module types.
    /// </summary>
    public static void OnStartServer()
    {
        // Reload the scene
        SceneStreamer.Reload();
    }

    /// <summary>
    /// Called when the user joins a server.
    /// </summary>
    public static void OnJoinServer()
    {
        // Send settings
        FusionPreferences.SendClientSettings();
    }

    /// <summary>
    /// Updates information about the new user.
    /// </summary>
    /// <param name="id"></param>
    public static void OnPlayerJoined(PlayerID id, bool isInitialJoin)
    {
        // Send client info
        FusionPreferences.SendClientSettings();

        // Update hooks
        MultiplayerHooking.InvokeOnPlayerJoined(id);

        // Send notification
        if (isInitialJoin && id.TryGetDisplayName(out var name))
        {
            NetworkNotifications.SendPlayerJoinedNotification(name);
        }
    }

    /// <summary>
    /// Cleans up a single user after they have left.
    /// </summary>
    /// <param name="longId"></param>
    public static void OnPlayerLeft(ClientPlatformID platformID)
    {
        var playerId = PlayerIDManager.GetPlayerID(platformID);

        // Make sure the player exists in our game
        if (playerId == null)
            return;

        // Send notification
        if (playerId.TryGetDisplayName(out var name))
        {
            NetworkNotifications.SendPlayerLeftNotification(name);
        }

        PlayerIDManager.UnregisterPlayer(platformID);

        MultiplayerHooking.InvokeOnPlayerLeft(playerId);
    }
}