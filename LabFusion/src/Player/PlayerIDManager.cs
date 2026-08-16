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
    /// Invoked whenever a PlayerID is registered for a client, including the local client.
    /// </summary>
    public static event PlayerDelegate PlayerRegistered;

    /// <summary>
    /// Invoked whenever a PlayerID is unregistered from a client leaving or the connection to the server being closed, including the local client.
    /// </summary>
    public static event PlayerDelegate PlayerUnregistered;

    /// <summary>
    /// Invoked whenever a player has just joined the server, including the local client.
    /// <para>This will not be invoked for already existing players that are registered whenever the client joins an active server.</para>
    /// </summary>
    public static event PlayerDelegate PlayerJoined;

    /// <summary>
    /// Invoked whenever a player has just left the server, including the local client.
    /// <para>This will not be invoked whenever players are unregistered upon the server or client losing connection.</para>
    /// </summary>
    public static event PlayerDelegate PlayerLeft;

    public static readonly HashSet<PlayerID> PlayerIDs = new();

    public static readonly Dictionary<ClientSmallID, PlayerID> SmallIDLookup = new();
    public static readonly Dictionary<ClientPlatformID, PlayerID> PlatformIDLookup = new();

    public static readonly HashSet<ClientSmallID> ReservedSmallIDs = new();

    public static int PlayerCount => PlayerIDs.Count;
    public static bool HasOtherPlayers => PlayerCount > 1;

    /// <summary>
    /// Returns true if a platform ID has been set for the local client.
    /// </summary>
    public static bool HasLocalPlatformID => LocalPlatformID.HasValue;

    /// <summary>
    /// Returns true if a small ID has been set for the local client.
    /// </summary>
    public static bool HasLocalSmallID => LocalSmallID.HasValue;

    /// <summary>
    /// The platform ID for the local client. This is only guaranteed to exist when the client is connected to a server.
    /// <para>Otherwise, it may exist if the current network layer supports persistent platform IDs, but may not exist if the platform ID is per connection.</para>
    /// </summary>
    public static ClientPlatformID? LocalPlatformID { get; private set; } = null;

    /// <summary>
    /// The small ID for the local client. This ID is determined by the server to simplify clients targeting other clients and will be null when the connection is not yet authorized.
    /// </summary>
    public static ClientSmallID? LocalSmallID { get; private set; } = null;
    public static PlayerID LocalID { get; private set; }

    // TODO: Remove! We may not always have a host client anymore!
    public static readonly ClientSmallID HostSmallID = new(0);

    /// <summary>
    /// Registers a new player after their client has been authorized.
    /// </summary>
    /// <param name="platformID"></param>
    /// <param name="smallID"></param>
    /// <param name="metadata"></param>
    /// <param name="isJoining"></param>
    /// <param name="playerID"></param>
    /// <returns></returns>
    public static bool RegisterPlayer(ClientPlatformID platformID, ClientSmallID smallID, Dictionary<string, string> metadata, bool isJoining, out PlayerID playerID)
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
            LocalSmallID = smallID;
            LocalID = playerID;
        }

        playerID.OnRegister();

        PlayerRegistered?.InvokeSafe(playerID, "invoking PlayerRegistered event");

        if (isJoining)
        {
            PlayerJoined?.InvokeSafe(playerID, "invoking PlayerJoined event");
        }

        return true;
    }

    /// <summary>
    /// Unregisters a player after they have lost connection.
    /// </summary>
    /// <param name="platformID"></param>
    /// <returns></returns>
    public static bool UnregisterPlayer(ClientPlatformID platformID, bool isLeaving)
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

        PlayerUnregistered?.InvokeSafe(playerID, "invoking PlayerUnregistered event");

        if (isLeaving)
        {
            PlayerLeft?.InvokeSafe(playerID, "invoking PlayerLeft event");
        }

        playerID.OnUnregister();

        if (playerID == LocalID)
        {
            LocalSmallID = null;
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
            UnregisterPlayer(playerID.PlatformID, false);
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

    /// <summary>
    /// Returns true if a platform ID matches the local client's platform ID.
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    public static bool IsClientMe(ClientPlatformID client) => LocalPlatformID == client;

    /// <summary>
    /// Sets the local platform ID. This should be established some time before the client's connection is authorized and remain persistent throughout.
    /// <para>The connected server should also be able to read the platform ID through the connection without relying on the client's validation.
    /// Otherwise, the client may be disconnected for having an invalid platform ID.</para>
    /// </summary>
    /// <param name="platformID"></param>
    public static void SetLocalPlatformID(ClientPlatformID? platformID = null) => LocalPlatformID = platformID;
}