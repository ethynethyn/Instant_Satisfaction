using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public Transform firePoint;
    public WeaponData currentWeapon;
    public PlayerDebugText debugText;

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
            debugText.SetWeapon(currentWeapon.weaponName);
        }
    }

    void Update()
    {
        if (currentWeapon != null)
            UpdateFireUI();
    }

    public void TryShoot(bool held)
    {
        if (currentWeapon == null) return;
        bool shouldFire = currentWeapon.automatic ? held : !held;
        if (shouldFire) Attack();
    }

    void UpdateFireUI()
    {
        debugText.SetFiring(isFiring);
    }

    public void ReplaceWeapon(WeaponData weapon)
    {
        currentWeapon = weapon;
        currentAmmo = weapon.magazineSize;
        reloading = false;
        isFiring = false;
        canShoot = true;
        debugText.SetWeapon(weapon.weaponName);
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
            debugText.SetWeapon(currentWeapon.weaponName);
        }
        else
        {
            currentAmmo = 0;
            debugText.SetWeapon("Unarmed");
        }

        debugText.SetFiring(false);
        debugText.SetReloading(false);
    }

    void Attack()
    {
        if (!canShoot || reloading) return;
        StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        canShoot = false;
        isFiring = true;

        Vector2 direction = controller.facingRight ? Vector2.right : Vector2.left;

        GameObject bullet = Instantiate(
            currentWeapon.projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        bullet.GetComponent<Projectile>().Initialize(
            direction,
            currentWeapon.projectileSpeed,
            currentWeapon.knockbackForce,
            gameObject
        );

        foreach (Collider2D ownerCol in GetComponentsInChildren<Collider2D>())
        {
            Collider2D bulletCol = bullet.GetComponent<Collider2D>();
            if (bulletCol != null)
                Physics2D.IgnoreCollision(bulletCol, ownerCol);
        }

        currentAmmo--;
        isFiring = false;

        if (currentAmmo <= 0)
        {
            reloading = true;
            debugText.SetReloading(true);
            yield return new WaitForSeconds(currentWeapon.reloadTime);
            currentAmmo = currentWeapon.magazineSize;
            reloading = false;
            debugText.SetReloading(false);
        }

        yield return new WaitForSeconds(currentWeapon.fireRate);
        canShoot = true;
    }
}