using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI - In Game Timer")]
    public TextMeshProUGUI timerText;
    public Color normalTimerColor = Color.white;
    public Color lowTimeColor = Color.red;
    public UIFeedback timerFX;
    public float startTime = 30f;
    private float currentTime;

    private int lastDisplayedSecond = -1;

    private bool gameActive = false;

    [Header("Curtain / Overlay")]
    public RectTransform endScreenTexture; // la misma cortina para inicio y fin
    public Vector2 curtainDownPos = new Vector2(0, 0);     // tapando pantalla
    public Vector2 curtainUpPos = new Vector2(0, 1080f); // fuera hacia arriba (ajustá según Canvas)

    [Header("Countdown Inicial")]
    public TextMeshProUGUI pregameCountdownText;
    public int pregameCountdownFrom = 3;
    public float pregameLastWordHold = 0.4f; // cuánto dura "¡YA!"

    [Header("Gameplay")]
    public Spawner spawner;

    public TargetMove targetMove;
    public TargetClick targetClick;
    public TargetMove targetMoveDanger;
    public TargetClick targetClickDanger;

    [Header("Results")]
    public ResultsPanelController resultsPanel;
    public ScoreManager scoreManager;
    public float resultsHoldSeconds = 2.5f; // cuánto tiempo quedan visibles antes del fade y salida
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        // Estado inicial
        currentTime = startTime;
        gameActive = false;

        // Asegurar cortina visible al comienzo
        if (endScreenTexture != null)
        {
            endScreenTexture.gameObject.SetActive(true);
            endScreenTexture.anchoredPosition = curtainDownPos; // abajo (tapando)
        }

        if (spawner != null)
            spawner.StopSpawning();

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(startTime).ToString();

        if (pregameCountdownText != null)
            pregameCountdownText.gameObject.SetActive(true);

        StartCoroutine(PreGameSequence());
    }
    void Update()
    {
        if (!gameActive) return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            EndGame();
        }

        int seconds = Mathf.CeilToInt(currentTime);
        if (timerText != null && seconds != lastDisplayedSecond)
        {
            timerText.text = seconds.ToString();
            lastDisplayedSecond = seconds;

            // Low time feedback
            if (seconds <= 5)
            {
                timerText.color = lowTimeColor;
                if (timerFX) timerFX.ShakePosition(8f, 0.14f, 40f); // sacudida breve cada cambio de segundo
            }
            else
            {
                timerText.color = normalTimerColor;
            }
        }
    }

    IEnumerator PreGameSequence()
    {
        // 1) Countdown
        yield return StartCoroutine(DoPregameCountdown());

        // 2) Subir cortina
        yield return StartCoroutine(AnimateCurtain(endScreenTexture, curtainDownPos, curtainUpPos, 1f));

        // 3) Ocultar cortina y countdown
        if (endScreenTexture != null)
            endScreenTexture.gameObject.SetActive(false);
        if (pregameCountdownText != null)
            pregameCountdownText.gameObject.SetActive(false);

        // 4) Comenzar juego
        StartGame();
    }

    IEnumerator DoPregameCountdown()
    {
        int t = pregameCountdownFrom;
        while (t > 0)
        {
            pregameCountdownText.text = t.ToString();
            yield return new WaitForSecondsRealtime(1f);
            t--;
        }
        pregameCountdownText.text = "¡YA!";
        yield return new WaitForSecondsRealtime(pregameLastWordHold);
    }

    IEnumerator AnimateCurtain(RectTransform rt, Vector2 from, Vector2 to, float duration)
    {
        if (rt == null) yield break;

        float elapsed = 0f;
        rt.gameObject.SetActive(true);
        rt.anchoredPosition = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // independiente de timeScale
            float t = Mathf.Clamp01(elapsed / duration);
            // easing suave
            float e = Mathf.SmoothStep(0f, 1f, t);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, e);
            yield return null;
        }
        rt.anchoredPosition = to;
    }

    void StartGame()
    {
        gameActive = true;
        currentTime = startTime;

        if (spawner != null)
            spawner.StartSpawning(startTime);

        if (targetMove != null) targetMove.gameOver = false;
        if (targetClick != null) targetClick.gameOver = false;
        if (targetMoveDanger != null) targetMoveDanger.gameOver = false;
        if (targetClickDanger != null) targetClickDanger.gameOver = false;
    }

    void EndGame()
    {
        if (!gameActive) return;
        gameActive = false;

        // Stop new spawns
        if (spawner != null)
            spawner.StopSpawning();

        // Halt all target movement
        TargetMove[] targets = FindObjectsOfType<TargetMove>();
        foreach (var t in targets)
            t.enabled = false;

        // Disable clicking
        TargetClick[] clicks = FindObjectsOfType<TargetClick>();
        foreach (var c in clicks)
            c.enabled = false;

        // Brute force flags
        if (targetMove != null) targetMove.gameOver = true;
        if (targetClick != null) targetClick.gameOver = true;
        if (targetMoveDanger != null) targetMoveDanger.gameOver = true;
        if (targetClickDanger != null) targetClickDanger.gameOver = true;

        // Bajar cortina al final
        StartCoroutine(ShowEndScreen());
    }

    IEnumerator ShowEndScreen()
    {
        yield return new WaitForSeconds(0.5f);

        if (endScreenTexture != null)
        {
            endScreenTexture.gameObject.SetActive(true);

            Vector2 startPos = curtainUpPos;
            Vector2 endPos = curtainDownPos;
            float duration = 1f;
            yield return StartCoroutine(AnimateCurtain(endScreenTexture, startPos, endPos, duration));
        }

        yield return StartCoroutine(ShowResultsThenExit());
    }

    IEnumerator ShowResultsThenExit()
    {
        // 1) Mostrar panel de resultados
        if (resultsPanel != null && scoreManager != null)
        {
            int score = scoreManager.CurrentScore;
            yield return StartCoroutine(resultsPanel.Show(score, resultsHoldSeconds));
        }
        else
        {
            // si falta algo, al menos esperá un poco para que se vea la cortina
            yield return new WaitForSecondsRealtime(1.0f);
        }

        // 2) Volver al menú principal
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void AdjustTime(float deltaSeconds)
    {
        currentTime = Mathf.Clamp(currentTime + deltaSeconds, 0f, startTime);
        int seconds = Mathf.CeilToInt(currentTime);
        if (timerText) timerText.text = seconds.ToString();

        if (seconds <= 5)
        {
            if (timerText) timerText.color = lowTimeColor;
        }
        else
        {
            if (timerText) timerText.color = normalTimerColor;
        }
    }

}//Atrás choclo de texto!!!
