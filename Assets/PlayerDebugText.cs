using UnityEngine;
using TMPro;

public class PlayerDebugText : MonoBehaviour
{
    public TextMeshProUGUI textUI;

    private string weapon = "Unarmed";

    private PlayerController2D.PlayerState state;

    private bool firing;
    private bool reloading;

    private bool isWinner = false; // 🔥 NEW

    void Update()
    {
        UpdateText();
    }

    public void SetWeapon(string weaponName)
    {
        weapon = weaponName;
    }

    public void SetState(PlayerController2D.PlayerState newState)
    {
        state = newState;
    }

    public void SetFiring(bool value)
    {
        firing = value;
    }

    public void SetReloading(bool value)
    {
        reloading = value;
    }

    // 🔥 NEW
    public void SetWinner(bool value)
    {
        isWinner = value;
    }

    void UpdateText()
    {
        // 🔥 HIGHEST PRIORITY
        if (isWinner)
        {
            textUI.text = "WINNER";
            return;
        }

        // PRIORITY:
        // Reloading > Firing > Jumping > Dashing > Moving > Weapon

        if (reloading)
        {
            textUI.text = "Reloading";
            return;
        }

        if (firing)
        {
            textUI.text = "Firing";
            return;
        }

        if (state == PlayerController2D.PlayerState.Jumping)
        {
            textUI.text = "Jumping";
            return;
        }

        if (state == PlayerController2D.PlayerState.Dashing)
        {
            textUI.text = "Dashing";
            return;
        }

        if (state == PlayerController2D.PlayerState.Moving)
        {
            textUI.text = "Moving";
            return;
        }

        textUI.text = weapon;
    }
}