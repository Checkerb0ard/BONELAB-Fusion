using Il2CppInterop.Runtime.Attributes;

using LabFusion.Network;

using MelonLoader;

using UnityEngine;

namespace LabFusion.MonoBehaviours;

[RegisterTypeInIl2Cpp]
public class MirrorIdentifier : MonoBehaviour
{
    public MirrorIdentifier(IntPtr intPtr) : base(intPtr) { }

    [HideFromIl2Cpp]
    public ClientSmallID ID { get; set; }

    public void Awake()
    {
        NetworkManager.ServerLost += OnServerLost;
    }

    public void OnDestroy()
    {
        NetworkManager.ServerLost -= OnServerLost;
    }

    private void OnServerLost() => Destroy(this);
}
