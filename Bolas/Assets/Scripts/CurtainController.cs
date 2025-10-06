using System;
using System.Collections;
using UnityEngine;

public class CurtainController : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform curtain;         // La imagen UI de la cortina
    public CanvasGroup curtainCanvasGroup; // Opcional: bloquea raycasts al inicio
    //public Canvas curtainCanvasGroup;

    [Header("Positions (UI Anchored)")]
    public Vector2 downAnchorPos;         // Cortina abajo (tapando)
    public Vector2 upAnchorPos;           // Cortina arriba (descubierto)

    [Header("Anim")]
    public float duration = 1.0f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool startDown = true;

    public event Action OnOpened;
    public event Action OnClosed;

    Coroutine current;

    void Awake()
    {
        if (curtain == null) curtain = GetComponent<RectTransform>();
        if (startDown)
        {
            curtain.anchoredPosition = downAnchorPos;
            if (curtainCanvasGroup)
            {
                curtainCanvasGroup.blocksRaycasts = true;  // bloquea clics
                curtainCanvasGroup.interactable = true;
            }
        }
        else
        {
            curtain.anchoredPosition = upAnchorPos;
            if (curtainCanvasGroup)
            {
                curtainCanvasGroup.blocksRaycasts = false;
                curtainCanvasGroup.interactable = false;
            }
        }
    }

    public void Open()
    {
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(Animate(downAnchorPos, upAnchorPos, () =>
        {
            if (curtainCanvasGroup)
            {
                curtainCanvasGroup.blocksRaycasts = false;
                curtainCanvasGroup.interactable = false;
            }
            OnOpened?.Invoke();
        }));
    }

    public void Close()
    {
        if (current != null) StopCoroutine(current);
        if (curtainCanvasGroup)
        {
            curtainCanvasGroup.blocksRaycasts = true;
            curtainCanvasGroup.interactable = true;
        }
        current = StartCoroutine(Animate(upAnchorPos, downAnchorPos, () =>
        {
            OnClosed?.Invoke();
        }));
    }

    IEnumerator Animate(Vector2 from, Vector2 to, Action done)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration; // no depende de Time.timeScale
            float e = curve.Evaluate(Mathf.Clamp01(t));
            curtain.anchoredPosition = Vector2.LerpUnclamped(from, to, e);
            yield return null;
        }
        curtain.anchoredPosition = to;
        done?.Invoke();
    }
}
