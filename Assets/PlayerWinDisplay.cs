using UnityEngine;
using System.Collections.Generic;

public class PlayerWinDisplay : MonoBehaviour
{
    [Header("Hide on win (e.g. player icon above head)")]
    public SpriteRenderer spriteToHide;

    [Header("Show on win (e.g. crown, stars, effects)")]
    public List<GameObject> objectsToShow = new List<GameObject>();

    private PlayerCombat combat;

    void Awake()
    {
        combat = GetComponent<PlayerCombat>();
    }

    public void ShowWin()
    {
        if (spriteToHide != null)
            spriteToHide.enabled = false;

        foreach (GameObject obj in objectsToShow)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        if (combat != null)
            combat.SetWeaponVisualActive(false);
    }

    public void HideWin()
    {
        if (spriteToHide != null)
            spriteToHide.enabled = true;

        foreach (GameObject obj in objectsToShow)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        if (combat != null)
            combat.SetWeaponVisualActive(true);
    }
}