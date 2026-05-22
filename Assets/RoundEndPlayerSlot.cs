using UnityEngine;
using TMPro;
using System.Collections;

public class RoundEndPlayerSlot : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI playerLabel;
    public TextMeshProUGUI bankText;
    public TextMeshProUGUI walletText;
    public TextMeshProUGUI readyText;
    public GameObject readyIndicator;

    [Header("Bank Icon")]
    public RectTransform bankIcon;

    [Header("Juice")]
    [Range(0f, 2f)] public float globalIntensity = 1f;

    [Header("Wallet Juice")]
    public float walletPopScale = 1.25f;
    public float walletDuration = 0.22f;
    public float walletDamping = 4f;

    [Header("Bank Juice")]
    public float bankTiltStrength = 25f;
    public float bankDuration = 0.35f;
    public float bankDamping = 4f;

    private int entryFee;
    private bool isGameOver;

    private Coroutine walletRoutine;
    private Coroutine bankRoutine;
    private Coroutine cannotReadyRoutine;

    private static readonly Color[] playerColours = new Color[]
    {
        new Color(0.92f, 0.18f, 0.18f),
        new Color(0.18f, 0.52f, 0.92f),
        new Color(0.18f, 0.80f, 0.28f),
        new Color(0.95f, 0.80f, 0.10f),
    };

    private Color playerColour = Color.white;

    // ─────────────────────────────────────────────

    public void Initialise(string label, PlayerWallet wallet,
                           bool gameOver, int playerIndex)
    {
        isGameOver = gameOver;
        entryFee = wallet != null ? wallet.transferAmount : 0;

        playerColour = playerIndex < playerColours.Length
            ? playerColours[playerIndex]
            : Color.white;

        if (playerLabel != null)
        {
            playerLabel.text = label;
            playerLabel.color = playerColour;
        }

        if (bankText != null)
            bankText.color = new Color(1f, 0.85f, 0.1f);

        if (walletText != null)
            walletText.color = new Color(0.2f, 0.85f, 0.3f);

        if (readyText != null)
            readyText.color = playerColour;

        if (bankIcon != null)
            bankIcon.localRotation = Quaternion.identity;

        if (walletText != null)
            walletText.rectTransform.localScale = Vector3.one;

        SetReady(false);
        Refresh(wallet);
    }

    public void Refresh(PlayerWallet wallet)
    {
        if (wallet == null) return;

        if (bankText != null)
            bankText.text = "Bank: $" + wallet.bankAmount;

        if (walletText != null)
            walletText.text = "Wallet: $" + wallet.walletAmount;

        if (!isGameOver && readyText != null && !wallet.isReady)
        {
            readyText.color = playerColour;
            readyText.text = wallet.CanReady()
                ? "Press SHOOT to ready"
                : "Min entry: $" + entryFee;
        }
    }

    // ─────────────────────────────────────────────
    // WALLET (IN / OUT DIFFERENT FEEL)
    // ─────────────────────────────────────────────

    public void PlayWalletImpulse(bool moneyIn)
    {
        if (walletText == null) return;

        if (walletRoutine != null)
            StopCoroutine(walletRoutine);

        walletRoutine = StartCoroutine(WalletPopRoutine(moneyIn));
    }

    IEnumerator WalletPopRoutine(bool moneyIn)
    {
        RectTransform rt = walletText.rectTransform;

        float elapsed = 0f;
        float duration = walletDuration / globalIntensity;

        Vector3 peak = moneyIn
            ? Vector3.one * walletPopScale
            : Vector3.one * (2f - walletPopScale); // inverse feel

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float s = Mathf.Sin(t * Mathf.PI) * Mathf.Exp(-t * walletDamping);

            rt.localScale = Vector3.LerpUnclamped(Vector3.one, peak, s);
            yield return null;
        }

        rt.localScale = Vector3.one;
    }

    // ─────────────────────────────────────────────
    // BANK (DIRECTIONAL TILT)
    // ─────────────────────────────────────────────

    public void PlayBankImpulse(bool moneyIntoBank)
    {
        if (bankIcon == null) return;

        if (bankRoutine != null)
            StopCoroutine(bankRoutine);

        bankRoutine = StartCoroutine(BankTiltRoutine(moneyIntoBank));
    }

    IEnumerator BankTiltRoutine(bool moneyIntoBank)
    {
        float elapsed = 0f;
        float duration = bankDuration / globalIntensity;

        // IN = right tilt, OUT = left tilt
        float direction = moneyIntoBank ? -1f : 1f;
        float peak = bankTiltStrength * direction * globalIntensity;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float angle = peak *
                          Mathf.Sin(t * Mathf.PI) *
                          Mathf.Exp(-t * bankDamping);

            bankIcon.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        bankIcon.localRotation = Quaternion.identity;
    }

    // ─────────────────────────────────────────────

    public void SetReady(bool ready)
    {
        if (readyIndicator != null)
            readyIndicator.SetActive(ready);

        if (readyText != null)
        {
            readyText.color = playerColour;

            if (ready)
                readyText.text = "READY!";
            else if (!isGameOver)
                readyText.text = "Min entry: $" + entryFee;
            else
                readyText.text = "";
        }
    }

    public void ShowCannotReady()
    {
        if (cannotReadyRoutine != null)
            StopCoroutine(cannotReadyRoutine);

        cannotReadyRoutine = StartCoroutine(CannotReadyRoutine());
    }

    IEnumerator CannotReadyRoutine()
    {
        if (readyText != null)
        {
            readyText.color = Color.red;
            readyText.text = "Need $" + entryFee + " to enter!";
        }

        yield return new WaitForSeconds(1.5f);

        if (readyText != null)
        {
            readyText.color = playerColour;
            readyText.text = "Min entry: $" + entryFee;
        }
    }
}