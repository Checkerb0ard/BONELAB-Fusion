using LabFusion.Network;

namespace LabFusion.SDK.Achievements;

public class HeadOfHouse : Achievement
{
    public override string Title => "Head Of House";

    public override string Description => "Start a server.";

    public override int BitReward => 50;

    protected override void OnRegister()
    {
        ServerManager.ServerStarted += OnServerStarted;
    }

    protected override void OnUnregister()
    {
        ServerManager.ServerStarted -= OnServerStarted;
    }

    private void OnServerStarted()
    {
        IncrementTask();
    }
}
