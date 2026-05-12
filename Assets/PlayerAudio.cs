using UnityEngine;

public class PlayerMovementAudio : MonoBehaviour
{
    [System.Serializable]
    public class SoundGroup
    {
        public AudioClip[] sounds;

        [Range(0f, 10f)]
        public float volume = 1f;
    }

    [Header("References")]
    public PlayerController2D controller;
    public Rigidbody2D rb;

    [Header("Jump")]
    public SoundGroup jumpSounds;

    [Header("Land")]
    public SoundGroup landSounds;

    [Header("Dash")]
    public SoundGroup dashSounds;

    [Header("Movement / Footsteps")]
    public SoundGroup movementSounds;

    [Tooltip("Delay between movement sounds")]
    public float movementInterval = 0.35f;

    [Header("Master Volume")]
    [Range(0f, 10f)]
    public float masterVolume = 1f;

    [Header("Movement Loop")]
    public AudioSource movementSource;

    private PlayerController2D.PlayerState previousState;
    private bool wasGrounded;
    private float movementTimer;

    void Start()
    {
        if (controller == null)
            controller = GetComponent<PlayerController2D>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        previousState = controller.currentState;
    }

    void Update()
    {
        // REMOVED HandleJump() - now called directly from PlayerController2D
        HandleLanding();
        HandleDash();
        HandleMovement();

        previousState = controller.currentState;
    }

    // NEW PUBLIC METHOD - called directly from PlayerController2D
    public void PlayJumpSound()
    {
        PlaySoundGroup(jumpSounds);
    }

    AudioClip GetRandomMovementClip()
    {
        if (movementSounds == null ||
            movementSounds.sounds == null ||
            movementSounds.sounds.Length == 0)
            return null;

        return movementSounds.sounds[
            Random.Range(0, movementSounds.sounds.Length)
        ];
    }

    void HandleLanding()
    {
        bool grounded =
            controller.currentState != PlayerController2D.PlayerState.Jumping;

        if (!wasGrounded && grounded)
        {
            PlaySoundGroup(landSounds);
        }

        wasGrounded = grounded;
    }

    void HandleDash()
    {
        if (previousState != PlayerController2D.PlayerState.Dashing &&
            controller.currentState == PlayerController2D.PlayerState.Dashing)
        {
            PlaySoundGroup(dashSounds);
        }
    }

    void HandleMovement()
    {
        bool moving =
            controller.currentState ==
            PlayerController2D.PlayerState.Moving;

        bool grounded =
            controller.currentState !=
            PlayerController2D.PlayerState.Jumping;

        bool hasVelocity =
            Mathf.Abs(rb.linearVelocity.x) > 0.2f;

        if (moving && grounded && hasVelocity)
        {
            if (!movementSource.isPlaying)
            {
                movementSource.clip = GetRandomMovementClip();
                movementSource.loop = true;
                movementSource.volume =
                    movementSounds.volume * masterVolume;
                movementSource.spatialBlend = 0f;
                movementSource.Play();
            }
        }
        else
        {
            if (movementSource.isPlaying)
            {
                movementSource.Stop();
            }
        }
    }

    void PlaySoundGroup(SoundGroup group)
    {
        if (group == null)
            return;

        if (group.sounds == null || group.sounds.Length == 0)
            return;

        AudioClip clip =
            group.sounds[
                Random.Range(0, group.sounds.Length)
            ];

        if (clip == null)
            return;

        GameObject tempAudio =
            new GameObject("TempPlayerAudio");

        tempAudio.transform.position =
            transform.position;

        AudioSource source =
            tempAudio.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume =
            group.volume * masterVolume;
        source.spatialBlend = 0f;
        source.rolloffMode =
            AudioRolloffMode.Linear;
        source.playOnAwake = false;
        source.loop = false;
        source.pitch =
            Random.Range(0.95f, 1.05f);

        source.Play();

        Destroy(
            tempAudio,
            clip.length + 0.1f
        );
    }
}