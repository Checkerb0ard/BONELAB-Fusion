using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.UI.Popups;
using LabFusion.Permissions;

namespace LabFusion.Network;

/// <summary>
/// Helper class for calling basic methods on the Server or Client.
/// </summary>
public static class NetworkHelper
{
    /// <summary>
    /// Returns true if this user is friended on the active network platform.
    /// </summary>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static bool IsFriend(ClientPlatformID platformID)
    {
        if (NetworkLayerManager.Layer != null)
            return NetworkLayerManager.Layer.IsFriend(platformID);

        return false;
    }

    /// <summary>
    /// Kicks a user from the game.
    /// </summary>
    /// <param name="id"></param>
    public static void KickUser(PlayerID id)
    {
        // Don't kick master users
        if (MasterPermissionsManager.IsMaster(id.PlatformID))
        {
            if (!id.TryGetDisplayName(out var name))
                name = "Wacky Willy";

            Notifier.Send(new Notification()
            {
                Title = "Failed to Kick User",

                Message = $"{name} has denied your kick request.",

                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.ERROR,
            });

            return;
        }

        ServerManager.SendDisconnect(id.PlatformID, "Kicked from Server");
    }

    /// <summary>
    /// Bans a user from the game.
    /// </summary>
    /// <param name="id"></param>
    public static void BanUser(PlayerID id)
    {
        // Don't ban master users
        if (MasterPermissionsManager.IsMaster(id.PlatformID))
        {
            if (!id.TryGetDisplayName(out var name))
                name = "Wacky Willy";

            Notifier.Send(new Notification()
            {
                Title = "Failed to Ban User",

                Message = $"{name} has denied your ban request.",

                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.ERROR,
            });

            return;
        }

        BanManager.Ban(new PlayerInfo(id), "Banned");
        ServerManager.SendDisconnect(id.PlatformID, "Banned from Server");
    }

    /// <summary>
    /// Checks if a user is banned.
    /// </summary>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static bool IsBanned(ClientPlatformID platformID)
    {
        // Check if the user is a master
        if (MasterPermissionsManager.IsMaster(platformID))
            return false;

        // Check the ban list
        foreach (var ban in BanManager.BanList.Bans)
        {
            if (ban.Player.PlatformID == platformID)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Pardons a user from the ban list.
    /// </summary>
    /// <param name="platformID"></param>
    public static void PardonUser(ClientPlatformID platformID)
    {
        BanManager.Pardon(platformID);
    }
}