using UnityEngine;

/// <summary>
/// Example script showing how to set up and use the MovementAudio system
/// This demonstrates the complete integration with the player character
/// </summary>
public class MovementAudioSetupExample : MonoBehaviour
{
    [Header("Setup Example")]
    [Tooltip("Run this to automatically configure MovementAudio")]
    public bool autoSetupMovementAudio = false;
    
    [Header("Example Audio Clips")]
    [Tooltip("Example footstep sounds - drag audio files here")]
    public AudioClip[] exampleFootstepSounds;
    
    void Start()
    {
        if (autoSetupMovementAudio)
        {
            SetupMovementAudio();
        }
    }
    
    /// <summary>
    /// Automatically set up MovementAudio system on this GameObject
    /// </summary>
    private void SetupMovementAudio()
    {
        // Check if Movement component exists
        Movement movementComponent = GetComponent<Movement>();
        if (movementComponent == null)
        {
            Debug.LogError("[MovementAudioSetup] Movement component not found! MovementAudio requires Movement component.");
            return;
        }
        
        // Check if MovementAudio already exists
        MovementAudio existingAudio = GetComponent<MovementAudio>();
        if (existingAudio != null)
        {
            Debug.Log("[MovementAudioSetup] MovementAudio component already exists.");
            ConfigureExistingAudio(existingAudio);
            return;
        }
        
        // Add MovementAudio component
        MovementAudio audioComponent = gameObject.AddComponent<MovementAudio>();
        
        Debug.Log("[MovementAudioSetup] MovementAudio component added successfully!");
        
        // Configure the audio component
        ConfigureExistingAudio(audioComponent);
    }
    
    /// <summary>
    /// Configure an existing MovementAudio component with example settings
    /// </summary>
    private void ConfigureExistingAudio(MovementAudio audioComponent)
    {
        // Apply example audio clips if available
        if (exampleFootstepSounds != null && exampleFootstepSounds.Length > 0)
        {
            // Note: Cannot directly set public arrays in runtime without reflection
            // In actual usage, users should drag clips to the inspector
            Debug.Log("[MovementAudioSetup] " + exampleFootstepSounds.Length + " example footstep sounds available.");
            Debug.Log("[MovementAudioSetup] Please drag these to the 'Footstep Sounds' array in the MovementAudio component inspector.");
        }
        
        // Configure recommended settings
        audioComponent.SetVolume(0.6f);  // Moderate volume
        audioComponent.SetFootstepInterval(0.4f);  // Nice walking pace
        audioComponent.SetFootstepAudioEnabled(true);
        
        Debug.Log("[MovementAudioSetup] MovementAudio configured with recommended settings:");
        Debug.Log("- Volume: " + audioComponent.GetVolume());
        Debug.Log("- Footstep Interval: " + audioComponent.GetFootstepInterval());
        Debug.Log("- Audio Enabled: " + audioComponent.IsFootstepAudioEnabled());
        
        Debug.Log("[MovementAudioSetup] Setup complete! Add footstep audio clips to the inspector to hear sounds.");
    }
    
    /// <summary>
    /// Public method to manually trigger setup (can be called from buttons, etc.)
    /// </summary>
    public void ManualSetup()
    {
        SetupMovementAudio();
    }
    
    void Update()
    {
        // Example of runtime audio control
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleFootstepAudio();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AdjustVolume();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AdjustInterval();
        }
    }
    
    /// <summary>
    /// Example: Toggle footstep audio on/off
    /// </summary>
    private void ToggleFootstepAudio()
    {
        MovementAudio audioComponent = GetComponent<MovementAudio>();
        if (audioComponent != null)
        {
            bool isEnabled = audioComponent.IsFootstepAudioEnabled();
            audioComponent.SetFootstepAudioEnabled(!isEnabled);
            Debug.Log("[MovementAudioSetup] Footstep audio " + (isEnabled ? "disabled" : "enabled"));
        }
    }
    
    /// <summary>
    /// Example: Cycle through different volume levels
    /// </summary>
    private void AdjustVolume()
    {
        MovementAudio audioComponent = GetComponent<MovementAudio>();
        if (audioComponent != null)
        {
            float currentVolume = audioComponent.GetVolume();
            float newVolume = currentVolume >= 0.9f ? 0.3f : currentVolume + 0.3f;
            audioComponent.SetVolume(newVolume);
            Debug.Log("[MovementAudioSetup] Volume adjusted to: " + newVolume);
        }
    }
    
    /// <summary>
    /// Example: Cycle through different footstep intervals
    /// </summary>
    private void AdjustInterval()
    {
        MovementAudio audioComponent = GetComponent<MovementAudio>();
        if (audioComponent != null)
        {
            float currentInterval = audioComponent.GetFootstepInterval();
            float newInterval = currentInterval >= 0.8f ? 0.2f : currentInterval + 0.2f;
            audioComponent.SetFootstepInterval(newInterval);
            Debug.Log("[MovementAudioSetup] Footstep interval adjusted to: " + newInterval);
        }
    }
}