using LabFusion.Network;

namespace LabFusion.SDK.Lobbies;

public delegate bool GenericLobbyDelegate(LobbyMetadata metadata);

public class GenericLobbyFilter : ILobbyFilter
{
    public string Title { get; set; }

    public GenericLobbyDelegate OnFilter { get; set; }

    public bool Active { get; set; } = false;

    public GenericLobbyFilter(string title, GenericLobbyDelegate onFilter)
    {
        Title = title;
        OnFilter = onFilter;
    }

    public bool FilterLobby(LobbyMetadata metadata) => OnFilter(metadata);

    public string GetTitle() => Title;

    public bool IsActive() => Active;

    public void SetActive(bool active) => Active = active;
}
