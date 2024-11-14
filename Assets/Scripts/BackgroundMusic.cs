using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;

    void Awake()
    {
        // Check if there's already an instance of BackgroundMusic
        if (instance == null)
        {
            // If no instance exists, set this one as the instance and mark it to not be destroyed
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this duplicate
            Destroy(gameObject);
        }
    }
}
