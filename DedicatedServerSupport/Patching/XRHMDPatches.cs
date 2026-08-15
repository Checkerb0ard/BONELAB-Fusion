using Il2CppSLZ.Marrow.Input;

using HarmonyLib;

namespace MarrowFusion.DedicatedServer;

[HarmonyPatch(typeof(XRHMD))]
public static class XRHMDPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(XRHMD.IsConnected))]
    [HarmonyPatch(MethodType.Getter)]
    public static bool IsConnectedPrefix(ref bool __result)
    {
        __result = true;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(XRHMD.IsUserPresent))]
    [HarmonyPatch(MethodType.Getter)]
    public static bool IsUserPresentPrefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}