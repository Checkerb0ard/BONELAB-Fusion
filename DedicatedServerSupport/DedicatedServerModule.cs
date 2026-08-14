using LabFusion;
using LabFusion.SDK.Modules;

namespace MarrowFusion.DedicatedServer;

public sealed class DedicatedServerModule : Module
{
    public static DedicatedServerModule Instance { get; private set; } = null;

    public static ModuleLogger Logger { get; private set; } = null;

    public override string Name => "Dedicated Server";
    public override string Author => FusionMod.ModAuthor;
    public override Version Version => FusionMod.Version;
    public override ConsoleColor Color => ConsoleColor.Red;

    protected override void OnModuleRegistered()
    {
        Instance = this;
        Logger = LoggerInstance;

        Logger.Log($"Launching as a dedicated server!");

        Logger.Warn("Dedicated servers are still in development! They may not be fully functional!");

        DedicatedServerManager.Initialize();
    }

    protected override void OnModuleUnregistered()
    {
        DedicatedServerManager.Deinitialize();
    }
}
