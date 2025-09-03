using UnityEngine;

public class TargetMove : MonoBehaviour
{
    public float speed = 2f;
    private Vector3 direction = Vector3.right;

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);

        // Destroy when off screen
        if (transform.position.x > 12f || transform.position.x < -12f)
        {
            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir;
    }
}


