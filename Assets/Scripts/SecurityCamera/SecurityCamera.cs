using System.Collections;
using UnityEngine;
using UnityEngine.UI;  // For accessing the UI Slider

public class SecurityCamera : MonoBehaviour
{
    [SerializeField] private FieldOfView fieldOfView;
    [SerializeField] private float alertnessIncreaseRate = 15f;
    private Transform player;
    private Player playerScript;

    private bool isDisabled = false; // Flag to check if the camera is disabled
    private Transform cameraLight; // Reference to the child object of the camera
    private SpriteRenderer cameraLightRenderer; // For opacity fading
    private float fadeDuration = 1f; // Duration of the fade effect
    private float originalAlpha = 1f; // Store the original opacity

    // Timer slider UI (will be found dynamically)
    private Slider timerSlider;
    [SerializeField] private float disableDuration = 5f;  // Duration for the camera to remain disabled

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerScript = player.GetComponent<Player>();
        fieldOfView = GetComponent<FieldOfView>();

        // Cache the child transform (assuming the child object is named "Light")
        cameraLight = transform.Find("Light"); // Adjust "Light" to your actual child name

        if (cameraLight == null)
        {
            Debug.LogWarning("No child object found with the name 'Light'!");
        }
        else
        {
            cameraLightRenderer = cameraLight.GetComponent<SpriteRenderer>();
            if (cameraLightRenderer == null)
            {
                Debug.LogWarning("No SpriteRenderer found on the camera light object!");
            }
            else
            {
                // Capture the initial opacity (alpha) of the camera light
                originalAlpha = cameraLightRenderer.color.a;
            }
        }

        // Find the Canvas object and Timer (Slider) inside it
        Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas != null)
        {
            timerSlider = canvas.transform.Find("Timer")?.GetComponent<Slider>();  // Timer is the Slider inside the Canvas
        }

        if (timerSlider == null)
        {
            Debug.LogWarning("No Timer (Slider) found inside the Canvas!");
        }
        else
        {
            // Initialize the timer slider UI
            timerSlider.gameObject.SetActive(false);  // Initially hidden
        }
    }

    void Update()
    {
        // Skip detection logic if the camera is disabled
        if (isDisabled) return;

        if (fieldOfView.targetDetected)
        {
            HandlePlayerDetection();
        }
    }

    private void HandlePlayerDetection()
    {
        Vector2 directionToPlayer = player.position - transform.position;
        float distanceToPlayerSqr = directionToPlayer.sqrMagnitude;

        float alertnessIncrease = alertnessIncreaseRate / Mathf.Max(distanceToPlayerSqr, 1f);
        playerScript.IncreaseAlertness(alertnessIncrease * Time.deltaTime);
    }

    // Function to disable the camera with opacity fade
    public void Disable()
    {
        if (!isDisabled)
        {
            isDisabled = true;
            StartCoroutine(FadeOutAndDisable());
        }
    }

    // Function to enable the camera with opacity fade
    public void Enable()
    {
        if (isDisabled)
        {
            isDisabled = false;
            StartCoroutine(FadeInAndEnable());
        }
    }

    // Coroutine to fade out and then disable the camera
    private IEnumerator FadeOutAndDisable()
    {
        if (cameraLightRenderer != null)
        {
            // Fade out the SpriteRenderer's material
            float timeElapsed = 0f;
            Color initialColor = cameraLightRenderer.color;
            while (timeElapsed < fadeDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, timeElapsed / fadeDuration);
                cameraLightRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            cameraLightRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        }

        // Disable the camera and its components after fade out
        fieldOfView.enabled = false;
        if (cameraLight != null)
        {
            cameraLight.gameObject.SetActive(false);
        }

        // Enable and start the timer slider UI
        if (timerSlider != null)
        {
            timerSlider.gameObject.SetActive(true); // Show the timer slider
            timerSlider.value = 100f;  // Set it to full (100%) initially
            StartCoroutine(TimerCountdown());
        }

        Debug.Log($"{gameObject.name} has been disabled.");
    }

    // Coroutine to fade in and then enable the camera gradually to the original opacity
    private IEnumerator FadeInAndEnable()
    {
        if (cameraLightRenderer != null)
        {
            // Fade in the SpriteRenderer's material gradually to the original alpha
            float timeElapsed = 0f;
            Color initialColor = cameraLightRenderer.color;
            float currentAlpha = initialColor.a;  // Start from the current alpha value

            // Gradually increase opacity to the original alpha value
            while (timeElapsed < fadeDuration)
            {
                float alpha = Mathf.Lerp(currentAlpha, originalAlpha, timeElapsed / fadeDuration);
                cameraLightRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Ensure the final color is set to the original alpha value
            cameraLightRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, originalAlpha);
        }

        // Re-enable the camera and its components after fade in
        fieldOfView.enabled = true;
        if (cameraLight != null)
        {
            cameraLight.gameObject.SetActive(true);
        }

        // Hide the timer slider UI
        if (timerSlider != null)
        {
            timerSlider.gameObject.SetActive(false);
        }

        Debug.Log($"{gameObject.name} has been enabled.");
    }

    // Timer countdown for the camera reenable
    private IEnumerator TimerCountdown()
    {
        float timeLeft = disableDuration;
        while (timeLeft > 0f)
        {
            timerSlider.value = (timeLeft / disableDuration)*100; // Update slider value based on remaining time
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        // Once the timer ends, enable the camera
        Enable();
    }
}
