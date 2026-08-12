using LabFusion.Network;
using LabFusion.Extensions;

namespace LabFusion.Player;

public delegate void PlayerDelegate(PlayerID playerID);

public static class PlayerIDManager
{
    public const int MaxNameLength = 32;

    public const int MinPlayerID = 0;
    public const int MaxPlayerID = byte.MaxValue;

    /// <summary>
    /// Invoked whenever a PlayerID is registered for a client.
    /// </summary>
    public static event PlayerDelegate PlayerRegistered;

    /// <summary>
    /// Invoked whenever a PlayerID is unregistered from a client leaving or the connection to the server being closed.
    /// </summary>
    public static event PlayerDelegate PlayerUnregistered;

    public static readonly HashSet<PlayerID> PlayerIDs = new();

    public static readonly Dictionary<ClientSmallID, PlayerID> SmallIDLookup = new();
    public static readonly Dictionary<ClientPlatformID, PlayerID> PlatformIDLookup = new();

    public static readonly HashSet<ClientSmallID> ReservedSmallIDs = new();

    public static int PlayerCount => PlayerIDs.Count;
    public static bool HasOtherPlayers => PlayerCount > 1;

    public static ClientPlatformID LocalPlatformID { get; private set; }
    public static ClientSmallID LocalSmallID { get; private set; }
    public static PlayerID LocalID { get; private set; }

    public static readonly ClientSmallID HostSmallID = new(0);

    /// <summary>
    /// Registers a new player after their client has been authorized.
    /// </summary>
    /// <param name="platformID"></param>
    /// <param name="smallID"></param>
    /// <param name="metadata"></param>
    /// <param name="playerID"></param>
    /// <returns></returns>
    public static bool RegisterPlayer(ClientPlatformID platformID, ClientSmallID smallID, Dictionary<string, string> metadata, out PlayerID playerID)
    {
        playerID = GetPlayerID(platformID);

        if (playerID != null)
        {
            return false;
        }

        playerID = GetPlayerID(smallID);

        if (playerID != null)
        {
            return false;
        }

        playerID = new PlayerID(platformID, smallID, metadata);

        PlayerIDs.Add(playerID);
        SmallIDLookup[playerID.SmallID] = playerID;
        PlatformIDLookup[playerID.PlatformID] = playerID;

        ReserveSmallID(playerID.SmallID);

        if (platformID == LocalPlatformID)
        {
            LocalID = playerID;
        }

        playerID.OnRegister();

        PlayerRegistered?.InvokeSafe(playerID, "invoking PlayerRegistered event");

        return true;
    }

    /// <summary>
    /// Unregisters a player after they have lost connection.
    /// </summary>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static bool UnregisterPlayer(ClientPlatformID platformID)
    {
        var playerID = GetPlayerID(platformID);

        if (playerID == null)
        {
            return false;
        }

        var smallID = playerID.SmallID;

        PlayerIDs.Remove(playerID);
        SmallIDLookup.Remove(smallID);
        PlatformIDLookup.Remove(platformID);

        UnreserveSmallID(smallID);

        playerID.OnUnregister();

        PlayerUnregistered?.InvokeSafe(playerID, "invoking PlayerUnregistered event");

        if (playerID == LocalID)
        {
            LocalID = null;
        }

        return true;
    }

    /// <summary>
    /// Unregisters all players when the connection is closed.
    /// </summary>
    public static void UnregisterPlayers()
    {
        var playerIDs = PlayerIDs.ToList();

        foreach (var playerID in playerIDs)
        {
            UnregisterPlayer(playerID.PlatformID);
        }
    }

    public static void ReserveSmallID(ClientSmallID smallID) => ReservedSmallIDs.Add(smallID);

    public static void UnreserveSmallID(ClientSmallID smallID) => ReservedSmallIDs.Remove(smallID);

    public static bool IsSmallIDReserved(ClientSmallID smallID) => ReservedSmallIDs.Contains(smallID);

    public static ClientSmallID? GetUniquePlayerID()
    {
        for (byte i = MinPlayerID; i < MaxPlayerID; i++)
        {
            var smallID = new ClientSmallID(i);

            if (!IsSmallIDReserved(smallID))
            {
                return smallID;
            }
        }

        return null;
    }

    public static PlayerID GetHostID()
    {
        return GetPlayerID(HostSmallID);
    }

    public static bool HasSmallID(ClientPlatformID platformID) => TryGetSmallID(platformID, out _);

    public static bool TryGetSmallID(ClientPlatformID platformID, out ClientSmallID smallID)
    {
        var playerID = GetPlayerID(platformID);

        if (playerID == null)
        {
            smallID = ClientSmallID.Empty;
            return false;
        }

        smallID = playerID.SmallID;
        return true;
    }

    public static bool TryGetPlatformID(ClientSmallID smallID, out ClientPlatformID platformID)
    {
        var playerID = GetPlayerID(smallID);

        if (playerID == null)
        {
            platformID = ClientPlatformID.Empty;
            return false;
        }

        platformID = playerID.PlatformID;
        return true;
    }

    public static bool TryGetPlatformID(ClientSmallID? smallID, out ClientPlatformID platformID)
    {
        if (!smallID.HasValue)
        {
            platformID = ClientPlatformID.Empty;
            return false;
        }

        return TryGetPlatformID(smallID.Value, out platformID);
    }

    public static PlayerID GetPlayerID(ClientSmallID smallID)
    {
        if (SmallIDLookup.TryGetValue(smallID, out var playerID))
        {
            return playerID;
        }

        return null;
    }

    public static PlayerID GetPlayerID(ClientPlatformID platformID)
    {
        if (PlatformIDLookup.TryGetValue(platformID, out var playerID))
        {
            return playerID;
        }

        return null;
    }

    public static bool HasPlayerID(ClientSmallID smallID) => SmallIDLookup.ContainsKey(smallID);

    public static bool HasPlayerID(ClientPlatformID platformID) => PlatformIDLookup.ContainsKey(platformID);

    public static void SetPlatformID(ClientPlatformID platformID)
    {
        LocalPlatformID = platformID;
    }
}