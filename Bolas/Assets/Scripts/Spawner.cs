using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject safeTargetPrefab;
    public GameObject dangerousTargetPrefab;

    [Range(0, 100)]
    public int dangerPercentage = 20; // % chance a target is dangerous

    public float spawnInterval = 2f;

    // Y positions for the three rows
    public float topRowY = 2.5f;
    public float middleRowY = 0f;
    public float bottomRowY = -2.5f;

    private int lastRow = -1; // no row yet

    void Start()
    {
        InvokeRepeating(nameof(SpawnTarget), 1f, spawnInterval);
    }

    void SpawnTarget()
    {
        // pick a row different from lastRow
        int row;
        do
        {
            row = Random.Range(0, 3);
        } while (row == lastRow);

        lastRow = row;

        Vector3 spawnPos;
        Vector3 direction;

        if (row == 0) // top row -> right
        {
            spawnPos = new Vector3(-10f, topRowY, 0f);
            direction = Vector3.right;
        }
        else if (row == 1) // middle row <- left
        {
            spawnPos = new Vector3(10f, middleRowY, 0f);
            direction = Vector3.left;
        }
        else // bottom row -> right
        {
            spawnPos = new Vector3(-10f, bottomRowY, 0f);
            direction = Vector3.right;
        }

        // Decide safe or dangerous
        GameObject prefabToSpawn;
        int roll = Random.Range(0, 100);

        if (roll < dangerPercentage && dangerousTargetPrefab != null)
            prefabToSpawn = dangerousTargetPrefab;
        else
            prefabToSpawn = safeTargetPrefab;

        // Create the target
        GameObject newTarget = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // Tell it which way to move
        TargetMove move = newTarget.GetComponent<TargetMove>();
        move.SetDirection(direction);
    }
}