using UnityEngine;

public class CubeEscapeAudio : MonoBehaviour
{
    public AudioClip clipSuccess;  // Assign an audio clip in the Inspector
    public AudioClip clipOutOfBounds;  // Assign an audio clip in the Inspector
    public float volume = 1.0f; // Adjust the volume

    private AudioSource audioSource;

    void Start()
    {
        // Add an AudioSource component if not already present
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayOutOfBounds()
    {
        audioSource.PlayOneShot(clipOutOfBounds, volume);
    }

    public void PlaySuccess()
    {
        audioSource.PlayOneShot(clipSuccess, volume);
    }
}
