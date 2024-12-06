using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public static void Play(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{clipName}");
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }
}
