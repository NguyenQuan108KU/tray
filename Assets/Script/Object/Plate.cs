using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Plate : MonoBehaviour
{
    public List<DragItem> items = new List<DragItem>(); 
    public float fadeTime = 0.25f;
    public float scaleTime = 0.25f;

    private void Start()
    {
        foreach (var item in items)
        {
            item.diskX = item.transform.localPosition.x;
        }
    }
    public DragItem GetFirstItem()
    {
        if (items.Count == 0) return null;
        return items[0];
    }
    public void HideAllItems()
    {
        foreach (var item in items)
        {
            var sr = item.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0f;
                sr.color = c;
            }
            item.isLocked = true;
        }
    }

    public void ShowItemsInListOrder(float fadeTime = 0.12f, float delayStep = 0.04f)
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < items.Count; i++)
        {
            DragItem item = items[i];
            if (item == null) continue;

            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            sr.DOKill();
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;

            item.isLocked = true;

            seq.Append(
                sr.DOFade(0.6f, fadeTime)
                  .SetTarget(sr)
                  .OnComplete(() =>
                  {
                      item.isLocked = false;
                  })
            );

            seq.AppendInterval(delayStep);
        }
    }



    public void RemoveItem(DragItem item)
    {
        items.Remove(item);
    }

    public bool IsEmpty()
    {
        return items.Count == 0;
    }
    public void FadeAndDestroy()
    {
        // Defensive: kill any existing tweens targeting this plate or its children
        DOTween.Kill(transform);
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs)
        {
            if (sr != null)
                DOTween.Kill(sr.transform);
        }

        // Build fade/scale sequence and set explicit targets so DOTween can manage them
        Sequence seq = DOTween.Sequence().SetTarget(transform);

        foreach (var sr in srs)
        {
            if (sr == null) continue;
            // ensure starting alpha is current alpha (already ok) then tween
            seq.Join(
                sr.DOFade(0f, fadeTime).SetTarget(sr)
            );
        }

        seq.Join(
            transform.DOScale(0.8f, scaleTime)
                .SetEase(Ease.InBack)
                .SetTarget(transform)
        );

        seq.OnComplete(() =>
        {
            // Kill again to ensure no stray tweens remain
            DOTween.Kill(transform);
            foreach (var sr in srs)
            {
                if (sr != null)
                    DOTween.Kill(sr.transform);
            }

            Destroy(gameObject);
        });
    }
}
