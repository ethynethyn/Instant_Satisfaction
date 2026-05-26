using UnityEngine;

[CreateAssetMenu(menuName = "Arena Fighter/Projectile Effect")]
public class ProjectileEffectData : ScriptableObject
{
    public ProjectileEffectType effectType;

    [Header("Freeze Settings")]
    [Tooltip("How long the account is frozen in seconds")]
    public float freezeDuration = 3f;

    [Header("Tax Settings")]
    [Tooltip("Percentage of wallet removed (0.3 = 30%)")]
    [Range(0f, 1f)]
    public float taxPercentage = 0.3f;

    [Header("Scam Settings")]
    [Tooltip("How much of the damage dealt is added to shooter bank")]
    [Range(0f, 1f)]
    public float scamHarvestRate = 1f;

    [Header("Visual Feedback")]
    public Color projectileColour = Color.white;
    public GameObject hitParticlePrefab;
}
