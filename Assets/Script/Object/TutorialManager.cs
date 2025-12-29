using DG.Tweening;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    public GameObject handPrefab;
    private GameObject currentHand;
    Transform pulsingTarget;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Sinh tay chỉ từ item → slot
    /// </summary>
    public void ShowHandHint(Slot fromSlot, Slot toSlot, DragItem item)
    {
        HideHint();

        currentHand = Instantiate(handPrefab);
        currentHand.transform.SetParent(null);

        HandHint hand = currentHand.GetComponent<HandHint>();
        hand.Play(fromSlot, toSlot, item);
    }


    public void HideHint()
    {
        if (currentHand != null)
        {
            var hand = currentHand.GetComponent<HandHint>();
            if (hand != null)
                hand.Stop();

            Destroy(currentHand);
            currentHand = null;
        }
    }

    public void ShowPulseHint(Transform target)
    {
        StopPulseHint();

        pulsingTarget = target;
        target.DOKill();

        target
            .DOLocalRotate(
                new Vector3(0, 0, 8f),   // góc lắc
                0.18f
            )
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }




    public void StopPulseHint()
    {
        if (pulsingTarget != null)
        {
            pulsingTarget.DOKill();
            pulsingTarget.localScale = Vector3.one;
            pulsingTarget = null;
        }
    }

}
