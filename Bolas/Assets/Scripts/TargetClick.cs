using UnityEngine;

public class TargetClick : MonoBehaviour
{
    public bool isDangerous = false;

    void OnMouseDown()
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

