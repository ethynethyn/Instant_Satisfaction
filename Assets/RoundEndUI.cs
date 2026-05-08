using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RoundEndUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Player Life Displays")]
    public List<TextMeshProUGUI> playerLivesTexts;

    [Header("Header Text")]
    public TextMeshProUGUI headerText;

    public void ShowRoundEnd(List<PlayerHealth> activePlayers, bool isGameOver)
    {
        panel.SetActive(true);

        if (isGameOver)
        {
            int winnerIndex = 0;
            for (int i = 1; i < activePlayers.Count; i++)
            {
                if (activePlayers[i].currentLives > activePlayers[winnerIndex].currentLives)
                    winnerIndex = i;
            }
            headerText.text = "Player " + (winnerIndex + 1) + " Wins!";
        }
        else
        {
            headerText.text = "Round Over";
        }

        // Show only the active player count
        for (int i = 0; i < playerLivesTexts.Count; i++)
        {
            if (i < activePlayers.Count)
            {
                // This player is active - show and update text
                playerLivesTexts[i].gameObject.SetActive(true);
                playerLivesTexts[i].text = "P" + (i + 1) + ": " + activePlayers[i].currentLives + " lives";
            }
            else
            {
                // This player is not active - hide
                playerLivesTexts[i].gameObject.SetActive(false);
            }
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}