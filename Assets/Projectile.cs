using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    [System.Serializable]
    
    
    public class CollisionSound
    {
        [Header("Detection")]
        public string tagName;
        public LayerMask layers;

        [Header("Sounds")]
        public AudioClip[] sounds;

        [Range(0f, 2f)]
        public float volume = 1f;
    }

    [System.Serializable]
    public class CollisionParticle
    {
        [Header("Detection")]
        public string tagName;
        public LayerMask layers;

        [Header("Particles")]
        public GameObject[] particlePrefabs;
    }
    
    public enum ProjectileEffectType
    {
        FreezeAccount,
        Tax,
        Scam,
        SwapWallet
    }

    private Vector2 direction;
    private float speed;
    private float knockback;
    private int damage;
    private int ownerInstanceID;
    private Vector3 startPos;

    [Header("Projectile")]
    public float maxDistance = 10f;

    [Header("Impact Particles")]
    public GameObject defaultImpactParticle;

    [Tooltip("Different particles for different tags/layers")]
    public List<CollisionParticle> collisionParticles =
        new List<CollisionParticle>();

    [Header("Ricochet")]
    public string[] ricochetTags;

    [Tooltip("Random angle added to ricochets")]
    public float ricochetSpread = 25f;

    private int remainingRicochets;

    [Header("Audio")]
    [Range(0f, 2f)]
    public float masterImpactVolume = 1f;

    [Tooltip("Fallback sound if no match is found")]
    public AudioClip defaultImpactSound;

    [Tooltip("Different sounds for different tags/layers")]
    public List<CollisionSound> collisionSounds =
        new List<CollisionSound>();

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(
        Vector2 dir,
        float projectileSpeed,
        float kb,
        GameObject ownerObject,
        int ricochetCount = 0,
        int projectileDamage = 50
    )
    {
        direction = dir.normalized;
        speed = projectileSpeed;
        knockback = kb;
        damage = projectileDamage;

        remainingRicochets = ricochetCount;

        ownerInstanceID = ownerObject
            .transform.root
            .gameObject
            .GetInstanceID();

        startPos = transform.position;

        UpdateRotation();
    }

    void Update()
    {
        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);

        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        int hitRootID =
            other.transform.root.gameObject.GetInstanceID();

        if (hitRootID == ownerInstanceID)
            return;

        PlayImpactSound(other);
        SpawnImpactParticles(other);

        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            Vector2 kb = direction * knockback;
            health.TakeHit(kb, damage);
            Destroy(gameObject);
            return;
        }

        bool ricochetSurface = false;
        foreach (string tag in ricochetTags)
        {
            if (other.CompareTag(tag))
            {
                ricochetSurface = true;
                break;
            }
        }

        if (ricochetSurface && remainingRicochets > 0)
        {
            remainingRicochets--;

            Vector2 normal = GetCollisionNormal(other);

            Vector2 reflectedDirection =
                Vector2.Reflect(direction, normal).normalized;

            float randomAngle =
                Random.Range(-ricochetSpread, ricochetSpread);

            reflectedDirection =
                Quaternion.Euler(0, 0, randomAngle) * reflectedDirection;

            direction = reflectedDirection.normalized;

            transform.position += (Vector3)(direction * 0.05f);

            UpdateRotation();
            return;
        }

        Destroy(gameObject);
    }

    Vector2 GetCollisionNormal(Collider2D other)
    {
        Vector2 closestPoint = other.ClosestPoint(transform.position);
        Vector2 normal =
            ((Vector2)transform.position - closestPoint).normalized;
        return normal;
    }

    void UpdateRotation()
    {
        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (spriteRenderer != null)
            spriteRenderer.flipY = direction.x < 0;
    }

    private void SpawnImpactParticles(Collider2D other)
    {
        GameObject particlePrefab = null;

        foreach (CollisionParticle collision in collisionParticles)
        {
            bool tagMatch =
                !string.IsNullOrEmpty(collision.tagName) &&
                other.CompareTag(collision.tagName);

            bool layerMatch =
                ((1 << other.gameObject.layer) & collision.layers) != 0;

            if (tagMatch || layerMatch)
            {
                if (collision.particlePrefabs != null &&
                    collision.particlePrefabs.Length > 0)
                {
                    particlePrefab =
                        collision.particlePrefabs[
                            Random.Range(0, collision.particlePrefabs.Length)
                        ];
                    break;
                }
            }
        }

        if (particlePrefab == null)
            particlePrefab = defaultImpactParticle;

        if (particlePrefab == null) return;

        Vector2 hitPoint = other.ClosestPoint(transform.position);
        Vector2 normal = GetCollisionNormal(other);
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, normal);

        GameObject particles = Instantiate(particlePrefab, hitPoint, rotation);

        ParticleSystem ps = particles.GetComponent<ParticleSystem>();
        if (ps != null)
            Destroy(particles, ps.main.duration + ps.main.startLifetime.constantMax);
        else
            Destroy(particles, 5f);
    }

    private void PlayImpactSound(Collider2D other)
    {
        AudioClip clipToPlay = null;
        float finalVolume = masterImpactVolume;

        foreach (CollisionSound collision in collisionSounds)
        {
            bool tagMatch =
                !string.IsNullOrEmpty(collision.tagName) &&
                other.CompareTag(collision.tagName);

            bool layerMatch =
                ((1 << other.gameObject.layer) & collision.layers) != 0;

            if (tagMatch || layerMatch)
            {
                if (collision.sounds != null && collision.sounds.Length > 0)
                {
                    clipToPlay =
                        collision.sounds[
                            Random.Range(0, collision.sounds.Length)
                        ];
                    finalVolume = collision.volume * masterImpactVolume;
                    break;
                }
            }
        }

        if (clipToPlay == null)
            clipToPlay = defaultImpactSound;

        if (clipToPlay == null) return;

        GameObject tempAudio = new GameObject("ImpactSound");
        tempAudio.transform.position = transform.position;

        AudioSource source = tempAudio.AddComponent<AudioSource>();
        source.clip = clipToPlay;
        source.volume = finalVolume;
        source.spatialBlend = 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.playOnAwake = false;
        source.loop = false;
        source.pitch = Random.Range(0.95f, 1.05f);
        source.Play();

        Destroy(tempAudio, clipToPlay.length + 0.1f);
    }
}