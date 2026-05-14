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
    [Header("Ricochet")]
    public int ricochetCount = 0;

    [Header("Recoil")]
    public float recoilForce = 5f;

    [Header("Spread")]
    public float spreadAngle = 0f;

    [Header("Projectiles")]
    public int projectileCount = 1;

    [Header("Magazine")]
    public int magazineSize = 6;
    public float reloadTime = 1f;

    [Header("Firing Mode")]
    public bool automatic = true;

    [Header("Camera Shake")]
    public float screenShake = 0.2f;

    [Header("FX")]
    public float muzzleFlashOffsetX = 0.2f;

    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    [Range(0f, 1f)] public float fireVolume = 1f;
    [Range(0f, 1f)] public float reloadVolume = 1f;
}