using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponData[] possibleWeapons;

    private WeaponData selectedWeapon;

    void Start()
    {
        selectedWeapon = possibleWeapons[
            Random.Range(0, possibleWeapons.Length)
        ];

        Debug.Log("Spawned weapon: " + selectedWeapon.weaponName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger hit: " + other.name);

        // 🔥 IMPORTANT FIX: search in parent too
        PlayerCombat combat = other.GetComponentInParent<PlayerCombat>();

        if (combat == null)
        {
            Debug.LogError("PlayerCombat NOT FOUND on player or parent");
            return;
        }

        Debug.Log("Equipping weapon: " + selectedWeapon.weaponName);

        combat.ReplaceWeapon(selectedWeapon);

        Destroy(gameObject);
    }
}