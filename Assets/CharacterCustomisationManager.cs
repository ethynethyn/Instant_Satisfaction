using UnityEngine;
using System.Collections.Generic;

public class CustomisationManager : MonoBehaviour
{
    public static CustomisationManager Instance;

    [Header("Scene")]
    public GameObject customisationLevelObject;

    public List<Transform> playerSpawnPoints;

    [Header("UI")]
    public GameObject customisationPanel;

    public GameObject submitButton;

    [Header("Players")]
    public List<CharacterCustomisation> playerCustomisations;

    private List<PlayerHealth> activePlayers =
        new List<PlayerHealth>();

    private bool customisationLocked = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        customisationPanel.SetActive(false);

        if (customisationLevelObject != null)
            customisationLevelObject.SetActive(false);
    }

    public void ShowCustomisation(
        List<PlayerHealth> players
    )
    {
        activePlayers = players;

        customisationPanel.SetActive(true);

        if (customisationLevelObject != null)
            customisationLevelObject.SetActive(true);

        // Allow customization when returning to this screen
        customisationLocked = false;

        for (int i = 0; i < activePlayers.Count; i++)
        {
            GameObject playerObj =
                activePlayers[i].gameObject;

            playerObj.SetActive(true);

            if (i < playerSpawnPoints.Count)
            {
                playerObj.transform.position =
                    playerSpawnPoints[i].position;
            }

            Rigidbody2D rb =
                playerObj.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            PlayerController2D controller =
                playerObj.GetComponent<PlayerController2D>();

            if (controller != null)
            {
                controller.enabled = true;
                controller.inputEnabled = false;
            }

            if (i < playerCustomisations.Count)
                playerCustomisations[i].ResetSelections();
        }

        for (int i = 0; i < playerCustomisations.Count; i++)
        {
            playerCustomisations[i].gameObject.SetActive(
                i < activePlayers.Count
            );
        }

        Debug.Log("CUSTOMISATION OPENED");
    }

    public void OnClickSubmit()
    {
        // Lock customization when submit is pressed
        customisationLocked = true;

        customisationPanel.SetActive(false);

        if (customisationLevelObject != null)
            customisationLevelObject.SetActive(false);

        foreach (PlayerHealth p in activePlayers)
        {
            PlayerController2D controller =
                p.GetComponent<PlayerController2D>();

            if (controller != null)
                controller.inputEnabled = false;

            Rigidbody2D rb = p.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        GameStateManager.Instance.SetState(
            GameStateManager.GameState.LevelSelect
        );
    }

    public bool IsCustomisationLocked()
    {
        return customisationLocked;
    }

    public void Hide()
    {
        customisationPanel.SetActive(false);

        if (customisationLevelObject != null)
            customisationLevelObject.SetActive(false);
    }
}