using LabFusion;
using LabFusion.Network;
using LabFusion.SDK.Modules;

using System.Reflection;

using Module = LabFusion.SDK.Modules.Module;

namespace MarrowFusion.Epic;

public class ModuleModule : Module
{
    public static ModuleLogger Logger { get; private set; } = null;

    public static Assembly ModuleAssembly { get; private set; } = null;

    public override string Name => "Epic";
    public override string Author => "Checkerboard";
    public override Version Version => FusionMod.Version;
    public override ConsoleColor Color => ConsoleColor.Gray;

    protected override void OnModuleRegistered()
    {
        Logger = LoggerInstance;

        ModuleAssembly = Assembly.GetExecutingAssembly();

        NetworkLayerManager.LoadLayers(ModuleAssembly);
    }
}
