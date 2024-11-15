using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;  

    private string lastScene;
    private string lastEnteredVent;
    private Transform spawnVent1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  
        }
        else
        {
            Destroy(gameObject);  
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Player player = FindObjectOfType<Player>();  

        if (player == null)
        {
            Debug.LogError("Player object not found in the scene!");
            return;
        }

        Transform playerTransform = player.transform; 

        if (scene.name == "Vents" && lastScene != "Vents")
        {
            spawnVent1 = GameObject.Find("Spawn_" + lastEnteredVent).transform;
            playerTransform.position = spawnVent1.position;
        }
        else if (scene.name == "GroundFloor" && lastScene == "Vents")
        {
            Transform spawnGroundFloor = GameObject.Find("Spawn_" + lastEnteredVent).transform;
            playerTransform.position = spawnGroundFloor.position;
        }
        else if (scene.name == "Basement")
        {
            Transform basementSpawn = GameObject.Find("Basement_Spawn").transform;
            playerTransform.position = basementSpawn.position;
        }
        

        if (lastScene != scene.name)
        {
            lastScene = scene.name;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;  
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;  
    }

    public void SetLastEnteredVent(string ventName)
    {
        lastEnteredVent = ventName;
    }

    public void SetLastScene()
    {
        lastScene = SceneManager.GetActiveScene().name;
        Debug.Log("Last scene set to: " + lastScene);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        Debug.Log("Loading scene: " + sceneName);
    }

    public void CheckAndLoadBasement(Transform playerTransform, GameObject stairsBasement)
    {
        if ((playerTransform.position - stairsBasement.transform.position).sqrMagnitude <= 1)
        {
            Debug.Log("Entering Basement");
            SceneManager.LoadScene("Basement", LoadSceneMode.Single);
        }
    }

    public void RestartScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex, LoadSceneMode.Single);
        Debug.Log("Restarting scene: " + SceneManager.GetActiveScene().name);
    }

    public string GetLastScene()
    {
        return lastScene;
    }
}
