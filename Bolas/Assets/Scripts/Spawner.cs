using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject safeTargetPrefab;
    public GameObject dangerousTargetPrefab;

    [Header("Spawn Rows (Y)")]
    public float topRowY = 2.5f;
    public float middleRowY = 0f;
    public float bottomRowY = -2.5f;

    [Header("Difficulty Ramp (lineal)")]
    [Tooltip("Intervalo al comienzo (más alto = más lento)")]
    public float intervalAtStart = 2.5f;
    [Tooltip("Intervalo al final (más bajo = más rápido)")]
    public float intervalAtEnd = 0.8f;

    [Range(0, 100)]
    [Tooltip("Porcentaje de peligrosos al comienzo")]
    public int dangerStartPercent = 10;
    [Range(0, 100)]
    [Tooltip("Porcentaje de peligrosos al final")]
    public int dangerEndPercent = 60;

    [Header("Control")]
    [Tooltip("Si está activo arranca solo con intervalAtStart y sin ramp (no recomendado si usas GameManager)")]
    public bool autoStart = false;

    private int lastRow = -1;

    // Estado de la rampa
    private bool _running = false;
    private float _totalDuration = 30f;   // lo setea GameManager
    private float _startTime;             // Time.time al iniciar
    private Coroutine _loop;

    void Start()
    {
        if (autoStart)
        {
            // Si autoStart, asumimos una duración por defecto para la rampa
            StartSpawning(_totalDuration);
        }
    }

    /// <summary>
    /// Llamado por GameManager: pasarle la duracion total de la partida (startTime)
    /// </summary>
    public void StartSpawning(float totalDuration)
    {
        _totalDuration = Mathf.Max(0.01f, totalDuration);
        if (_loop != null) StopCoroutine(_loop);
        _running = true;
        _startTime = Time.time;
        _loop = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        _running = false;
        if (_loop != null)
        {
            StopCoroutine(_loop);
            _loop = null;
        }
    }

    IEnumerator SpawnLoop()
    {
        // Programación de spawns por tiempo “objetivo” para que la rampa sea precisa
        float nextSpawnTime = Time.time;

        while (_running)
        {
            // Progreso normalizado 0..1 basado en tiempo transcurrido / duracion total
            float elapsed = Time.time - _startTime;
            float progress = Mathf.Clamp01(elapsed / _totalDuration);

            // Lerp lineal de dificultad:
            float currentInterval = Mathf.Lerp(intervalAtStart, intervalAtEnd, progress);
            int currentDanger = Mathf.RoundToInt(Mathf.Lerp(dangerStartPercent, dangerEndPercent, progress));

            // Hacemos el spawn con el danger actual
            SpawnTarget(currentDanger);

            // Agendamos el próximo spawn según el intervalo vigente
            nextSpawnTime += Mathf.Max(0.01f, currentInterval);
            float wait = Mathf.Max(0f, nextSpawnTime - Time.time);
            yield return new WaitForSeconds(wait);
        }
    }

    void SpawnTarget(int dangerPercentageNow)
    {
        // Elegimos fila distinta a la última
        int row;
        do { row = Random.Range(0, 3); } while (row == lastRow);
        lastRow = row;

        Vector3 spawnPos;
        Vector3 direction;

        if (row == 0) // top -> derecha
        {
            spawnPos = new Vector3(-10f, topRowY, 0f);
            direction = Vector3.right;
        }
        else if (row == 1) // middle <- izquierda
        {
            spawnPos = new Vector3(10f, middleRowY, 0f);
            direction = Vector3.left;
        }
        else // bottom -> derecha
        {
            spawnPos = new Vector3(-10f, bottomRowY, 0f);
            direction = Vector3.right;
        }

        // Decide safe / dangerous según el % actual
        GameObject prefabToSpawn;
        int roll = Random.Range(0, 100);

        if (roll < dangerPercentageNow && dangerousTargetPrefab != null)
            prefabToSpawn = dangerousTargetPrefab;
        else
            prefabToSpawn = safeTargetPrefab;

        // Instanciar y setear dirección
        GameObject newTarget = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        var move = newTarget.GetComponent<TargetMove>();
        if (move != null) move.SetDirection(direction);
    }
}
