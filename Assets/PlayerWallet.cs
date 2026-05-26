using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerWallet : MonoBehaviour
{
    [Header("Starting Values")]
    public int startingBank   = 500;
    public int transferAmount = 50;

    [HideInInspector] public int  bankAmount;
    [HideInInspector] public int  walletAmount;
    [HideInInspector] public bool isReady = false;

    [Header("HUD")]
    public TextMeshProUGUI activeBalanceText;

    [Header("Slot Machine Settings")]
    public float slotMachineDuration = 0.4f;

    [Header("Colours")]
    public Color colourDefault  = Color.white;
    public Color colourAdd      = new Color(0.2f,  0.85f, 0.3f);
    public Color colourSubtract = new Color(0.95f, 0.15f, 0.15f);
    public Color colourFrozen   = new Color(0.3f,  0.6f,  1f);

    [HideInInspector] public bool accountFrozen = false;

    private Coroutine freezeRoutine;
    private Coroutine slotRoutine;

    void Awake()
    {
        bankAmount   = startingBank;
        walletAmount = 0;
        RefreshBalanceText();
    }

    // ─────────────────────────────────────────────────────────
    // SLOT MACHINE DISPLAY
    // ─────────────────────────────────────────────────────────
    void AnimateToValue(int fromValue, int toValue)
    {
        if (activeBalanceText == null) return;
        if (slotRoutine != null) StopCoroutine(slotRoutine);
        slotRoutine = StartCoroutine(SlotRoutine(fromValue, toValue));
    }

    IEnumerator SlotRoutine(int fromValue, int toValue)
    {
        if (activeBalanceText == null) yield break;

        bool  adding      = toValue >= fromValue;
        Color targetColor = adding ? colourAdd : colourSubtract;

        activeBalanceText.color = targetColor;

        int   totalSteps   = Mathf.Abs(toValue - fromValue);
        float stepDuration = totalSteps > 0
            ? slotMachineDuration / totalSteps
            : slotMachineDuration;

        // If totalSteps is very large cap the frame rate
        // so it doesnt become invisible fast flicker
        float minStepDuration = 0.016f;
        if (stepDuration < minStepDuration)
        {
            int   cappedSteps    = Mathf.FloorToInt(
                slotMachineDuration / minStepDuration);
            stepDuration         = slotMachineDuration / cappedSteps;
            float stepSize       = (float)totalSteps / cappedSteps;
            int   direction      = adding ? 1 : -1;
            float current        = fromValue;

            for (int i = 0; i < cappedSteps; i++)
            {
                current += stepSize * direction;
                activeBalanceText.text =
                    "$" + Mathf.RoundToInt(current);
                yield return new WaitForSeconds(stepDuration);
            }
        }
        else
        {
            int current   = fromValue;
            int direction = adding ? 1 : -1;

            while (current != toValue)
            {
                current += direction;
                activeBalanceText.text = "$" + current;
                yield return new WaitForSeconds(stepDuration);
            }
        }

        // Always land exactly on final value
        activeBalanceText.text  = "$" + toValue;
        activeBalanceText.color = targetColor;

        // Fade back to default colour
        yield return StartCoroutine(
            FadeToColour(targetColor, colourDefault, 0.3f));

        slotRoutine = null;
    }

    IEnumerator FadeToColour(Color from, Color to, float duration)
    {
        if (activeBalanceText == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            activeBalanceText.color =
                Color.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        activeBalanceText.color = to;
    }

    // ─────────────────────────────────────────────────────────
    // FREEZE
    // ─────────────────────────────────────────────────────────
    public void FreezeAccount(float duration)
    {
        if (freezeRoutine != null)
            StopCoroutine(freezeRoutine);

        freezeRoutine = StartCoroutine(FreezeRoutine(duration));
    }

    IEnumerator FreezeRoutine(float duration)
    {
        accountFrozen = true;

        if (slotRoutine != null)
        {
            StopCoroutine(slotRoutine);
            slotRoutine = null;
        }

        if (activeBalanceText != null)
            activeBalanceText.color = colourFrozen;

        Debug.Log(gameObject.name + " account FROZEN for "
            + duration + "s");

        yield return new WaitForSeconds(duration);

        accountFrozen = false;
        freezeRoutine = null;

        if (activeBalanceText != null)
            activeBalanceText.color = colourDefault;

        Debug.Log(gameObject.name + " account UNFROZEN");
    }

    // ─────────────────────────────────────────────────────────
    // SAFE ADD — respects freeze
    // ─────────────────────────────────────────────────────────
    public bool TryAddToBank(int amount)
    {
        if (accountFrozen) return false;

        bankAmount += amount;
        RefreshBalanceDisplay();
        return true;
    }

    public bool TryAddToCurrentHealth(int amount, PlayerHealth health)
    {
        if (accountFrozen) return false;
        if (health == null) return false;

        int oldValue          = health.currentHealth;
        health.currentHealth += amount;
        UpdateBalanceFromHealth(health.currentHealth, oldValue);
        return true;
    }

    // ─────────────────────────────────────────────────────────
    // TAX
    // ─────────────────────────────────────────────────────────
    public int ApplyTax(float percentage, PlayerHealth health)
    {
        if (health == null) return 0;

        int taxAmount = Mathf.FloorToInt(health.currentHealth * percentage);
        taxAmount     = Mathf.Clamp(taxAmount, 0, health.currentHealth);

        int oldValue          = health.currentHealth;
        health.currentHealth -= taxAmount;
        UpdateBalanceFromHealth(health.currentHealth, oldValue);

        if (health.currentHealth <= 0)
        {
            health.currentHealth = 0;
            health.ForceDie();
        }

        return taxAmount;
    }

    // ─────────────────────────────────────────────────────────
    // SWAP
    // ─────────────────────────────────────────────────────────
    public static void SwapHealth(PlayerHealth a, PlayerHealth b)
    {
        if (a == null || b == null) return;

        int oldA        = a.currentHealth;
        int oldB        = b.currentHealth;

        a.currentHealth = oldB;
        b.currentHealth = oldA;

        PlayerWallet walletA = a.GetComponent<PlayerWallet>();
        PlayerWallet walletB = b.GetComponent<PlayerWallet>();

        if (walletA != null)
            walletA.UpdateBalanceFromHealth(a.currentHealth, oldA);
        if (walletB != null)
            walletB.UpdateBalanceFromHealth(b.currentHealth, oldB);

        if (a.currentHealth <= 0) a.ForceDie();
        if (b.currentHealth <= 0) b.ForceDie();
    }

    // ─────────────────────────────────────────────────────────
    // EXISTING METHODS
    // ─────────────────────────────────────────────────────────
    public void SetWalletFromSurvivedHealth(int survivedHealth)
    {
        walletAmount = survivedHealth;
        RefreshBalanceText();
    }

    public void ResetWalletOnDeath()
    {
        walletAmount  = 0;
        accountFrozen = false;

        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
            freezeRoutine = null;
        }

        if (slotRoutine != null)
        {
            StopCoroutine(slotRoutine);
            slotRoutine = null;
        }

        if (activeBalanceText != null)
            activeBalanceText.color = colourDefault;

        RefreshBalanceText();
    }

    public void MoveToWallet()
    {
        if (isReady)       return;
        if (accountFrozen) return;
        if (bankAmount < transferAmount) return;

        bankAmount   -= transferAmount;
        walletAmount += transferAmount;
        RefreshBalanceText();
    }

    public void MoveToBank()
    {
        if (isReady) return;
        if (walletAmount < transferAmount) return;

        walletAmount -= transferAmount;
        bankAmount   += transferAmount;
        RefreshBalanceText();
    }

    public bool CanReady()
    {
        return walletAmount >= transferAmount;
    }

    public void SetReady(bool ready) => isReady = ready;
    public void ResetReady()         => isReady = false;

    public void UpdateBalanceFromHealth(int currentHealth,
                                        int oldHealth = -1)
    {
        if (activeBalanceText == null) return;
        if (accountFrozen) return;

        if (oldHealth < 0) oldHealth = currentHealth;

        AnimateToValue(oldHealth, currentHealth);
    }

    public void RefreshBalanceDisplay()
    {
        if (activeBalanceText == null) return;

        if (slotRoutine != null)
        {
            StopCoroutine(slotRoutine);
            slotRoutine = null;
        }

        activeBalanceText.text  = "$" + bankAmount;
        activeBalanceText.color = colourDefault;
    }

    void RefreshBalanceText()
    {
        if (activeBalanceText == null) return;

        if (slotRoutine != null)
        {
            StopCoroutine(slotRoutine);
            slotRoutine = null;
        }

        activeBalanceText.text  = "$" + walletAmount;
        activeBalanceText.color = colourDefault;
    }
}