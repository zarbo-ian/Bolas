using UnityEngine;
using TMPro;
using System.Collections;

[DisallowMultipleComponent]
public class UIFeedback : MonoBehaviour
{
    public RectTransform target;
    public bool useUnscaledTime = false;

    private Vector3 baseScale;
    private Vector3 basePos;
    private Quaternion baseRot;

    private Coroutine running;

    void Awake()
    {
        if (!target) target = transform as RectTransform;
        baseScale = target.localScale;
        basePos = target.anchoredPosition3D;
        baseRot = target.localRotation;
    }

    public void ResetTRS()
    {
        if (running != null) StopCoroutine(running);
        target.localScale = baseScale;
        target.anchoredPosition3D = basePos;
        target.localRotation = baseRot;
        running = null;
    }

    //Crece un poco y rota un poco, vuelve suave al inicio.
    public void PunchScaleRotate(float scaleAmount = 0.08f, float rotZ = 6f, float duration = 0.18f)
    {
        StartRoutine(CoPunch(scaleAmount, rotZ, duration));
    }

    //Sacudida sutil en posición, vuelve al centro. amplitude en píxeles.
    public void ShakePosition(float amplitude = 4f, float duration = 0.18f, float frequency = 35f)
    {
        StartRoutine(CoShakePos(amplitude, duration, frequency));
    }

    //Sacudida sutil en rotación Z, vuelve a 0.
    public void ShakeRotation(float degrees = 4f, float duration = 0.18f, float frequency = 35f)
    {
        StartRoutine(CoShakeRot(degrees, duration, frequency));
    }

    void StartRoutine(IEnumerator co)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(co);
    }

    IEnumerator CoPunch(float scaleAmt, float rotZ, float dur)
    {
        float t = 0f;
        Vector3 upScale = baseScale * (1f + scaleAmt);
        Quaternion rotA = Quaternion.Euler(0, 0, rotZ);
        Quaternion rotB = Quaternion.Euler(0, 0, -rotZ * 0.35f); // pequeña contra-oscilación

        // fase up
        while (t < dur * 0.45f)
        {
            t += Delta();
            float u = t / (dur * 0.45f);
            u = Smooth(u);
            target.localScale = Vector3.LerpUnclamped(baseScale, upScale, u);
            target.localRotation = Quaternion.SlerpUnclamped(baseRot, rotA, u);
            yield return null;
        }

        // fase down con contra-oscilación
        float t2 = 0f;
        Vector3 s0 = target.localScale;
        Quaternion r0 = target.localRotation;
        while (t2 < dur * 0.55f)
        {
            t2 += Delta();
            float u = t2 / (dur * 0.55f);
            u = Smooth(u);
            target.localScale = Vector3.LerpUnclamped(s0, baseScale, u);
            // pasa por un pequeño “rebote”
            target.localRotation = Quaternion.SlerpUnclamped(r0, Quaternion.SlerpUnclamped(rotB, baseRot, u), u);
            yield return null;
        }

        ResetTRS();
    }

    IEnumerator CoShakePos(float amp, float dur, float freq)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Delta();
            float decay = 1f - Mathf.Clamp01(t / dur);
            float angle = (useUnscaledTime ? Time.unscaledTime : Time.time) * freq * Mathf.PI * 2f;
            Vector2 dir = new Vector2(Mathf.PerlinNoise(0, angle) - 0.5f, Mathf.PerlinNoise(angle, 0) - 0.5f).normalized;
            target.anchoredPosition3D = basePos + (Vector3)(dir * amp * decay); //Como odio esta mantemática, che
            yield return null;
        }
        ResetTRS();
    }

    IEnumerator CoShakeRot(float deg, float dur, float freq)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Delta();
            float decay = 1f - Mathf.Clamp01(t / dur);
            float angle = Mathf.Sin((useUnscaledTime ? Time.unscaledTime : Time.time) * freq) * deg * decay;
            target.localRotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }
        ResetTRS();
    }

    float Delta() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    static float Smooth(float x) => x * x * (3f - 2f * x); // smoothstep
}
