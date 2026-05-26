using UnityEngine;

[CreateAssetMenu(menuName = "Arena Fighter/Shift")]
public class ShiftData : ScriptableObject
{
    public string shiftName;

    [Header("Visuals")]
    public Sprite shiftSprite;

    [Header("Reward")]
    public int walletReward = 50;

    [Header("Audio")]
    public AudioClip pickupSound;
    [Range(0f, 1f)]
    public float pickupVolume = 1f;
}
