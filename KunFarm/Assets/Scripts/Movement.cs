using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f; // Speed of the movement

    [Header("References")]
    public Animator animator;

    [Header("Audio Settings")]
    public AudioSource audioSource; // AudioSource component
    public AudioClip[] footstepSounds; // Mảng âm thanh tiếng bước
    public float footstepInterval = 0.5f; // Khoảng thời gian giữa các tiếng bước
    [Range(0f, 1f)]
    public float footstepVolume = 0.5f; // Âm lượng tiếng bước
    public bool randomizePitch = true; // Có ngẫu nhiên hóa pitch không
    [Range(0.8f, 1.2f)]
    public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)]
    public float maxPitch = 1.1f;

    // Speed modifier for PlayerStats system
    private float speedModifier = 1f;

    private Vector3 direction; // Direction of movement
    private Vector3 lastMovementDirection = Vector3.down; // Default facing down
    private Vector3 lastFacingDirection = Vector3.down; // Keep track of facing for idle animation

    // Audio variables
    private float footstepTimer;
    private bool wasMoving = false;
    private int lastFootstepIndex = -1; // Để tránh phát cùng 1 âm thanh liên tiếp

    void Start()
    {
        // Auto-find animator if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();

        // Auto-find AudioSource if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // Tạo AudioSource nếu chưa có
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Configure AudioSource
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    void Update()
    {
        HandleInput();
        AnimateMovement(direction);
        HandleFootstepAudio();
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
    /// Xử lý âm thanh tiếng bước chân
    /// </summary>
    private void HandleFootstepAudio()
    {
        bool isCurrentlyMoving = IsMoving();

        // PHÁT ÂM THANH NGAY KHI BẮT ĐẦU DI CHUYỂN
        if (isCurrentlyMoving && !wasMoving)
        {
            PlayFootstepSound();
            footstepTimer = 0f; // Reset timer
        }

        // Tiếp tục phát âm thanh trong quá trình di chuyển
        if (isCurrentlyMoving)
        {
            // Tính interval dựa trên tốc độ - làm cho phát nhanh hơn
            float adjustedInterval = footstepInterval / (GetCurrentSpeed() * 0.2f); // Tăng hệ số để phát nhanh hơn

            footstepTimer += Time.deltaTime;

            if (footstepTimer >= adjustedInterval)
            {
                PlayFootstepSound();
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        wasMoving = isCurrentlyMoving;
    }

    /// <summary>
    /// Phát âm thanh tiếng bước
    /// </summary>
    private void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0 || audioSource == null)
            return;

        // Chọn âm thanh ngẫu nhiên (tránh lặp lại âm thanh trước đó)
        int soundIndex;
        if (footstepSounds.Length == 1)
        {
            soundIndex = 0;
        }
        else
        {
            do
            {
                soundIndex = Random.Range(0, footstepSounds.Length);
            } while (soundIndex == lastFootstepIndex && footstepSounds.Length > 1);
        }

        lastFootstepIndex = soundIndex;

        // Thiết lập âm thanh
        audioSource.clip = footstepSounds[soundIndex];
        audioSource.volume = footstepVolume;

        // Ngẫu nhiên hóa pitch nếu được bật
        if (randomizePitch)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
        }
        else
        {
            audioSource.pitch = 1f;
        }

        // Phát âm thanh
        audioSource.Play();
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

    /// <summary>
    /// Dừng âm thanh bước chân (có thể gọi từ script khác)
    /// </summary>
    public void StopFootstepAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        footstepTimer = 0f;
    }

    /// <summary>
    /// Thiết lập âm lượng tiếng bước (có thể gọi từ Settings)
    /// </summary>
    public void SetFootstepVolume(float volume)
    {
        footstepVolume = Mathf.Clamp01(volume);
    }
}