using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;  

    private string lastScene;

    private string currentScene;
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
        string currentScene = SceneManager.GetActiveScene().name;

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
            currentScene = scene.name;
        }
        else if (scene.name == "GroundFloor" && lastScene == "Vents")
        {
            Transform spawnGroundFloor = GameObject.Find("Spawn_" + lastEnteredVent).transform;
            playerTransform.position = spawnGroundFloor.position;
            currentScene = scene.name;

        }
        else if (scene.name == "Basement" && lastScene == "GroundFloor")
        {
            Transform basementSpawn = GameObject.Find("Spawn_GroundToBasement").transform;
            playerTransform.position = basementSpawn.position;
            currentScene = scene.name;

        }
        else if (scene.name == "GroundFloor" && lastScene == "Basement")
        {
            Transform basementSpawn = GameObject.Find("Spawn_BasementToGround").transform;
            playerTransform.position = basementSpawn.position;
            currentScene = scene.name;

        }
        else if (scene.name == "TopFloor" && lastScene == "GroundFloor")
        {
            Transform basementSpawn = GameObject.Find("Spawn_GroundToTop").transform;
            playerTransform.position = basementSpawn.position;
            currentScene = scene.name;

        }
        else if (scene.name == "GroundFloor" && lastScene == "TopFloor")
        {
            Transform basementSpawn = GameObject.Find("Spawn_TopToGround").transform;
            playerTransform.position = basementSpawn.position;
            currentScene = scene.name;

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

    public string getCurrentScene()
    {
        return SceneManager.GetActiveScene().name;
    }

   

    public void RoomTransition(Transform playerTransform, GameObject gateway, string room)
    {
       
        if ((playerTransform.position - gateway.transform.position).sqrMagnitude <= 0.5)
        {
            SceneManager.LoadScene(room, LoadSceneMode.Single);
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
