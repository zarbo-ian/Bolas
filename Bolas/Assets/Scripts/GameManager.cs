using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float startTime = 30f;
    private float currentTime;

    private bool gameActive = true;

    public RectTransform endScreenTexture;
    public Spawner spawner;

    public TargetMove targetMove;
    public TargetClick targetClick;
    public TargetMove targetMoveDanger;
    public TargetClick targetClickDanger;

    void Start()
    {
        currentTime = startTime;

        if (endScreenTexture != null)
            endScreenTexture.gameObject.SetActive(false); // hide overlay at start
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
        timerText.text = seconds.ToString();
    }

    void EndGame()
    {
        gameActive = false;

        // Stop new spawns
        if (spawner != null)
            spawner.enabled = false;

        // Halt all target movement
        TargetMove[] targets = FindObjectsOfType<TargetMove>();
        foreach (var t in targets)
            t.enabled = false;

        // Disable clicking
        TargetClick[] clicks = FindObjectsOfType<TargetClick>();
        foreach (var c in clicks)
            c.enabled = false;

        //This is brute forced
        targetMove.gameOver = true;
        targetClick.gameOver = true;
        targetMoveDanger.gameOver = true;
        targetClickDanger.gameOver = true;

        StartCoroutine(ShowEndScreen());
    }

    IEnumerator ShowEndScreen()
    {
        yield return new WaitForSeconds(0.5f);

        if (endScreenTexture != null)
        {
            endScreenTexture.gameObject.SetActive(true);

            // Animate it lowering from top
            Vector2 startPos = new Vector2(0, Screen.height);
            Vector2 endPos = Vector2.zero;
            float duration = 1f;
            float elapsed = 0f;

            endScreenTexture.anchoredPosition = startPos;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                endScreenTexture.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / duration);
                yield return null;
            }

            endScreenTexture.anchoredPosition = endPos;
        }
    }
}
