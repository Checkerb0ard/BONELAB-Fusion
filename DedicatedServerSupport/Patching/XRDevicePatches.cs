using HarmonyLib;

using Il2CppSLZ.Marrow.Input;

namespace MarrowFusion.DedicatedServer;

[HarmonyPatch(typeof(XRDevice))]
public static class XRDevicePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(XRDevice.IsTracking))]
    [HarmonyPatch(MethodType.Getter)]
    public static bool IsTrackingPrefix(ref bool __result)
    {
        __result = true;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(XRDevice.IsConnected))]
    [HarmonyPatch(MethodType.Getter)]
    public static bool IsConnectedPrefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}
