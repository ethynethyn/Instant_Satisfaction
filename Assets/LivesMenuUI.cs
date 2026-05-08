using UnityEngine;
using TMPro;

public class LivesMenuUI : MonoBehaviour
{
    public TMP_Text livesText;

    public int selectedLives = 3;

    private PlayerHealth[] allPlayers;

    void Start()
    {
        // 🔥 AUTO FIND ALL PLAYERS IN SCENE
        allPlayers = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

        UpdateUI();
    }

    void UpdateUI()
    {
        if (livesText != null)
            livesText.text = "Lives: " + selectedLives;
    }

    public void Increase()
    {
        selectedLives++;
        UpdateUI();
    }

    public void Decrease()
    {
        selectedLives = Mathf.Max(1, selectedLives - 1);
        UpdateUI();
    }

    public void ApplyLives()
    {
        if (allPlayers == null || allPlayers.Length == 0)
        {
            Debug.LogError("No PlayerHealth found in scene!");
            return;
        }

        foreach (PlayerHealth p in allPlayers)
        {
            if (p == null) continue;

            p.maxLives = selectedLives;
            p.ApplyLives();
        }

        Debug.Log("Applied lives: " + selectedLives + " to " + allPlayers.Length + " players");
    }
}