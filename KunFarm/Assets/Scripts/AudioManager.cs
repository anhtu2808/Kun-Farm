
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        // Force unmute Unity audio
        AudioListener.volume = 1.0f;

        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            Debug.Log("AudioSource found! Clip: " + (audioSource.clip ? audioSource.clip.name : "NULL"));
            Debug.Log("Volume: " + audioSource.volume);
            Debug.Log("AudioListener Volume: " + AudioListener.volume);

            // Đảm bảo settings đúng
            audioSource.playOnAwake = true;
            audioSource.loop = true;
            audioSource.mute = false; // Force unmute

            // Force play
            audioSource.Play();
            Debug.Log("Force Play() called");

            Invoke("CheckAudioStatus", 1f);
        }
        else
        {
            Debug.LogError("No AudioSource found!");
        }
    }


    void CheckAudioStatus()
    {
        Debug.Log("=== FINAL CHECK ===");
        Debug.Log("Is playing: " + audioSource.isPlaying);
        Debug.Log("Time: " + audioSource.time);
        Debug.Log("Audio time length: " + audioSource.clip.length);
    }
}