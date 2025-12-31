using DG.Tweening;
using System.Collections;
using UnityEngine;

public class EndCart_Win : MonoBehaviour
{
    public GameObject confettiLeft;
    public GameObject confettiRight;
    public GameObject winText;
    public GameObject chest;
    public GameObject button;
    

    public float textDelay = 0.5f;
    public float chestDelay = 0.5f;

    void OnEnable()
    {
        confettiLeft.SetActive(false);
        confettiRight.SetActive(false);
        winText.SetActive(false);
        chest.SetActive(false);

        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        confettiLeft.SetActive(true);
        confettiRight.SetActive(true);

        yield return new WaitForSeconds(textDelay);

        winText.SetActive(true);

        yield return new WaitForSeconds(chestDelay);

        chest.SetActive(true);
        yield return new WaitForSeconds(chestDelay);
        button.SetActive(true);

        Vector3 baseScale = Vector3.one * 0.01f;

        // reset an toàn
        button.transform.DOKill();
        button.transform.localScale = Vector3.zero;

        // tween xuất hiện
        button.transform
            .DOScale(baseScale, 0.9f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // loop to nhỏ quanh scale gốc
                button.transform
                    .DOScale(baseScale * 1.2f, 0.7f)
                    .SetEase(Ease.InOutQuad)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetLink(button);
            });
    }
}
