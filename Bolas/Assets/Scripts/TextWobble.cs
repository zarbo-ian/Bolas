using UnityEngine;
using TMPro;
public class TextWobble : MonoBehaviour
{
    // Configuraciones básicas
    public float rotationAmount = 5f;       // Grados de rotación máximos
    public float rotationSpeed = 2f;        // Velocidad del cambio de rotación
    public float scaleAmount = 0.1f;        // Variación máxima de escala
    public float scaleSpeed = 2f;           // Velocidad del pulso

    private TextMeshProUGUI tmp;
    private Vector3 originalScale;
    private float randomOffsetRot;
    private float randomOffsetScale;

    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        originalScale = transform.localScale;

        // Pequeños offsets para que distintos textos no se sincronicen
        randomOffsetRot = Random.Range(0f, 100f);
        randomOffsetScale = Random.Range(0f, 100f);
    }

    void Update()
    {
        // Rotación oscilante
        float rotationZ = Mathf.Sin(Time.time * rotationSpeed + randomOffsetRot) * rotationAmount;

        // Escala oscilante
        float scaleFactor = 1f + Mathf.Sin(Time.time * scaleSpeed + randomOffsetScale) * scaleAmount;

        // Aplicar transformaciones sin mover la posición
        transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        transform.localScale = originalScale * scaleFactor;
    }
}

