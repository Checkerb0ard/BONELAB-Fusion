using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Scene;

namespace LabFusion.SDK.Achievements;

public class AroundTheWorld : Achievement
{
    public override string Title => "Around The World";

    public override string Description => "Play 10 different levels in one multiplayer session.";

    public override int BitReward => 700;

    private readonly List<string> _levels = new();

    protected override void OnRegister()
    {
        MultiplayerHooking.OnMainSceneInitialized += OnMainSceneInitialized;
        ClientManager.ClientDisconnected += OnClientDisconnected;
    }

    protected override void OnUnregister()
    {
        MultiplayerHooking.OnMainSceneInitialized -= OnMainSceneInitialized;
        ClientManager.ClientDisconnected -= OnClientDisconnected;
    }

    private void OnMainSceneInitialized()
    {
        if (!ClientManager.IsClientConnected)
        {
            return;
        }

        if (!PlayerIDManager.HasOtherPlayers)
        {
            return;
        }

        if (_levels.Contains(FusionSceneManager.Barcode))
        {
            return;
        }

        _levels.Add(FusionSceneManager.Barcode);

        // If we have over 10 unique levels, reward the achievement
        if (_levels.Count >= 10)
        {
            IncrementTask();
            _levels.Clear();
        }
    }

    private void OnClientDisconnected(string reason)
    {
        // Clear our visited levels
        _levels.Clear();
    }
}
