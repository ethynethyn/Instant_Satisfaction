using UnityEngine;

[System.Serializable]
public class RoundEndUIJuiceProfile : MonoBehaviour
{
    [Header("GLOBAL JUICE MULTIPLIER")]
    [Range(0f, 2f)] public float globalIntensity = 1f;

    [Header("BANK IMPACT")]
    public float bankTiltStrength = 28f;
    public float bankTiltDuration = 0.45f;
    public float bankSpringDamping = 4f;

    [Header("WALLET IMPACT")]
    public float walletSquashX = 1.35f;
    public float walletSquashY = 0.70f;
    public float walletStretchX = 0.70f;
    public float walletStretchY = 1.35f;
    public float walletDuration = 0.35f;
    public float walletDamping = 3.5f;

    [Header("READY FEEDBACK")]
    public float cannotReadyFlashTime = 1.5f;

    [Header("COLOUR JUICE")]
    public float readyPulseSpeed = 2f;
    public float readyPulseStrength = 0.15f;

    [Header("EXTRA FEEL")]
    public bool randomSlotVariation = true;
    [Range(0f, 0.3f)] public float slotVariation = 0.12f;
}