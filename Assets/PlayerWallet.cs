using UnityEngine;
using TMPro;

public class PlayerWallet : MonoBehaviour
{
    [Header("Starting Values")]
    public int startingBank = 500;
    public int transferAmount = 50;

    [HideInInspector] public int bankAmount;
    [HideInInspector] public int walletAmount;

    [HideInInspector] public bool isReady = false;

    [Header("HUD")]
    public TextMeshProUGUI activeBalanceText;

    void Awake()
    {
        bankAmount = startingBank;
        walletAmount = 0;

        RefreshBalanceText();
    }

    public void SetWalletFromSurvivedHealth(int survivedHealth)
    {
        walletAmount = survivedHealth;
        RefreshBalanceText();
    }

    public void ResetWalletOnDeath()
    {
        walletAmount = 0;
        RefreshBalanceText();
    }

    public void MoveToWallet()
    {
        if (isReady) return;
        if (bankAmount < transferAmount) return;

        bankAmount -= transferAmount;
        walletAmount += transferAmount;

        RefreshBalanceText();
    }

    public void MoveToBank()
    {
        if (isReady) return;
        if (walletAmount < transferAmount) return;

        walletAmount -= transferAmount;
        bankAmount += transferAmount;

        RefreshBalanceText();
    }

    public bool CanReady()
    {
        return walletAmount >= transferAmount;
    }

    public void SetReady(bool ready)
    {
        isReady = ready;
    }

    public void ResetReady()
    {
        isReady = false;
    }

    public void UpdateBalanceFromHealth(int currentHealth)
    {
        if (activeBalanceText != null)
            activeBalanceText.text = "$" + currentHealth;
    }

    void RefreshBalanceText()
    {
        if (activeBalanceText != null)
            activeBalanceText.text = "$" + walletAmount;
    }
}