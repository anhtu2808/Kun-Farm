using UnityEngine;

public class WateringAudio : MonoBehaviour
{
    // Kéo file âm thanh bạn muốn phát vào đây từ cửa sổ Project
    public AudioClip wateringSound;

    // Component để phát âm thanh
    private AudioSource audioSource;

    void Start()
    {
        // Lấy component AudioSource, nếu chưa có thì thêm một cái mới
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = 0.3f;
    }

    // Đây là hàm chúng ta sẽ gọi từ Animation Event
    public void PlayWateringSound()
    {
        if (wateringSound != null)
        {
            // PlayOneShot cho phép phát âm thanh mà không làm gián đoạn âm thanh khác
            audioSource.PlayOneShot(wateringSound);
        }
    }
}