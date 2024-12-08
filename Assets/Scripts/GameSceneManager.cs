using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

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

    private bool cutsceneEnd = false;

    public Vector3 storedPosition;



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
        // LoadScene(currentScene);
        Debug.Log(currentScene);


    }
    private void Start()
    {
        StartCoroutine(WaitForPlayerAndRestore());
    }

    private IEnumerator WaitForPlayerAndRestore()
    {
        yield return new WaitUntil(() => Player.Instance != null && Player.Instance.isLoaded); // Add a flag in Player to indicate loading completion
        RestoreDestroyedItems();
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    // Find all players in the scen
        

        Transform playerTransform = Player.Instance.transform;

        // Handle blackout if power is off
        if (powerOff)
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

        // Handle cutscene logic if cutscene hasn't ended
        if (!cutsceneEnd)
        {
            StartCoroutine(FadeToGroundFloor(playerTransform));  // Start fade effect
            cutsceneEnd = true;
        }

        // Handle scene transitions based on previous and current scene
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
       

        // Update last scene after processing
        if (lastScene != scene.name)
        {
            lastScene = scene.name;
        }

        storedPosition = playerTransform.position;
        RestoreDestroyedItems();

    
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

    public void RestoreDestroyedItems()
    {
        // Get the set of destroyed items for the current scene
        HashSet<string> destroyedItems = ItemStateManager.Instance.GetDestroyedItemsForScene(currentScene);

        // Loop through each destroyed item and restore it
        foreach (string itemId in destroyedItems)
        {
            // Find the item by its name (assuming itemId is the name of the item)
            GameObject item = GameObject.Find(itemId);
            
            if (item != null)
            {
                // Restore the item (set it active)
                item.SetActive(false);
                Debug.Log("Item " + itemId + " is restored and enabled.");
            }
            else
            {
                Debug.LogWarning("Item with ID " + itemId + " not found in the scene.");
            }
        }
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

    private IEnumerator FadeToGroundFloor(Transform playerTransform)
{
    // Find and activate the FadeOut object (assumes it's already in the scene)
    GameObject fadeOut = GameObject.Find("FadeOut");
    if (fadeOut != null)
    {
        fadeOut.SetActive(true);

        // Get the SpriteRenderer or CanvasGroup for fading
        SpriteRenderer spriteRenderer = fadeOut.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("FadeOut object doesn't have a SpriteRenderer component!");
            yield break;
        }

        // Start with full opacity
        Color startColor = spriteRenderer.color;
        startColor.a = 1f;  // Set opacity to 1 (fully visible)
        spriteRenderer.color = startColor;

        // Gradually decrease the opacity over time (fade out)
        float fadeDuration = 2f;  // Duration of the fade effect
        float startTime = Time.time;

        while (Time.time - startTime < fadeDuration)
        {
            float lerpFactor = (Time.time - startTime) / fadeDuration;
            Color newColor = spriteRenderer.color;
            newColor.a = Mathf.Lerp(1f, 0f, lerpFactor);  // Fade from 1 to 0
            spriteRenderer.color = newColor;

            yield return null;  // Wait until next frame
        }

        // Ensure the opacity is fully 0 after the fade
        Color finalColor = spriteRenderer.color;
        finalColor.a = 0f;
        spriteRenderer.color = finalColor;
    }

 
    // Deactivate fade-out after the transition
    if (fadeOut != null)
    {
        fadeOut.SetActive(false);
    }
}

}
