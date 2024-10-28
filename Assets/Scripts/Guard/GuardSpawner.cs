using System.Collections.Generic;
using UnityEngine;

public class GuardSpawner : MonoBehaviour
{
    public Transform player; // Reference to the player’s transform
    public float spawnDistance = 10f; // Distance within which guards spawn
    public GameObject guardPrefab; // Guard prefab to spawn
    public List<Transform> spawnPoints; // List of guard spawn points

    private Dictionary<Transform, GameObject> activeGuards = new Dictionary<Transform, GameObject>();

    void Update()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            float distanceToPlayer = Vector3.Distance(player.position, spawnPoint.position);

            // Spawn a guard if within the spawn distance and not already spawned
            if (distanceToPlayer <= spawnDistance)
            {
                if (!activeGuards.ContainsKey(spawnPoint))
                {
                    GameObject guard = Instantiate(guardPrefab, spawnPoint.position, Quaternion.identity);
                    activeGuards[spawnPoint] = guard; // Track the spawned guard
                }
            }
            else
            {
                // If player moves away, remove guard to free up resources
                if (activeGuards.ContainsKey(spawnPoint))
                {
                    Destroy(activeGuards[spawnPoint]);
                    activeGuards.Remove(spawnPoint);
                }
            }
        }
    }
}
