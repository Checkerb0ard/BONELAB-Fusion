using LabFusion.Network;

namespace LabFusion.SDK.Lobbies;

public interface IOLDLobbyFilter
{
    string GetTitle();

    bool IsActive();

    void SetActive(bool active);

    bool FilterLobby(LobbyMetadata metadata);
}
