using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI - In Game Timer")]
    public TextMeshProUGUI timerText;
    public float startTime = 30f;
    private float currentTime;

    private bool gameActive = false;

    [Header("Curtain / Overlay")]
    public RectTransform endScreenTexture; // la misma cortina para inicio y fin
    public Vector2 curtainDownPos = new Vector2(0, 0);     // tapando pantalla
    public Vector2 curtainUpPos = new Vector2(0, 1080f); // fuera hacia arriba (ajustá según Canvas)

    [Header("Countdown Inicial")]
    public TextMeshProUGUI pregameCountdownText;
    public int pregameCountdownFrom = 3;
    public float pregameLastWordHold = 0.5f; // cuánto dura "¡YA!"

    [Header("Gameplay")]
    public Spawner spawner;

    public TargetMove targetMove;
    public TargetClick targetClick;
    public TargetMove targetMoveDanger;
    public TargetClick targetClickDanger;

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

        // Asegurar spawner detenido hasta que arranque el juego
        if (spawner != null)
            spawner.StopSpawning();

        // Ocultar timer in-game si querés hasta que comience
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(startTime).ToString();

        // Mostrar countdown y correr pre-game
        if (pregameCountdownText != null)
            pregameCountdownText.gameObject.SetActive(true);

        StartCoroutine(PreGameSequence());
    }

    void Update()
    {
        if (!gameActive) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            EndGame();
        }

        int seconds = Mathf.CeilToInt(currentTime);
        if (timerText != null)
            timerText.text = seconds.ToString();
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
            spawner.StartSpawning();

        // Si querés resetear flags/estado por las dudas:
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

        // Brute force flags que ya tenías
        if (targetMove != null) targetMove.gameOver = true;
        if (targetClick != null) targetClick.gameOver = true;
        if (targetMoveDanger != null) targetMoveDanger.gameOver = true;
        if (targetClickDanger != null) targetClickDanger.gameOver = true;

        // Bajar cortina al final (tu lógica original)
        StartCoroutine(ShowEndScreen());
    }

    IEnumerator ShowEndScreen()
    {
        yield return new WaitForSeconds(0.5f);

        if (endScreenTexture != null)
        {
            endScreenTexture.gameObject.SetActive(true);

            // Animar bajando desde arriba a centro
            Vector2 startPos = curtainUpPos;
            Vector2 endPos = curtainDownPos;
            float duration = 1f;
            yield return StartCoroutine(AnimateCurtain(endScreenTexture, startPos, endPos, duration));
        }
    }
}
