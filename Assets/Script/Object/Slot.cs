using UnityEngine;

public class Slot : MonoBehaviour
{
    public Transform anchor;
    public DragItem currentItem;
    public Tray tray;

    private void Awake()
    {
        tray = GetComponentInParent<Tray>();
        EnsureCurrentItem();
    }

    private void OnValidate()
    {
        EnsureCurrentItem();
    }

    public bool CanAcceptItem()
    {
        if (tray == null) return true;
        if (tray.isClosed) return false;
        return true;
    }

    // 🔥 CHỈ TÌM ITEM – BỎ QUA ANCHOR
    void EnsureCurrentItem()
    {
        currentItem = null;

        foreach (Transform child in transform)
        {
            if (child == anchor) continue;

            DragItem item = child.GetComponent<DragItem>();
            if (item != null)
            {
                currentItem = item;
                return;
            }
        }
    }

    // 🔥 SLOT TRỐNG KHI KHÔNG CÓ ITEM
    public bool IsEmpty()
    {
        return currentItem == null;
    }

    // 🔥 SET ITEM TỪ DISK / DRAG
    public void SetItem(DragItem item)
    {
        currentItem = item;
    }

    public void SetItemDisk(DragItem item)
    {
        currentItem = item;
        item.transform.SetParent(transform, true);
        item.transform.position = anchor.position;
    }

    // 🔥 LẤY ITEM ĐÚNG (KHÔNG BẮT NHẦM ANCHOR)
    public DragItem GetItem()
    {
        return currentItem;
    }

    // 🔥 GỌI KHI ITEM BỊ DESTROY (MATCH)
    public void ClearItem()
    {
        currentItem = null;
    }
}
