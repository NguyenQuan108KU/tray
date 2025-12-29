using UnityEngine;
using DG.Tweening;

public class EndCart_Lose : MonoBehaviour
{
    public GameObject actionButton;
    public GameObject emoji;
    public GameObject logo;
    public GameObject icon;
    public GameObject praticle;

    [Header("Anim")]
    public float buttonScaleTime = 0.35f;

    [Header("Item Anim")]
    public float itemStartScale = 0.4f;
    public float itemPopScale = 1f;
    public float itemInTime = 0.25f;
    public float itemSettleTime = 0.05f;

    private Tween buttonPulse;
    [Header("Item Anim")]
    public float itemFinalScale = 0.85f;   // 👈 scale cuối

    [Header("Item Final Scale")]
    public float emojiFinalScale = 0.45f;
    public float otherItemFinalScale = 0.85f;



    void Awake()
    {
        InitState();
    }

    void InitState()
    {
        SetItemInit(emoji);
        SetItemInit(logo);
        SetItemInit(icon);

        if (praticle != null)
            praticle.SetActive(false);

        actionButton.transform.localScale = Vector3.zero;
    }

    void SetItemInit(GameObject go)
    {
        if (go == null) return;
        go.transform.localScale = Vector3.one * itemStartScale;
        go.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        DOTween.KillAll();
        buttonPulse?.Kill();

        InitState();

        DOVirtual.DelayedCall(0.01f, () =>
        {
            Sequence seq = DOTween.Sequence();

            // ===== ITEMS + PARTICLE (CÙNG LÚC) =====
            seq.AppendCallback(() =>
            {
                if (emoji) emoji.SetActive(true);
                if (logo) logo.SetActive(true);
                if (icon) icon.SetActive(true);
                if (praticle) praticle.SetActive(true);
            });

            JoinItemAnim(seq, emoji, emojiFinalScale);
            JoinItemAnim(seq, logo, otherItemFinalScale);
            JoinItemAnim(seq, icon, otherItemFinalScale);


            // ===== BUTTON SAU =====
            seq.AppendInterval(itemInTime + itemSettleTime + 0.1f);
            seq.Append(
                actionButton.transform
                    .DOScale(1f, buttonScaleTime)
                    .SetEase(Ease.OutBack)
            );

            seq.OnComplete(StartButtonPulse);
        });
    }

    void JoinItemAnim(Sequence mainSeq, GameObject go, float finalScale)
    {
        if (go == null) return;

        mainSeq.Join(
            go.transform
                .DOScale(finalScale, itemInTime)
                .SetEase(Ease.OutCubic)
        );
    }




    void StartButtonPulse()
    {
        buttonPulse?.Kill();
        buttonPulse = actionButton.transform
            .DOScale(1.15f, 0.6f)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void Hide()
    {
        buttonPulse?.Kill();

        Sequence seq = DOTween.Sequence();
        seq.Join(actionButton.transform.DOScale(0f, 0.2f));

        HideItem(seq, emoji);
        HideItem(seq, logo);
        HideItem(seq, icon);

        if (praticle != null)
            praticle.SetActive(false);

        seq.OnComplete(() => gameObject.SetActive(false));
    }

    void HideItem(Sequence seq, GameObject go)
    {
        if (go == null) return;
        seq.Join(go.transform.DOScale(0f, 0.2f));
    }
}
