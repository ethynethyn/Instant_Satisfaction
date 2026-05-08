using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject menuPanel;

    [Header("Player GameObjects")]
    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;

    void Start()
    {
        menuPanel.SetActive(true);

        player1.SetActive(false);
        player2.SetActive(false);
        if (player3 != null) player3.SetActive(false);
        if (player4 != null) player4.SetActive(false);

        Time.timeScale = 0f;
    }

    public void OnClick2Player()
    {
        player1.SetActive(true);
        player2.SetActive(true);
        if (player3 != null) player3.SetActive(false);
        if (player4 != null) player4.SetActive(false);

        GameManager.Instance.SetPlayerCount(2);
        GameStateManager.Instance.SetState(GameStateManager.GameState.Customisation);
    }

    public void OnClick3Player()
    {
        player1.SetActive(true);
        player2.SetActive(true);
        if (player3 != null) player3.SetActive(true);
        if (player4 != null) player4.SetActive(false);

        GameManager.Instance.SetPlayerCount(3);
        GameStateManager.Instance.SetState(GameStateManager.GameState.Customisation);
    }

    public void OnClick4Player()
    {
        player1.SetActive(true);
        player2.SetActive(true);
        if (player3 != null) player3.SetActive(true);
        if (player4 != null) player4.SetActive(true);

        GameManager.Instance.SetPlayerCount(4);
        GameStateManager.Instance.SetState(GameStateManager.GameState.Customisation);
    }

    void GoToCustomisation(int playerCount)
    {
        menuPanel.SetActive(false);
        Time.timeScale = 1f;

        GameManager.Instance.SetPlayerCount(playerCount);
        CustomisationManager.Instance.ShowCustomisation(GameManager.Instance.GetActivePlayers());
    }

    public void ReturnToMainMenu()
    {
        player1.SetActive(false);
        player2.SetActive(false);
        if (player3 != null) player3.SetActive(false);
        if (player4 != null) player4.SetActive(false);

       
        CustomisationManager.Instance.Hide();
        GameManager.Instance.ResetAllPlayers();

        menuPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}