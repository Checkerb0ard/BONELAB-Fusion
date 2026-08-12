using LabFusion.Utilities;

namespace LabFusion.Network;

/// <summary>
/// The manager for searching for publicly available lobbies.
/// </summary>
public abstract class Matchmaker
{
    /// <summary>
    /// The result from a matchmaker search.
    /// </summary>
    public struct MatchmakerResult
    {
        /// <summary>
        /// Indicates that the matchmaker found no lobbies.
        /// </summary>
        public static readonly MatchmakerResult Empty = new(Array.Empty<LobbyMetadata>());

        /// <summary>
        /// Indicates that the matchmaker failed to search for lobbies.
        /// </summary>
        public static readonly MatchmakerResult Failed = new()
        {
            IsSuccess = false,
            Lobbies = Array.Empty<LobbyMetadata>()
        }; 

        /// <summary>
        /// Returns true if the matchmaker search was successful and there was at least one lobby returned.
        /// </summary>
        public readonly bool HasLobbies => IsSuccess && Lobbies != null && Lobbies.Count > 0;

        /// <summary>
        /// Returns true if the matchmaker search was successful.
        /// <para>This does not indicate whether any lobbies were returned. The matchmaker can successfully connect to the remote API but still return no lobbies.</para>
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The metadata for the returned lobbies. Can be null.
        /// </summary>
        public ArraySegment<LobbyMetadata> Lobbies { get; set; }

        /// <summary>
        /// Creates a new matchmaker search result given an array of lobby metadata.
        /// </summary>
        /// <param name="lobbies"></param>
        public MatchmakerResult(ArraySegment<LobbyMetadata> lobbies)
        {
            IsSuccess = true;
            Lobbies = lobbies;
        }
    }

    /// <summary>
    /// Creates a lobby query that can be used to filter a search.
    /// </summary>
    /// <returns></returns>
    public abstract ILobbyQuery CreateQuery();

    /// <summary>
    /// Searches for lobbies given a query and returns the result in a callback.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="callback"></param>
    public void SearchLobbies(ILobbyQuery query, Action<MatchmakerResult> callback) => SearchLobbies(query, callback, CancellationToken.None);

    /// <summary>
    /// Searches for lobbies given a query and returns the result in a callback.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="callback"></param>
    /// <param name="cancellationToken"></param>
    public void SearchLobbies(ILobbyQuery query, Action<MatchmakerResult> callback, CancellationToken cancellationToken)
    {
        Task.Run(Search, CancellationToken.None);

        async void Search()
        {
            var result = await SearchLobbiesAsync(query, cancellationToken);

            ThreadHelper.RunOnMainThread(() => { OnSearchCompleted(result); });
        }

        void OnSearchCompleted(MatchmakerResult result)
        {
            callback?.Invoke(result);
        }
    }

    /// <summary>
    /// Searches for lobbies asynchronously given a query.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task<MatchmakerResult> SearchLobbiesAsync(ILobbyQuery query) => await SearchLobbiesAsync(query, CancellationToken.None);

    /// <summary>
    /// Searches for lobbies asynchronously given a query.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<MatchmakerResult> SearchLobbiesAsync(ILobbyQuery query, CancellationToken cancellationToken)
    {
        MatchmakerResult result;

        try
        {
            result = await TrySearchLobbiesAsync(query, cancellationToken);
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("searching for lobbies", ex);

            result = MatchmakerResult.Failed;
        }

        return result;
    }

    /// <summary>
    /// Attempts to search for lobbies given a query.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<MatchmakerResult> TrySearchLobbiesAsync(ILobbyQuery query, CancellationToken cancellationToken);
}
