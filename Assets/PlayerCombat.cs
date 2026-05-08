using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat")]
    public Transform firePoint;
    public WeaponData currentWeapon;
    public PlayerDebugText debugText;

    [Header("Weapon Visual")]
    public Transform weaponHolder;

    private GameObject currentWeaponVisual;

    private PlayerController2D controller;
    private WeaponData startingWeapon;

    private bool canShoot = true;
    private bool reloading = false;
    private bool isFiring = false;

    private int currentAmmo;

    void Awake()
    {
        startingWeapon = currentWeapon;
    }

    void Start()
    {
        controller = GetComponent<PlayerController2D>();

        if (currentWeapon != null)
        {
            currentAmmo = currentWeapon.magazineSize;

            if (debugText != null)
                debugText.SetWeapon(currentWeapon.weaponName);
        }

        UpdateWeaponVisual();
    }

    void Update()
    {
        if (currentWeapon != null)
            UpdateFireUI();
    }

    public void TryShoot(bool held)
    {
        if (currentWeapon == null)
            return;

        bool shouldFire =
            currentWeapon.automatic
            ? held
            : !held;

        if (shouldFire)
            Attack();
    }

    void UpdateFireUI()
    {
        if (debugText != null)
            debugText.SetFiring(isFiring);
    }

    public void ReplaceWeapon(WeaponData weapon)
    {
        currentWeapon = weapon;

        currentAmmo = weapon.magazineSize;

        reloading = false;
        isFiring = false;
        canShoot = true;

        if (debugText != null)
            debugText.SetWeapon(weapon.weaponName);

        UpdateWeaponVisual();
    }

    public void ResetForRound()
    {
        currentWeapon = startingWeapon;

        reloading = false;
        isFiring = false;
        canShoot = true;

        if (currentWeapon != null)
        {
            currentAmmo = currentWeapon.magazineSize;

            if (debugText != null)
                debugText.SetWeapon(currentWeapon.weaponName);
        }
        else
        {
            currentAmmo = 0;

            if (debugText != null)
                debugText.SetWeapon("Unarmed");
        }

        if (debugText != null)
        {
            debugText.SetFiring(false);
            debugText.SetReloading(false);
        }

        UpdateWeaponVisual();
    }

    void Attack()
    {
        if (!canShoot || reloading)
            return;

        StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        canShoot = false;
        isFiring = true;

        Vector2 direction =
            controller.facingRight
            ? Vector2.right
            : Vector2.left;

        GameObject bullet = Instantiate(
            currentWeapon.projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        Projectile projectile =
            bullet.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                direction,
                currentWeapon.projectileSpeed,
                currentWeapon.knockbackForce,
                gameObject
            );
        }

        // Ignore collisions with owner
        foreach (Collider2D ownerCol in GetComponentsInChildren<Collider2D>())
        {
            Collider2D bulletCol =
                bullet.GetComponent<Collider2D>();

            if (bulletCol != null)
                Physics2D.IgnoreCollision(
                    bulletCol,
                    ownerCol
                );
        }

        currentAmmo--;

        isFiring = false;

        // Reload
        if (currentAmmo <= 0)
        {
            reloading = true;

            if (debugText != null)
                debugText.SetReloading(true);

            yield return new WaitForSeconds(
                currentWeapon.reloadTime
            );

            currentAmmo =
                currentWeapon.magazineSize;

            reloading = false;

            if (debugText != null)
                debugText.SetReloading(false);
        }

        yield return new WaitForSeconds(
            currentWeapon.fireRate
        );

        canShoot = true;
    }

    void UpdateWeaponVisual()
    {
        // Destroy previous visual
        if (currentWeaponVisual != null)
            Destroy(currentWeaponVisual);

        // Safety checks
        if (currentWeapon == null)
            return;

        if (currentWeapon.weaponSprite == null)
            return;

        if (weaponHolder == null)
        {
            Debug.LogWarning(
                "Weapon Holder missing on: " + gameObject.name
            );

            return;
        }

        // Create visual object
        currentWeaponVisual =
            new GameObject("WeaponVisual");

        currentWeaponVisual.transform.SetParent(
            weaponHolder
        );

        currentWeaponVisual.transform.localPosition =
            currentWeapon.localPosition;

        currentWeaponVisual.transform.localEulerAngles =
            currentWeapon.localRotation;

        currentWeaponVisual.transform.localScale =
            currentWeapon.localScale;

        SpriteRenderer sr =
            currentWeaponVisual.AddComponent<SpriteRenderer>();

        sr.sprite = currentWeapon.weaponSprite;

        // Optional sorting setup
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 10;
    }
}