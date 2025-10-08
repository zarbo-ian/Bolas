using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public enum DangerPenaltyMode { LosePoint, LoseTime }

    [Header("UI")]
    public TextMeshProUGUI scoreText;

    [Header("Scoring")]
    public int pointsPerSafeHit = 1;
    public int CurrentScore => _score;

    [Header("Danger Penalty")]
    public DangerPenaltyMode penaltyMode = DangerPenaltyMode.LosePoint;
    public int pointsLostOnDanger = 1;          // usado si LosePoint
    public float secondsLostOnDanger = 2f;      // usado si LoseTime

    [Header("Refs")]
    public GameManager gameManager;

    [Header("FX")]
    public UIFeedback scoreFX;
    public UIFeedback timerFX;

    private int _score = 0;

    void Start()
    {
        UpdateUI();
    }

    public void OnSafeHit()
    {
        _score += pointsPerSafeHit;
        UpdateUI();
        if (scoreFX) scoreFX.PunchScaleRotate(0.08f, 6f, 0.18f);
    }
    public void OnDangerHit()
    {
        if (penaltyMode == DangerPenaltyMode.LosePoint)
        {
            _score = Mathf.Max(0, _score - Mathf.Abs(pointsLostOnDanger));
            UpdateUI();
            if (scoreFX) scoreFX.ShakePosition(8f, 0.18f, 35f);
        }
        else // LoseTime
        {
            if (gameManager != null)
            {
                gameManager.AdjustTime(-Mathf.Abs(secondsLostOnDanger));
                if (timerFX) timerFX.ShakePosition(8f, 0.18f, 35f);
            }
        }
    }
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = _score.ToString();
    }

    public void ResetScore()
    {
        _score = 0;
        UpdateUI();
    }
}
