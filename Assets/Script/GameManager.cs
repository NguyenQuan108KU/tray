using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("Point")]
    public TextMeshProUGUI pointText;
    public int point;
    public GameObject ECWin;
    public bool startTimer;

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

    [LunaPlaygroundField("Audio", 0, "Audio")]
    public bool audio;
    public int clickCount = 0;
    public bool isClick;
    public bool finishGame = false;
    public bool startGame = false;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnGlobalClick();
        }
    }
    public void AddPoint(int p)
    {
        point += p;
        pointText.text = point.ToString();
        if(point == 5)
        {
            ECWin.SetActive(true);
            CountdownTimer.instance.StopCountdown();
            GameManager.instance.finishGame = true;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.win);
        }
    }
    public void OnGlobalClick()
    {
        if (!isClickToLog || finishGame) return;

        clickCount++;
        Debug.Log("Click Count: " + clickCount);
        if (clickCount >= clicksToLog)
        {
            if (!isClick)
            {
                isClick = true;
                Debug.Log("Installing Luna Playground for Unity due to excessive clicks...");
                Luna.Unity.LifeCycle.GameEnded();
            }
            Debug.Log("Installing.");
            Luna.Unity.Playable.InstallFullGame();
        }
    }



    public void InstallGame()
    {
        Luna.Unity.Playable.InstallFullGame();
    }
    public void EndGame()
    {
        Luna.Unity.LifeCycle.GameEnded();
    }
}
