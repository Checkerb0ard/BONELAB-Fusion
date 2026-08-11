using UnityEngine;

using MelonLoader;

using LabFusion.Network;

namespace LabFusion.MonoBehaviours;

[RegisterTypeInIl2Cpp]
public class DestroyOnDisconnect : MonoBehaviour
{
    public DestroyOnDisconnect(IntPtr intPtr) : base(intPtr) { }

    private void Awake()
    {
        NetworkManager.ServerLost += OnServerLost;
    }

    private void OnDestroy()
    {
        NetworkManager.ServerLost -= OnServerLost;
    }

    private void OnServerLost() => Destroy(gameObject);
}
