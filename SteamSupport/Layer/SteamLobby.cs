using Steamworks.Data;

using LabFusion.Network;

namespace MarrowFusion.Steam;

public class SteamLobby : NetworkLobby
{
    private Lobby _lobby;

    public SteamLobby(Lobby lobby)
    {
        _lobby = lobby;
    }

    protected override ServerID OnGetServerID() => new(_lobby.Owner.Id);

    protected override void OnSetMetadata(string key, string value)
    {
        value ??= string.Empty;

        _lobby.SetData(key, value);
    }

    protected override string OnGetMetadata(string key) => _lobby.GetData(key);
}
