using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Players")]
    public List<PlayerHealth> players;

    [Header("Round Settings")]
    public float roundEndDelay = 1.5f;
    public float roundStartDelay = 2f;

    [Header("Camera Settings")]
    public float cameraLerpSpeed = 5f;

    [Header("UI")]
    public RoundEndUI roundEndUI;

    [Header("Spawner")]
    public WeaponSpawner weaponSpawner;

    [Header("Main Menu")]
    public MainMenu mainMenu;

    [Header("Winner Cameras")]
    public Camera mainCamera;

    [Header("Game Over")]
    public GameObject gameOverObject;

    public Camera player1WinCamera;
    public Camera player2WinCamera;
    public Camera player3WinCamera;
    public Camera player4WinCamera;

    private List<PlayerDebugText> debugTexts =
    new List<PlayerDebugText>();

    private Vector3 originalCamPosition;
    private float originalCamSize;

    private bool roundOver = false;
    private bool gameActive = false;

    private List<PlayerHealth> activePlayers =
        new List<PlayerHealth>();

    private List<Transform> activeSpawnPoints =
        new List<Transform>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Capture camera state as early as possible before any other systems modify it
        if (mainCamera != null)
        {
            originalCamPosition =
                mainCamera.transform.position;

            originalCamSize =
                mainCamera.orthographicSize;
        }
    }

    void Start()
    {
        if (roundEndUI != null)
            roundEndUI.Hide();

        if (gameOverObject != null)
            gameOverObject.SetActive(false);
    }

    void Update()
    {
        if (gameActive &&
            Input.GetKeyDown(KeyCode.Escape))
        {
            EndGameEarly();
        }
    }

    void CacheDebugTexts()
    {
        debugTexts.Clear();

        foreach (PlayerHealth p in activePlayers)
        {
            if (p == null) continue;

            PlayerDebugText dbg =
                p.GetComponentInChildren<PlayerDebugText>();

            if (dbg != null)
                debugTexts.Add(dbg);
        }
    }
    void SetWinnerText(PlayerHealth winner)
    {
        foreach (PlayerDebugText dbg in debugTexts)
        {
            if (dbg == null) continue;

            dbg.SetWinner(false);
        }

        PlayerDebugText winnerDbg =
            winner.GetComponentInChildren<PlayerDebugText>();

        if (winnerDbg != null)
            winnerDbg.SetWinner(true);
    }

    void ClearWinnerText()
    {
        foreach (PlayerDebugText dbg in debugTexts)
        {
            if (dbg != null)
                dbg.SetWinner(false);
        }
    }



    public void ForceMenuState()
    {
        gameActive = false;

        StopAllCoroutines();

        FreezeAllPlayers();

        ResetCameraImmediate();

        ClearWeapons();
    }

    void EndGameEarly()
    {
        StopAllCoroutines();

        if (roundEndUI != null)
            roundEndUI.Hide();

        FreezeAllPlayers();

        ResetCameraImmediate();

        ClearWeapons();

        gameActive = false;

        ResetAllPlayers();

        if (mainMenu != null)
            mainMenu.ReturnToMainMenu();
    }

    public void SetPlayerCount(int playerCount)
    {
        activePlayers.Clear();

        for (int i = 0; i < players.Count; i++)
        {
            bool active = i < playerCount;

            if (players[i] != null)
                players[i].gameObject.SetActive(active);

            if (active)
                activePlayers.Add(players[i]);
        }
    }

    public List<PlayerHealth> GetActivePlayers()
    {
        return activePlayers;
    }

    public void SetSpawnPoints(
        List<Transform> spawnPoints
    )
    {
        activeSpawnPoints = spawnPoints;
    }

    public void BeginRounds()
    {
        if (activePlayers.Count == 0)
        {
            Debug.LogError("NO ACTIVE PLAYERS");
            return;
        }

        if (activeSpawnPoints == null ||
            activeSpawnPoints.Count == 0)
        {
            Debug.LogError("NO SPAWN POINTS");
            return;
        }

        gameActive = true;

        StartRound();
    }

    public void ResetAllPlayers()
    {
        foreach (PlayerHealth p in players)
        {
            if (p == null)
                continue;

            p.currentLives = p.maxLives;

            PlayerCombat combat =
                p.GetComponent<PlayerCombat>();

            if (combat != null)
                combat.ResetForRound();

            PlayerController2D controller =
                p.GetComponent<PlayerController2D>();

            if (controller != null)
            {
                controller.enabled = false;
                controller.inputEnabled = false;
            }

            Rigidbody2D rb = p.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        activePlayers.Clear();

        ClearWeapons();
    }

    List<Transform> GetShuffledSpawnPoints()
    {
        List<Transform> shuffled =
            new List<Transform>(activeSpawnPoints);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);

            Transform temp = shuffled[i];
            shuffled[i] = shuffled[rand];
            shuffled[rand] = temp;
        }

        return shuffled;
    }

    void StartRound()
    {
        roundOver = false;

        ResetCameraImmediate();

        ClearWeapons();

        // Build list of players still alive in the match
        List<PlayerHealth> survivingPlayers =
            new List<PlayerHealth>();

        foreach (PlayerHealth p in activePlayers)
        {
            if (p != null && p.currentLives > 0)
            {
                survivingPlayers.Add(p);
            }
            else if (p != null)
            {
                // Keep eliminated players disabled
                p.gameObject.SetActive(false);
            }
        }

        List<Transform> shuffledSpawns =
            GetShuffledSpawnPoints();

        if (shuffledSpawns.Count < survivingPlayers.Count)
        {
            Debug.LogError(
                "NOT ENOUGH SPAWN POINTS!"
            );

            return;
        }

        // Spawn ONLY surviving players
        for (int i = 0; i < survivingPlayers.Count; i++)
        {
            PlayerHealth player =
                survivingPlayers[i];

            player.ResetForRound(
                shuffledSpawns[i].position
            );

            PlayerCombat combat =
                player.GetComponent<PlayerCombat>();

            if (combat != null)
                combat.ResetForRound();

            Rigidbody2D rb =
                player.GetComponent<Rigidbody2D>();

            if (rb != null)
                rb.bodyType = RigidbodyType2D.Dynamic;

            PlayerController2D controller =
                player.GetComponent<PlayerController2D>();

            if (controller != null)
            {
                controller.enabled = true;
                controller.inputEnabled = true;
            }
        }
    }

    public void OnPlayerDied()
    {
        if (roundOver)
            return;

        int aliveThisRound = 0;

        foreach (PlayerHealth p in activePlayers)
        {
            // Still active in current round
            if (p != null && p.gameObject.activeSelf)
            {
                aliveThisRound++;
            }
        }

        // ROUND OVER
        // only one player still standing
        if (aliveThisRound <= 1)
        {
            roundOver = true;

            // Count players with lives remaining
            int playersStillInGame = 0;

            foreach (PlayerHealth p in activePlayers)
            {
                if (p != null && p.currentLives > 0)
                {
                    playersStillInGame++;
                }
            }

            // GAME OVER
            // only one player has lives left
            if (playersStillInGame <= 1)
            {
                PlayerHealth eliminated = null;

                foreach (PlayerHealth p in activePlayers)
                {
                    if (p != null && p.currentLives <= 0)
                    {
                        eliminated = p;
                        break;
                    }
                }

                StartCoroutine(
                    GameOverSequence(eliminated)
                );
            }
            else
            {
                // NEXT ROUND
                StartCoroutine(
                    RoundEndSequence()
                );
            }
        }
    }

    void FreezeAllPlayers()
    {
        foreach (PlayerHealth p in activePlayers)
        {
            if (p == null)
                continue;

            PlayerController2D controller =
                p.GetComponent<PlayerController2D>();

            if (controller != null)
            {
                controller.enabled = false;
                controller.inputEnabled = false;
            }

            Rigidbody2D rb = p.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    void ClearWeapons()
    {
        WeaponPickup[] pickups =
            FindObjectsByType<WeaponPickup>(
                FindObjectsSortMode.None
            );

        foreach (WeaponPickup pickup in pickups)
        {
            Destroy(pickup.gameObject);
        }
    }

    Camera GetWinnerCamera(PlayerHealth winner)
    {
        int index = activePlayers.IndexOf(winner);

        switch (index)
        {
            case 0:
                return player1WinCamera;

            case 1:
                return player2WinCamera;

            case 2:
                return player3WinCamera;

            case 3:
                return player4WinCamera;
        }

        return null;
    }

    IEnumerator LerpMainCameraTo(Camera targetCam, float duration)
    {
        if (targetCam == null || mainCamera == null)
            yield break;

        Vector3 startPos = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;

        Vector3 targetPos = new Vector3(
            targetCam.transform.position.x,
            targetCam.transform.position.y,
            startPos.z
        );

        float targetSize = 1.4f; // 🔥 desired zoom

        float elapsed = 0f;
        bool reachedTarget = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            // position lerp (smooth + stable)
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetPos,
                cameraLerpSpeed * Time.deltaTime
            );

            // size lerp (zoom in/out)
            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize,
                targetSize,
                cameraLerpSpeed * Time.deltaTime
            );

            // snap condition (prevents micro drift)
            if (!reachedTarget &&
                Vector3.Distance(mainCamera.transform.position, targetPos) < 0.01f)
            {
                mainCamera.transform.position = targetPos;
                mainCamera.orthographicSize = targetSize;
                reachedTarget = true;
            }

            // lock once reached
            if (reachedTarget)
            {
                mainCamera.transform.position = targetPos;
                mainCamera.orthographicSize = targetSize;
            }

            yield return null;
        }

        // final safety lock
        mainCamera.transform.position = targetPos;
        mainCamera.orthographicSize = targetSize;
    }

    void ResetCameraImmediate()
    {
        if (mainCamera == null)
            return;

        mainCamera.transform.position =
            new Vector3(
                originalCamPosition.x,
                originalCamPosition.y,
                originalCamPosition.z
            );

        mainCamera.orthographicSize =
            originalCamSize;
    }

    IEnumerator RoundEndSequence()
    {
        FreezeAllPlayers();

        CacheDebugTexts();

        PlayerHealth winner = null;

        foreach (PlayerHealth p in activePlayers)
        {
            if (p != null && p.gameObject.activeSelf)
            {
                winner = p;
                break;
            }
        }

        if (winner != null)
            SetWinnerText(winner);

        Camera targetCam =
            GetWinnerCamera(winner);

        if (targetCam != null)
        {
            yield return StartCoroutine(
                LerpMainCameraTo(
                    targetCam,
                    roundEndDelay
                )
            );
        }
        else
        {
            yield return new WaitForSeconds(
                roundEndDelay
            );
        }

        if (roundEndUI != null)
        {
            roundEndUI.ShowRoundEnd(
                activePlayers,
                false
            );
        }

        ClearWeapons();

        yield return new WaitForSeconds(
            roundStartDelay
        );

        ClearWinnerText(); // 🔥 RESET DEBUG TEXT HERE

        if (roundEndUI != null)
            roundEndUI.Hide();

        ResetCameraImmediate();

        StartRound();
    }

    IEnumerator GameOverSequence(PlayerHealth eliminated)
    {
        FreezeAllPlayers();

        CacheDebugTexts();

        PlayerHealth winner = null;

        foreach (PlayerHealth p in activePlayers)
        {
            if (p != null &&
                p != eliminated &&
                p.currentLives > 0)
            {
                winner = p;
                break;
            }
        }

        if (winner != null)
            SetWinnerText(winner);

        Camera targetCam =
            GetWinnerCamera(winner);

        if (targetCam != null)
        {
            yield return StartCoroutine(
                LerpMainCameraTo(
                    targetCam,
                    roundEndDelay
                )
            );
        }
        else
        {
            yield return new WaitForSeconds(
                roundEndDelay
            );
        }

        ClearWeapons();

        if (roundEndUI != null)
        {
            roundEndUI.ShowRoundEnd(
                activePlayers,
                true

            );
            if (gameOverObject != null)
                gameOverObject.SetActive(true);
        }

        // 🔥 GAME NOW STAYS ON FINAL SCREEN FOREVER
        gameActive = false;

        // Optional:
        // wait for escape key to return to menu
        while (!Input.GetKeyDown(KeyCode.Escape))
        {
            yield return null;
        }

        ClearWinnerText();

        if (roundEndUI != null)
            roundEndUI.Hide();

        ResetCameraImmediate();

        ResetAllPlayers();

        if (mainMenu != null)
            mainMenu.ReturnToMainMenu();
    }
}