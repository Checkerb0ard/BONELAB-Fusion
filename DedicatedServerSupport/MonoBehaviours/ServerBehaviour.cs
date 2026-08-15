using Il2CppInterop.Runtime.Attributes;

using UnityEngine;

namespace MarrowFusion.DedicatedServer;

public class ServerBehaviour : MonoBehaviour
{
    public ServerBehaviour(IntPtr intPtr) : base(intPtr) { }

    [HideFromIl2Cpp]
    public ServerCamera Camera { get; set; }

    private void Awake()
    {
        Camera = gameObject.AddComponent<ServerCamera>();
    }

    private void OnGUI()
    {
        DedicatedServerGUI.OnGUI();
    }
}
