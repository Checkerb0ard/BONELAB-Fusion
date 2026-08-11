using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Entities;

using MarrowFusion.Bonelab.Messages;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

namespace MarrowFusion.Bonelab.Patching;

[HarmonyPatch(typeof(PullCordDevice))]
public static class PullCordDevicePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PullCordDevice.Update))]
    public static void Update(PullCordDevice __instance)
    {
        // Make sure we have a server
        if (!NetworkManager.HasServer)
        {
            return;
        }

        // If this is a networked player,
        // We need to disable the avatars inside the body log
        // This way, the net players won't accidentally change their avatar
        if (NetworkPlayerManager.HasExternalPlayer(__instance.rm))
        {
            for (var i = 0; i < __instance.avatarCrateRefs.Length; i++)
            {
                __instance.avatarCrateRefs[i].Barcode = Barcode.EmptyBarcode();
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PullCordDevice.EnableBall))]
    public static void EnableBall(PullCordDevice __instance)
    {
        if (!IsPullCordNetworked(__instance))
        {
            return;
        }

        ClientManager.RelayModule<BodyLogToggleMessage, BodyLogToggleData>(new() { IsEnabled = true, }, CommonMessageRoutes.ReliableToOtherClients);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PullCordDevice.DisableBall))]
    public static void DisableBall(PullCordDevice __instance)
    {
        if (!IsPullCordNetworked(__instance))
        {
            return;
        }

        ClientManager.RelayModule<BodyLogToggleMessage, BodyLogToggleData>(new() { IsEnabled = false, }, CommonMessageRoutes.ReliableToOtherClients);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PullCordDevice.PlayAvatarParticleEffects))]
    public static void PlayAvatarParticleEffects(PullCordDevice __instance)
    {
        if (!IsPullCordNetworked(__instance))
        {
            return;
        }

        ClientManager.RelayModule<BodyLogEffectMessage, EmptyData>(new(), CommonMessageRoutes.UnreliableToOtherClients);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PullCordDevice.OnBallGripDetached))]
    public static void OnBallGripDetached(PullCordDevice __instance, Hand hand)
    {
        if (!IsPullCordExternal(__instance))
        {
            return;
        }

        // External rigs shouldn't be able to insert their body log
        var avatarPanelView = __instance.apv;

        if (avatarPanelView != null && avatarPanelView.bodyLog == __instance)
        {
            avatarPanelView.bodyLog = null;
        }

        __instance.apv = null;
        __instance.isHandleInReceiver = false;
        __instance.isBallInReceiver = false;
    }

    private static bool IsPullCordNetworked(PullCordDevice pullCordDevice)
    {
        return ClientManager.IsClientConnected && pullCordDevice.rm.IsLocalPlayer();
    }

    private static bool IsPullCordExternal(PullCordDevice pullCordDevice)
    {
        if (!NetworkManager.HasServer)
        {
            return false;
        }

        var rigManager = pullCordDevice.rm;

        if (NetworkRig.Cache.TryGet(rigManager, out var networkRig) && !networkRig.NetworkEntity.IsOwner)
        {
            return true;
        }

        return false;
    }
}