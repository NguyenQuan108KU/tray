using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    public static CountdownTimer instance;
    public TMP_Text timerText;
    public int startSeconds = 30;

    [Header("Warning Effect")]
    public int warningTime = 5;
    public Color warningColor = Color.red;
    public float pulseScale = 1.3f;
    public float pulseSpeed = 0.5f;

    private Coroutine countdownCo;
    private Coroutine pulseCo;
    private Vector3 originalScale;
    private Color originalColor;

    public GameObject backgroundWarning;
    public GameObject timeUpPanel;
    public bool hasStarted = false;

    [Header("Background Blink")]
    public float bgMinAlpha = 0.25f;
    public float bgMaxAlpha = 1f;
    public float bgBlinkSpeed = 4f;

    private Coroutine bgBlinkCo;
    private Image bgImage;
    private Color bgOriginalColor;


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
    void Start()
    {
        originalScale = timerText.transform.localScale;
        originalColor = timerText.color;

        // cache background image and hide it initially
        if (backgroundWarning != null)
        {
            bgImage = backgroundWarning.GetComponent<Image>();
            if (bgImage != null)
                bgOriginalColor = bgImage.color;
            backgroundWarning.SetActive(false);
        }

        //StartCountdown();
    }

    public void StartCountdown()
    {
        // stop any previous blink
        StopBackgroundBlink();

        if (countdownCo != null)
            StopCoroutine(countdownCo);

        countdownCo = StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        int timeLeft = startSeconds;

        // ensure hidden at start
        if (backgroundWarning != null)
            backgroundWarning.SetActive(false);

        while (timeLeft > 0)
        {
            UpdateText(timeLeft);

            // 🔊 play warning behavior when in warning period
            if (timeLeft <= warningTime)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.warningTick);

                timerText.color = warningColor;

                if (backgroundWarning != null && bgBlinkCo == null)
                {
                    backgroundWarning.SetActive(true);
                    // ensure original color cached
                    if (bgImage != null)
                        bgOriginalColor = bgImage.color;
                    bgBlinkCo = StartCoroutine(BackgroundBlink());
                }
            }
            else
            {
                // restore normal state if not in warning
                timerText.color = originalColor;
                if (bgBlinkCo != null)
                    StopBackgroundBlink();
            }

            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        UpdateText(0);
        OnTimeUp();
    }



    void UpdateText(int totalSeconds)
    {
        //AudioManager.Instance.PlaySFX(AudioManager.Instance.pop);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    IEnumerator BackgroundBlink()
    {
        if (bgImage == null)
            yield break;

        bool visible = false;

        while (true)
        {
            visible = !visible;

            Color c = bgOriginalColor;
            c.a = visible ? bgMaxAlpha : bgMinAlpha;
            bgImage.color = c;

            yield return new WaitForSeconds(1f);
        }
    }


    void StopBackgroundBlink()
    {
        if (bgBlinkCo != null)
        {
            StopCoroutine(bgBlinkCo);
            bgBlinkCo = null;
        }

        if (bgImage != null)
            bgImage.color = bgOriginalColor;

        if (backgroundWarning != null)
            backgroundWarning.SetActive(false);
    }

    //IEnumerator PulseText()
    //{
    //    while (true)
    //    {
    //        // to lên
    //        yield return ScaleTo(originalScale * pulseScale);
    //        // nhỏ lại
    //        yield return ScaleTo(originalScale);
    //    }
    //}

    IEnumerator ScaleTo(Vector3 target)
    {
        Vector3 start = timerText.transform.localScale;
        float t = 0f;

        while (t < pulseSpeed)
        {
            t += Time.deltaTime;
            timerText.transform.localScale = Vector3.Lerp(start, target, t / pulseSpeed);
            yield return null;
        }
    }

    void OnTimeUp()
    {
        // stop blink when time up
        StopBackgroundBlink();

        timerText.text = "00:00";

        if (timeUpPanel != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.lose);
            timeUpPanel.GetComponent<EndCart_Lose>()?.Show();
            GameManager.Instance.finishGame = true;
            GameManager.Instance.EndGame();
        }
    }


}
