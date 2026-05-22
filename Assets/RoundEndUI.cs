using UnityEngine;
using System.Collections.Generic;

public class RoundEndUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Player Slots")]
    public List<RoundEndPlayerSlot> playerSlots =
        new List<RoundEndPlayerSlot>();

    private List<PlayerHealth> currentPlayers =
        new List<PlayerHealth>();

    private bool waitingForReady = false;

    // ─────────────────────────────────────────────

    void Update()
    {
        if (!waitingForReady) return;

        for (int i = 0; i < currentPlayers.Count; i++)
        {
            PlayerHealth p = currentPlayers[i];
            if (p == null) continue;

            PlayerWallet wallet = p.GetComponent<PlayerWallet>();
            PlayerController2D controller = p.GetComponent<PlayerController2D>();

            if (wallet == null || controller == null) continue;

            if (!wallet.isReady)
            {
                // ───── WALLET → BANK
                if (Input.GetKeyDown(controller.dashKey))
                {
                    if (wallet.bankAmount >= wallet.transferAmount)
                    {
                        wallet.MoveToWallet();

                        RefreshSlot(i);

                        // money OUT of bank
                        playerSlots[i].PlayBankImpulse(false);

                        // money IN to wallet
                        playerSlots[i].PlayWalletImpulse(true);
                    }
                }

                // ───── BANK ← WALLET
                if (Input.GetKeyDown(controller.jumpKey))
                {
                    if (wallet.walletAmount >= wallet.transferAmount)
                    {
                        wallet.MoveToBank();

                        RefreshSlot(i);

                        // money IN to bank
                        playerSlots[i].PlayBankImpulse(true);

                        // money OUT of wallet
                        playerSlots[i].PlayWalletImpulse(false);
                    }
                }

                // ───── READY
                if (Input.GetKeyDown(controller.shootKey))
                {
                    if (wallet.CanReady())
                    {
                        wallet.SetReady(true);

                        if (i < playerSlots.Count && playerSlots[i] != null)
                            playerSlots[i].SetReady(true);
                    }
                    else
                    {
                        if (i < playerSlots.Count && playerSlots[i] != null)
                            playerSlots[i].ShowCannotReady();
                    }
                }
            }
            else
            {
                // UNREADY
                if (Input.GetKeyDown(controller.shootKey))
                {
                    wallet.SetReady(false);

                    if (i < playerSlots.Count && playerSlots[i] != null)
                        playerSlots[i].SetReady(false);

                    RefreshSlot(i);
                }
            }
        }

        // ─────────────────────────────────────────────
        // ALL READY CHECK
        // ─────────────────────────────────────────────

        bool allReady = true;

        foreach (PlayerHealth p in currentPlayers)
        {
            if (p == null) continue;

            PlayerWallet wallet = p.GetComponent<PlayerWallet>();

            if (wallet != null && !wallet.isReady)
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
        {
            waitingForReady = false;
            GameManager.Instance.OnAllPlayersReady();
        }
    }

    // ─────────────────────────────────────────────

    public void ShowRoundEnd(List<PlayerHealth> players, bool isGameOver)
    {
        currentPlayers = players;

        panel.SetActive(true);
        waitingForReady = !isGameOver;

        for (int i = 0; i < playerSlots.Count; i++)
        {
            if (playerSlots[i] == null) continue;

            bool active = i < players.Count;
            playerSlots[i].gameObject.SetActive(active);

            if (active)
            {
                PlayerWallet wallet = players[i].GetComponent<PlayerWallet>();

                playerSlots[i].Initialise(
                    "P" + (i + 1),
                    wallet,
                    isGameOver,
                    i
                );
            }
        }
    }

    // ─────────────────────────────────────────────

    public void Hide()
    {
        panel.SetActive(false);
        waitingForReady = false;
    }

    // ─────────────────────────────────────────────

    void RefreshSlot(int index)
    {
        if (index >= playerSlots.Count) return;
        if (playerSlots[index] == null) return;

        PlayerWallet wallet =
            currentPlayers[index].GetComponent<PlayerWallet>();

        playerSlots[index].Refresh(wallet);
    }
}