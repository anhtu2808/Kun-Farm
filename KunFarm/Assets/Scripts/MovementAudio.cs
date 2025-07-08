using UnityEngine;

public class MovementAudio : MonoBehaviour
{
    [Header("Footstep Audio Settings")]
    [Tooltip("Array of footstep audio clips for variety")]
    public AudioClip[] footstepSounds;
    
    [Header("Audio Configuration")]
    [Tooltip("Volume of footstep sounds (0-1)")]
    [Range(0f, 1f)]
    public float volume = 0.5f;
    
    [Tooltip("Time interval between footstep sounds (in seconds)")]
    [Range(0.1f, 1f)]
    public float footstepInterval = 0.4f;
    
    [Tooltip("Enable/disable footstep audio")]
    public bool enableFootstepAudio = true;
    
    [Header("Debug")]
    [Tooltip("Show debug information in console")]
    public bool showDebugInfo = false;
    
    // Private components and variables
    private AudioSource audioSource;
    private Movement movementScript;
    private float footstepTimer = 0f;
    private bool wasMovingLastFrame = false;
    
    void Start()
    {
        InitializeComponents();
    }
    
    void Update()
    {
        if (enableFootstepAudio && movementScript != null)
        {
            HandleFootstepAudio();
        }
    }
    
    /// <summary>
    /// Initialize required components
    /// </summary>
    private void InitializeComponents()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure AudioSource settings
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;
        
        // Get Movement component
        movementScript = GetComponent<Movement>();
        if (movementScript == null)
        {
            Debug.LogError("[MovementAudio] Movement component not found on " + gameObject.name);
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[MovementAudio] Initialized on " + gameObject.name + 
                     " with " + (footstepSounds != null ? footstepSounds.Length : 0) + " footstep sounds");
        }
    }
    
    /// <summary>
    /// Handle footstep audio logic based on movement state
    /// </summary>
    private void HandleFootstepAudio()
    {
        bool isCurrentlyMoving = movementScript.IsMoving();
        
        // Player started moving
        if (isCurrentlyMoving && !wasMovingLastFrame)
        {
            footstepTimer = 0f; // Reset timer when starting to move
            if (showDebugInfo)
                Debug.Log("[MovementAudio] Player started moving");
        }
        // Player stopped moving
        else if (!isCurrentlyMoving && wasMovingLastFrame)
        {
            if (showDebugInfo)
                Debug.Log("[MovementAudio] Player stopped moving");
        }
        
        // Update footstep timer and play sounds while moving
        if (isCurrentlyMoving)
        {
            footstepTimer += Time.deltaTime;
            
            if (footstepTimer >= footstepInterval)
            {
                PlayFootstepSound();
                footstepTimer = 0f; // Reset timer
            }
        }
        
        wasMovingLastFrame = isCurrentlyMoving;
    }
    
    /// <summary>
    /// Play a random footstep sound
    /// </summary>
    private void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0)
        {
            if (showDebugInfo)
                Debug.LogWarning("[MovementAudio] No footstep sounds assigned!");
            return;
        }
        
        // Select a random footstep sound
        int randomIndex = Random.Range(0, footstepSounds.Length);
        AudioClip selectedClip = footstepSounds[randomIndex];
        
        if (selectedClip != null)
        {
            // Update volume before playing
            audioSource.volume = volume;
            audioSource.PlayOneShot(selectedClip, volume);
            
            if (showDebugInfo)
                Debug.Log("[MovementAudio] Playing footstep sound: " + selectedClip.name);
        }
        else
        {
            if (showDebugInfo)
                Debug.LogWarning("[MovementAudio] Footstep sound at index " + randomIndex + " is null!");
        }
    }
    
    /// <summary>
    /// Manually play a footstep sound (for external calls)
    /// </summary>
    public void PlayFootstepSoundManual()
    {
        PlayFootstepSound();
    }
    
    /// <summary>
    /// Set volume at runtime
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
            audioSource.volume = volume;
    }
    
    /// <summary>
    /// Set footstep interval at runtime
    /// </summary>
    public void SetFootstepInterval(float newInterval)
    {
        footstepInterval = Mathf.Clamp(newInterval, 0.1f, 1f);
    }
    
    /// <summary>
    /// Enable/disable footstep audio at runtime
    /// </summary>
    public void SetFootstepAudioEnabled(bool enabled)
    {
        enableFootstepAudio = enabled;
        
        if (showDebugInfo)
            Debug.Log("[MovementAudio] Footstep audio " + (enabled ? "enabled" : "disabled"));
    }
    
    /// <summary>
    /// Get current volume setting
    /// </summary>
    public float GetVolume()
    {
        return volume;
    }
    
    /// <summary>
    /// Get current footstep interval setting
    /// </summary>
    public float GetFootstepInterval()
    {
        return footstepInterval;
    }
    
    /// <summary>
    /// Check if footstep audio is enabled
    /// </summary>
    public bool IsFootstepAudioEnabled()
    {
        return enableFootstepAudio;
    }
}