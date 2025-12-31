using DG.Tweening;
using UnityEngine;

public class PackTarget : MonoBehaviour
{
    public ItemType packType;
    public Transform attachPoint;

    [Header("Slot Info")]
    public int slotIndex;

    [Header("Capacity")]
    public int capacity = 5;
    public int currentCount = 0;
    public bool isFull = false;

    public void AddItems(int count)
    {
        if (isFull) return;

        currentCount += count;

        if (currentCount >= capacity)
        {
            currentCount = capacity; // khóa cứng
            isFull = true;

            enabled = false;

            PackManager.instance.OnPackFilled(this);
        }
        else
        {
            Punch();
        }
    }

    void Punch()
    {
        transform.DOPunchScale(Vector3.one * 0.03f, 0.25f, 1, 0.7f);
    }
    public void FlyUp(System.Action onComplete)
    {
        if (!this || !gameObject) return;

        transform.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.SetTarget(transform);

        seq.Append(
            transform.DOPunchScale(Vector3.one * 0.03f, 0.25f, 1, 0.7f)
        );

        seq.AppendInterval(0.2f);

        seq.Append(
            transform.DOMoveY(transform.position.y + 6f, 0.8f)
                .SetEase(Ease.InQuad)
        );

        seq.OnComplete(() => onComplete?.Invoke());
    }


}
