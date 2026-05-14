using UnityEngine;
using System.Collections;

public class PedestrianSpawner : MonoBehaviour
{
    [Header("Pedestrian Prefabs")]
    [SerializeField] private GameObject[] pedestrianPrefabs;

    [Header("Pedestrian Areas")]
    [SerializeField] private BoxCollider[] pedestrianAreas;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxTryFindSpawnPoint = 50;

    [Header("Spawn Obstruction Check")]
    [SerializeField] private LayerMask obstructLayer;
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private float castHeight = 0.5f;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnPedestrian();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnPedestrian()
    {
        if (pedestrianPrefabs == null || pedestrianPrefabs.Length == 0)
        {
            Debug.LogWarning("No pedestrian prefab assigned.");
            return;
        }

        if (pedestrianAreas == null || pedestrianAreas.Length == 0)
        {
            Debug.LogWarning("No pedestrian area assigned.");
            return;
        }

        GameObject randomPrefab = pedestrianPrefabs[Random.Range(0, pedestrianPrefabs.Length)];
        BoxCollider selectedArea = pedestrianAreas[Random.Range(0, pedestrianAreas.Length)];

        float prefabY = randomPrefab.transform.position.y;

        bool foundSpawn = TryGetClearSpawnPoint(selectedArea, prefabY, out Vector3 spawnPosition);

        if (!foundSpawn)
        {
            Debug.LogWarning("Failed to find clear pedestrian spawn point.");
            return;
        }

        GameObject pedestrian = Instantiate(randomPrefab, spawnPosition, Quaternion.identity);

        SimplePedestrianWalker walker = pedestrian.GetComponent<SimplePedestrianWalker>();

        if (walker != null)
        {
            walker.InitDestination(selectedArea);
        }
        else
        {
            Debug.LogWarning("Pedestrian prefab does not have SimplePedestrianWalker script.");
        }
    }

    private bool TryGetClearSpawnPoint(BoxCollider area, float yPosition, out Vector3 spawnPosition)
    {
        for (int i = 0; i < maxTryFindSpawnPoint; i++)
        {
            Vector3 randomPoint = GetRandomPointInArea(area, yPosition);

            if (!IsPointObstructed(randomPoint))
            {
                spawnPosition = randomPoint;
                return true;
            }
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    private Vector3 GetRandomPointInArea(BoxCollider area, float yPosition)
    {
        Bounds bounds = area.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(randomX, yPosition, randomZ);
    }

    private bool IsPointObstructed(Vector3 position)
    {
        Vector3 checkPosition = new Vector3(position.x, castHeight, position.z);

        Collider[] hits = Physics.OverlapSphere(
            checkPosition,
            checkRadius,
            obstructLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            Debug.Log("Spawn blocked by: " + hit.name);
            return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        if (pedestrianAreas == null) return;

        foreach (BoxCollider area in pedestrianAreas)
        {
            if (area == null) continue;
            Gizmos.DrawWireCube(area.bounds.center, area.bounds.size);
        }
    }
}