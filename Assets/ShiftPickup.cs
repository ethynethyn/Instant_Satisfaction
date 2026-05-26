using UnityEngine;

public class ShiftPickup : MonoBehaviour
{
    public ShiftData shiftData;

    [HideInInspector] public Transform spawnPoint;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (shiftData != null && spriteRenderer != null)
            spriteRenderer.sprite = shiftData.shiftSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsRoundOver) return;

        if (shiftData == null) return;

        Debug.Log("ShiftPickup trigger hit by: " + other.name);

        PlayerWallet wallet =
            other.GetComponentInParent<PlayerWallet>();

        if (wallet == null)
            wallet = other.transform.root
                .GetComponentInChildren<PlayerWallet>();

        if (wallet == null)
        {
            Debug.Log("No PlayerWallet found on: " + other.name);
            return;
        }

        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            health = other.transform.root
                .GetComponentInChildren<PlayerHealth>();

        // Respect freeze — if account is frozen neither
        // bank nor health is updated
        if (wallet.accountFrozen)
        {
            Debug.Log(other.name
                + " tried to collect shift but account is FROZEN");

            PlayPickupSound();

            if (WeaponSpawner.Instance != null)
                WeaponSpawner.Instance.OnPickupCollected(spawnPoint);

            Destroy(gameObject);
            return;
        }

        // Use safe methods that respect freeze state
        wallet.TryAddToBank(shiftData.walletReward);

        if (health != null)
            wallet.TryAddToCurrentHealth(shiftData.walletReward, health);

        Debug.Log(other.name + " completed shift: "
            + shiftData.shiftName
            + " +$" + shiftData.walletReward
            + " | New bank: $" + wallet.bankAmount
            + " | New health: $"
            + (health != null ? health.currentHealth : 0));

        PlayPickupSound();

        if (WeaponSpawner.Instance != null)
            WeaponSpawner.Instance.OnPickupCollected(spawnPoint);

        Destroy(gameObject);
    }

    void PlayPickupSound()
    {
        if (shiftData.pickupSound == null) return;

        GameObject tempAudio = new GameObject("ShiftPickupAudio");
        tempAudio.transform.position = transform.position;

        AudioSource source = tempAudio.AddComponent<AudioSource>();
        source.clip         = shiftData.pickupSound;
        source.volume       = shiftData.pickupVolume;
        source.spatialBlend = 0f;
        source.rolloffMode  = AudioRolloffMode.Linear;
        source.playOnAwake  = false;
        source.loop         = false;
        source.pitch        = Random.Range(0.98f, 1.02f);
        source.Play();

        Destroy(tempAudio, shiftData.pickupSound.length + 0.1f);
    }
}