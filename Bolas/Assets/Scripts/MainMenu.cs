using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Button Fade")]
    public CanvasGroup buttonGroup;      // CanvasGroup del botón o su contenedor
    public float buttonDelay = 0.2f;
    public float buttonFadeDuration = 0.5f;

    [Header("Text Intro (Scale-in)")]
    public RectTransform textIntroParent; // Padre que escalará de chiquito a normal
    public float textDelay = 0.0f;
    public float textIntroDuration = 0.45f;
    public float textStartScale = 0.02f;
    public float textEndScale = 1f;

    [Header("Timing")]
    public bool useUnscaledTime = true; // si querés que ignore timeScale

    void Start()
    {
        // Estados iniciales
        if (buttonGroup)
        {
            buttonGroup.alpha = 0f;
            buttonGroup.interactable = false;
            buttonGroup.blocksRaycasts = false;
        }

        if (textIntroParent)
            textIntroParent.localScale = Vector3.one * textStartScale;

        // Lanzar animaciones
        if (buttonGroup) StartCoroutine(FadeInButton());
        if (textIntroParent) StartCoroutine(ScaleInText());
    }

    IEnumerator FadeInButton()
    {
        if (buttonDelay > 0) yield return Wait(buttonDelay);

        float t = 0f;
        while (t < buttonFadeDuration)
        {
            t += Delta();
            float u = Mathf.Clamp01(t / buttonFadeDuration);
            u = Smooth(u);
            buttonGroup.alpha = Mathf.Lerp(0f, 1f, u);
            yield return null;
        }
        buttonGroup.alpha = 1f;
        buttonGroup.interactable = true;
        buttonGroup.blocksRaycasts = true;
    }

    IEnumerator ScaleInText()
    {
        if (textDelay > 0) yield return Wait(textDelay);

        float t = 0f;
        Vector3 a = Vector3.one * textStartScale;
        Vector3 b = Vector3.one * textEndScale;

        while (t < textIntroDuration)
        {
            t += Delta();
            float u = Mathf.Clamp01(t / textIntroDuration);
            // leve ease con micro-overshoot para que se sienta vivo (opcional)
            float s = OvershootEase(Smooth(u), 1.04f);
            textIntroParent.localScale = Vector3.LerpUnclamped(a, b, s);
            yield return null;
        }
        textIntroParent.localScale = b;
    }

    float Delta() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    WaitForSecondsRealtime Wait(float s) => new WaitForSecondsRealtime(s);
    static float Smooth(float x) => x * x * (3f - 2f * x); // smoothstep
    static float OvershootEase(float u, float overshoot = 1.05f)
    {
        // interpola un pelín más allá de 1 y vuelve (muy sutil)
        return Mathf.LerpUnclamped(0f, overshoot, u);
    }
    public void PlayGame()
    {
        Debug.Log("Play button pressed!");
        SceneManager.LoadScene("GameScene");
    }
}



