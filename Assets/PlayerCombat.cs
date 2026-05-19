using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat")]
    public Transform firePoint;
    public WeaponData currentWeapon;
    public PlayerDebugText debugText;

    [Header("Weapon Visual")]
    public Transform weaponHolder;

    public Transform muzzleFlash;
    private GameObject currentWeaponVisual;

    [Header("Muzzle Flash")]
    public GameObject muzzleFlashObject;
    public float muzzleFlashDuration = 0.05f;

    [Header("Audio")]
    public AudioSource audioSource;

    private Coroutine muzzleFlashRoutine;
    private PlayerController2D controller;
    private WeaponData startingWeapon;

    private bool canShoot = true;
    private bool reloading = false;
    private bool isFiring = false;

    private int currentAmmo;

    public float weaponOffsetX = 0f;

    void Awake()
    {
        startingWeapon = currentWeapon;
    }

    void Start()
    {
        controller = GetComponent<PlayerController2D>();

        if (currentWeapon != null)
            currentAmmo = currentWeapon.magazineSize;

        UpdateWeaponVisual();
    }

    void Update()
    {
        UpdateFireUI();
    }

    // -----------------------------
    // SHOOT INPUT
    // -----------------------------
    public void TryShoot(bool held)
    {
        if (currentWeapon == null)
            return;

        bool shouldFire = currentWeapon.automatic ? held : !held;

        if (shouldFire)
            Attack();
    }

    void Attack()
    {
        if (!canShoot || reloading || isFiring)
            return;

        StartCoroutine(ShootRoutine());
    }

    // -----------------------------
    // SHOOT ROUTINE
    // -----------------------------
    IEnumerator ShootRoutine()
    {
        isFiring = true;
        canShoot = false;

        // 🔊 FIRE SOUND
        if (currentWeapon.fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(currentWeapon.fireSound, currentWeapon.fireVolume);
        }

        TriggerMuzzleFlash();

        Vector2 baseDirection =
            controller.facingRight ? Vector2.right : Vector2.left;

        List<Collider2D> spawnedBulletColliders = new List<Collider2D>();

        for (int i = 0; i < currentWeapon.projectileCount; i++)
        {
            float spread = Random.Range(-currentWeapon.spreadAngle, currentWeapon.spreadAngle);

            Vector2 finalDirection = Quaternion.Euler(0, 0, spread) * baseDirection;

            float angle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg;

            GameObject bullet = Instantiate(
                currentWeapon.projectilePrefab,
                firePoint.position,
                Quaternion.Euler(0, 0, angle)
            );

            Projectile projectile = bullet.GetComponent<Projectile>();

            if (projectile != null)
            {
                projectile.Initialize(
     finalDirection,
     currentWeapon.projectileSpeed,
     currentWeapon.knockbackForce,
     gameObject,
     currentWeapon.ricochetCount
 );
            }

            Collider2D bulletCol = bullet.GetComponent<Collider2D>();

            if (bulletCol != null)
            {
                spawnedBulletColliders.Add(bulletCol);

                foreach (Collider2D ownerCol in GetComponentsInChildren<Collider2D>())
                {
                    Physics2D.IgnoreCollision(bulletCol, ownerCol);
                }
            }
        }

        // bullet vs bullet ignore
        for (int i = 0; i < spawnedBulletColliders.Count; i++)
        {
            for (int j = i + 1; j < spawnedBulletColliders.Count; j++)
            {
                Physics2D.IgnoreCollision(spawnedBulletColliders[i], spawnedBulletColliders[j]);
            }
        }

        // recoil
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 recoilDirection = (controller.facingRight ? Vector2.left : Vector2.right);
            rb.AddForce(recoilDirection * currentWeapon.recoilForce, ForceMode2D.Impulse);
        }

        // camera shake
        DynamicCameraFocus2D cam = Camera.main.GetComponent<DynamicCameraFocus2D>();

        if (cam != null)
            cam.AddShake(currentWeapon.screenShake);

        // ammo
        currentAmmo--;

        // -----------------------------
        // RELOAD (FIXED - ONLY ONCE)
        // -----------------------------
        if (currentAmmo <= 0 && !reloading)
        {
            reloading = true;

            if (debugText != null)
                debugText.SetReloading(true);

            if (currentWeapon.reloadSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(currentWeapon.reloadSound, currentWeapon.reloadVolume);
            }

            yield return new WaitForSeconds(currentWeapon.reloadTime);

            currentAmmo = currentWeapon.magazineSize;
            reloading = false;

            if (debugText != null)
                debugText.SetReloading(false);
        }

        isFiring = false;

        yield return new WaitForSeconds(currentWeapon.fireRate);

        canShoot = true;
    }

    // -----------------------------
    // MUZZLE FLASH
    // -----------------------------
    void TriggerMuzzleFlash()
    {
        if (muzzleFlashObject == null || firePoint == null || currentWeapon == null)
            return;

        if (muzzleFlashRoutine != null)
            StopCoroutine(muzzleFlashRoutine);

        muzzleFlashRoutine = StartCoroutine(MuzzleFlashRoutine());

        muzzleFlashObject.transform.SetParent(firePoint);
        muzzleFlashObject.transform.localRotation = Quaternion.identity;

        float dir = controller.facingRight ? 1f : -1f;

        Vector3 localPos = Vector3.zero;
        localPos.x = currentWeapon.muzzleFlashOffsetX * dir;

        muzzleFlashObject.transform.localPosition = localPos;
    }

    IEnumerator MuzzleFlashRoutine()
    {
        muzzleFlashObject.SetActive(true);
        yield return new WaitForSeconds(muzzleFlashDuration);
        muzzleFlashObject.SetActive(false);
    }

    // -----------------------------
    // WEAPON SYSTEM
    // -----------------------------
    public void ReplaceWeapon(WeaponData weapon)
    {
        currentWeapon = weapon;
        currentAmmo = weapon.magazineSize;

        reloading = false;
        isFiring = false;
        canShoot = true;

        UpdateWeaponVisual();
    }

    public void ResetForRound()
    {
        currentWeapon = startingWeapon;

        reloading = false;
        isFiring = false;
        canShoot = true;

        if (currentWeapon != null)
            currentAmmo = currentWeapon.magazineSize;

        // Stop any in-flight coroutines (shoot, reload, muzzle flash)
        StopAllCoroutines();

        // Force muzzle flash off — coroutine may have been killed mid-run
        if (muzzleFlashObject != null)
            muzzleFlashObject.SetActive(false);

        muzzleFlashRoutine = null;

        if (debugText != null)
            debugText.ResetDebugState();

        UpdateWeaponVisual();
    }

    void UpdateFireUI()
    {
        if (debugText != null)
            debugText.SetFiring(isFiring);
    }

    // -----------------------------
    // VISUALS
    // -----------------------------
    void UpdateWeaponVisual()
    {
        if (currentWeaponVisual != null)
            Destroy(currentWeaponVisual);

        if (currentWeapon == null || weaponHolder == null)
            return;

        currentWeaponVisual = new GameObject("WeaponVisual");
        currentWeaponVisual.transform.SetParent(weaponHolder);

        float dir = controller.facingRight ? 1f : -1f;

        Vector3 weaponPos = currentWeapon.localPosition;
        weaponPos.x += weaponOffsetX * dir;

        currentWeaponVisual.transform.localPosition = weaponPos;
        currentWeaponVisual.transform.localEulerAngles = currentWeapon.localRotation;
        currentWeaponVisual.transform.localScale = currentWeapon.localScale;

        SpriteRenderer sr = currentWeaponVisual.AddComponent<SpriteRenderer>();
        sr.sprite = currentWeapon.weaponSprite;
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 10;

        // Apply current facing direction immediately so the weapon
        // is correctly oriented on pickup without needing to move first
        sr.flipX = !controller.facingRight;

        // Also sync firePoint and weaponHolder positions to current facing
        ApplyFacingToOffsets(controller.facingRight);
    }

    void ApplyFacingToOffsets(bool facingRight)
    {
        PlayerController2D pc = controller;
        if (pc == null) return;

        if (pc.firePoint != null)
        {
            // Read the default local pos via reflection isn't available,
            // so we just correct the sign of x directly
            Vector3 p = pc.firePoint.localPosition;
            p.x = facingRight ? Mathf.Abs(p.x) : -Mathf.Abs(p.x);
            pc.firePoint.localPosition = p;
        }

        if (pc.weaponHolder != null)
        {
            Vector3 p = pc.weaponHolder.localPosition;
            p.x = facingRight ? Mathf.Abs(p.x) : -Mathf.Abs(p.x);
            pc.weaponHolder.localPosition = p;
        }
    }

    public void SetWeaponVisualActive(bool active)
    {
        if (currentWeaponVisual != null)
            currentWeaponVisual.SetActive(active);
    }

}