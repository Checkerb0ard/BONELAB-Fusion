namespace LabFusion.Network;

public interface ILobbyQuery
{
    ILobbyQuery WithEqual(string key, string value);

    ILobbyQuery WithComparison(string key, int value, LobbyQueryComparison comparison);
}
