using UnityEngine;

public class PlayerDebugText : MonoBehaviour
{
    [Header("Icons")]
    public GameObject playerIconObject;
    public GameObject reloadIconObject;
    public GameObject winnerIconObject;

    private PlayerController2D.PlayerState state;
    private bool firing;
    private bool reloading;
    private bool isWinner = false;

    private PlayerCombat combat;

    void Start()
    {
        combat = GetComponentInParent<PlayerCombat>();
        ResetDebugState();
    }

    void Update()
    {
        UpdateIcons();
    }

    public void SetState(PlayerController2D.PlayerState newState) => state = newState;
    public void SetFiring(bool value) => firing = value;
    public void SetReloading(bool value) => reloading = value;

    public void SetWinner(bool value)
    {
        isWinner = value;
        UpdateIcons();
    }

    public void ResetDebugState()
    {
        firing = false;
        reloading = false;
        isWinner = false;
        UpdateIcons();
    }

    void UpdateIcons()
    {
        if (isWinner)
        {
            SetPlayerIcon(false);
            SetReloadIcon(false);
            SetWinnerIcon(true);
            return;
        }

        SetWinnerIcon(false);

        if (reloading)
        {
            SetPlayerIcon(false);
            SetReloadIcon(true);
            return;
        }

        SetPlayerIcon(true);
        SetReloadIcon(false);
    }

    void SetPlayerIcon(bool active)
    {
        if (playerIconObject != null)
            playerIconObject.SetActive(active);
    }

    void SetReloadIcon(bool active)
    {
        if (reloadIconObject != null)
            reloadIconObject.SetActive(active);
    }

    void SetWinnerIcon(bool active)
    {
        if (winnerIconObject != null)
            winnerIconObject.SetActive(active);
    }
}