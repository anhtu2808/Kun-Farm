using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiggingAudio : MonoBehaviour
{
    [Header("Digging Audio Settings")]
    // Kéo file âm thanh đào đất vào đây từ cửa sổ Project
    public AudioClip diggingSound;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.5f, 2f)]
    public float pitch = 1f;

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

        // Cấu hình AudioSource cho sound effects
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
    }

    // Hàm này sẽ được gọi từ Animation Event hoặc từ Tool
    public void PlayDiggingSound()
    {
        if (diggingSound != null && audioSource != null)
        {
            // PlayOneShot cho phép phát âm thanh mà không làm gián đoạn âm thanh khác
            audioSource.PlayOneShot(diggingSound, volume);
            Debug.Log("Playing digging sound: " + diggingSound.name);
        }
        else
        {
            Debug.LogWarning("DiggingSound or AudioSource is null!");
        }
    }

    // Hàm phát âm thanh với pitch ngẫu nhiên để tạo sự đa dạng
    public void PlayDiggingSoundWithRandomPitch()
    {
        if (diggingSound != null && audioSource != null)
        {
            // Tạo pitch ngẫu nhiên từ 0.8 đến 1.2 để âm thanh không bị lặp lại nhàm chán
            float randomPitch = Random.Range(0.8f, 1.2f);
            audioSource.pitch = randomPitch;
            audioSource.PlayOneShot(diggingSound, volume);

            // Reset pitch về giá trị gốc
            audioSource.pitch = pitch;
        }
    }
}
