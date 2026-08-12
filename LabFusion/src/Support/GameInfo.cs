using UnityEngine;

namespace LabFusion.Support;

public static class GameInfo
{
    public static string GameName => _gameNameCached;

    private static readonly string _gameNameCached = Application.productName;
}
