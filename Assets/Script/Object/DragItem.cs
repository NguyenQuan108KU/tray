using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragItem : MonoBehaviour
{
    private static DragItem currentDrag;

    private Vector3 offset;
    private Vector3 startPos;
    private Transform startParent;
    private Camera cam;
    private Tween tween;
    private SpriteRenderer sr;
    public Vector3 startScale;
    private Slot startSlot;
    public bool isLocked = false;
    public bool isCheckUI;
    

    [Header("Outline")]
    public float focusOutlineSize = 1f;
    public float outlineTweenTime = 0.12f;
    private GameObject outline;
    public float diskX;


    private void Awake()
    {
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
        outline = transform.GetChild(0).gameObject;
        outline.SetActive(false); // mặc định tắt
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isCheckUI)
            {
                return;
            }
            if (!CountdownTimer.instance.hasStarted)
            {
                CountdownTimer.instance.hasStarted = true;
                CountdownTimer.instance.StartCountdown();
            }
            TryPick();
        }

        if (Input.GetMouseButton(0) && currentDrag == this)
        {
            //if (isCheckUI) return;
            Drag();
        }

        if (Input.GetMouseButtonUp(0) && currentDrag == this)
        {
            Drop();
        }
    }

    void TryPick()
    {
        if (!IsInSlot()) return;
        if (GameManager.Instance.tutGame.activeSelf)
        {
            GameManager.Instance.tutGame.SetActive(false);
        }
        if (isLocked || GameManager.Instance.finishGame) return;
        if (isLocked || GameManager.Instance.finishGame) return;

        TrayManager.instance.OnUserBeginInteract(); // ✅ ĐÚNG
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject)
            {
                currentDrag = this;
                AudioManager.Instance.PlaySFX(AudioManager.Instance.drag);
                tween?.Kill();
                startPos = transform.position;
                startParent = transform.parent;
                startSlot = startParent ? startParent.GetComponent<Slot>() : null;
                offset = transform.position - (Vector3)mouseWorld;
                sr.sortingOrder = 10;
                outline.GetComponent<SpriteRenderer>().sortingOrder = 9;
                ShowOutline(true);
                return;
            }
        }
    }
    void Drag()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        transform.position = mouseWorld + offset;
    }

    void Drop()
    {
        TrayManager.instance.OnUserEndInteract();
        GameManager.Instance.clickCount++;
        sr.sortingOrder = 0;
        outline.GetComponent<SpriteRenderer>().sortingOrder = -1;
        ShowOutline(false);
        PlayDropScale();

        if (isCheckUI)
        {
            Return();
            currentDrag = null;
            return;
        }

        Slot slot = FindNearestSlot();

        if (slot != null
    && slot.CanAcceptItem()
    && (slot.IsEmpty() || slot.transform == startParent))
        {
            Snap(slot);
        }
        else
        {
            Return();
        }
        if (GameManager.Instance.clickCount >= GameManager.Instance.clicksToLog && !GameManager.Instance.isClick)
        {
            GameManager.Instance.isClick = true;
            Luna.Unity.Playable.InstallFullGame();
        }
        currentDrag = null;
    }
    void Snap(Slot slot)
    {
        Slot oldSlot = startParent ? startParent.GetComponent<Slot>() : null;
        Tray oldTray = null;

        if (oldSlot != null)
        {
            oldTray = oldSlot.GetComponentInParent<Tray>();
            oldSlot.SetItem(null);
        }

        transform.SetParent(slot.transform);

        tween = transform
            .DOLocalMove(slot.anchor.localPosition, 0.25f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                slot.SetItem(this);

                // CHECK MATCH TRAY MỚI
                Tray newTray = slot.GetComponentInParent<Tray>();
                if (newTray != null)
                    newTray.CheckMatch();

                // SAU KHI TWEEN XONG MỚI CHECK TRAY CŨ
                if (oldTray != null)
                    oldTray.CheckAfterItemRemoved();
            });
    }
    void Return()
    {
        if (startSlot == null || startSlot.anchor == null)
            return;

        transform.SetParent(startSlot.transform);

        tween = transform.DOLocalMove(
                    startSlot.anchor.localPosition,
                    0.5f
                )
                .SetEase(Ease.OutQuad);
    }

    Slot FindNearestSlot()
    {
        Slot[] slots = FindObjectsOfType<Slot>();
        float min = 1f;
        Slot best = null;

        foreach (var s in slots)
        {
            if (s.anchor == null) continue;

            // Skip occupied slots except the one we started from
            if (!s.IsEmpty() && s.transform != startParent) continue;

            float d = Vector2.Distance(transform.position, s.anchor.position);
            if (d < min)
            {
                min = d;
                best = s;
            }
        }
        return best;
    }
    void PlayDropScale()
    {
        tween?.Kill();

        transform.localScale = startScale;

        transform.DOScale(startScale * 0.85f, 0.1f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                transform.DOScale(startScale, 0.12f)
                    .SetEase(Ease.OutBack);
            });
    }
    bool IsInSlot()
    {
        return transform.parent != null
            && transform.parent.GetComponent<Slot>() != null;
    }

    void ShowOutline(bool show)
    {
        if (outline != null)
            outline.SetActive(show);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("UIHeader"))
        {
            isCheckUI = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("UIHeader"))
        {
            isCheckUI = false;
        }
    }
}
