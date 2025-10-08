using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;

[DisallowMultipleComponent]
public class ResultsPanelController : MonoBehaviour
{
    [Header("UI Refs")]
    public RectTransform panel;           // Panel con Image de fondo para el puntaje (RectTransform)
    public TextMeshProUGUI scoreText;     // TMP que muestra el score
    public TextMeshProUGUI messageText;   // TMP para la frase

    [Header("Layout / Positions (Anchored)")]
    public Vector2 offscreenFrom = new Vector2(-1400f, 0f); // desde la izquierda
    public Vector2 centerPos = new Vector2(0f, 0f);      // posición centrada

    [Header("Timings")]
    public float slideDuration = 0.45f;     // entrada del panel
    public float messagePopDuration = 0.28f;// escalado del mensaje
    public float fadeOutDuration = 0.35f;   // desvanecer al final
    public bool useUnscaledTime = true;     // para que no dependa del timescale

    [Header("Message Pop")]
    public float messageStartScale = 0.02f; // arranca casi invisible
    public float messageEndScale = 1.0f;  // escala final

    [Header("Ranges")]
    public int lowMax = 4;   // 0..lowMax  => bajo
    public int midMax = 10;  // lowMax+1..midMax => medio ; > midMax => alto
    [TextArea] public string[] lowMessages = new[] { "Mejor suerte la próxima", "¡Seguí intentando!", "Cada intento suma" };
    [TextArea] public string[] midMessages = new[] { "¡Nada mal!", "Vas por buen camino", "¡Seguí así!" };
    [TextArea] public string[] highMessages = new[] { "¡Muy bien hecho!", "¡Excelente!", "¡Crack total!" };

    [Header("Optional")]
    public CanvasGroup canvasGroup;

    Coroutine running;

    void Awake()
    {
        if (!panel) panel = transform as RectTransform;
        if (!canvasGroup)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f; // oculto al inicio
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public IEnumerator Show(int score, float holdSeconds)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(CoShow(score, holdSeconds));
        yield return running;
        running = null;
    }

    IEnumerator CoShow(int score, float holdSeconds)
    {
        // Preparación
        if (scoreText) scoreText.text = score.ToString();
        if (messageText)
        {
            messageText.text = PickMessage(score);
            messageText.rectTransform.localScale = Vector3.one * messageStartScale;
        }

        panel.anchoredPosition = offscreenFrom;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        // 1) Slide in del panel
        yield return LerpAnchored(panel, offscreenFrom, centerPos, slideDuration);

        // 2) Pop del mensaje
        if (messageText)
            yield return ScalePop(messageText.rectTransform, messageStartScale, messageEndScale, messagePopDuration);

        // 3) Mantener en pantalla
        yield return Wait(holdSeconds);

        // 4) Fade out de todo
        yield return Fade(canvasGroup, 1f, 0f, fadeOutDuration);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    string PickMessage(int score)
    {
        if (score <= lowMax)
            return RandomFrom(lowMessages, "Mejor suerte la próxima");
        if (score <= midMax)
            return RandomFrom(midMessages, "¡Nada mal!");
        return RandomFrom(highMessages, "¡Muy bien hecho!");
    }

    string RandomFrom(string[] arr, string fallback)
    {
        if (arr != null && arr.Length > 0)
            return arr[Random.Range(0, arr.Length)];
        return fallback;
    }

    // Helpers de animación

    IEnumerator LerpAnchored(RectTransform rt, Vector2 from, Vector2 to, float dur)
    {
        if (dur <= 0f) { rt.anchoredPosition = to; yield break; }
        float t = 0f;
        while (t < 1f)
        {
            t += Delta() / dur;
            float u = Smooth(t);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, u);
            yield return null;
        }
        rt.anchoredPosition = to;
    }

    IEnumerator ScalePop(RectTransform rt, float from, float to, float dur)
    {
        if (dur <= 0f) { rt.localScale = Vector3.one * to; yield break; }
        float t = 0f;
        Vector3 a = Vector3.one * from;
        Vector3 b = Vector3.one * to;
        while (t < 1f)
        {
            t += Delta() / dur;
            float u = OvershootEase(u: Smooth(t)); // leve rebote agradable
            rt.localScale = Vector3.LerpUnclamped(a, b, u);
            yield return null;
        }
        rt.localScale = b;
    }

    IEnumerator Fade(CanvasGroup cg, float from, float to, float dur)
    {
        if (!cg) yield break;
        if (dur <= 0f) { cg.alpha = to; yield break; }
        float t = 0f;
        cg.alpha = from;
        while (t < 1f)
        {
            t += Delta() / dur;
            float u = Smooth(t);
            cg.alpha = Mathf.LerpUnclamped(from, to, u);
            yield return null;
        }
        cg.alpha = to;
    }

    float Delta() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    static float Smooth(float x) => Mathf.Clamp01(x * x * (3f - 2f * x));
    static float OvershootEase(float u)
    {
        // pequeña sobre-extensión (sutil). Cambiá 1.05f si querés más/menos rebote
        return Mathf.LerpUnclamped(0f, 1.05f, u);
    }

    IEnumerator Wait(float seconds)
    {
        if (seconds <= 0f) yield break;
        if (useUnscaledTime) yield return new WaitForSecondsRealtime(seconds);
        else yield return new WaitForSeconds(seconds);
    }
}
