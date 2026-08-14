using Il2CppSLZ.Marrow.Data;

using Il2CppOculus.Platform;
using Il2CppOculus.Platform.Models;

using MelonLoader;

using System.Reflection;

namespace LabFusion.Utilities;

/// <summary>
/// The platforms the game can be on.
/// </summary>
public enum GamePlatform
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
/// Helper for determining information about the current platform that the game is running on.
/// </summary>
public static class PlatformHelper
{
    /// <summary>
    /// The assembly name for the game's Facepunch.Steamworks dll.
    /// </summary>
    public const string SteamworksAssemblyName = "Il2CppFacepunch.Steamworks.Win64";

    /// <summary>
    /// The type name for the game's SteamClient.
    /// </summary>
    public const string SteamworksSteamClientName = "Il2CppSteamworks.SteamClient";

    /// <summary>
    /// The property name for the game's SteamClient.IsValid.
    /// </summary>
    public const string SteamworksSteamClientIsValidName = "IsValid";

    /// <summary>
    /// The property name for the game's SteamClient.Name.
    /// </summary>
    public const string SteamworksSteamClientNameName = "Name";

    /// <summary>
    /// The method name for the game's SteamClient.Init.
    /// </summary>
    public const string SteamworksSteamClientInitName = "Init";

    /// <summary>
    /// The method name for the game's SteamClient.Shutdown.
    /// </summary>
    public const string SteamworksSteamClientShutdownName = "Shutdown";

    /// <summary>
    /// The assembly name for the game's Oculus.Platform dll.
    /// </summary>
    public const string OculusPlatformAssemblyName = "Il2CppOculus.Platform";

    /// <summary>
    /// Returns true if the game is running on android.
    /// </summary>
    public static bool IsAndroid => _isAndroidCached;

    private static readonly bool _isAndroidCached = MelonUtils.CurrentPlatform == (MelonPlatformAttribute.CompatiblePlatforms)3;

    private static string _platformUsername = null;

    /// <summary>
    /// Gets the platform the game is currently running on.
    /// </summary>
    /// <returns></returns>
    public static GamePlatform GetPlatform()
    {
        var localData = PlatformSelectionData.LocalData;

        if (localData == null)
        {
            return GamePlatform.None;
        }

        var vrPlatform = localData.platform;

        return vrPlatform switch
        {
            VRPlatform.Steam => GamePlatform.Steam,
            VRPlatform.OculusHome => GamePlatform.MetaPCVR,
            VRPlatform.OculusQuest => GamePlatform.MetaQuest,
            _ => GamePlatform.None,
        };
    }

    /// <summary>
    /// Gets the AppID for the game on its current platform.
    /// </summary>
    /// <returns></returns>
    public static string GetAppID() => PlatformSelectionData.AppId();

    /// <summary>
    /// Attempts to get the username for the account signed into the game's platform, or null if none is found.
    /// </summary>
    /// <returns></returns>
    public static async Task<string> GetPlatformUsernameAsync()
    {
        if (_platformUsername != null)
        {
            return _platformUsername;
        }

        var platform = await ThreadHelper.RunOnMainThreadAsTask(GetPlatform);

        string result = null;

        try
        {
            switch (platform)
            {
                case GamePlatform.Steam:
                    result = await GetSteamUsernameAsync();
                    break;
                case GamePlatform.MetaPCVR:
                case GamePlatform.MetaQuest:
                    result = await GetOculusUsernameAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("getting platform username", ex);
            result = null;
        }

        _platformUsername = result;
        return _platformUsername;
    }

    /// <summary>
    /// Attempts to get the game's Facepunch.Steamworks assembly.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryGetSteamworksAssembly(out Assembly result) => TryGetAssemblyByName(SteamworksAssemblyName, out result);

    /// <summary>
    /// Attempts to get the game's Oculus.Platform assembly.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryGetOculusPlatformAssembly(out Assembly result) => TryGetAssemblyByName(OculusPlatformAssemblyName, out result);

    private static async Task<string> GetSteamUsernameAsync()
    {
        if (!TryGetSteamworksAssembly(out var assembly))
        {
            return null;
        }

        var steamClientType = assembly.GetType(SteamworksSteamClientName);

        if (steamClientType == null)
        {
            return null;
        }

        var isValidProperty = steamClientType.GetProperty(SteamworksSteamClientIsValidName, BindingFlags.Public | BindingFlags.Static);

        if (isValidProperty == null)
        {
            return null;
        }

        bool isValid = await ThreadHelper.RunOnMainThreadAsTask(GetIsValid);

        if (!isValid)
        {
            var initMethod = steamClientType.GetMethod(SteamworksSteamClientInitName, BindingFlags.Public | BindingFlags.Static);

            if (initMethod == null)
            {
                return null;
            }

            var appID = await ThreadHelper.RunOnMainThreadAsTask(GetAppID);

            uint steamAppID = uint.Parse(appID);

            await ThreadHelper.RunOnMainThreadAsTask(InvokeInit);

            void InvokeInit()
            {
                initMethod.Invoke(null, new object[] { steamAppID, true });
            }
        }

        var nameProperty = steamClientType.GetProperty(SteamworksSteamClientNameName, BindingFlags.Public | BindingFlags.Static);

        if (nameProperty == null)
        {
            return null;
        }

        return await ThreadHelper.RunOnMainThreadAsTask(GetName);

        bool GetIsValid() => (bool)isValidProperty.GetValue(null);
        string GetName() => (string)nameProperty.GetValue(null);
    }

    private static async Task<string> GetOculusUsernameAsync()
    {
        if (!TryGetOculusPlatformAssembly(out _))
        {
            return null;
        }

        string username = null;
        bool requestCompleted = false;

        await ThreadHelper.RunOnMainThreadAsTask(GetLoggedInUser);

        while (!requestCompleted)
        {
            await Task.Delay(50);
        }

        return username;

        void GetLoggedInUser()
        {
            try
            {
                var onComplete = OnGetLoggedInUser;

                Users.GetLoggedInUser().OnComplete(onComplete);
            }
            catch (Exception ex)
            {
                username = null;
                requestCompleted = true;

                FusionLogger.LogException("getting Oculus username", ex);
            }
        }

        void OnGetLoggedInUser(Message<User> message)
        {
            string result = null;

            try
            {
                if (!message.IsError)
                {
                    var user = message.Data;
                    result = user.OculusID;
                }
            }
            catch (Exception ex)
            {
                result = null;

                FusionLogger.LogException("getting Oculus username", ex);
            }

            username = result;
            requestCompleted = true;
        }
    }

    private static bool TryGetAssemblyByName(string name, out Assembly result)
    {
        result = null;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            if (assembly.FullName.StartsWith(name))
            {
                result = assembly;
                return true;
            }
        }

        return false;
    }
}