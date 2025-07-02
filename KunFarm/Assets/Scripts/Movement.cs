using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f; // Speed of the movement
    
    [Header("References")]
    public Animator animator;
    
    // Speed modifier for PlayerStats system
    private float speedModifier = 1f;
    
    private Vector3 direction; // Direction of movement
    private Vector3 lastMovementDirection = Vector3.down; // Default facing down
    private Vector3 lastFacingDirection = Vector3.down; // Keep track of facing for idle animation
    
    void Start()
    {
        // Auto-find animator if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();
        AnimateMovement(direction);
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
    }
    
    private void HandleInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        direction = new Vector3(horizontal, vertical, 0);
        
        // Store last movement direction for facing direction
        if (direction.magnitude > 0.1f)
        {
            lastMovementDirection = direction.normalized;
            // Also update facing direction for animation
            lastFacingDirection = GetPrimaryDirection(lastMovementDirection);
        }
    }
    
    /// <summary>
    /// Convert movement direction to primary direction (up/down/left/right)
    /// </summary>
    private Vector3 GetPrimaryDirection(Vector3 dir)
    {
        dir = dir.normalized;
        
        // Determine primary direction based on larger component
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            // Horizontal movement is stronger
            return dir.x > 0 ? Vector3.right : Vector3.left;
    }
        else
        {
            // Vertical movement is stronger (or equal)
            return dir.y > 0 ? Vector3.up : Vector3.down;
        }
    }
    
    private void HandleMovement()
    {
        // Move the player (only in FixedUpdate to avoid double movement)
        float currentSpeed = speed * speedModifier;
        transform.position += direction.normalized * currentSpeed * Time.fixedDeltaTime;
    }
    
    void AnimateMovement(Vector3 direction)
       {
            if (animator != null)
            {
                if (direction.magnitude > 0)
                {
                // Moving: use current movement direction
                    animator.SetBool("isMoving", true);
                    animator.SetFloat("horizontal", direction.x);
                    animator.SetFloat("vertical", direction.y);
                }
                else
                {
                // Idle: use last facing direction to maintain facing
                    animator.SetBool("isMoving", false);
                animator.SetFloat("horizontal", lastFacingDirection.x);
                animator.SetFloat("vertical", lastFacingDirection.y);
            }
        }
    }
    
    /// <summary>
    /// Get current movement direction for other scripts (ToolManager)
    /// </summary>
    public Vector3 GetMovementDirection()
    {
        return direction;
    }
    
    /// <summary>
    /// Get player's current facing direction based on last movement
    /// Used by ToolManager for hoeing direction
    /// </summary>
    public Vector3 GetFacingDirection()
    {
        return lastFacingDirection; // Use primary direction for consistent facing
    }
    
    /// <summary>
    /// Check if player is currently moving
    /// </summary>
    public bool IsMoving()
    {
        return direction.magnitude > 0.1f;
    }
    
    /// <summary>
    /// Get the Animator component for ToolManager
    /// </summary>
    public Animator GetAnimator()
    {
        return animator;
    }
    
    /// <summary>
    /// Set speed modifier for PlayerStats system
    /// </summary>
    public void SetSpeedModifier(float modifier)
    {
        speedModifier = Mathf.Clamp(modifier, 0f, 2f); // Clamp để tránh giá trị quá cao/thấp
    }
    
    /// <summary>
    /// Get current effective speed (including modifier)
    /// </summary>
    public float GetCurrentSpeed()
    {
        return speed * speedModifier;
    }
    
    /// <summary>
    /// Get current speed modifier
    /// </summary>
    public float GetSpeedModifier()
    {
        return speedModifier;
    }
}
