using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class ProgressBrain : MonoBehaviour
{
    public static ProgressBrain instance;

    [Header("UI")]
    public RectTransform brain;
    public RectTransform startPoint;
    public RectTransform endPoint;
    public GameObject endcartWin;

    [Header("Progress")]
    public int maxScore = 200;
    public int scorePerTray = 25;
    public int currentScore = 0;

    [Header("Anim")]
    public float moveTime = 0.3f;

    [Header("Drain Timer")]
    public float idleDelay = 2f;
    public float drainInterval = 1f;
    public int drainAmount = 1;

    private float idleTimer;
    private float drainTimer;

    private Vector2 startPos;
    private Vector2 endPos;

    public int index = 0;
    public TextMeshProUGUI text;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // 🔥 UI POSITION
        startPos = startPoint.anchoredPosition;
        endPos = endPoint.anchoredPosition;
    }

    private void Update()
    {
        if (GameManager.Instance.finishGame)
            return;

        idleTimer += Time.deltaTime;

        if (idleTimer < idleDelay)
            return;

        drainTimer += Time.deltaTime;

        if (drainTimer >= drainInterval)
        {
            drainTimer = 0f;
            ReduceScore(drainAmount);
        }
    }

    public void AddTrayMatch()
    {
        index++;
        text.text = index.ToString();

        currentScore += scorePerTray;
        currentScore = Mathf.Clamp(currentScore, 0, maxScore);

        if (currentScore >= maxScore || index >= 10)
        {
            StartCoroutine(ActiveWin());
            GameManager.Instance.finishGame = true;
        }

        ResetIdleTimer();
        UpdateBrainPosition();
    }

    void ReduceScore(int amount)
    {
        if (currentScore <= 0)
            return;

        currentScore -= amount;
        currentScore = Mathf.Clamp(currentScore, 0, maxScore);

        UpdateBrainPosition();
    }

    void ResetIdleTimer()
    {
        idleTimer = 0f;
        drainTimer = 0f;
    }

    void UpdateBrainPosition()
    {
        float progress = (float)currentScore / maxScore;
        Vector2 targetPos = Vector2.Lerp(startPos, endPos, progress);

        brain
            .DOAnchorPos(targetPos, moveTime)
            .SetEase(Ease.OutCubic);
    }

    IEnumerator ActiveWin()
    {
        yield return new WaitForSeconds(1f);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.lose);
        endcartWin.GetComponent<EndCart_Lose>()?.Show();
        GameManager.Instance.EndGame();
        GameManager.Instance.finishGame = true;
        Debug.Log("Win Game");
    }
}
