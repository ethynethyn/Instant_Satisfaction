using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LivesMenuUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text livesText;

    [Header("Players")]
    public List<PlayerHealth> players =
        new List<PlayerHealth>();

    [Header("Settings")]
    public int selectedLives = 3;

    void Start()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (livesText != null)
        {
            livesText.text =
                "Lives: " + selectedLives;
        }
    }

    // =========================
    // BUTTONS
    // =========================

    public void Increase()
    {
        selectedLives++;

        ApplyLives();
    }

    public void Decrease()
    {
        selectedLives =
            Mathf.Max(1, selectedLives - 1);

        ApplyLives();
    }

    // Direct UI button methods
    public void SetLives1()
    {
        selectedLives = 1;
        ApplyLives();
    }

    public void SetLives3()
    {
        selectedLives = 3;
        ApplyLives();
    }

    public void SetLives5()
    {
        selectedLives = 5;
        ApplyLives();
    }

    public void SetLives10()
    {
        selectedLives = 10;
        ApplyLives();
    }

    // =========================
    // APPLY
    // =========================

    public void ApplyLives()
    {
        UpdateUI();

        if (players == null || players.Count == 0)
        {
            Debug.LogError(
                "No PlayerHealth assigned!"
            );

            return;
        }

        foreach (PlayerHealth p in players)
        {
            if (p == null)
                continue;

            p.maxLives = selectedLives;

            p.ApplyLives();
        }

        Debug.Log(
            "Applied " +
            selectedLives +
            " lives to " +
            players.Count +
            " players"
        );
    }
}