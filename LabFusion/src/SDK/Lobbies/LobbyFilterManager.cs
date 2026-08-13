using LabFusion.Network;
using LabFusion.Safety;

namespace LabFusion.SDK.Lobbies;

public static class OLDLobbyFilterManager
{
    private static readonly List<IOLDLobbyFilter> _lobbyFilters = new();
    public static List<IOLDLobbyFilter> LobbyFilters => _lobbyFilters;

    public static event Action<IOLDLobbyFilter> OnAddedFilter;

    public static OLDGenericLobbyFilter FriendsFilter { get; } = new("Friends Only", (l) =>
    {
        return NetworkLayerManager.Layer.IsFriend(new ClientPlatformID(l.LobbyInfo.LobbyID.ToString()));
    });

    public static void LoadBuiltInFilters()
    {
        AddLobbyFilter(FriendsFilter);
    }

    public static void AddLobbyFilter(IOLDLobbyFilter filter)
    {
        _lobbyFilters.Add(filter);

        OnAddedFilter?.Invoke(filter);
    }

    public static bool CheckOptionalFilters(LobbyMetadata metadata)
    {
        foreach (var filter in LobbyFilters)
        {
            if (filter.IsActive() && !filter.FilterLobby(metadata))
            {
                return false;
            }
        }

        return true;
    }

    public static bool CheckPersistentFilters(LobbyMetadata metadata)
    {
        var lobbyInfo = metadata.LobbyInfo;

        if (!lobbyInfo.ValidateLobby())
        {
            return false;
        }

        if (GlobalBanManager.IsBanned(lobbyInfo))
        {
            return false;
        }

        return true;
    }
}