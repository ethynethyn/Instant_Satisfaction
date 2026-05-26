using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Lives")]
    public int maxLives = 3;
    [HideInInspector] public int currentLives;

    [Header("Health")]
    [HideInInspector] public int currentHealth = 0;

    private PlayerWallet wallet;

    void Awake()
    {
        wallet = GetComponent<PlayerWallet>();
    }

    void Start()
    {
        ApplyLives();
    }

    public void ApplyLives()
    {
        currentLives = maxLives;
    }

    public void ApplyHealthFromWallet()
    {
        if (wallet != null)
            currentHealth = wallet.walletAmount;
        else
            currentHealth = 100;

        if (currentHealth <= 0)
            currentHealth = 50;

        if (wallet != null)
            wallet.UpdateBalanceFromHealth(currentHealth);
    }

    public void TakeHit(Vector2 knockbackForce, int damage = 0)
    {
        int oldHealth  = currentHealth;
        currentHealth -= damage;

        if (rb != null && knockbackForce != Vector2.zero)
            rb.AddForce(knockbackForce, ForceMode2D.Impulse);

        if (wallet != null)
            wallet.UpdateBalanceFromHealth(currentHealth, oldHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    // Called by wallet effects that bypass normal damage
    public void ForceDie()
    {
        currentHealth = 0;
        Die();
    }

    void Die()
    {
        currentLives--;
        gameObject.SetActive(false);
        GameManager.Instance.OnPlayerDied();
    }

    public void ResetForRound(Vector3 spawnPosition)
    {
        rb.linearVelocity  = Vector2.zero;
        transform.position = spawnPosition;
        gameObject.SetActive(true);

        ApplyHealthFromWallet();
    }
}
