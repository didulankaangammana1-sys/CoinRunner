using UnityEngine;

/// <summary>Handles pickups, solid hazards, and falling out of the level.</summary>
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerInteractions : MonoBehaviour
{
    [SerializeField] private float fallLimitY = -10f;

    private Rigidbody2D body;
    private Vector3 startPosition;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    private void Update()
    {
        if (transform.position.y < fallLimitY)
            Respawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddCoin();

            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Goal"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.LevelComplete();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Hazard"))
            Respawn();
    }

    private void Respawn()
    {
        transform.position = startPosition;
        body.linearVelocity = Vector2.zero;
    }
}
