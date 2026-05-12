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

    private Vector2 direction;
    private float speed;
    private float knockback;
    private int ownerInstanceID;

    private Vector3 startPos;

    [Header("Projectile")]
    public float maxDistance = 10f;

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
        GameObject ownerObject
    )
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

        // ROTATE PROJECTILE TO MATCH DIRECTION
        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0, 0, angle);

        // OPTIONAL SPRITE FLIP
        if (spriteRenderer != null)
        {
            spriteRenderer.flipY = direction.x < 0;
        }
    }

    void Update()
    {
        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);

        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
        {
            Debug.Log("Projectile destroyed: max distance reached");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Projectile hit: " + other.name);

        // IGNORE OWNER COLLISION
        int hitRootID =
            other.transform.root.gameObject.GetInstanceID();

        if (hitRootID == ownerInstanceID)
        {
            Debug.Log("Hit own collider, ignoring");
            return;
        }

        // PLAY IMPACT SOUND
        PlayImpactSound(other);

        // CHECK FOR PLAYER HEALTH
        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            Debug.Log(
                "PlayerHealth found on: "
                + health.gameObject.name
            );

            Vector2 kb = direction * knockback;

            health.TakeHit(kb);

            Destroy(gameObject);
            return;
        }

        Debug.Log(
            "No PlayerHealth found — destroying projectile"
        );

        Destroy(gameObject);
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
                if (collision.sounds != null &&
                    collision.sounds.Length > 0)
                {
                    clipToPlay =
                        collision.sounds[
                            Random.Range(0, collision.sounds.Length)
                        ];

                    finalVolume =
                        collision.volume * masterImpactVolume;

                    break;
                }
            }
        }

        if (clipToPlay == null)
        {
            clipToPlay = defaultImpactSound;
        }

        if (clipToPlay == null)
            return;

        // CREATE TEMP AUDIO OBJECT
        GameObject tempAudio =
            new GameObject("ImpactSound");

        tempAudio.transform.position =
            transform.position;

        AudioSource source =
            tempAudio.AddComponent<AudioSource>();

        source.clip = clipToPlay;

        // IMPORTANT SETTINGS
        source.volume = finalVolume;

        source.spatialBlend = 0f; // FULL 2D SOUND

        source.rolloffMode =
            AudioRolloffMode.Linear;

        source.playOnAwake = false;

        source.loop = false;

        source.pitch =
            Random.Range(0.95f, 1.05f);

        source.Play();

        Destroy(
            tempAudio,
            clipToPlay.length + 0.1f
        );
    }
}