using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Tooltip("Frames for opening: Doors_1 → Doors_3 → Doors_2")]
    public Sprite[] openSequence;    // size = 3
    [Tooltip("Frames for closing: Doors_2 → Doors_3 → Doors_1")]
    public Sprite[] closeSequence;   // size = 3
    [Tooltip("Seconds per frame")]
    public float stepTime = 0.1f;

    [Tooltip("Collider that blocks the player when door is closed")]
    public Collider2D blockingCollider;

    private SpriteRenderer spriteRenderer;
    private Coroutine animCoroutine;
    private bool isOpen = false;
    private bool playerAtDoor = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Initialize as closed
        if (openSequence != null && openSequence.Length > 0)
            spriteRenderer.sprite = openSequence[0];
        if (blockingCollider != null)
            blockingCollider.enabled = true;
    }

    void Update()
    {
        if (playerAtDoor && Input.GetKeyDown(KeyCode.E))
            ToggleDoor();
    }

    void ToggleDoor()
    {
        isOpen = !isOpen;
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateDoor(isOpen));
    }

    IEnumerator AnimateDoor(bool opening)
    {
        Sprite[] seq = opening ? openSequence : closeSequence;

        foreach (var frame in seq)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(stepTime);
        }

        if (blockingCollider != null)
            blockingCollider.enabled = !opening;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerAtDoor = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerAtDoor = false;
    }
}
