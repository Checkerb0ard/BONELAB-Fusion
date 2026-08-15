using HarmonyLib;

using Il2CppSLZ.Marrow.Input;

namespace MarrowFusion.DedicatedServer;

[HarmonyPatch(typeof(XRApi._InitializeXRLoader_d__60))]
public static class XRApiInitializeXRLoaderPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(XRApi._InitializeXRLoader_d__60.MoveNext))]
    public static bool MoveNextPrefix()
    {
        return false;
    }
}
