using Steamworks.Data;
using Steamworks;

using LabFusion.Network;

namespace MarrowFusion.Steam;

/// <summary>
/// A lobby wrapper for Steamworks' matchmaking system.
/// </summary>
public class SteamLobby : NetworkLobby
{
    private const string ServerIDKey = "SteamLobbyID";

    private Lobby _lobby;

    public SteamLobby(Lobby lobby)
    {
        _lobby = lobby;
    }

    /// <summary>
    /// Steamworks only provides the lobby owner's ID after joining a lobby.
    /// <para>Since lobbies are only used for matchmaking, this would be a waste, so the owner ID must be stored as metadata.</para>
    /// </summary>
    /// <param name="steamID"></param>
    public void SetServerID(SteamId steamID)
    {
        _lobby.SetData(ServerIDKey, steamID.Value.ToString());
    }

    protected override ServerID OnGetServerID() => new(ulong.Parse(_lobby.GetData(ServerIDKey)));

    protected override void OnSetMetadata(string key, string value)
    {
        value ??= string.Empty;

        _lobby.SetData(key, value);
    }

    protected override string OnGetMetadata(string key) => _lobby.GetData(key);
}
