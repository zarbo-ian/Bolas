using UnityEngine;

public class TargetClick : MonoBehaviour
{
    public bool isDangerous = false;
    public bool gameOver;

    private void Start()
    {
        gameOver = false;
    }

    void OnMouseDown()
    {
        if (gameOver == false)
        {
            if (isDangerous)
            {
                // TODO: subtract points / end game / penalty
            }
            else
            {
                // TODO: add points
            }

            Destroy(gameObject);
        }
    }
}

