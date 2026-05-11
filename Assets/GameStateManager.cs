using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public enum GameState
    {
        Menu,
        Customisation,
        LevelSelect,
        Playing,
        RoundEnd
    }

    [Header("Current State")]
    public GameState currentState;

    [Header("References")]
    public MainMenu mainMenu;

    public CustomisationManager customisationManager;

    public LevelManager levelManager;

    public GameManager gameManager;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        SetState(GameState.Menu);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;

        Debug.Log("GAME STATE CHANGED TO: " + newState);

        switch (newState)
        {
            case GameState.Menu:
                EnterMenu();
                break;

            case GameState.Customisation:
                EnterCustomisation();
                break;

            case GameState.LevelSelect:
                EnterLevelSelect();
                break;

            case GameState.Playing:
                EnterPlaying();
                break;

            case GameState.RoundEnd:
                EnterRoundEnd();
                break;
        }
    }

    void EnterMenu()
    {
        Time.timeScale = 1f;

        if (mainMenu != null &&
            mainMenu.menuPanel != null)
        {
            mainMenu.menuPanel.SetActive(true);
        }

        if (customisationManager != null)
            customisationManager.Hide();

        if (levelManager != null &&
            levelManager.levelSelectPanel != null)
        {
            levelManager.levelSelectPanel.SetActive(false);
        }

        if (gameManager != null)
            gameManager.ForceMenuState();

        Debug.Log("ENTERED MENU");
    }

    void EnterCustomisation()
    {
        Time.timeScale = 1f;

        if (mainMenu != null &&
            mainMenu.menuPanel != null)
        {
            mainMenu.menuPanel.SetActive(false);
        }

        if (levelManager != null &&
            levelManager.levelSelectPanel != null)
        {
            levelManager.levelSelectPanel.SetActive(false);
        }

        if (customisationManager != null &&
            gameManager != null)
        {
            customisationManager.ShowCustomisation(
                gameManager.GetActivePlayers()
            );
        }

        Debug.Log("ENTERED CUSTOMISATION");
    }

    void EnterLevelSelect()
    {
        Time.timeScale = 1f;

        if (customisationManager != null)
            customisationManager.Hide();

        if (levelManager != null)
            levelManager.ShowLevelSelect();

        Debug.Log("ENTERED LEVEL SELECT");
    }

    void EnterPlaying()
    {
        Time.timeScale = 1f;

        if (levelManager != null &&
            levelManager.levelSelectPanel != null)
        {
            levelManager.levelSelectPanel.SetActive(false);
        }

        if (gameManager != null)
            gameManager.BeginRounds();

        Debug.Log("ENTERED PLAYING");
    }

    void EnterRoundEnd()
    {
        Debug.Log("ENTERED ROUND END");
    }
}