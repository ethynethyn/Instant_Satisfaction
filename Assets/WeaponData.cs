using UnityEngine;

[CreateAssetMenu(menuName = "Arena Fighter/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;

    [Header("Visuals")]
    public Sprite weaponSprite;

    public Vector3 localPosition;
    public Vector3 localRotation;
    public Vector3 localScale = Vector3.one;

    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Combat")]
    public float fireRate = 0.2f;
    public float projectileSpeed = 20f;
    public float knockbackForce = 10f;

    [Header("Magazine")]
    public int magazineSize = 6;
    public float reloadTime = 1f;

    [Header("Firing Mode")]
    public bool automatic = true;
}