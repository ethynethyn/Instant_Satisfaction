using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Levels")]
    public List<LevelData> levels;

    [Header("UI")]
    public GameObject levelSelectPanel;
    public Image previewImage;
    public TextMeshProUGUI levelNameText;

    private int currentIndex = 0;
    private LevelData activeLevel;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        levelSelectPanel.SetActive(false);

        Debug.Log("LEVEL COUNT: " + levels.Count);

        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i] == null)
            {
                Debug.LogError("LEVEL " + i + " IS NULL");
            }
            else
            {
                Debug.Log("LEVEL " + i + ": " + levels[i].levelName);
            }
        }
    }

    public void ShowLevelSelect()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("NO LEVELS ASSIGNED");
            return;
        }

        Time.timeScale = 1f;

        currentIndex = 0;

        levelSelectPanel.SetActive(true);

        RefreshUI();

        Debug.Log("LEVEL SELECT OPENED");
    }

    public void OnClickNext()
    {
        if (levels == null || levels.Count == 0)
            return;

        currentIndex++;

        if (currentIndex >= levels.Count)
            currentIndex = 0;

        RefreshUI();
    }

    public void OnClickPrevious()
    {
        if (levels == null || levels.Count == 0)
            return;

        currentIndex--;

        if (currentIndex < 0)
            currentIndex = levels.Count - 1;

        RefreshUI();
    }

    public void OnClickSelect()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("NO LEVELS TO SELECT");
            return;
        }

        foreach (LevelData level in levels)
        {
            if (level != null && level.levelObject != null)
                level.levelObject.SetActive(false);
        }

        activeLevel = levels[currentIndex];

        if (activeLevel == null)
        {
            Debug.LogError("ACTIVE LEVEL IS NULL");
            return;
        }

        if (activeLevel.levelObject != null)
            activeLevel.levelObject.SetActive(true);

        Debug.Log(
            "Selected level spawn count: "
            + activeLevel.playerSpawnPoints.Count
        );

        GameManager.Instance.SetSpawnPoints(activeLevel.playerSpawnPoints);

        WeaponSpawner.Instance.SetSpawnPoints(
            activeLevel.weaponSpawnPoints
        );

        levelSelectPanel.SetActive(false);

        GameStateManager.Instance.SetState(
            GameStateManager.GameState.Playing
        );
    }

    void RefreshUI()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("NO LEVELS FOUND");
            return;
        }

        LevelData current = levels[currentIndex];

        if (current == null)
        {
            Debug.LogError("CURRENT LEVEL IS NULL");
            return;
        }

        Debug.Log("REFRESHING UI: " + current.levelName);

        if (levelNameText != null)
            levelNameText.text = current.levelName;
        else
            Debug.LogError("LEVEL NAME TEXT IS NULL");

        if (previewImage != null)
            previewImage.sprite = current.previewImage;
        else
            Debug.LogError("PREVIEW IMAGE IS NULL");
    }
}