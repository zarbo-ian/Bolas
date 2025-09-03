using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float startTime = 30f;     // seconds, adjustable
    private float currentTime;

    private bool gameActive = true;

    public GameObject endScreenTexture; // A UI Image/Panel covering the screen

    void Start()
    {
        currentTime = startTime;
        endScreenTexture.SetActive(false); // Hide at start
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

        // Update timer text (format: 00:00)
        int seconds = Mathf.CeilToInt(currentTime);
        timerText.text = seconds.ToString();
    }

    void EndGame()
    {
        gameActive = false;

        // Halt all target movement
        TargetMove[] targets = FindObjectsOfType<TargetMove>();
        foreach (var t in targets)
        {
            t.enabled = false; // stop movement
        }

        // Also prevent further clicking
        TargetClick[] clicks = FindObjectsOfType<TargetClick>();
        foreach (var c in clicks)
        {
            c.enabled = false;
        }

        // Wait 2 seconds, then show overlay
        StartCoroutine(ShowEndScreen());
    }

    IEnumerator ShowEndScreen()
    {
        yield return new WaitForSeconds(2f);

        endScreenTexture.SetActive(true); // show second screen
    }
}
