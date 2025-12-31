using UnityEngine;
using DG.Tweening;

public class NotiBox : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform box;      // Box
    public CanvasGroup bg;          

    [Header("Timing")]
    public float delay = 3f;
    public float animTime = 2f;

    Vector3 boxOriginScale;

    void Awake()
    {
        // Lưu scale gốc (1.5, 1.5, 1.5)
        boxOriginScale = box.localScale;
    }

    void OnEnable()
    {
        // Reset trạng thái
        box.localScale = boxOriginScale;
        bg.alpha = 1f;
        bg.blocksRaycasts = true;

        DOVirtual.DelayedCall(delay, Hide);
    }

    void Hide()
    {
        Sequence seq = DOTween.Sequence();
        seq.Join(
            box.DOScale(0f, animTime)
               .SetEase(Ease.InOutSine)
        );
        // BG mờ dần
        seq.Join(
            bg.DOFade(0f, animTime)
        );

        seq.OnComplete(() =>
        {
            bg.blocksRaycasts = false;
            gameObject.SetActive(false);
            TrayManager.instance.StartTut();
            GameManager.instance.startGame = true;
        });
    }
}
