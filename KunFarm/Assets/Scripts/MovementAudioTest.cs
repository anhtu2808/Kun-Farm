using UnityEngine;

/// <summary>
/// Simple test script to validate MovementAudio integration
/// This can be temporarily added to test the footstep system
/// </summary>
public class MovementAudioTest : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTests = false;
    public KeyCode testKey = KeyCode.T;
    
    private MovementAudio audioComponent;
    private Movement movementComponent;
    
    void Start()
    {
        if (!runTests) return;
        
        audioComponent = GetComponent<MovementAudio>();
        movementComponent = GetComponent<Movement>();
        
        Debug.Log("[MovementAudioTest] Test script initialized");
        
        if (audioComponent == null)
            Debug.LogError("[MovementAudioTest] MovementAudio component not found!");
        if (movementComponent == null)
            Debug.LogError("[MovementAudioTest] Movement component not found!");
    }
    
    void Update()
    {
        if (!runTests) return;
        
        // Test manual footstep trigger
        if (Input.GetKeyDown(testKey))
        {
            TestFootstepSystem();
        }
        
        // Log movement state changes
        if (movementComponent != null)
        {
            bool isMoving = movementComponent.IsMoving();
            if (isMoving)
            {
                // Only log occasionally to avoid spam
                if (Time.frameCount % 60 == 0) // Every 60 frames (roughly 1 second at 60fps)
                {
                    Debug.Log("[MovementAudioTest] Player is moving - footstep audio should be playing");
                }
            }
        }
    }
    
    private void TestFootstepSystem()
    {
        Debug.Log("[MovementAudioTest] Running footstep system tests...");
        
        if (audioComponent != null)
        {
            // Test current settings
            Debug.Log("Current Volume: " + audioComponent.GetVolume());
            Debug.Log("Current Interval: " + audioComponent.GetFootstepInterval());
            Debug.Log("Audio Enabled: " + audioComponent.IsFootstepAudioEnabled());
            
            // Test manual footstep
            audioComponent.PlayFootstepSoundManual();
            Debug.Log("Manual footstep sound triggered");
            
            // Test volume adjustment
            float originalVolume = audioComponent.GetVolume();
            audioComponent.SetVolume(0.8f);
            Debug.Log("Volume changed from " + originalVolume + " to " + audioComponent.GetVolume());
        }
        else
        {
            Debug.LogError("[MovementAudioTest] MovementAudio component not available for testing");
        }
    }
}