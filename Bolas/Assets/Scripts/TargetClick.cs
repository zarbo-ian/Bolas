using UnityEngine;

public class TargetClick : MonoBehaviour
{
    public bool isDangerous = false;
    public ScoreManager scoreManager;

    public bool gameOver = false;
    void Start()
    {
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();
    }

    void OnMouseDown()
    {
        if (gameOver) return;

        if (scoreManager != null)
        {
            if (isDangerous)
                scoreManager.OnDangerHit(); //ඞ
            else
                scoreManager.OnSafeHit();
        }

        Destroy(gameObject);
    }
}
