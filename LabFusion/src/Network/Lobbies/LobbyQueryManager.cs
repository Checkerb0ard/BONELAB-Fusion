using LabFusion.Support;

namespace LabFusion.Network;

public static class LobbyQueryManager
{
    public static ILobbyQuery WithPersistentFilters(this ILobbyQuery query)
    {
        return query
            .WithEqual(LobbyKeys.IdentifierKey, bool.TrueString)
            .WithEqual(LobbyKeys.HasLobbyOpenKey, bool.TrueString)
            .WithEqual(LobbyKeys.GameKey, GameInfo.GameName);
    }

    public static ILobbyQuery WithCode(this ILobbyQuery query, string code)
    {
        return query.WithEqual(LobbyKeys.LobbyCodeKey, code.ToUpper());
    }

    public static ILobbyQuery WithMatchmakerFilters(this ILobbyQuery query, MatchmakerFilters filters)
    {
        if (filters.FilterFull)
        {
            query = query.WithFullHidden();
        }

        if (filters.FilterMismatchingVersions)
        {
            query = query.WithMatchingVersions();
        }

        return query;
    }

    public static ILobbyQuery WithFullHidden(this ILobbyQuery query)
    {
        return query.WithEqual(LobbyKeys.FullKey, bool.FalseString);
    }

    public static ILobbyQuery WithMatchingVersions(this ILobbyQuery query)
    {
        var version = FusionMod.Version;
        var versionMajor = version.Major;
        var versionMinor = version.Minor;

        return query
            .WithComparison(LobbyKeys.VersionMajorKey, versionMajor, LobbyQueryComparison.Equal)
            .WithComparison(LobbyKeys.VersionMinorKey, versionMinor, LobbyQueryComparison.Equal);
    }

    public static ILobbyQuery WithPrivateHidden(this ILobbyQuery query)
    {
        return query
            .WithComparison(LobbyKeys.PrivacyKey, (int)ServerPrivacy.PRIVATE, LobbyQueryComparison.NotEqual)
            .WithComparison(LobbyKeys.PrivacyKey, (int)ServerPrivacy.LOCKED, LobbyQueryComparison.NotEqual);
    }
}
