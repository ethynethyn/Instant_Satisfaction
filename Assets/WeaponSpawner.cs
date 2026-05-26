using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponSpawner : MonoBehaviour
{
    public static WeaponSpawner Instance;

    [Header("Prefabs")]
    public List<GameObject> weaponPickupPrefabs = new List<GameObject>();
    public List<GameObject> shiftPickupPrefabs  = new List<GameObject>();

    [Header("Timing")]
    public float spawnIntervalMin = 3f;
    public float spawnIntervalMax = 8f;

    [Header("Odds")]
    [Range(0f, 1f)]
    [Tooltip("0 = always weapon, 1 = always shift")]
    public float shiftSpawnChance = 0.3f;

    private List<Transform> spawnPoints = new List<Transform>();

    private Dictionary<Transform, GameObject> occupiedPoints =
        new Dictionary<Transform, GameObject>();

    private Dictionary<Transform, Coroutine> spawnCoroutines =
        new Dictionary<Transform, Coroutine>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetSpawnPoints(List<Transform> points)
    {
        StopAllSpawnCoroutines();

        spawnPoints = points;
        occupiedPoints.Clear();
        spawnCoroutines.Clear();

        foreach (Transform point in spawnPoints)
        {
            occupiedPoints[point]  = null;
            spawnCoroutines[point] = StartCoroutine(SpawnRoutine(point));
        }
    }

    public void OnPickupCollected(Transform spawnPoint)
    {
        if (spawnPoint == null) return;
        if (!occupiedPoints.ContainsKey(spawnPoint)) return;

        occupiedPoints[spawnPoint] = null;
    }

    IEnumerator SpawnRoutine(Transform point)
    {
        while (true)
        {
            float interval = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(interval);

            if (GameManager.Instance != null &&
                GameManager.Instance.IsRoundOver)
                continue;

            if (IsPointOccupied(point))
                continue;

            GameObject prefab = PickPrefab();
            if (prefab == null) continue;

            GameObject spawned = Instantiate(
                prefab, point.position, Quaternion.identity);

            occupiedPoints[point] = spawned;

            WeaponPickup wp = spawned.GetComponent<WeaponPickup>();
            if (wp != null) wp.spawnPoint = point;

            ShiftPickup sp = spawned.GetComponent<ShiftPickup>();
            if (sp != null) sp.spawnPoint = point;

            yield return new WaitUntil(() => !IsPointOccupied(point));
        }
    }

    bool IsPointOccupied(Transform point)
    {
        if (!occupiedPoints.ContainsKey(point)) return false;

        GameObject obj = occupiedPoints[point];

        if (obj == null)
        {
            occupiedPoints[point] = null;
            return false;
        }

        return true;
    }

    GameObject PickPrefab()
    {
        bool hasWeapons = weaponPickupPrefabs != null &&
                          weaponPickupPrefabs.Count > 0;
        bool hasShifts  = shiftPickupPrefabs  != null &&
                          shiftPickupPrefabs.Count  > 0;

        if (hasWeapons && !hasShifts)
            return weaponPickupPrefabs[
                Random.Range(0, weaponPickupPrefabs.Count)];

        if (hasShifts && !hasWeapons)
            return shiftPickupPrefabs[
                Random.Range(0, shiftPickupPrefabs.Count)];

        if (!hasWeapons && !hasShifts)
        {
            Debug.LogError("WeaponSpawner: no prefabs assigned.");
            return null;
        }

        if (Random.value <= shiftSpawnChance)
            return shiftPickupPrefabs[
                Random.Range(0, shiftPickupPrefabs.Count)];
        else
            return weaponPickupPrefabs[
                Random.Range(0, weaponPickupPrefabs.Count)];
    }

    void StopAllSpawnCoroutines()
    {
        foreach (var kvp in spawnCoroutines)
            if (kvp.Value != null)
                StopCoroutine(kvp.Value);

        spawnCoroutines.Clear();
    }

    public void ClearAndRestart()
    {
        StopAllSpawnCoroutines();

        WeaponPickup[] weaponPickups =
            FindObjectsByType<WeaponPickup>(FindObjectsSortMode.None);
        foreach (WeaponPickup pickup in weaponPickups)
            Destroy(pickup.gameObject);

        ShiftPickup[] shiftPickups =
            FindObjectsByType<ShiftPickup>(FindObjectsSortMode.None);
        foreach (ShiftPickup pickup in shiftPickups)
            Destroy(pickup.gameObject);

        occupiedPoints.Clear();
        spawnCoroutines.Clear();

        if (spawnPoints != null && spawnPoints.Count > 0)
            SetSpawnPoints(spawnPoints);
    }
}