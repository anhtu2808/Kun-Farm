using UnityEngine;

/// <summary>
/// Movement audio system for character footstep sounds
/// Integrates with Movement.cs to play footstep sounds when player moves
/// </summary>
public class MovementAudio : MonoBehaviour
{
    [Header("Footstep Sound Settings")]
    [Tooltip("Array of footstep audio clips for variety")]
    public AudioClip[] footstepSounds;
    
    [Header("Audio Settings")]
    [Tooltip("Volume of footstep sounds (0-1)")]
    [Range(0f, 1f)]
    public float volume = 0.5f;
    
    [Tooltip("Time between footstep sounds (seconds)")]
    [Range(0.1f, 2f)]
    public float stepFrequency = 0.5f;
    
    [Tooltip("Random pitch variation for more natural sound")]
    [Range(0f, 0.5f)]
    public float pitchVariation = 0.1f;

    [Header("References")]
    [Tooltip("Movement component reference (auto-found if not assigned)")]
    public Movement movementComponent;

    // Private variables
    private AudioSource audioSource;
    private float stepTimer = 0f;
    private bool wasMovingLastFrame = false;
    private int lastSoundIndex = -1;

    void Start()
    {
        InitializeAudioComponent();
        FindMovementComponent();
    }

    void Update()
    {
        HandleMovementAudio();
    }

    /// <summary>
    /// Initialize AudioSource component
    /// </summary>
    private void InitializeAudioComponent()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource for footsteps
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    /// <summary>
    /// Find Movement component automatically if not assigned
    /// </summary>
    private void FindMovementComponent()
    {
        if (movementComponent == null)
        {
            movementComponent = GetComponent<Movement>();
            if (movementComponent == null)
            {
                movementComponent = FindObjectOfType<Movement>();
            }
        }

        if (movementComponent == null)
        {
            Debug.LogWarning("[MovementAudio] No Movement component found! Footstep sounds will not work.");
        }
    }

    /// <summary>
    /// Handle movement audio logic
    /// </summary>
    private void HandleMovementAudio()
    {
        if (movementComponent == null || footstepSounds == null || footstepSounds.Length == 0)
            return;

        bool isMoving = movementComponent.IsMoving();

        // Update step timer when moving
        if (isMoving)
        {
            stepTimer += Time.deltaTime;

            // Play footstep sound at intervals
            if (stepTimer >= stepFrequency)
            {
                PlayFootstepSound();
                stepTimer = 0f;
            }
        }
        else
        {
            // Reset timer when not moving
            stepTimer = 0f;
        }

        wasMovingLastFrame = isMoving;
    }

    /// <summary>
    /// Play a random footstep sound with pitch variation
    /// </summary>
    private void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0)
            return;

        // Select random sound (avoid playing same sound twice in a row if possible)
        int soundIndex = GetRandomSoundIndex();
        AudioClip soundToPlay = footstepSounds[soundIndex];

        if (soundToPlay != null)
        {
            // Apply volume
            audioSource.volume = volume;

            // Apply pitch variation for more natural sound
            float basePitch = 1f;
            float pitchOffset = Random.Range(-pitchVariation, pitchVariation);
            audioSource.pitch = basePitch + pitchOffset;

            // Play sound
            audioSource.PlayOneShot(soundToPlay);
        }

        lastSoundIndex = soundIndex;
    }

    /// <summary>
    /// Get random sound index, avoiding repetition when possible
    /// </summary>
    private int GetRandomSoundIndex()
    {
        if (footstepSounds.Length == 1)
            return 0;

        int index;
        do
        {
            index = Random.Range(0, footstepSounds.Length);
        } 
        while (index == lastSoundIndex && footstepSounds.Length > 1);

        return index;
    }

    /// <summary>
    /// Update volume setting at runtime
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Update step frequency at runtime
    /// </summary>
    public void SetStepFrequency(float newFrequency)
    {
        stepFrequency = Mathf.Clamp(newFrequency, 0.1f, 2f);
    }

    /// <summary>
    /// Update pitch variation at runtime
    /// </summary>
    public void SetPitchVariation(float newVariation)
    {
        pitchVariation = Mathf.Clamp(newVariation, 0f, 0.5f);
    }

    /// <summary>
    /// Check if movement audio is properly configured
    /// </summary>
    public bool IsConfigured()
    {
        return movementComponent != null && 
               footstepSounds != null && 
               footstepSounds.Length > 0 && 
               audioSource != null;
    }

    /// <summary>
    /// Force play a footstep sound (for testing)
    /// </summary>
    public void PlayTestFootstep()
    {
        PlayFootstepSound();
    }

    // Debug info for Inspector
    void OnValidate()
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}