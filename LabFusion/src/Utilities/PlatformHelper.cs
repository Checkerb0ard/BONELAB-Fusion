using Il2CppSLZ.Marrow.Data;

using MelonLoader;

namespace LabFusion.Utilities;

/// <summary>
/// Helper for determining information about the current platform that the game is running on.
/// </summary>
public static class PlatformHelper
{
    /// <summary>
    /// The platforms the game can be on.
    /// </summary>
    public enum Platform
    {
        /// <summary>
        /// No valid platform was found.
        /// </summary>
        None,

        /// <summary>
        /// The steam store.
        /// </summary>
        Steam,

        /// <summary>
        /// The meta store on PCVR.
        /// </summary>
        MetaPCVR,

        /// <summary>
        /// The meta store on standalone Quest.
        /// </summary>
        MetaQuest
    }

    /// <summary>
    /// Returns true if the game is running on android.
    /// </summary>
    public static bool IsAndroid => _isAndroidCached;

    private static readonly bool _isAndroidCached = MelonUtils.CurrentPlatform == (MelonPlatformAttribute.CompatiblePlatforms)3;

    /// <summary>
    /// Gets the platform the game is currently running on.
    /// </summary>
    /// <returns></returns>
    public static Platform GetPlatform()
    {
        var localData = PlatformSelectionData.LocalData;

        if (localData == null)
        {
            return Platform.None;
        }

        var vrPlatform = localData.platform;

        return vrPlatform switch
        {
            VRPlatform.Steam => Platform.Steam,
            VRPlatform.OculusHome => Platform.MetaPCVR,
            VRPlatform.OculusQuest => Platform.MetaQuest,
            _ => Platform.None,
        };
    }
}