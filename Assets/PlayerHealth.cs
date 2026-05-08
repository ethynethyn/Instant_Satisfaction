using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public Rigidbody2D rb;

    public int maxLives = 3;

    [HideInInspector] public int currentLives;

    void Start()
    {
        ApplyLives();
    }

    public void ApplyLives()
    {
        currentLives = maxLives;
    }

    public void TakeHit(Vector2 knockbackForce)
    {
        currentLives--;

        gameObject.SetActive(false);

        GameManager.Instance.OnPlayerDied();
    }

    public void ResetForRound(Vector3 spawnPosition)
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = spawnPosition;
        gameObject.SetActive(true);
    }
}