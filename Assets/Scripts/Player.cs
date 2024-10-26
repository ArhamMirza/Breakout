using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Import for UI components

public class Player : MonoBehaviour
{
    public float maxAlertness = 100f; // Maximum alertness level
    private float alertness; // Current alertness level

    [Header("Player Components")]
    public PlayerControl playerControl; // Reference to PlayerControl script
    public Inventory inventory; // Reference to Inventory script

    [Header("Alertness UI")]
    public Slider alertnessSlider; // UI Slider to represent alertness level
    public Image alertnessFill; // Fill image of the slider to change color based on alertness
    public Color lowAlertnessColor = Color.green; // Color when alertness is low
    public Color highAlertnessColor = Color.red; // Color when alertness is high

    [Header("Alertness Settings")]
    public float baseAlertnessIncrease = 10f; // Base amount for alertness increase
    public float alertnessMultiplier = 1.0f; // Multiplier to adjust the rate of increase
    public float exponentialFactor = 2f; // Exponential factor for ramping up
    public float alertnessDecrementRate = 1f; // Rate at which alertness decreases

    void Start()
    {
        // Initialize alertness to half of the max value
        alertness = 0;

        // Reference other components on the player
        if (playerControl == null)
        {
            playerControl = GetComponent<PlayerControl>();
        }
        
        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }

        // Initialize the alertness slider UI
        if (alertnessSlider != null)
        {
            alertnessSlider.maxValue = maxAlertness;
            alertnessSlider.value = alertness;
            UpdateAlertnessUI();
        }
    }

    void Update()
    {
        // Call any necessary methods to update alertness or manage other behaviors
        Debug.Log("Alertness Meter: " + alertness);
        UpdateAlertness(Time.deltaTime);
    }

    // Method to update alertness level (e.g., it could decrease over time naturally)
    void UpdateAlertness(float deltaTime)
    {
        if (alertness > 0)
        {
            alertness -= alertnessDecrementRate * deltaTime; // Decrease alertness over time
            alertness = Mathf.Clamp(alertness, 0, maxAlertness);
            UpdateAlertnessUI(); // Update the UI
        }
    }

    // Method to increase alertness by a certain amount
    public void SetAlertness(float value)
    {
        alertness = Mathf.Clamp(value, 0, maxAlertness);
    }
    public void IncreaseAlertness(float amount)
    {
        // Use an exponential function to increase alertness
        float increaseAmount = baseAlertnessIncrease * alertnessMultiplier * Mathf.Pow(exponentialFactor, amount);
        alertness += increaseAmount;
        alertness = Mathf.Clamp(alertness, 0, maxAlertness);
        UpdateAlertnessUI(); // Update the UI
    }

    // Method to decrease alertness by a certain amount
    public void DecreaseAlertness(float amount)
    {
        alertness -= amount;
        alertness = Mathf.Clamp(alertness, 0, maxAlertness);
        UpdateAlertnessUI(); // Update the UI
    }

    // Get the current alertness level as a percentage
    public float GetAlertnessPercentage()
    {
        return (alertness / maxAlertness) * 100f;
    }

    // Update the UI slider and fill color based on alertness
    private void UpdateAlertnessUI()
    {
        if (alertnessSlider != null)
        {
            alertnessSlider.value = alertness;

            // Change fill color based on alertness level
            float alertnessFraction = alertness / maxAlertness;
            alertnessFill.color = Color.Lerp(lowAlertnessColor, highAlertnessColor, alertnessFraction);
        }
    }
}
