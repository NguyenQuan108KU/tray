using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tray : MonoBehaviour
{
    public float spacing = 0.2f;      // khoảng cách giữa item
    public float moveTime = 0.2f;
    public GameObject diskPrefab;
    public Transform diskTransform;
    public float shrinkTime = 0.1f;
    public float itemToDiskTime = 2f;
    public float attachDelay = 0.15f;   // item chậm theo disk
    public float followSmooth = 0.25f;  // độ mượt
    public Vector3 diskItemScale = new Vector3(0.03f, 0.03f, 0.03f);
    public GameObject soldOutPrefabs;
    public GameObject[] listItem;
    public int maxSlot = 3;
    public bool isCompleted = false;
    public Slot[] slots;
    public bool isClosed = false;
    [Header("Match Effect")]
    public GameObject fireEffectPrefab;
    public float fireEffectTime = 2f; // thời gian animation lửa
    public bool isInTutorialArea = false;
    bool hasHandledEmpty = false;
    public Plate diskItem;

    public List<Plate> disks = new List<Plate>();

    // disk đang dùng
    private int currentDiskIndex = 0;

    // NEW: flag while we are animating filling from disk
    public bool isRefilling = false;
    private int refillingCounter = 0;

    private void Start()
    {
        slots = GetComponentsInChildren<Slot>();

        for (int d = 0; d < disks.Count; d++)
        {
            Plate disk = disks[d];

            if (d == 0)
            {
                foreach (var item in disk.items)
                {
                    var sr = item.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        Color c = sr.color;
                        c.a = 0.6f;          
                        sr.color = c;
                    }

                    item.isLocked = false;
                }
            }
            else
            {
                disk.HideAllItems();
            }
        }
    }

    public void CheckMatch()
    {
        //  CHỈ LẤY ITEM TRÊN SLOT
        List<DragItem> items = new List<DragItem>();

        foreach (Slot slot in slots)
        {
            if (!slot.IsEmpty() && slot.currentItem != null)
            {
                items.Add(slot.currentItem);
            }
        }

        if (items.Count < 3) return;

        var groups = items.GroupBy(i =>
        {
            var sr = i.GetComponent<SpriteRenderer>();
            return sr != null && sr.sprite != null
                ? sr.sprite.name
                : i.gameObject.name;
        });

        foreach (var g in groups)
        {
            if (g.Count() >= 3)
            {
                isCompleted = true;
                GameManager.Instance.point += 1;

                var matchedItems = g.Take(3).ToList();

                foreach (var item in matchedItems)
                {
                    item.isLocked = true;
                }

                StartCoroutine(PlayFireThenMerge(matchedItems));

                ProgressBrain.instance.AddTrayMatch();
                AudioManager.Instance.PlaySFX(AudioManager.Instance.match);
                return;
            }
        }
    }
    public void NotifySlotChanged()
    {
        // còn item → reset cờ
        if (HasAnyItemInSlot())
        {
            hasHandledEmpty = false;
            return;
        }

        // tray trống thật sự
        if (!hasHandledEmpty)
        {
            hasHandledEmpty = true;
            TryHandleAfterMatch();
        }
    }
    public bool HasAnyItemInSlot()
    {
        foreach (Slot slot in GetComponentsInChildren<Slot>())
        {
            if (!slot.IsEmpty())
                return true;
        }
        return false;
    }


    IEnumerator PlayFireThenMerge(List<DragItem> items)
    {
        // 1️⃣ Spawn fire theo TRAY (LOCAL SPACE)
        GameObject fire = Instantiate(
            fireEffectPrefab,
            transform // parent luôn là Tray
        );

        fire.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        fire.transform.localRotation = Quaternion.identity;
        //fire.transform.localScale = Vector3.one;

        // 2️⃣ Animator
        Animator anim = fire.GetComponent<Animator>();

        if (anim != null)
        {
            anim.Play(0, 0, 0f);

            // lấy thời lượng clip
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            float clipLength = state.length;

            yield return new WaitForSeconds(clipLength);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }


        Destroy(fire);

        // Clear the slot references that held matched items so refill logic can detect empty slots.
        // Important: do this before MoveToCenter/SpawnDisk so RefillFromDisk sees these slots as empty.
        foreach (var it in items)
        {
            if (it == null) continue;
            Transform parent = it.transform.parent;
            if (parent == null) continue;
            Slot s = parent.GetComponent<Slot>();
            if (s != null)
            {
                s.ClearItem(); // mark the slot empty
            }
        }

        MoveToCenter(items);
    }
    void MoveToCenter(List<DragItem> items)
    {
        if (items == null || items.Count == 0) return;

        // 🔥 ÉP TẤT CẢ ITEM CHUNG 1 PARENT (TRAY)
        foreach (var it in items)
        {
            if (it == null || it.transform == null) continue;
            it.transform.SetParent(transform, true);
        }

        items = items.Where(i => i != null && i.transform != null).OrderBy(i => i.transform.position.x).ToList();

        if (items.Count < 1) return;
        for (int i = 0; i < items.Count; i++)
        {
            var sr = items[i].GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = i + 3;
        }

        // safe: ensure we have an index 1
        int centerIndex = Mathf.Clamp(1, 0, items.Count - 1);
        DragItem center = items[centerIndex];
        Vector3 centerLocalPos = center.transform.localPosition;

        float smallOffset = 0.3f;
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item == null || item.transform == null) continue;

            item.transform.DOKill();

            float offsetX = (i - centerIndex) * smallOffset;
            Vector3 targetLocalPos = centerLocalPos + new Vector3(offsetX, 0, 0);

            // protect the tween by setting its target and skipping if transform destroyed
            seq.Join(
                item.transform.DOLocalMove(targetLocalPos, moveTime)
                    .SetEase(Ease.OutBack)
                    .SetTarget(item.transform)
            );
        }

        seq.OnComplete(() =>
        {
            SpawnDisk(items);
        });
    }



    void SpawnDisk(List<DragItem> items)
    {
        // Guard input
        if (items == null) return;

        if (DiskTransform.instance == null)
        {
            Debug.LogWarning("DiskTransform.instance is null");
            return;
        }

        GameObject diskObj = Instantiate(
            diskPrefab,
            DiskTransform.instance.transform.position,
            Quaternion.identity
        );

        Disk disk = diskObj.GetComponent<Disk>();

        float jumpPower = 0.5f;
        int jumpCount = 1;
        float flyTime = 0.4f;

        // 🔥 TÍNH TÂM THẾ GIỚI CỦA NHÓM
        Vector3 groupCenter = Vector3.zero;
        int validCount = 0;
        foreach (var item in items)
        {
            if (item == null || item.transform == null) continue;
            groupCenter += item.transform.position;
            validCount++;
        }
        if (validCount == 0) return;
        groupCenter /= validCount;

        Sequence groupSeq = DOTween.Sequence();

        // capture items snapshot to avoid mutation/closure issues
        var itemsSnapshot = items.Where(i => i != null && i.transform != null).ToList();

        for (int i = 0; i < itemsSnapshot.Count; i++)
        {
            var item = itemsSnapshot[i];
            if (item == null || item.transform == null) continue;
            item.transform.DOKill();

            // 🔥 offset GIỮ NGUYÊN HÌNH DẠNG
            Vector3 offsetFromCenter = item.transform.position - groupCenter;

            Vector3 worldScale = item.transform.lossyScale;

            // tách khỏi Tray
            item.transform.SetParent(null, true);
            item.transform.localScale = worldScale;

            Vector3 targetPos = disk.transform.position + offsetFromCenter * 0.6f;

            groupSeq.Join(
                item.transform.DOJump(
                    targetPos,
                    jumpPower,
                    jumpCount,
                    flyTime
                ).SetEase(Ease.InOutSine)
                 .SetTarget(item.transform)
            );

            groupSeq.Join(
                item.transform.DOScale(worldScale * 0.35f, flyTime)
                    .SetEase(Ease.InQuad)
                    .SetTarget(item.transform)
            );
        }

        groupSeq.AppendInterval(attachDelay);

        groupSeq.OnComplete(() =>
        {
            // when complete, attach surviving items
            foreach (var item in itemsSnapshot)
            {
                if (item == null || item.transform == null) continue;

                item.transform.SetParent(disk.transform, true);
                item.transform.localScale = Vector3.one * 0.35f;
                disk.AddItem(item.transform);
            }
            // Immediately attempt refill from the CURRENT supply disk
            TryHandleAfterMatch();
        });
    }


    public void CloseTray()
    {
        isClosed = true;
        GameObject soldOut = Instantiate(soldOutPrefabs);

        Transform t = soldOut.transform;
        t.SetParent(this.transform, false);

        SpriteRenderer sr = soldOut.GetComponent<SpriteRenderer>();

        Vector3 startLocalPos = new Vector3(0f, 1f, 0f);
        Vector3 hitPos = new Vector3(0f, 0.1f, 0f);
        Vector3 bouncePos = new Vector3(0f, 0.13f, 0f);

        t.localPosition = startLocalPos;
        t.localScale = Vector3.one * 0.8f;

        if (sr != null)
            sr.color = new Color(1f, 1f, 1f, 0f);

        Sequence seq = DOTween.Sequence();

        // 1️⃣ Xuất hiện (đứng yên)
        if (sr != null)
            seq.Append(sr.DOFade(1f, 0.18f).SetTarget(sr));

        seq.Join(
            t.DOScale(1f, 0.1f)
             .SetEase(Ease.OutQuad)
             .SetTarget(t)
        );

        // 2️⃣ Rơi xuống (sau khi đã hiện)
        seq.Append(
            t.DOLocalMove(hitPos, 0.25f)
             .SetEase(Ease.InQuad)
             .SetTarget(t)
        );
        //seq.AppendCallback(() =>
        //{
        //    AudioManager.Instance.PlaySFX(AudioManager.Instance.closeBox);
        //});
        // 3️⃣ Nảy rất nhẹ
        seq.Append(
            t.DOLocalMove(bouncePos, 0.08f)
             .SetEase(Ease.OutQuad)
             .SetTarget(t)
        );

        // 4️⃣ Ổn định vị trí
        seq.Append(
            t.DOLocalMove(hitPos, 0.06f)
             .SetEase(Ease.InQuad)
             .SetTarget(t)
        );
        seq.OnComplete(() =>
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.closeBox);

        });
    }
    //Lấy số lượng item cùng loại
    public int GetMaxSameItemCount()
    {
        if (isCompleted) return 0;
        DragItem[] items = GetComponentsInChildren<DragItem>();
        if (items.Length == 0) return 0;

        return items
            .GroupBy(i =>
            {
                var sr = i.GetComponent<SpriteRenderer>();
                return sr != null && sr.sprite != null
                    ? sr.sprite.name
                    : i.gameObject.name;
            })
            .Max(g => g.Count());
    }
    public Slot GetEmptySlot()
    {
        if (isCompleted) return null;

        foreach (Slot slot in slots)
        {
            if (slot.IsEmpty())
                return slot;
        }
        return null;
    }
    public DragItem GetAnyMatchingItem()
    {
        if (isCompleted) return null;

        DragItem[] items = GetComponentsInChildren<DragItem>();
        if (items.Length == 0) return null;

        var groups = items
            .GroupBy(i =>
            {
                var sr = i.GetComponent<SpriteRenderer>();
                return sr != null && sr.sprite != null
                    ? sr.sprite.name
                    : i.gameObject.name;
            })
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (groups == null)
            return null;

        // lấy 1 item bất kỳ trong nhóm đó
        return groups.First();
    }
    public string GetMainItemKey()
    {
        DragItem[] items = GetComponentsInChildren<DragItem>();
        if (items.Length == 0) return null;

        return items
            .GroupBy(i =>
            {
                var sr = i.GetComponent<SpriteRenderer>();
                return sr != null && sr.sprite != null
                    ? sr.sprite.name
                    : i.gameObject.name;
            })
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
    }

    public void RefillFromDisk(Plate diskItem)
    {
        if (diskItem == null || diskItem.IsEmpty())
            return;

        // 1️⃣ Sort disk items theo X (trái → phải)
        var diskItems = diskItem.items
            .OrderBy(i => i.transform.position.x)
            .ToList();

        // 2️⃣ Sort slots theo X (trái → phải)
        var orderedSlots = slots
            .Where(s => s != null && s.anchor != null)
            .OrderBy(s => s.anchor.position.x)
            .ToList();

        float moveTime = 0.22f;   // ↓ từ 0.38
        float scaleTime = 0.18f;   // ↓ từ 0.32
        float delayStep = 0.04f;   // ↓ từ 0.08

        bool diskWillBeEmpty = false;
        int animIndex = 0;

        // NEW: mark we're refilling so Slot.CanAcceptItem blocks incoming drags
        isRefilling = true;
        refillingCounter = 0;

        // 3️⃣ Map theo vị trí X
        foreach (DragItem item in diskItems)
        {
            if (item == null || item.transform == null)
                continue;

            // tìm slot trống có X gần nhất
            Slot slot = FindBestSlotByX(orderedSlots, item.transform.position.x);
            if (slot == null)
                continue;

            // prepare one animation -> increment counter
            refillingCounter++;

            diskItem.RemoveItem(item);
            diskWillBeEmpty = diskItem.IsEmpty();

            item.transform.DOKill();

            Vector3 startScale = item.transform.localScale;
            item.transform.SetParent(slot.transform, true);
            item.transform.localScale = startScale;

            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }

            float delay = animIndex * delayStep;
            animIndex++;

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);

            seq.Append(
                item.transform.DOLocalMove(
                    slot.anchor.localPosition,
                    moveTime
                ).SetEase(Ease.OutCubic)
                 .SetTarget(item.transform)
            );

            seq.Join(
                item.transform.DOScale(Vector3.one, scaleTime)
                    .SetEase(Ease.OutBack, 0.8f)
                    .SetTarget(item.transform)
            );

            seq.OnComplete(() =>
            {
                if (item == null || item.transform == null)
                {
                    // decrement counter even if item destroyed
                    refillingCounter--;
                    if (refillingCounter <= 0)
                    {
                        // finished all item animations
                        isRefilling = false;
                        currentDiskIndex++;
                        Plate nextDisk = CurrentDisk;
                        if (nextDisk != null)
                            nextDisk.ShowItemsInListOrder();
                    }
                    return;
                }

                item.transform.localPosition = Vector3.zero;
                slot.SetItem(item);
                item.startScale = Vector3.one; // hoặc item.transform.localScale
                if (diskWillBeEmpty)
                {
                    diskItem.FadeAndDestroy();
                }

                // animation finished for this item
                refillingCounter--;
                if (refillingCounter <= 0)
                {
                    // finished all item animations
                    isRefilling = false;
                    currentDiskIndex++;
                    Plate nextDisk = CurrentDisk;
                    if (nextDisk != null)
                        nextDisk.ShowItemsInListOrder();
                }
            });
        }

        // If no animations were started, ensure state updates and advance disk index
        if (refillingCounter == 0)
        {
            isRefilling = false;
            currentDiskIndex++;
            Plate nextDisk = CurrentDisk;
            if (nextDisk != null)
                nextDisk.ShowItemsInListOrder();
        }
    }

    Slot FindBestSlotByX(List<Slot> orderedSlots, float itemX)
    {
        Slot best = null;
        float bestDist = float.MaxValue;

        foreach (Slot s in orderedSlots)
        {
            if (s.currentItem != null) continue;

            float dx = Mathf.Abs(s.anchor.position.x - itemX);
            if (dx < bestDist)
            {
                bestDist = dx;
                best = s;
            }
        }

        return best;
    }

    public Plate CurrentDisk
    {
        get
        {
            // Guard against out-of-range index and null list
            if (disks == null || currentDiskIndex < 0 || currentDiskIndex >= disks.Count)
                return null;
            return disks[currentDiskIndex];
        }
    }


    public void TryHandleAfterMatch()
    {
        Plate disk = CurrentDisk;
        Debug.Log(", Disk: " + (disk != null ? disk.name : "null"));
        if (disk != null)
        {
            RefillFromDisk(disk);
        }
        else
        {
            CloseTray(); // ✅ hết disk thật sự
        }
    }
    public void NotifySlotChangedTray()
    {
        // còn item → reset cờ
        if (HasAnyItemInSlot())
        {
            hasHandledEmpty = false;
            return;
        }

        // tray trống thật sự
        if (!hasHandledEmpty)
        {
            hasHandledEmpty = true;
            FillTray();
        }
    }
    public void FillTray()
    {
        Plate disk = CurrentDisk;
        if (disk != null)
        {
            RefillFromDisk(disk);
        }
       
    }
    // Called by DragItem after an item leaves a tray (keeps behavior explicit)
    public void CheckAfterItemRemoved()
    {
        // reuse existing behavior
        NotifySlotChangedTray();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("TutorialArea"))
        {
            isInTutorialArea = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("TutorialArea"))
        {
            isInTutorialArea = false;
        }
    }
    //public bool HasEmptySlot()
    //{
    //    return GetComponentsInChildren<DragItem>().Length < maxSlot;
    //}

    //public void Disappear()
    //{
    //    transform.DOKill();

    //    TrayManager manager = GetComponentInParent<TrayManager>();
    //    Sequence seq = DOTween.Sequence();
    //    seq.Append(
    //        transform.DOScale(Vector3.zero, 0.18f)
    //            .SetEase(Ease.InOutCubic)
    //    );
    //    seq.OnComplete(() =>
    //    {
    //        Destroy(gameObject);
    //    });
    //}
}
