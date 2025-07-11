﻿using LabFusion.Network;
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
        MultiplayerHooking.OnDisconnected += OnDisconnect;
    }

    protected override void OnUnregister()
    {
        MultiplayerHooking.OnMainSceneInitialized -= OnMainSceneInitialized;
        MultiplayerHooking.OnDisconnected -= OnDisconnect;
    }

    private void OnMainSceneInitialized()
    {
        // Make sure we have a server and this level hasn't already been visited
        if (NetworkInfo.HasServer && PlayerIDManager.HasOtherPlayers && !_levels.Contains(FusionSceneManager.Barcode))
        {
            _levels.Add(FusionSceneManager.Barcode);

            // If we have over 10 unique levels, reward the achievement
            if (_levels.Count >= 10)
            {
                IncrementTask();
                _levels.Clear();
            }
        }
    }

    private void OnDisconnect()
    {
        // Clear our visited levels
        _levels.Clear();
    }
}
