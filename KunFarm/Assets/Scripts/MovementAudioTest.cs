using UnityEngine;

/// <summary>
/// Test script for MovementAudio functionality
/// Provides debug methods to test footstep sounds
/// </summary>
public class MovementAudioTest : MonoBehaviour
{
    [Header("Test Settings")]
    public MovementAudio movementAudioComponent;
    public bool showDebugInfo = true;

    void Start()
    {
        // Auto-find MovementAudio if not assigned
        if (movementAudioComponent == null)
        {
            movementAudioComponent = GetComponent<MovementAudio>();
            if (movementAudioComponent == null)
            {
                movementAudioComponent = FindObjectOfType<MovementAudio>();
            }
        }

        if (showDebugInfo)
        {
            Debug.Log("[MovementAudioTest] Starting movement audio test...");
            TestConfiguration();
        }
    }

    void Update()
    {
        // Test controls (only in Unity Editor)
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TestSingleFootstep();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TestVolumeControl();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestFrequencyControl();
        }
        #endif
    }

    /// <summary>
    /// Test MovementAudio configuration
    /// </summary>
    private void TestConfiguration()
    {
        if (movementAudioComponent == null)
        {
            Debug.LogError("[MovementAudioTest] No MovementAudio component found!");
            return;
        }

        bool isConfigured = movementAudioComponent.IsConfigured();
        Debug.Log($"[MovementAudioTest] MovementAudio configured: {isConfigured}");

        if (!isConfigured)
        {
            Debug.LogWarning("[MovementAudioTest] MovementAudio is not properly configured. Check footstep sounds and movement component.");
        }
    }

    /// <summary>
    /// Test playing a single footstep sound
    /// </summary>
    public void TestSingleFootstep()
    {
        if (movementAudioComponent != null)
        {
            movementAudioComponent.PlayTestFootstep();
            if (showDebugInfo)
                Debug.Log("[MovementAudioTest] Played test footstep");
        }
    }

    /// <summary>
    /// Test volume control
    /// </summary>
    public void TestVolumeControl()
    {
        if (movementAudioComponent != null)
        {
            float testVolume = Random.Range(0.1f, 1f);
            movementAudioComponent.SetVolume(testVolume);
            if (showDebugInfo)
                Debug.Log($"[MovementAudioTest] Set volume to: {testVolume:F2}");
        }
    }

    /// <summary>
    /// Test frequency control
    /// </summary>
    public void TestFrequencyControl()
    {
        if (movementAudioComponent != null)
        {
            float testFrequency = Random.Range(0.2f, 1f);
            movementAudioComponent.SetStepFrequency(testFrequency);
            if (showDebugInfo)
                Debug.Log($"[MovementAudioTest] Set step frequency to: {testFrequency:F2}");
        }
    }

    /// <summary>
    /// Log current MovementAudio status
    /// </summary>
    public void LogStatus()
    {
        if (movementAudioComponent == null)
        {
            Debug.Log("[MovementAudioTest] No MovementAudio component found");
            return;
        }

        Debug.Log($"[MovementAudioTest] MovementAudio Status:");
        Debug.Log($"  - Configured: {movementAudioComponent.IsConfigured()}");
        Debug.Log($"  - Has footstep sounds: {movementAudioComponent.footstepSounds != null && movementAudioComponent.footstepSounds.Length > 0}");
        Debug.Log($"  - Volume: {movementAudioComponent.volume}");
        Debug.Log($"  - Step frequency: {movementAudioComponent.stepFrequency}");
        Debug.Log($"  - Pitch variation: {movementAudioComponent.pitchVariation}");
    }
}