using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;  

    private string lastScene;

    private string currentScene;
    private string lastEnteredVent;
    private Transform spawnVent1;
    private string currentGateway;

    private bool powerOff;

    private GameObject blackoutScreen;

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

        if(powerOff)
        {

            GameObject blackout = GameObject.Find("BlackOut");
            
            if (blackout == null) 
            {
                // Search all objects (including inactive ones)
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "BlackOut")
                    {
                        blackout = obj;
                        break;
                    }
                }
            }

            if (blackout != null)
            {
                blackout.SetActive(true); // Enable the GameObject
                Debug.Log("BlackOut GameObject activated.");
            }
            else
            {
                Debug.LogError("BlackOut GameObject not found!");
            }
        }


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
            Transform spawn = GameObject.Find("Spawn_GroundToBasement").transform;
            playerTransform.position = spawn.position;
            currentScene = scene.name;

        }
        else if (scene.name == "GroundFloor" && lastScene == "Basement")
        {
            Transform spawn = GameObject.Find("Spawn_BasementToGround").transform;
            playerTransform.position = spawn.position;
            currentScene = scene.name;

        }
        else if (scene.name == "TopFloor" && lastScene == "GroundFloor")
        {
            Transform spawn = GameObject.Find("Spawn_GroundToTop").transform;
            playerTransform.position = spawn.position;
            currentScene = scene.name;

        }
        else if (scene.name == "GroundFloor" && lastScene == "TopFloor")
        {
            Transform spawn = GameObject.Find("Spawn_TopToGround").transform;
            playerTransform.position = spawn.position;
            currentScene = scene.name;

        }
        else if (scene.name == "Outside" && lastScene == "TopFloor")
        {
            Transform spawn;
            if (currentGateway == "Window_1")
            {
                spawn = GameObject.Find("Spawn_WindowToOut1").transform;

            }
            else if (currentGateway == "Window_2")
            {
                spawn = GameObject.Find("Spawn_WindowToOut2").transform;
            }
            else
            {
                spawn = null;
            }
            playerTransform.position = spawn.position;
            currentScene = scene.name;

        }
        else if (scene.name == "TopFloor" && lastScene == "Outside")
        {
            Transform spawn;
            if (currentGateway == "Window_1")
            {
                spawn = GameObject.Find("Spawn_Window1").transform;

            }
            else if (currentGateway == "Window_2")
            {
                spawn = GameObject.Find("Spawn_Window2").transform;
            }
            else
            {
                spawn = null;
            }
            playerTransform.position = spawn.position;
            currentScene = scene.name;

        }
        else if (scene.name == "Outside" && lastScene == "Vents")
        {
            Transform spawn = GameObject.Find("Spawn_VentToOut").transform;
            playerTransform.position = spawn.position;
            currentScene = scene.name;

        }
        else if (scene.name == "Vents" && lastScene == "Outside")
        {
            Transform spawn = GameObject.Find("Spawn_OutToVent").transform;
            playerTransform.position = spawn.position;
            currentScene = scene.name;

        } 
        else if (scene.name == "GroundFloor" && lastScene == "Outside")
        {
            Transform spawn = GameObject.Find("Spawn_OutsideToGround").transform;
            playerTransform.position = spawn.position;
            currentScene = scene.name;

        } 
        else if (scene.name == "Outside" && lastScene == "GroundFloor")
        {
        
            Transform spawn = GameObject.Find("Spawn_GroundToOutside").transform;
            playerTransform.position = spawn.position;
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

    public void TurnOffPower()
    {
        powerOff = true;
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
        currentGateway = gateway.name;
        if (currentGateway == "Window_1" || currentGateway == "Window_2")
        {
            SceneManager.LoadScene(room, LoadSceneMode.Single);
            return;

        }
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
