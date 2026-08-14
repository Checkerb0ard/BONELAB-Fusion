using Il2CppInterop.Runtime.Injection;

using System.Reflection;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarrowFusion.DedicatedServer;

public static class DedicatedServerManager
{
    public static GameObject ServerGameObject { get; private set; } = null;

    public static ServerBehaviour ServerBehaviour { get; private set; } = null;

    public static Assembly Assembly { get; private set; } = null;

    public static HarmonyLib.Harmony HarmonyInstance { get; private set; } = null;

    private static bool _firstSceneLoaded = false;

    public static void Initialize()
    {
        Assembly = Assembly.GetExecutingAssembly();

        RegisterMonoBehaviors();
        HookEvents();
        PatchAll();
        XRDisabler.Initialize();
    }

    public static void Deinitialize()
    {

    }

    private static void RegisterMonoBehaviors()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ServerBehaviour>();
        ClassInjector.RegisterTypeInIl2Cpp<ServerCamera>();
    }

    private static void HookEvents()
    {
        var sceneLoaded = OnSceneLoaded;
        SceneManager.sceneLoaded += sceneLoaded;
    }

    private static void PatchAll()
    {
        HarmonyInstance = new(Assembly.FullName);
        HarmonyInstance.PatchAll(Assembly);
    }

    private static void CreateServerGameObject()
    {
        ServerGameObject = new("ServerBehaviour")
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset
        };

        GameObject.DontDestroyOnLoad(ServerGameObject);

        ServerBehaviour = ServerGameObject.AddComponent<ServerBehaviour>();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_firstSceneLoaded)
        {
            return;
        }

        _firstSceneLoaded = true;

        CreateServerGameObject();
    }
}
