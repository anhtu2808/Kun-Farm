using UnityEngine;

public class ChoppingAudio : MonoBehaviour
{
    // Kéo file âm thanh bạn muốn phát vào đây từ cửa sổ Project
    public AudioClip choppingSound;

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
    }

    // Đây là hàm chúng ta sẽ gọi từ Animation Event
    public void PlayChoppingSound()
    {
        if (choppingSound != null)
        {
            // PlayOneShot cho phép phát âm thanh mà không làm gián đoạn âm thanh khác
            audioSource.PlayOneShot(choppingSound);
        }
    }
}