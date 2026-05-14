using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponData[] possibleWeapons;

    private WeaponData selectedWeapon;

    void Start()
    {
        // Initial random weapon for visual/debug
        if (possibleWeapons.Length > 0)
        {
            selectedWeapon =
                possibleWeapons[
                    Random.Range(0, possibleWeapons.Length)
                ];

            Debug.Log("Spawned weapon: " + selectedWeapon.weaponName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger hit: " + other.name);

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
            "Equipping weapon: " +
            selectedWeapon.weaponName
        );

        combat.ReplaceWeapon(selectedWeapon);

        Destroy(gameObject);
    }
}