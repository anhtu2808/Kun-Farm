using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupAudio : MonoBehaviour
{
    [Header("Pickup Sound")]
    public AudioClip pickupSound; // Chỉ cần 1 âm thanh cho tất cả item

    private AudioSource audioSource;

    void Start()
    {
        // Lấy component AudioSource, nếu chưa có thì thêm một cái mới
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Cài đặt AudioSource
        audioSource.playOnAwake = false;
        audioSource.volume = 0.7f; // Âm lượng vừa phải
    }

    public void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }
}
