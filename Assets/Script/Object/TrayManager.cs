using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TrayManager : MonoBehaviour
{
    public static TrayManager instance;

    [Header("-------------------------------Grill Setting-------------------------------")]
    [SerializeField] float stepX = 2f;  // khoảng cách ngang
    [SerializeField] float stepY = 2f;  // khoảng cách dọc

    [Header("-------------------------------Trio IQ Setting-------------------------------")]
    [SerializeField] private Transform column01;
    [SerializeField] private Transform column02;
    [SerializeField] private Transform column03;

    [Header("--------------------------------------------------------------")]
    public float spacing = 1.2f;

    public int visibleCount = 4;
    public float moveTime = 0.5f;
    private float trayHeight;
    private float step;
    int sorting = 0;
    [Header("Duo Item Setting")]

    public List<Transform> activeTrays = new List<Transform>();
    private Queue<GameObject> trayPool = new Queue<GameObject>();
    public float idleTime = 0f;
    public float hintDelay = 3f;
    public bool isTutorialShowing = true;
    bool isInteracting = false;
    [Header("Tutorial State")]
    public bool isFirstTutorial = true;
    [Header("Tutorial Manual Override")]
    //public Tray manualTray = null;
    //public DragItem manualItem = null;
    bool hasShownFirstTutorial = false;

    public Tray currentTargetTray;
    public Tray currentSourceTray;
    public DragItem currentItem;
    public bool isShowTutorialHint;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // Defensive: ensure list is not null (Inspector may have set it to null).
        if (activeTrays == null)
            activeTrays = new List<Transform>();

        // Do NOT modify serialized lists while in the editor outside Play mode.
        if (Application.isPlaying)
            PopulateActiveTraysFromColumns();
    }

    void Start()
    {
        Debug.Log(column01.name);
        Debug.Log(column02.name);
        Debug.Log(column03.name);
        StartCoroutine(InitializeRoutine());

        // Populate only at runtime — avoids editor-side SerializedObject issues.
        if (Application.isPlaying)
            PopulateActiveTraysFromColumns();

        // If columns haven't been assigned at design time, warn and skip alignment.
        if (column01 == null && column02 == null && column03 == null)
        {
            Debug.LogWarning("TrayManager: column01/column02/column03 are null. Assign them in the Inspector or call Populate/Align after creating columns at runtime.");
            return;
        }

        AlignAllColumns();
    }

    //Khoảng cách giữa các cột
    public void AlignColumns()
    {
        if (column01 != null)
            column01.localPosition = new Vector3(-stepX, 0f, 0f);

        if (column02 != null)
            column02.localPosition = new Vector3(0f, -1f, 0f);

        if (column03 != null)
            column03.localPosition = new Vector3(stepX, 0f, 0f);
    }
    //Khoảng Cách giữa các Tray trong column
    public void AlignTraysInColumn(Transform column)
    {
        if (column == null) return;

        for (int i = 0; i < column.childCount; i++)
        {
            Transform tray = column.GetChild(i);

            float y = -i * stepY;   // stepY = khoảng cách giữa tray
            tray.localPosition = new Vector3(0f, y, tray.localPosition.z);
        }
    }
    //Sắp xếp (Gọi trong start)
    public void AlignAllColumns()
    {
        // Align columns only if any column is assigned
        if (column01 != null || column02 != null || column03 != null)
            AlignColumns();

        AlignTraysInColumn(column01);
        AlignTraysInColumn(column02);
        AlignTraysInColumn(column03);
    }

    // Populate activeTrays safely from the configured columns.
    // This provides a robust default if the inspector left activeTrays empty or null.
    public void PopulateActiveTraysFromColumns()
    {
        if (activeTrays == null)
            activeTrays = new List<Transform>();
        else
            activeTrays.Clear();

        Transform[] cols = new Transform[] { column01, column02, column03 };
        foreach (var col in cols)
        {
            if (col == null) continue;
            for (int i = 0; i < col.childCount; i++)
            {
                activeTrays.Add(col.GetChild(i));
            }
        }
    }

    public void OnUserBeginInteract()
    {
        isInteracting = true;
        idleTime = 0f;
        //manualItem = null;

        // Turn off the fixed-first-tutorial mode on the player's first interaction
        if (isFirstTutorial)
            isFirstTutorial = false;

        //if (isTutorialShowing)
        //{
        //    isTutorialShowing = false;
        //    TutorialManager.instance?.HideHint();
        //    TutorialManager.instance?.StopPulseHint();
        //}
        TutorialManager.instance?.HideHint();
        TutorialManager.instance?.StopPulseHint();
    }
    public void OnUserEndInteract()
    {
        isInteracting = false;
        isShowTutorialHint = true;
        idleTime = 0f; //  bắt đầu đếm 3s từ đây
    }
    public void ResetIdle()
    {
        idleTime = 0f;
    }
    System.Collections.IEnumerator InitializeRoutine()
    {
        yield return new WaitForEndOfFrame();
        if (isFirstTutorial)
        {
            Invoke(nameof(ShowTutorialHint), 0.2f);
            isTutorialShowing = false;
        }
    }
    void SpawnTrayAtTop()
    {
        if (trayPool.Count == 0)
        {

            return;
        }
        GameObject prefab = trayPool.Dequeue();

        GameObject tray = Instantiate(prefab, transform);
        tray.GetComponent<SpriteRenderer>().sortingOrder = sorting--;

        float startY = (activeTrays.Count) * step * 0.5f;
        float spawnY = startY + step - 0.5f;

        tray.transform.localPosition = new Vector3(0, spawnY, 0);

        activeTrays.Insert(0, tray.transform);
    }
    public void CompleteTray(Transform completedTray)
    {
        if (!activeTrays.Contains(completedTray)) return;

        int index = activeTrays.IndexOf(completedTray);


        activeTrays.RemoveAt(index);


        completedTray.SetParent(null, true);

        float fallTime = moveTime;
        float shrinkTime = 0.35f;

        completedTray.DOKill();

        Sequence seq = DOTween.Sequence();


        seq.AppendCallback(() =>
        {
            SpawnTrayAtTop();
            AlignAnimated();
        });


        seq.AppendInterval(fallTime);


        seq.Append(
            completedTray.DOScale(0f, shrinkTime)
                .SetEase(Ease.InBack)
        );


        seq.OnComplete(() =>
        {
            Destroy(completedTray.gameObject);
        });
    }
    public void AlignAnimated()
    {
        if (activeTrays.Count == 0) return;

        int totalSlots = Mathf.Max(visibleCount, activeTrays.Count);
        float startY = (totalSlots - 1) * step * 0.5f;

        int startSlot = (activeTrays.Count < visibleCount)
            ? (visibleCount - activeTrays.Count)
            : 0;

        for (int i = 0; i < activeTrays.Count; i++)
        {
            int slotIndex = startSlot + i;
            float targetY = startY - slotIndex * step;

            Transform tray = activeTrays[i];
            tray.DOKill();

            float currentY = tray.localPosition.y;


            if (currentY > targetY + 0.01f)
            {
                float y = targetY;

                Sequence seq = DOTween.Sequence();
                seq.Append(
                    tray.DOLocalMoveY(y, 0.25f)
                        .SetEase(Ease.InCubic)
                );
                seq.Append(
                    tray.DOLocalMoveY(y + 0.4f, 0.15f)
                        .SetEase(Ease.OutCubic)
                );

                seq.Append(
                    tray.DOLocalMoveY(y, 0.15f)
                        .SetEase(Ease.InCubic)
                );


                seq.Append(
                    tray.DOLocalMoveY(y + 0.15f, 0.1f)
                        .SetEase(Ease.OutCubic)
                );

                seq.Append(
                    tray.DOLocalMoveY(y, 0.1f)
                        .SetEase(Ease.InCubic)
                );
                seq.Append(
                    tray.DOLocalMoveY(y + 0.05f, 0.08f)
                        .SetEase(Ease.OutCubic)
                );
                seq.Append(
                    tray.DOLocalMoveY(y, 0.08f)
                        .SetEase(Ease.InCubic)
                );
            }
            else
            {
                tray.DOLocalMoveY(targetY, 0.25f)
                    .SetEase(Ease.OutQuad);
            }
        }
    }
    public List<Tray> GetTraysWithMaxSameItem()
    {
        int max = 0;
        List<Tray> result = new List<Tray>();

        foreach (Transform trayTf in activeTrays)
        {
            Tray tray = trayTf.GetComponent<Tray>();
            if (tray == null) continue;
            int count = tray.GetMaxSameItemCount();
            if (count < 2) continue;

            if (count > max)
            {
                max = count;
                result.Clear();
                result.Add(tray);
            }
            else if (count == max)
            {
                result.Add(tray);
            }
        }
        return result;
    }
    //Chọn ngẫu nhiên 1 tray
    public (Tray tray, Slot slot)? GetRandomTrayEmptySlot()
    {
        var trays = GetTraysWithMaxSameItem();

        var validTrays = trays
            .Where(t => !t.isCompleted && t.GetEmptySlot() != null)
            .ToList();

        if (validTrays.Count == 0)
            return null;

        Tray tray = validTrays[Random.Range(0, validTrays.Count)];
        Slot slot = tray.GetEmptySlot();

        return (tray, slot);
    }
    public (Tray tray, Slot slot, DragItem item)? GetRandomTraySlotAndItem()
    {
        var traySlot = GetRandomTrayEmptySlot();
        if (!traySlot.HasValue)
            return null;

        Tray tray = traySlot.Value.tray;
        Slot slot = traySlot.Value.slot;

        DragItem item = tray.GetAnyMatchingItem();
        if (item == null)
            return null;

        return (tray, slot, item);
    }
    public void ShowTutorialHint()
    {
        Tray targetTray = currentTargetTray;
        Tray sourceTray = currentSourceTray;
        DragItem item = currentItem;

        // ✅ Validate reference
        if (targetTray == null || targetTray.isCompleted)
            targetTray = GetRandomVisibleTrayWithEmptySlot();

        if (targetTray == null)
            return;

        if (sourceTray == null || sourceTray == targetTray)
            sourceTray = GetRandomVisibleTrayWithItemExcept(targetTray);

        if (sourceTray == null)
            return;

        if (item == null || item.transform.GetComponentInParent<Tray>() != sourceTray)
            item = GetAnyItemFromTray(sourceTray);

        if (item == null)
            return;

        Slot fromSlot = item.GetComponentInParent<Slot>();
        Slot targetSlot = targetTray.GetEmptySlot();

        if (fromSlot == null || targetSlot == null)
            return;

        // cache lại
        currentTargetTray = targetTray;
        currentSourceTray = sourceTray;
        currentItem = item;

        currentTargetTray = targetTray;
        currentSourceTray = sourceTray;
        currentItem = item;

        // isTutorialShowing = true;

        TutorialManager.instance.ShowHandHint(fromSlot, targetSlot, item);
    }
    public void ShowIdleItemHint()
    {
        isShowTutorialHint = false;
        DragItem item = GetAnyItemInSlot();
        if (item == null)
            return;


        TutorialManager.instance.ShowPulseHint(item.transform);
    }
    DragItem GetAnyItemInSlot()
    {
        // Defensive: if activeTrays was not assigned in inspector try to populate from columns.
        if (activeTrays == null || activeTrays.Count == 0)
        {
            PopulateActiveTraysFromColumns();
            if (activeTrays == null || activeTrays.Count == 0)
                return null;
        }

        List<DragItem> candidates = new List<DragItem>();

        foreach (Transform trayTf in activeTrays)
        {
            if (trayTf == null) continue;
            Tray tray = trayTf.GetComponent<Tray>();
            if (tray == null) continue;
            if (tray.isCompleted) continue;
            if (!tray.isInTutorialArea) continue;

            foreach (Slot slot in tray.GetComponentsInChildren<Slot>())
            {
                if (slot.IsEmpty()) continue;

                DragItem item = slot.currentItem;
                if (item == null) continue;

                // ✅ đảm bảo item là con trực tiếp của slot
                if (item.transform.parent != slot.transform)
                    continue;

                candidates.Add(item);
            }
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    Tray GetRandomVisibleTrayWithEmptySlot()
    {
        List<Tray> list = new List<Tray>();

        foreach (Transform tf in activeTrays)
        {
            Tray tray = tf.GetComponent<Tray>();
            if (tray == null) continue;
            if (tray.isCompleted) continue;
            if (!tray.isInTutorialArea) continue;
            if (tray.GetEmptySlot() == null) continue;

            list.Add(tray);
        }

        if (list.Count == 0)
            return null;

        return list[Random.Range(0, list.Count)];
    }
    Tray GetRandomVisibleTrayWithItemExcept(Tray exclude)
    {
        List<Tray> list = new List<Tray>();

        foreach (Transform tf in activeTrays)
        {
            Tray tray = tf.GetComponent<Tray>();
            if (tray == null) continue;
            if (tray == exclude) continue;
            if (tray.isCompleted) continue;
            if (!tray.isInTutorialArea) continue;
            if (tray.GetComponentsInChildren<DragItem>().Length == 0) continue;

            list.Add(tray);
        }

        if (list.Count == 0)
            return null;

        return list[Random.Range(0, list.Count)];
    }
    public (Tray tray, Slot slot)? GetFallbackTrayAndSlot()
    {

        foreach (Transform trayTf in activeTrays)
        {
            Tray tray = trayTf.GetComponent<Tray>();
            if (tray == null) continue;
            if (!tray.isInTutorialArea) continue;
            if (tray.isCompleted) continue;

            if (tray.GetComponentsInChildren<DragItem>().Length >= 1 &&
                tray.GetEmptySlot() != null)
            {
                return (tray, tray.GetEmptySlot());
            }
        }
        return null;
    }
    public DragItem GetAnyItemFromOtherTray(Tray excludeTray)
    {
        List<DragItem> items = new List<DragItem>();

        foreach (Transform trayTf in activeTrays)
        {
            Tray tray = trayTf.GetComponent<Tray>();
            if (tray == null) continue;
            if (tray == excludeTray) continue;
            if (tray.isCompleted) continue;

            items.AddRange(tray.GetComponentsInChildren<DragItem>());
        }

        if (items.Count == 0)
            return null;

        return items[Random.Range(0, items.Count)];
    }

    public void NotifyUserInteraction()
    {
        idleTime = 0f;

        //if (isTutorialShowing)
        //{
        //    isTutorialShowing = false;
        //    TutorialManager.instance.StopPulseHint();
        //    TutorialManager.instance.HideHint();
        //}
        TutorialManager.instance.StopPulseHint();
        TutorialManager.instance.HideHint();
    }
    public DragItem GetItemFromOtherTray(Tray targetTray)
    {
        string key = targetTray.GetMainItemKey();
        if (string.IsNullOrEmpty(key))
            return null;

        List<DragItem> candidates = new List<DragItem>();

        foreach (Transform trayTf in activeTrays)
        {
            Tray tray = trayTf.GetComponent<Tray>();
            if (tray == null) continue;
            if (tray == targetTray) continue;
            if (tray.isCompleted) continue;

            foreach (var item in tray.GetComponentsInChildren<DragItem>())
            {
                var sr = item.GetComponent<SpriteRenderer>();
                string itemKey = sr != null && sr.sprite != null
                    ? sr.sprite.name
                    : item.gameObject.name;

                if (itemKey == key)
                    candidates.Add(item);
            }
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }
    DragItem GetAnyItemFromTray(Tray tray)
    {
        var items = tray.GetComponentsInChildren<DragItem>();
        if (items.Length == 0)
            return null;

        return items[Random.Range(0, items.Length)];
    }
    public Tray GetRandomVisibleTray()
    {
        List<Tray> candidates = new List<Tray>();

        foreach (Transform trayTf in activeTrays)
        {
            Tray tray = trayTf.GetComponent<Tray>();
            if (tray == null) continue;
            if (tray.isCompleted) continue;
            if (!tray.isInTutorialArea) continue;

            candidates.Add(tray);
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    // Editor-time cleanup to remove stale/null entries created by inspector edits or deleted objects.
    // Runs in editor when values change and prevents SerializedObjectNotCreatableException.
    private void OnValidate()
    {
        if (activeTrays == null) return;
        activeTrays.RemoveAll(t => t == null);
    }

    private void Update()
    {
        if (GameManager.Instance.finishGame) return;
        if (isTutorialShowing) return;

        // If user is not interacting, accumulate idle time and show tutorial after hintDelay seconds.
        if (!isInteracting)
        {
            idleTime += Time.deltaTime;

            if (idleTime >= hintDelay && isShowTutorialHint)
            {
                idleTime = 0f;
                // Show the idle tutorial (pulse) — adjust to ShowTutorialHint() if you prefer full hand hint.
                ShowIdleItemHint();
            }
        }
        else
        {
            // While interacting keep timer reset
            idleTime = 0f;
        }
    }
}
