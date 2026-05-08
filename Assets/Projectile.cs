using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float knockback;
    private int ownerInstanceID;

    private Vector3 startPos;

    public float maxDistance = 10f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector2 dir, float projectileSpeed, float kb, GameObject ownerObject)
    {
        direction = dir.normalized;
        speed = projectileSpeed;
        knockback = kb;

        ownerInstanceID = ownerObject
            .GetComponentInParent<Transform>()
            .root
            .gameObject
            .GetInstanceID();

        startPos = transform.position;

        // Flip projectile sprite based on direction
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);

        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
        {
            Debug.Log("Projectile destroyed: max distance reached");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Projectile hit: " + other.name + " on " + other.gameObject.name);

        // Ignore owner collision
        int hitRootID = other.transform.root.gameObject.GetInstanceID();

        if (hitRootID == ownerInstanceID)
        {
            Debug.Log("Hit own collider, ignoring");
            return;
        }

        // Look for PlayerHealth in parents
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            Debug.Log("PlayerHealth found on: " + health.gameObject.name + " — applying hit");

            Vector2 kb = direction * knockback;

            health.TakeHit(kb);

            Destroy(gameObject);
            return;
        }

        Debug.Log("No PlayerHealth found — destroying projectile on: " + other.name);

        Destroy(gameObject);
    }
}