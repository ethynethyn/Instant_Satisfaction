using UnityEngine;
using System.Collections.Generic;

public class WeaponSpawner : MonoBehaviour
{
    public static WeaponSpawner Instance;

    public GameObject pickupPrefab;
    public float spawnInterval = 5f;

    private List<Transform> spawnPoints = new List<Transform>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Spawning starts only after level is selected and SetSpawnPoints is called
    }

    public void SetSpawnPoints(List<Transform> points)
    {
        spawnPoints = points;

        CancelInvoke(nameof(SpawnWeapon));
        InvokeRepeating(nameof(SpawnWeapon), 2f, spawnInterval);
    }

    void SpawnWeapon()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return;
        if (GameManager.Instance != null && GameManager.Instance.IsRoundOver) return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
        Instantiate(pickupPrefab, point.position, Quaternion.identity);
    }

    public void ClearAndRestart()
    {
        foreach (WeaponPickup pickup in FindObjectsByType<WeaponPickup>(FindObjectsSortMode.None))
            Destroy(pickup.gameObject);

        CancelInvoke(nameof(SpawnWeapon));

        if (spawnPoints != null && spawnPoints.Count > 0)
            InvokeRepeating(nameof(SpawnWeapon), spawnInterval, spawnInterval);
    }


}