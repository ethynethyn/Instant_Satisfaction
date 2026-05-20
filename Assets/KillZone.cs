using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health != null)
            health.TakeHit(Vector2.zero);
    }
}