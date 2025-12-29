using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CheckSize : MonoBehaviour
{
    [Header("Camera Size")]
    public float size16by9 = 4.6f;   // 16:9
    public float size18by9 = 5.0f;   // 18:9
    public float sizeLong = 5.6f;   // 20:9+

    [Header("Disk")]
    public Transform diskTransform;
    public Vector3 diskPos16by9 = new Vector3(3.55f, 2.87f, 0f);
    public Vector3 diskPosLong = new Vector3(1.15f, 3.6f, 0f);

    public Transform trayManager;
    public GameObject button_doc;
    public GameObject target;
    public GameObject timer;

    private Camera cam;
    private float lastAspect;

    void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateCamera();
    }

    void Update()
    {
        // aspect đổi → layout đổi (Editor + WebGL + Luna đều OK)
        if (Mathf.Abs(cam.aspect - lastAspect) > 0.01f)
        {
            UpdateCamera();
        }
    }

    void UpdateCamera()
    {
        lastAspect = cam.aspect;

        bool isPortrait = cam.aspect < 1f;

        if (!isPortrait)
        {
            cam.orthographicSize = size16by9;
            trayManager.transform.position = new Vector3(0, -0.75f, 0);
            target.GetComponent<RectTransform>().anchoredPosition = new Vector2(173, -127);
            timer.GetComponent<RectTransform>().anchoredPosition = new Vector2(-281, -127);
            diskTransform.transform.position = new Vector3(0, 3.3f, 0);
            //if (diskTransform)
            //diskTransform.position = diskPos16by9;

            return;
        }

        float ratio = 1f / cam.aspect;

        if (ratio < 1.8f)
        {
            cam.orthographicSize = size16by9;
            trayManager.transform.position = new Vector3(0, -0.25f, 0);
            button_doc.GetComponent<RectTransform>().transform.localPosition = new Vector3(113, -1325f, 0);
            target.GetComponent<RectTransform>().anchoredPosition = new Vector2(111, -87);
            timer.GetComponent<RectTransform>().anchoredPosition = new Vector2(-281, -87);
            diskTransform.transform.position = new Vector3(0.15f, 3.48f, 0);
            //if (diskTransform)
            //diskTransform.position = diskPos16by9;
        }
        else
        {
            cam.orthographicSize = sizeLong;
            trayManager.transform.position = new Vector3(0, 0, 0);
            button_doc.GetComponent<RectTransform>().anchoredPosition = new Vector2(113, -1583);
            diskTransform.transform.position = new Vector3(0.15f, 4f, 0);
            target.GetComponent<RectTransform>().anchoredPosition = new Vector2(111, -204);
            timer.GetComponent<RectTransform>().anchoredPosition = new Vector2(-281, -204);
            //if (diskTransform)
            //diskTransform.position = diskPosLong;
        }
    }

}
