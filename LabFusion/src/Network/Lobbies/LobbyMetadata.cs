using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Marrow;

using System.Text.Json;

namespace LabFusion.Network;

public struct LobbyMetadata
{
    public static readonly LobbyMetadata Empty = new()
    {
        LobbyInfo = null,
        ServerID = ServerID.Empty,
        HasLobbyOpen = false,
        LobbyCode = null,
        Privacy = ServerPrivacy.PUBLIC,
        Full = false,
        VersionMajor = 0,
        VersionMinor = 0,
        Game = null,
    };

    public LobbyInfo LobbyInfo { get; set; }

    public ServerID ServerID { get; set; }

    public bool HasLobbyOpen { get; set; }

    public string LobbyCode { get; set; }

    public ServerPrivacy Privacy { get; set; }

    public bool Full { get; set; }

    public int VersionMajor { get; set; }

    public int VersionMinor { get; set; }

    public string Game { get; set; }

    /// <summary>
    /// Creates lobby metadata from the current server.
    /// <para>This is not safe to call on other threads as it can return game state.</para>
    /// </summary>
    /// <returns></returns>
    public static LobbyMetadata CreateFromServer()
    {
        var lobbyInfo = LobbyInfoManager.LobbyInfo;

        if (lobbyInfo == null)
        {
            return Empty;
        }

        return new LobbyMetadata()
        {
            LobbyInfo = lobbyInfo,
            HasLobbyOpen = ServerManager.IsServerRunning,
            LobbyCode = lobbyInfo.LobbyCode,
            Privacy = lobbyInfo.Privacy,
            Full = lobbyInfo.PlayerCount >= lobbyInfo.MaxPlayers,
            VersionMajor = lobbyInfo.LobbyVersion.Major,
            VersionMinor = lobbyInfo.LobbyVersion.Minor,
            Game = Support.GameInfo.GameName,
        };
    }

    /// <summary>
    /// Writes lobby metadata from the current server to a NetworkLobby.
    /// <para>This is not safe to call on other threads as it can return game state.</para>
    /// </summary>
    /// <param name="lobby"></param>
    public static void WriteServerToLobby(NetworkLobby lobby)
    {
        var metadata = CreateFromServer();

        metadata.WriteToLobby(lobby);
    }

    /// <summary>
    /// Attempts to read metadata from a NetworkLobby, or returns false if the metadata is invalid.
    /// <para>This should be safe to call on other threads as long as NetworkLobby is safe to be read on other threads.</para>
    /// </summary>
    /// <param name="lobby"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public static bool TryReadFromLobby(NetworkLobby lobby, out LobbyMetadata metadata)
    {
        metadata = ReadFromLobby(lobby);

        if (!metadata.HasLobbyOpen)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Reads metadata from a NetworkLobby.
    /// <para>This should be safe to call on other threads as long as NetworkLobby is safe to be read on other threads.</para>
    /// </summary>
    /// <param name="lobby"></param>
    /// <returns></returns>
    public static LobbyMetadata ReadFromLobby(NetworkLobby lobby)
    {
        var info = new LobbyMetadata()
        {
            ServerID = lobby.GetServerID(),
            HasLobbyOpen = lobby.GetMetadata(LobbyKeys.HasLobbyOpenKey) == bool.TrueString,
            LobbyCode = lobby.GetMetadata(LobbyKeys.LobbyCodeKey),
            Game = lobby.GetMetadata(LobbyKeys.GameKey),
            Full = lobby.GetMetadata(LobbyKeys.FullKey) == bool.TrueString,
        };

        if (lobby.TryGetMetadata(LobbyKeys.PrivacyKey, out var rawPrivacy) && int.TryParse(rawPrivacy, out var privacyInt))
        {
            info.Privacy = (ServerPrivacy)privacyInt;
        }

        if (lobby.TryGetMetadata(LobbyKeys.VersionMajorKey, out var rawVersionMajor) && int.TryParse(rawVersionMajor, out var versionMajorInt))
        {
            info.VersionMajor = versionMajorInt;
        }

        if (lobby.TryGetMetadata(LobbyKeys.VersionMinorKey, out var rawVersionMinor) && int.TryParse(rawVersionMinor, out var versionMinorInt))
        {
            info.VersionMinor = versionMinorInt;
        }

        // Check if we can get the main lobby info
        if (lobby.TryGetMetadata(nameof(LobbyInfo), out var json))
        {
            try
            {
                info.LobbyInfo = JsonSerializer.Deserialize<LobbyInfo>(json);
            }
            catch
            {
                info.HasLobbyOpen = false;
            }
        }
        else
        {
            info.HasLobbyOpen = false;
        }

        return info;
    }

    /// <summary>
    /// Writes the metadata to a NetworkLobby.
    /// <para>This should be safe to call on other threads as long as NetworkLobby is safe to be written to on other threads.</para>
    /// </summary>
    /// <param name="lobby"></param>
    public readonly void WriteToLobby(NetworkLobby lobby)
    {
        lobby.SetMetadata(LobbyKeys.IdentifierKey, bool.TrueString);
        lobby.SetMetadata(LobbyKeys.HasLobbyOpenKey, HasLobbyOpen.ToString());
        lobby.SetMetadata(LobbyKeys.LobbyCodeKey, LobbyCode?.ToUpper());
        lobby.SetMetadata(LobbyKeys.PrivacyKey, ((int)Privacy).ToString());
        lobby.SetMetadata(LobbyKeys.FullKey, Full.ToString());
        lobby.SetMetadata(LobbyKeys.VersionMajorKey, VersionMajor.ToString());
        lobby.SetMetadata(LobbyKeys.VersionMinorKey, VersionMinor.ToString());
        lobby.SetMetadata(LobbyKeys.GameKey, Game);
        lobby.SetMetadata(nameof(LobbyInfo), JsonSerializer.Serialize(LobbyInfo));
    }

    // TODO: Move somewhere else?
    // Should not be called when reading lobbies definitely not thread safe
    public readonly bool CheckClientHasLevel()
    {
        return AssetWarehouseSearcher.HasCrate<LevelCrate>(new(LobbyInfo.LevelBarcode));
    }

    public readonly Action CreateJoinDelegate()
    {
        var serverID = ServerID;

        return () =>
        {
            NetworkLayerManager.Layer?.ConnectToServer(serverID);
        };
    }
}
