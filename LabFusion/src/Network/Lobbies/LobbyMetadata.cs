using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Marrow;
using LabFusion.Utilities;

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
    };

    public LobbyInfo LobbyInfo { get; set; }

    public ServerID ServerID { get; set; }

    public bool HasLobbyOpen { get; set; }

    public string LobbyCode { get; set; }

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
    /// Attempts to read metadata from a NetworkLobby, or returns false if the metadata is invalid. This will also catch and log any errors from reading the metadata.
    /// <para>This should be safe to call on other threads as long as NetworkLobby is safe to be read on other threads.</para>
    /// </summary>
    /// <param name="lobby"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public static bool TryReadFromLobby(NetworkLobby lobby, out LobbyMetadata metadata)
    {
        try
        {
            metadata = ReadFromLobby(lobby);

            if (!metadata.HasLobbyOpen)
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("reading metadata from lobby", ex);

            metadata = Empty;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Reads metadata from a NetworkLobby. This will not catch errors from reading the metadata.
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
        };

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
