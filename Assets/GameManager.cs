using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Players")]
    public List<PlayerHealth> players;

    [Header("Round Timing")]
    public float roundEndDelay = 1.5f;
    public float delayBeforeWinnerCam = 0.5f;
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

    [Header("Pre Fight Countdown")]
    public GameObject countdown3Object;
    public GameObject countdown2Object;
    public GameObject countdown1Object;
    public GameObject fightObject;

    [Tooltip("Objects enabled during countdown")]
    public List<GameObject> preFightObjects =
        new List<GameObject>();

    public float countdownNumberDuration = 1f;
    public float fightDuration = 0.8f;

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

        // Disable countdown visuals initially
        HideCountdownObjects();

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

            // inside the StartRound spawn loop, replace the existing combat reset block:
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat != null)
                combat.ResetForRound();

            PlayerController2D controller = player.GetComponent<PlayerController2D>();
            if (controller != null)
            {
                controller.ResetForRound();   // ← new: clears movement/dash/jump state
                controller.enabled = true;
                controller.inputEnabled = false;
            }

            if (combat != null)
                combat.ResetForRound();

            Rigidbody2D rb =
                player.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.linearVelocity = Vector2.zero;
            }

          
            if (controller != null)
            {
                controller.enabled = true;

                // IMPORTANT
                // players spawned but cannot move yet
                controller.inputEnabled = false;
            }
        }

        StartCoroutine(PreFightCountdownRoutine());
    }

    IEnumerator PreFightCountdownRoutine()
    {
        // Enable pre-fight objects
        SetPreFightObjects(true);

        // 3
        ShowOnlyCountdownObject(countdown3Object);

        yield return new WaitForSeconds(
            countdownNumberDuration
        );

        // 2
        ShowOnlyCountdownObject(countdown2Object);

        yield return new WaitForSeconds(
            countdownNumberDuration
        );

        // 1
        ShowOnlyCountdownObject(countdown1Object);

        yield return new WaitForSeconds(
            countdownNumberDuration
        );

        // FIGHT
        ShowOnlyCountdownObject(fightObject);

        // EXTRA HOLD TIME (no new variables)
        yield return new WaitForSeconds(1f);

        // ENABLE PLAYER INPUT
        foreach (PlayerHealth p in activePlayers)
        {
            if (p == null || !p.gameObject.activeSelf)
                continue;

            PlayerController2D controller =
                p.GetComponent<PlayerController2D>();

            if (controller != null)
            {
                controller.inputEnabled = true;
            }
        }

        // optional: let FIGHT linger slightly longer visually
        yield return new WaitForSeconds(fightDuration);

        yield return new WaitForSeconds(
            fightDuration
        );

        HideCountdownObjects();

        // Disable pre-fight objects
        SetPreFightObjects(false);
    }

    void ShowOnlyCountdownObject(GameObject target)
    {
        HideCountdownObjects();

        if (target != null)
            target.SetActive(true);
    }

    void HideCountdownObjects()
    {
        if (countdown3Object != null)
            countdown3Object.SetActive(false);

        if (countdown2Object != null)
            countdown2Object.SetActive(false);

        if (countdown1Object != null)
            countdown1Object.SetActive(false);

        if (fightObject != null)
            fightObject.SetActive(false);
    }

    void SetPreFightObjects(bool state)
    {
        foreach (GameObject obj in preFightObjects)
        {
            if (obj != null)
                obj.SetActive(state);
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

        float targetSize = 1.4f;

        float elapsed = 0f;
        bool reachedTarget = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetPos,
                cameraLerpSpeed * Time.deltaTime
            );

            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize,
                targetSize,
                cameraLerpSpeed * Time.deltaTime
            );

            if (!reachedTarget &&
                Vector3.Distance(mainCamera.transform.position, targetPos) < 0.01f)
            {
                mainCamera.transform.position = targetPos;
                mainCamera.orthographicSize = targetSize;
                reachedTarget = true;
            }

            if (reachedTarget)
            {
                mainCamera.transform.position = targetPos;
                mainCamera.orthographicSize = targetSize;
            }

            yield return null;
        }

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

        Camera targetCam = GetWinnerCamera(winner);

        yield return new WaitForSeconds(delayBeforeWinnerCam);

        // Show win display after the delay, just as camera starts moving
        if (winner != null)
        {
            PlayerWinDisplay winDisplay =
                winner.GetComponent<PlayerWinDisplay>();
            if (winDisplay != null)
                winDisplay.ShowWin();
        }

        if (targetCam != null)
        {
            yield return StartCoroutine(
                LerpMainCameraTo(targetCam, roundEndDelay));
        }
        else
        {
            yield return new WaitForSeconds(roundEndDelay);
        }

        if (roundEndUI != null)
            roundEndUI.ShowRoundEnd(activePlayers, false);

        ClearWeapons();

        yield return new WaitForSeconds(roundStartDelay);

        // Clean up win display before next round
        HideAllWinDisplays();

        ClearWinnerText();

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
            if (p != null && p != eliminated && p.currentLives > 0)
            {
                winner = p;
                break;
            }
        }

        if (winner != null)
            SetWinnerText(winner);

        Camera targetCam = GetWinnerCamera(winner);

        yield return new WaitForSeconds(delayBeforeWinnerCam);

        // Show win display after the delay
        if (winner != null)
        {
            PlayerWinDisplay winDisplay =
                winner.GetComponent<PlayerWinDisplay>();
            if (winDisplay != null)
                winDisplay.ShowWin();
        }

        if (targetCam != null)
        {
            yield return StartCoroutine(
                LerpMainCameraTo(targetCam, roundEndDelay));
        }
        else
        {
            yield return new WaitForSeconds(roundEndDelay);
        }

        ClearWeapons();

        if (roundEndUI != null)
        {
            roundEndUI.ShowRoundEnd(activePlayers, true);

            if (gameOverObject != null)
                gameOverObject.SetActive(true);
        }

        gameActive = false;

        while (!Input.GetKeyDown(KeyCode.Escape))
            yield return null;

        HideAllWinDisplays();
        ClearWinnerText();

        if (roundEndUI != null)
            roundEndUI.Hide();

        ResetCameraImmediate();
        ResetAllPlayers();

        if (mainMenu != null)
            mainMenu.ReturnToMainMenu();
    }

    void HideAllWinDisplays()
    {
        foreach (PlayerHealth p in activePlayers)
        {
            if (p == null) continue;

            PlayerWinDisplay winDisplay =
                p.GetComponent<PlayerWinDisplay>();

            if (winDisplay != null)
                winDisplay.HideWin();
        }
    }

    public bool IsRoundOver => roundOver;
}