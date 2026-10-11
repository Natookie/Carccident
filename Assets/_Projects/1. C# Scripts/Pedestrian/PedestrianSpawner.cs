using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PedestrianSpawner : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private GameObject[] pedestrianPrefabs;
    [SerializeField] private BoxCollider[] pedestrianAreas;

    [Header("SPAWN SETTING")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxPedestrians = 20;
    [SerializeField] private int maxTryFindSpawnPoint = 50;
    [SerializeField] private Transform pedesParent;

    [Header("OBSTRUCTION CHECK")]
    [SerializeField] private LayerMask obstructLayer;
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private float castHeight = 0.5f;

    private List<GameObject> pedestrianPool;
    private List<GameObject> activePedestrians;
    private Coroutine spawnCoroutine;
    private Collider[] overlapResults = new Collider[10];
    private Dictionary<GameObject, float> pedestrianHeights;

    void Awake() => InitializePool();
    void Start() => StartSpawner();
    void OnDestroy() => StopSpawner();

    void InitializePool(){
        pedestrianPool = new List<GameObject>();
        activePedestrians = new List<GameObject>();
        pedestrianHeights = new Dictionary<GameObject, float>();

        if(pedestrianPrefabs == null || pedestrianPrefabs.Length == 0){
            Debug.LogWarning("No pedestrian prefab assigned.");
            return;
        }

        foreach(GameObject prefab in pedestrianPrefabs){
            float prefabHeight = GetPedestrianHeight(prefab);
            
            for(int i = 0; i < maxPedestrians / pedestrianPrefabs.Length; i++){
                GameObject pedestrian = Instantiate(prefab);
                pedestrian.transform.SetParent(pedesParent);
                pedestrian.SetActive(false);
                pedestrianPool.Add(pedestrian);
                pedestrianHeights[pedestrian] = prefabHeight;
            }
        }
    }

    float GetPedestrianHeight(GameObject prefab){
        CapsuleCollider capsule = prefab.GetComponent<CapsuleCollider>();
        if(capsule != null){
            float scale = prefab.transform.localScale.y;
            float worldHeight = capsule.height * scale;
            return worldHeight;
        }
        
        return 1.8f;
    }

    public void StartSpawner(){
        if(spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawner(){
        if(spawnCoroutine != null){
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    IEnumerator SpawnLoop(){
        while(true){
            SpawnPedestrian();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnPedestrian(){
        if(pedestrianPrefabs == null || pedestrianPrefabs.Length == 0) return;
        if(pedestrianAreas == null || pedestrianAreas.Length == 0) return;

        GameObject pedestrian = GetPooledPedestrian();
        if(pedestrian == null) return;

        BoxCollider selectedArea = pedestrianAreas[Random.Range(0, pedestrianAreas.Length)];
        float pedestrianHeight = pedestrianHeights[pedestrian];
        
        bool foundSpawn = TryGetClearSpawnPoint(selectedArea, pedestrianHeight, out Vector3 spawnPosition);
        if(!foundSpawn){
            ReturnToPool(pedestrian);
            return;
        }

        spawnPosition.y += pedestrianHeight * 0.5f;
        pedestrian.transform.position = spawnPosition;
        pedestrian.SetActive(true);
        activePedestrians.Add(pedestrian);

        SimplePedestrianWalker walker = pedestrian.GetComponent<SimplePedestrianWalker>();
        if(walker != null) walker.InitDestination(selectedArea, obstructLayer);
    }

    GameObject GetPooledPedestrian(){
        foreach(GameObject pedestrian in pedestrianPool){
            if(!pedestrian.activeInHierarchy) return pedestrian;
        }
        return null;
    }

    public void ReturnToPool(GameObject pedestrian){
        if(pedestrian == null) return;
        
        pedestrian.SetActive(false);
        activePedestrians.Remove(pedestrian);
        
        pedestrian.transform.position = Vector3.zero;
        pedestrian.transform.rotation = Quaternion.identity;
    }

    public void ReturnAllToPool(){
        foreach(GameObject pedestrian in activePedestrians.ToArray()){
            ReturnToPool(pedestrian);
        }
    }

    bool TryGetClearSpawnPoint(BoxCollider area, float pedestrianHeight, out Vector3 spawnPosition){
        for(int i = 0; i < maxTryFindSpawnPoint; i++){
            Vector3 randomPoint = GetRandomPointInArea(area);

            if(!IsSpawnPointObstructed(randomPoint)){
                spawnPosition = randomPoint;
                return true;
            }
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    Vector3 GetRandomPointInArea(BoxCollider area){
        Bounds bounds = area.bounds;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        float groundY = bounds.min.y;

        return new Vector3(randomX, groundY, randomZ);
    }

    bool IsSpawnPointObstructed(Vector3 position){
        Vector3 checkPosition = new Vector3(position.x, castHeight, position.z);

        int hitCount = Physics.OverlapSphereNonAlloc(
            checkPosition,
            checkRadius,
            overlapResults,
            obstructLayer,
            QueryTriggerInteraction.Collide
        );

        for(int i = 0; i < hitCount; i++){
            if(overlapResults[i] != null && overlapResults[i].transform != transform)
                return true;
        }

        return false;
    }

    void OnDrawGizmosSelected(){
        Gizmos.color = Color.cyan;

        if(pedestrianAreas == null) return;

        foreach(BoxCollider area in pedestrianAreas){
            if(area == null) continue;
            Gizmos.DrawWireCube(area.bounds.center, area.bounds.size);
        }
    }
}