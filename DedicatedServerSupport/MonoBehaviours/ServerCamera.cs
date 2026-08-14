using UnityEngine;

namespace MarrowFusion.DedicatedServer;

public class ServerCamera : MonoBehaviour
{
    public ServerCamera(IntPtr intPtr) : base(intPtr) { }

    public Camera Camera { get; set; } = null;

    public AudioListener AudioListener { get; set; } = null;

    private void Awake()
    {
        Camera = gameObject.AddComponent<Camera>();
        Camera.depth = 100000f;
        Camera.clearFlags = CameraClearFlags.SolidColor;
        Camera.backgroundColor = Color.black;
        Camera.cullingMask = 0;

        AudioListener = gameObject.AddComponent<AudioListener>();
        AudioListener.volume = 0f;
    }
}
