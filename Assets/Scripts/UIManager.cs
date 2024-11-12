using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Reference to the Canvas GameObject
    [SerializeField] private GameObject canvas;

    void Awake()
    {
        // Check if an instance of UIManager already exists
        if (Instance == null)
        {
            // Set the current instance as the singleton
            Instance = this;

            // Prevent the UIManager from being destroyed when switching scenes
            DontDestroyOnLoad(gameObject);
            
            // Ensure the Canvas persists across scenes
            PersistCanvas();
        }
        else
        {
            // If an instance already exists, destroy this duplicate
            Destroy(gameObject);
        }
    }

    // Ensure the Canvas persists
    private void PersistCanvas()
    {
        if (canvas != null)
        {
            DontDestroyOnLoad(canvas); // Persist the Canvas and all its children across scenes
        }
        else
        {
            Debug.LogError("Canvas is not assigned in the UIManager!");
        }
    }
}
