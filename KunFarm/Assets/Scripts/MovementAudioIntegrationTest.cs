using UnityEngine;

/// <summary>
/// Complete integration test for Movement Audio System
/// Tests the interaction between Movement.cs and MovementAudio.cs
/// </summary>
public class MovementAudioIntegrationTest : MonoBehaviour
{
    [Header("Test Components")]
    public Movement movementComponent;
    public MovementAudio movementAudioComponent;
    
    [Header("Test Results")]
    [SerializeField] private bool integrationPassed = false;
    [SerializeField] private string lastTestResult = "";
    
    private bool isTestingMovement = false;
    private float testTimer = 0f;

    void Start()
    {
        FindComponents();
        RunInitialTests();
    }

    void Update()
    {
        // Run continuous integration test
        if (isTestingMovement)
        {
            ContinuousIntegrationTest();
        }

        // Test controls
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartMovementTest();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            StopMovementTest();
        }
    }

    /// <summary>
    /// Find required components
    /// </summary>
    private void FindComponents()
    {
        if (movementComponent == null)
            movementComponent = FindObjectOfType<Movement>();
        
        if (movementAudioComponent == null)
            movementAudioComponent = FindObjectOfType<MovementAudio>();
    }

    /// <summary>
    /// Run initial integration tests
    /// </summary>
    private void RunInitialTests()
    {
        Debug.Log("[MovementAudioIntegrationTest] Running initial tests...");

        // Test 1: Components exist
        bool componentsExist = TestComponentsExist();
        
        // Test 2: Configuration valid
        bool configurationValid = TestConfiguration();
        
        // Test 3: Method accessibility
        bool methodsAccessible = TestMethodAccess();
        
        integrationPassed = componentsExist && configurationValid && methodsAccessible;
        
        lastTestResult = integrationPassed ? 
            "✅ All integration tests passed!" : 
            "❌ Some integration tests failed. Check debug log.";
            
        Debug.Log($"[MovementAudioIntegrationTest] {lastTestResult}");
    }

    /// <summary>
    /// Test if components exist and are accessible
    /// </summary>
    private bool TestComponentsExist()
    {
        bool movementExists = movementComponent != null;
        bool audioExists = movementAudioComponent != null;
        
        Debug.Log($"[Test] Movement component exists: {movementExists}");
        Debug.Log($"[Test] MovementAudio component exists: {audioExists}");
        
        return movementExists && audioExists;
    }

    /// <summary>
    /// Test component configuration
    /// </summary>
    private bool TestConfiguration()
    {
        if (movementAudioComponent == null) return false;
        
        bool isConfigured = movementAudioComponent.IsConfigured();
        Debug.Log($"[Test] MovementAudio configured: {isConfigured}");
        
        return isConfigured;
    }

    /// <summary>
    /// Test method accessibility between components
    /// </summary>
    private bool TestMethodAccess()
    {
        if (movementComponent == null || movementAudioComponent == null) 
            return false;

        try
        {
            // Test Movement.IsMoving() method access
            bool isMoving = movementComponent.IsMoving();
            Debug.Log($"[Test] Movement.IsMoving() accessible: true, value: {isMoving}");
            
            // Test MovementAudio public methods
            movementAudioComponent.SetVolume(0.5f);
            movementAudioComponent.SetStepFrequency(0.5f);
            Debug.Log("[Test] MovementAudio public methods accessible: true");
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Test] Method access failed: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Start movement simulation test
    /// </summary>
    public void StartMovementTest()
    {
        isTestingMovement = true;
        testTimer = 0f;
        Debug.Log("[MovementAudioIntegrationTest] Starting movement simulation test...");
        Debug.Log("Move the character to test footstep audio integration!");
    }

    /// <summary>
    /// Stop movement simulation test
    /// </summary>
    public void StopMovementTest()
    {
        isTestingMovement = false;
        Debug.Log("[MovementAudioIntegrationTest] Movement test stopped.");
    }

    /// <summary>
    /// Continuous integration test while movement testing is active
    /// </summary>
    private void ContinuousIntegrationTest()
    {
        testTimer += Time.deltaTime;
        
        // Log movement state every 2 seconds
        if (testTimer >= 2f)
        {
            if (movementComponent != null && movementAudioComponent != null)
            {
                bool isMoving = movementComponent.IsMoving();
                Vector3 facingDirection = movementComponent.GetFacingDirection();
                float currentSpeed = movementComponent.GetCurrentSpeed();
                
                Debug.Log($"[Integration Test] Movement Status:");
                Debug.Log($"  - Is Moving: {isMoving}");
                Debug.Log($"  - Facing Direction: {facingDirection}");
                Debug.Log($"  - Current Speed: {currentSpeed:F2}");
                Debug.Log($"  - Audio Configured: {movementAudioComponent.IsConfigured()}");
            }
            
            testTimer = 0f;
        }
    }

    /// <summary>
    /// Manual test trigger for Inspector
    /// </summary>
    [ContextMenu("Run Integration Test")]
    public void RunManualTest()
    {
        RunInitialTests();
    }

    /// <summary>
    /// Test footstep sound manually
    /// </summary>
    [ContextMenu("Test Footstep Sound")]
    public void TestFootstepSound()
    {
        if (movementAudioComponent != null)
        {
            movementAudioComponent.PlayTestFootstep();
            Debug.Log("[MovementAudioIntegrationTest] Manual footstep test triggered");
        }
    }
}