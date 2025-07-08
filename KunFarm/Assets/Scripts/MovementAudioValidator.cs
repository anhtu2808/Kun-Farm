using UnityEngine;

/// <summary>
/// Integration validation for MovementAudio system
/// Ensures proper connection with Movement.cs
/// </summary>
public class MovementAudioValidator : MonoBehaviour
{
    [Header("Validation Results")]
    [SerializeField] private bool movementComponentFound = false;
    [SerializeField] private bool audioComponentFound = false;
    [SerializeField] private bool configurationValid = false;
    [SerializeField] private string validationMessage = "";

    void Start()
    {
        ValidateIntegration();
    }

    /// <summary>
    /// Validate MovementAudio integration
    /// </summary>
    public void ValidateIntegration()
    {
        Debug.Log("[MovementAudioValidator] Starting validation...");

        // Check for Movement component
        Movement movement = FindObjectOfType<Movement>();
        movementComponentFound = (movement != null);
        
        if (!movementComponentFound)
        {
            validationMessage = "ERROR: No Movement component found in scene";
            Debug.LogError(validationMessage);
            return;
        }

        // Check for MovementAudio component
        MovementAudio movementAudio = FindObjectOfType<MovementAudio>();
        audioComponentFound = (movementAudio != null);
        
        if (!audioComponentFound)
        {
            validationMessage = "WARNING: No MovementAudio component found. Add MovementAudio to Player GameObject.";
            Debug.LogWarning(validationMessage);
            return;
        }

        // Check configuration
        configurationValid = movementAudio.IsConfigured();
        
        if (!configurationValid)
        {
            validationMessage = "WARNING: MovementAudio not properly configured. Check footstep sounds and component references.";
            Debug.LogWarning(validationMessage);
        }
        else
        {
            validationMessage = "SUCCESS: MovementAudio system properly integrated and configured!";
            Debug.Log(validationMessage);
        }

        // Test Movement.IsMoving() method availability
        TestMovementMethods(movement);
        
        // Test MovementAudio methods
        TestAudioMethods(movementAudio);
    }

    /// <summary>
    /// Test Movement component methods
    /// </summary>
    private void TestMovementMethods(Movement movement)
    {
        try
        {
            bool isMoving = movement.IsMoving();
            Vector3 facingDirection = movement.GetFacingDirection();
            float currentSpeed = movement.GetCurrentSpeed();
            
            Debug.Log($"[MovementAudioValidator] Movement methods test:");
            Debug.Log($"  - IsMoving(): {isMoving}");
            Debug.Log($"  - GetFacingDirection(): {facingDirection}");
            Debug.Log($"  - GetCurrentSpeed(): {currentSpeed}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MovementAudioValidator] Movement methods test failed: {e.Message}");
        }
    }

    /// <summary>
    /// Test MovementAudio component methods
    /// </summary>
    private void TestAudioMethods(MovementAudio movementAudio)
    {
        try
        {
            bool isConfigured = movementAudio.IsConfigured();
            
            Debug.Log($"[MovementAudioValidator] MovementAudio methods test:");
            Debug.Log($"  - IsConfigured(): {isConfigured}");
            Debug.Log($"  - Volume: {movementAudio.volume}");
            Debug.Log($"  - Step Frequency: {movementAudio.stepFrequency}");
            Debug.Log($"  - Pitch Variation: {movementAudio.pitchVariation}");
            Debug.Log($"  - Footstep Sounds Count: {(movementAudio.footstepSounds?.Length ?? 0)}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MovementAudioValidator] MovementAudio methods test failed: {e.Message}");
        }
    }

    /// <summary>
    /// Public method to rerun validation (callable from Inspector)
    /// </summary>
    [ContextMenu("Revalidate Integration")]
    public void RevalidateIntegration()
    {
        ValidateIntegration();
    }
}