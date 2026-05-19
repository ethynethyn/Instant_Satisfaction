using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponData[] possibleWeapons;

    private WeaponData selectedWeapon;

    void Start()
    {
        // Destroy self if another pickup already exists at this spawn point
        Collider2D[] overlaps =
            Physics2D.OverlapCircleAll(
                transform.position,
                0.1f
            );

        foreach (Collider2D col in overlaps)
        {
            if (col.gameObject != gameObject &&
                col.GetComponent<WeaponPickup>() != null)
            {
                Destroy(gameObject);
                return;
            }
        }

        // Initial random weapon for visual/debug
        if (possibleWeapons.Length > 0)
        {
            selectedWeapon =
                possibleWeapons[
                    Random.Range(
                        0,
                        possibleWeapons.Length
                    )
                ];

            Debug.Log(
                "Spawned weapon: "
                + selectedWeapon.weaponName
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsRoundOver) return;
        Debug.Log(
            "Trigger hit: "
            + other.name
        );

        PlayerCombat combat =
            other.GetComponentInParent<PlayerCombat>();

        if (combat == null)
        {
            Debug.LogError(
                "PlayerCombat NOT FOUND on player or parent"
            );

            return;
        }

        // No weapons available
        if (possibleWeapons.Length == 0)
            return;

        WeaponData currentWeapon =
            combat.currentWeapon;

        // If only one weapon exists, use it
        if (possibleWeapons.Length == 1)
        {
            selectedWeapon = possibleWeapons[0];
        }
        else
        {
            // Keep rerolling until different
            do
            {
                selectedWeapon =
                    possibleWeapons[
                        Random.Range(
                            0,
                            possibleWeapons.Length
                        )
                    ];
            }
            while (selectedWeapon == currentWeapon);
        }

        Debug.Log(
            "Equipping weapon: "
            + selectedWeapon.weaponName
        );

        // PLAY PICKUP SOUND
        PlayPickupSound();

        // EQUIP WEAPON
        combat.ReplaceWeapon(selectedWeapon);

        Destroy(gameObject);
    }

    void PlayPickupSound()
    {
        if (selectedWeapon == null)
            return;

        if (selectedWeapon.pickupSound == null)
            return;

        GameObject tempAudio =
            new GameObject("WeaponPickupAudio");

        tempAudio.transform.position =
            transform.position;

        AudioSource source =
            tempAudio.AddComponent<AudioSource>();

        source.clip =
            selectedWeapon.pickupSound;

        source.volume =
            selectedWeapon.pickupVolume;

        source.spatialBlend = 0f;

        source.rolloffMode =
            AudioRolloffMode.Linear;

        source.playOnAwake = false;

        source.loop = false;

        source.pitch =
            Random.Range(0.98f, 1.02f);

        source.Play();

        Destroy(
            tempAudio,
            selectedWeapon.pickupSound.length + 0.1f
        );
    }
}