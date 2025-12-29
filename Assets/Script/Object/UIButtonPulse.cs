using UnityEngine;
using DG.Tweening;

public class UIButtonPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float scaleUp = 1.25f;     // độ to lên
    public float duration = 0.45f;    // thời gian 1 nhịp

    private Vector3 originalScale;
    private Tween pulseTween;

    void OnEnable()
    {
        originalScale = transform.localScale;

        pulseTween = transform
            .DOScale(originalScale * scaleUp, duration)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void OnDisable()
    {
        pulseTween?.Kill();
        transform.localScale = originalScale;
    }
}
