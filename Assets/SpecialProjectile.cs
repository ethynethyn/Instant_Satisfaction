using UnityEngine;

public class SpecialProjectile : MonoBehaviour
{
    public ProjectileEffectData effectData;

    // Set by PlayerCombat when fired — needed for Scam and Swap
    [HideInInspector] public GameObject owner;

    private int     damage;
    private Vector2 direction;
    private float   speed;
    private float   knockback;
    private int     ownerInstanceID;
    private Vector3 startPos;

    public float maxDistance = 10f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector2 dir, float projectileSpeed,
                           float kb, GameObject ownerObject,
                           int projectileDamage)
    {
        direction  = dir.normalized;
        speed      = projectileSpeed;
        knockback  = kb;
        damage     = projectileDamage;
        owner      = ownerObject;

        ownerInstanceID = ownerObject.transform.root
            .gameObject.GetInstanceID();

        startPos = transform.position;

        // Tint the sprite to the effect colour
        if (spriteRenderer != null && effectData != null)
            spriteRenderer.color = effectData.projectileColour;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);

        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        int hitRootID =
            other.transform.root.gameObject.GetInstanceID();

        if (hitRootID == ownerInstanceID) return;

        PlayerHealth recipientHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (recipientHealth == null)
            recipientHealth = other.transform.root
                .GetComponentInChildren<PlayerHealth>();

        if (recipientHealth == null)
        {
            Destroy(gameObject);
            return;
        }

        PlayerWallet recipientWallet =
            recipientHealth.GetComponent<PlayerWallet>();

        SpawnHitParticle();
        ApplyKnockback(recipientHealth);

        if (effectData != null)
        {
            switch (effectData.effectType)
            {
                case ProjectileEffectType.FreezeAccount:
                    ApplyFreeze(recipientWallet);
                    break;

                case ProjectileEffectType.Tax:
                    ApplyTax(recipientHealth, recipientWallet);
                    break;

                case ProjectileEffectType.Scam:
                    ApplyScam(recipientHealth, recipientWallet);
                    break;

                case ProjectileEffectType.SwapWallet:
                    ApplySwap(recipientHealth);
                    break;
            }
        }

        Destroy(gameObject);
    }

    // ─────────────────────────────────────────────────────────
    void ApplyFreeze(PlayerWallet recipientWallet)
    {
        if (recipientWallet == null) return;

        recipientWallet.FreezeAccount(effectData.freezeDuration);

        Debug.Log("FREEZE applied to "
            + recipientWallet.gameObject.name
            + " for " + effectData.freezeDuration + "s");
    }

    void ApplyTax(PlayerHealth recipientHealth,
                  PlayerWallet recipientWallet)
    {
        if (recipientWallet == null) return;

        int taxed = recipientWallet.ApplyTax(
            effectData.taxPercentage, recipientHealth);

        Debug.Log("TAX applied: -$" + taxed
            + " from " + recipientHealth.gameObject.name);
    }

    void ApplyScam(PlayerHealth recipientHealth,
                   PlayerWallet recipientWallet)
    {
        if (recipientWallet == null || owner == null) return;

        // Deal normal damage first
        int damageToDeal = Mathf.Min(damage, recipientHealth.currentHealth);

        recipientHealth.TakeHit(Vector2.zero, damage);

        // Harvest a portion of the damage dealt into the shooter's bank
        int harvest = Mathf.FloorToInt(
            damageToDeal * effectData.scamHarvestRate);

        PlayerWallet ownerWallet =
            owner.GetComponentInParent<PlayerWallet>();

        if (ownerWallet == null)
            ownerWallet = owner.transform.root
                .GetComponentInChildren<PlayerWallet>();

        if (ownerWallet != null)
        {
            ownerWallet.bankAmount += harvest;
            ownerWallet.RefreshBalanceDisplay();

            Debug.Log("SCAM: harvested $" + harvest
                + " for " + owner.name);
        }

        return; // TakeHit already handled death check
    }

    void ApplySwap(PlayerHealth recipientHealth)
    {
        if (owner == null) return;

        PlayerHealth ownerHealth =
            owner.GetComponentInParent<PlayerHealth>();

        if (ownerHealth == null)
            ownerHealth = owner.transform.root
                .GetComponentInChildren<PlayerHealth>();

        if (ownerHealth == null) return;

        Debug.Log("SWAP: "
            + ownerHealth.gameObject.name
            + " ($" + ownerHealth.currentHealth + ") <-> "
            + recipientHealth.gameObject.name
            + " ($" + recipientHealth.currentHealth + ")");

        PlayerWallet.SwapHealth(ownerHealth, recipientHealth);
    }

    // ─────────────────────────────────────────────────────────
    void ApplyKnockback(PlayerHealth health)
    {
        if (health.rb != null && knockback > 0f)
            health.rb.AddForce(
                direction * knockback, ForceMode2D.Impulse);
    }

    void SpawnHitParticle()
    {
        if (effectData == null ||
            effectData.hitParticlePrefab == null) return;

        GameObject p = Instantiate(
            effectData.hitParticlePrefab,
            transform.position,
            Quaternion.identity);

        Destroy(p, 3f);
    }
}