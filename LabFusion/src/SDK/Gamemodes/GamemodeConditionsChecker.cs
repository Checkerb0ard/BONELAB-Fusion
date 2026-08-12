using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.SDK.Gamemodes;

public static class GamemodeConditionsChecker
{
    internal static void OnInitializeMelon()
    {
        GamemodeManager.GamemodeChanged += OnGamemodeChanged;

        PlayerIDManager.PlayerJoined += OnPlayerCountChanged;
        PlayerIDManager.PlayerLeft += OnPlayerCountChanged;

        MultiplayerHooking.OnMainSceneInitialized += AutoCheckConditions;
    }

    private static void OnGamemodeChanged(Gamemode gamemode)
    {
        AutoCheckConditions();
    }

    private static void OnPlayerCountChanged(PlayerID playerID)
    {
        AutoCheckConditions();
    }

    private static void AutoCheckConditions()
    {
        if (GamemodeManager.ActiveGamemode == null)
        {
            return;
        }

        if (!GamemodeManager.ActiveGamemode.ManualReady)
        {
            GamemodeManager.ValidateReadyConditions();
        }
    }
}
