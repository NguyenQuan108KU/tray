using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Click")]
    [LunaPlaygroundField("Enable Click", 0, "Click")]
    public bool isClickToLog;
    [SerializeField]
    [LunaPlaygroundField("Count Click", 0, "Click")]
    public int clicksToLog = 15;

    // ================== TIMER (THÊM) ==================
    [Header("Timer")]
    [LunaPlaygroundField("Enable Timer", 0, "Timer")]
    public bool isTimer;

    [LunaPlaygroundField("Timer Seconds", 0, "Timer")]
    public int time = 30;
    // ==================================================

    public int clickCount = 0;
    public bool isClick;
    public bool finishGame = false;
    public TextMeshProUGUI textPoint;
    public int point = 0;
    public GameObject tutGame;

    // ===== Timer runtime =====
    float currentTime;
    bool lastIsTimer;
    Coroutine timerCo;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnGlobalClick();
        }
        // 👉 Detect bật / tắt timer khi đang chơi
        if (isTimer != lastIsTimer)
        {
            if (isTimer)
                StartTimer();
            else
                StopTimer();

            lastIsTimer = isTimer;
        }
    }
    public void OnGlobalClick()
    {
        if (!isClickToLog || finishGame) return;

        clickCount++;
        Debug.Log(clickCount);
        if (clickCount >= clicksToLog)
        {
            // EndGame chỉ 1 lần
            if (!isClick)
            {
                isClick = true;
                Debug.Log("End");
                Luna.Unity.LifeCycle.GameEnded();
            }

            // Từ click 15 trở đi → click nào cũng ra store
            Luna.Unity.Playable.InstallFullGame();
        }
    }
    // ================= TIMER LOGIC =================

    void StartTimer()
    {
        currentTime = time;

        if (timerCo != null)
            StopCoroutine(timerCo);

        timerCo = StartCoroutine(TimerRoutine());
    }

    IEnumerator TimerRoutine()
    {
        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            yield return null;
        }

        // ⏰ Hết thời gian → ra store
        InstallGame();
    }

    void StopTimer()
    {
        if (timerCo != null)
        {
            StopCoroutine(timerCo);
            timerCo = null;
        }
    }
    public void RegisterClick()
    {
        if (!isClickToLog || finishGame) return;

        clickCount++;
        Debug.Log(clickCount);

        if (clickCount >= clicksToLog && !isClick)
        {
            isClick = true;
            Luna.Unity.LifeCycle.GameEnded();
            Luna.Unity.Playable.InstallFullGame();
        }
    }

    // ============== CODE CŨ CỦA BẠN ==============

    public void InstallGame()
    {
        Luna.Unity.Playable.InstallFullGame();
    }

    public void EndGame()
    {
        Luna.Unity.LifeCycle.GameEnded();
    }
}
