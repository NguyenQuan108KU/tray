using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAutoSize : MonoBehaviour
{
    [Header("Camera Size")]
    public float size16by9 = 4.5f;
    public float size18by9 = 5.0f;
    public float sizeLong  = 5.6f;

    public float portraitSize;
    public float landscapeSize = 4.5f;

    [Header("Disk")]
    public Transform diskTransform;
    public Vector3 diskPos16by9 = new Vector3(3.55f, 2.87f, 0f);
    public Vector3 diskPosLong  = new Vector3(1.15f, 3.6f, 0f);

    private Camera cam;

    private ScreenOrientation lastOrientation;
    private int lastWidth;
    private int lastHeight;
    private bool lastIsPortrait;
    private float aspect;

    void Awake()
    {
        cam = GetComponent<Camera>();
        ApplySize();
    }
    void ApplySize()
    {
        lastOrientation = Screen.orientation;
        lastWidth  = Screen.width;
        lastHeight = Screen.height;

        bool isPortrait = Screen.height > Screen.width;
        aspect = (float)Screen.height / Screen.width;
        // ================= DỌC =================
        if (aspect < 1.8f)
        {
            cam.orthographicSize = size16by9;
            if (diskTransform) diskTransform.position = diskPos16by9;
        }
        else if (aspect < 2.0f)
        {
            cam.orthographicSize = size18by9;
        }
        else
        {
            cam.orthographicSize = sizeLong;
            if (diskTransform) diskTransform.position = diskPosLong;
        }
    }
}
