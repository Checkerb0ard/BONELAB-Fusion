using LabFusion.Network;

using Steamworks;
using Steamworks.Data;

namespace MarrowFusion.Steam;

public struct SteamLobbyQuery : ILobbyQuery
{
    public LobbyQuery LobbyQuery { get; set; }

    public SteamLobbyQuery(LobbyQuery lobbyQuery)
    {
        LobbyQuery = lobbyQuery.FilterDistanceWorldwide();
    }

    public readonly ILobbyQuery WithEqual(string key, string value)
    {
        LobbyQuery.WithKeyValue(key, value);
        return this;
    }

    public readonly ILobbyQuery WithComparison(string key, int value, LobbyQueryComparison comparison)
    {
        LobbyQuery.AddNumericalFilter(key, value, GetSteamworksLobbyComparison(comparison));
        return this;
    }

    private static LobbyComparison GetSteamworksLobbyComparison(LobbyQueryComparison comparison)
    {
        return comparison switch
        {
            LobbyQueryComparison.LessThanOrEqualTo => LobbyComparison.EqualToOrLessThan,
            LobbyQueryComparison.LessThan => LobbyComparison.LessThan,
            LobbyQueryComparison.GreaterThan => LobbyComparison.GreaterThan,
            LobbyQueryComparison.GreaterThanOrEqualTo => LobbyComparison.EqualToOrGreaterThan,
            LobbyQueryComparison.NotEqual => LobbyComparison.NotEqual,
            _ => LobbyComparison.Equal,
        };
    }
}
