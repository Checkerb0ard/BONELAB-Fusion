using LabFusion.Network;

namespace LabFusion.SDK.Achievements;

public class WarmWelcome : Achievement
{
    public override string Title => "Warm Welcome";

    public override string Description => "Join a server.";

    public override int BitReward => 50;

    protected override void OnRegister()
    {
        ClientManager.ClientConnected += OnClientConnected;
    }

    protected override void OnUnregister()
    {
        ClientManager.ClientConnected -= OnClientConnected;
    }

    private void OnClientConnected()
    {
        // Don't treat joining our own server as joining a server!
        if (ServerManager.IsServerRunning)
        {
            return;
        }

        IncrementTask();
    }
}
