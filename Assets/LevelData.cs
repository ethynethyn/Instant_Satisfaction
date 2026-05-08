using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    [Header("Info")]
    public string levelName;

    public Sprite previewImage;

    [Header("Level")]
    public GameObject levelObject;

    [Header("Spawn Points")]
    public List<Transform> playerSpawnPoints;

    public List<Transform> weaponSpawnPoints;
}