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

        // Send notification
        if (isInitialJoin && id.TryGetDisplayName(out var name))
        {
            NetworkNotifications.SendPlayerJoinedNotification(name);
        }
    }
}