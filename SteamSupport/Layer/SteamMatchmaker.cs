using LabFusion.Network;

using Steamworks;

namespace MarrowFusion.Steam;

public sealed class SteamMatchmaker : Matchmaker
{
    public override ILobbyQuery CreateQuery() => new SteamLobbyQuery(SteamMatchmaking.LobbyList);

    protected override async Task<MatchmakerResult> TrySearchLobbiesAsync(ILobbyQuery query, CancellationToken cancellationToken)
    {
        if (query is not SteamLobbyQuery steamLobbyQuery)
        {
            throw new ArgumentException(null, nameof(query));
        }

        var lobbies = await steamLobbyQuery.LobbyQuery.RequestAsync();

        if (lobbies == null || lobbies.Length <= 0)
        {
            return MatchmakerResult.Empty;
        }

        var results = new LobbyMetadata[lobbies.Length];
        int resultCount = 0;

        foreach (var lobby in lobbies)
        {
            try
            {
                if (lobby.Owner.IsMe)
                {
                    continue;
                }

                using var networkLobby = new SteamLobby(lobby);

                if (!LobbyMetadata.TryReadFromLobby(networkLobby, out var metadata))
                {
                    continue;
                }

                results[resultCount++] = metadata;
            }
            catch (Exception ex)
            {
                SteamModule.Logger.LogException("parsing lobby result", ex);
            }
        }

        var resultSegment = new ArraySegment<LobbyMetadata>(results, 0, resultCount);

        return new MatchmakerResult(resultSegment);
    }
}