using UnityEngine;

public class TargetMove : MonoBehaviour
{
    public bool gameOver = false;

    public float speed = 2f;
    private Vector3 direction = Vector3.right;
    private void Start()
    {
        gameOver = false;
    }

    void Update()
    {
        if (gameOver == false)
        {
            transform.Translate(direction * speed * Time.deltaTime);

            // Destroy when off screen
            if (transform.position.x > 12f || transform.position.x < -12f)
            {
                Destroy(gameObject);
            }
        }
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir;
    }
}


