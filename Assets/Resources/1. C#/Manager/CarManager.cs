using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;

public class CarManager : MonoBehaviour
{
    public static CarManager Instance {get; private set;}
    
    [Header("CAR SETTINGS")]
    [SerializeField] private GameObject[] carPrefabs;
    [SerializeField] private Transform carParent;
    
    [Header("SPAWN SETTINGS")]
    [SerializeField] private float baseSpawnInterval = 2f;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private int maxActiveCars = 20;
    [Space(10)]
    [SerializeField] private float spawnHeightOffset = 0.5f;
    [SerializeField] private Renderer northSouthRoad;
    [SerializeField] private Renderer eastWestRoad;
    [ReadOnly] public Transform[] spawnPoints;
    
    [Header("DEBUG SPAWN")]
    [SerializeField] private bool showDebug = true;
    [ShowIf("showDebug")][SerializeField] private bool showDebugGUI = true;
    [Space(5)]
    [ShowIf("showDebug"), ReadOnly] public Transform debugNorthRightTurn;
    [ShowIf("showDebug"), ReadOnly] public Transform debugNorthStraight;
    [Space(10)]
    [ShowIf("showDebug"), ReadOnly] public Transform debugSouthRightTurn;
    [ShowIf("showDebug"), ReadOnly] public Transform debugSouthStraight;
    [Space(10)]
    [ShowIf("showDebug"), ReadOnly] public Transform debugEastRightTurn;
    [ShowIf("showDebug"), ReadOnly] public Transform debugEastStraight;
    [Space(10)]
    [ShowIf("showDebug"), ReadOnly] public Transform debugWestRightTurn;
    [ShowIf("showDebug"), ReadOnly] public Transform debugWestStraight;
    
    [Header("DIFFICULTY SCALING")]
    [SerializeField] private AnimationCurve spawnRateCurve;
    [SerializeField] private AnimationCurve maxCarsCurve;
    
    private RoadSpawnerManager roadSpawnerManager;
    private List<CarLogic> activeCars = new List<CarLogic>();
    private Queue<CarLogic> carPool = new Queue<CarLogic>();
    private float spawnTimer = 0f;
    private int nextCarID = 0;
    
    private int selectedDebugIndex = -1;
    private string[] debugOptions = new string[]{
        "1 - North Right Turn",
        "2 - East Right Turn",
        "3 - South Right Turn",
        "4 - West Right Turn",
        "5 - North Straight",
        "6 - East Straight",
        "7 - South Straight",
        "8 - West Straight"
    };
    
    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        roadSpawnerManager = FindFirstObjectByType<RoadSpawnerManager>();
    }
    
    void Start(){
        for(int i = 0; i < 15; i++) CreateNewCarAndAddToPool();
        spawnTimer = GetCurrentSpawnInterval();
        
        if(showDebug) InitializeDebugSpawnPoints();
    }
    
    [Button("Initialize Debug Spawn Points", EButtonEnableMode.Editor)]
    void InitializeDebugSpawnPoints(){
        foreach(Transform sp in spawnPoints){
            int laneID = roadSpawnerManager != null ? roadSpawnerManager.GetLaneID(sp) : -1;
            
            switch(laneID){
                case 0: debugNorthRightTurn = sp; break;
                case 1: debugNorthStraight = sp; break;
                case 2: debugSouthRightTurn = sp; break;
                case 3: debugSouthStraight = sp; break;
                case 4: debugEastRightTurn = sp; break;
                case 5: debugEastStraight = sp; break;
                case 6: debugWestRightTurn = sp; break;
                case 7: debugWestStraight = sp; break;
            }
        }
    }
    
    void Update(){
        if(showDebug) HandleDebugInput();
        
        if(spawnTimer <= 0f){
            if(activeCars.Count < GetCurrentMaxCars()){
                SpawnRandomCar();
                spawnTimer = GetCurrentSpawnInterval();
            }else spawnTimer = 0.5f;
        }else spawnTimer -= Time.deltaTime;
    }
    
    void HandleDebugInput(){
        if(Input.GetKeyDown(KeyCode.Alpha1)) selectedDebugIndex = 0;
        if(Input.GetKeyDown(KeyCode.Alpha2)) selectedDebugIndex = 1;
        if(Input.GetKeyDown(KeyCode.Alpha3)) selectedDebugIndex = 2;
        if(Input.GetKeyDown(KeyCode.Alpha4)) selectedDebugIndex = 3;
        if(Input.GetKeyDown(KeyCode.Alpha5)) selectedDebugIndex = 4;
        if(Input.GetKeyDown(KeyCode.Alpha6)) selectedDebugIndex = 5;
        if(Input.GetKeyDown(KeyCode.Alpha7)) selectedDebugIndex = 6;
        if(Input.GetKeyDown(KeyCode.Alpha8)) selectedDebugIndex = 7;
        
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)){
            if(selectedDebugIndex >= 0){
                SpawnSelectedDebugCar();
                selectedDebugIndex = -1;
            }
        }
        
        if(Input.GetKeyDown(KeyCode.Escape)) selectedDebugIndex = -1;
    }
    
    void SpawnSelectedDebugCar(){
        Transform spawnPoint = null;
        CarLogic.TurnIntent intent = CarLogic.TurnIntent.Straight;
        
        switch(selectedDebugIndex){
            case 0://North_Right
                spawnPoint = debugNorthRightTurn;
                intent = CarLogic.TurnIntent.Right;
                break;
            case 1://East_Right
                spawnPoint = debugEastRightTurn;
                intent = CarLogic.TurnIntent.Right;
                break;
            case 2://South_Right
                spawnPoint = debugSouthRightTurn;
                intent = CarLogic.TurnIntent.Right;
                break;
            case 3://West_Right
                spawnPoint = debugWestRightTurn;
                intent = CarLogic.TurnIntent.Right;
                break;
            case 4://North_Straight
                spawnPoint = debugNorthStraight;
                intent = CarLogic.TurnIntent.Straight;
                break;
            case 5://East_Straight
                spawnPoint = debugEastStraight;
                intent = CarLogic.TurnIntent.Straight;
                break;
            case 6://South_Straight
                spawnPoint = debugSouthStraight;
                intent = CarLogic.TurnIntent.Straight;
                break;
            case 7://West_Straight
                spawnPoint = debugWestStraight;
                intent = CarLogic.TurnIntent.Straight;
                break;
        }
        
        if(spawnPoint != null) SpawnDebugCar(spawnPoint, intent);
    }
    
    void OnGUI(){
        if(!showDebugGUI) return;
        
        if(selectedDebugIndex >= 0){
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.fontSize = 24;
            boxStyle.alignment = TextAnchor.MiddleCenter;
            
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 20;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = Color.yellow;
            
            float boxWidth = 400;
            float boxHeight = 150;
            float x = (Screen.width / 2) - (boxWidth / 2);
            float y = (Screen.height / 2) - (boxHeight / 2);
            
            GUI.Box(new Rect(x, y, boxWidth, boxHeight), "", boxStyle);
            
            string selectedText = debugOptions[selectedDebugIndex].Substring(3);
            GUI.Label(new Rect(x, y + 30, boxWidth, 50), $"Selected: {selectedText}", labelStyle);
            GUI.Label(new Rect(x, y + 90, boxWidth, 40), "Press ENTER to spawn | ESC to cancel", labelStyle);
        }
    }
    
    void SpawnRandomCar(){
        if(carPool.Count == 0) CreateNewCarAndAddToPool();
        
        CarLogic newCar = carPool.Dequeue();
        
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        int laneID = roadSpawnerManager != null ? roadSpawnerManager.GetLaneID(spawnPoint) : -1;
        CarLogic.TurnIntent turnIntent = DetermineTurnIntent(spawnPoint);
        
        Transform turnTarget = null;
        Transform stopArea = null;
        
        if(roadSpawnerManager != null){
            if(turnIntent == CarLogic.TurnIntent.Right) turnTarget = roadSpawnerManager.GetTurnTarget(spawnPoint);
            stopArea = roadSpawnerManager.GetStopArea(spawnPoint);
        }
        
        Vector3 spawnPos = GetSpawnPositionOnRoad(spawnPoint.position);

        newCar.Initialize(nextCarID++, laneID, spawnPos, turnIntent, turnTarget, stopArea);
        newCar.transform.SetParent(carParent);
        newCar.name = $"Car_{newCar.carID}";
        
        newCar.gameObject.SetActive(true);
        activeCars.Add(newCar);
    }
    
    CarLogic.TurnIntent DetermineTurnIntent(Transform spawnPoint){
        if(roadSpawnerManager == null) return CarLogic.TurnIntent.Straight;
        
        var info = roadSpawnerManager.GetLaneInfo(spawnPoint);
        if(info == null) return CarLogic.TurnIntent.Straight;
        
        return info.laneType == "RightTurn" ? CarLogic.TurnIntent.Right : CarLogic.TurnIntent.Straight;
    }
    
    void SpawnDebugCar(Transform spawnPoint, CarLogic.TurnIntent intent){
        if(carPool.Count == 0) CreateNewCarAndAddToPool();
        
        CarLogic newCar = carPool.Dequeue();
        int laneID = roadSpawnerManager != null ? roadSpawnerManager.GetLaneID(spawnPoint) : -1;
        
        Transform turnTarget = null;
        Transform stopArea = null;
        
        if(roadSpawnerManager != null){
            if(intent == CarLogic.TurnIntent.Right) turnTarget = roadSpawnerManager.GetTurnTarget(spawnPoint);
            stopArea = roadSpawnerManager.GetStopArea(spawnPoint);
        }
        
        Vector3 spawnPos = GetSpawnPositionOnRoad(spawnPoint.position);
        newCar.Initialize(nextCarID++, laneID, spawnPos, intent, turnTarget, stopArea);
        newCar.gameObject.SetActive(true);
        newCar.transform.SetParent(carParent);
        newCar.name = $"DebugCar_{newCar.carID}";
        activeCars.Add(newCar);
    }
    
    void CreateNewCarAndAddToPool(){
        if(carPrefabs.Length == 0){
            Debug.LogError("No car prefabs assigned!");
            return;
        }
        
        GameObject carPrefab = carPrefabs[Random.Range(0, carPrefabs.Length)];
        GameObject newCarObj = Instantiate(carPrefab);
        CarLogic newCar = newCarObj.GetComponent<CarLogic>();
        
        if(newCar == null){
            Debug.LogError("Car prefab missing CarLogic!");
            Destroy(newCarObj);
            return;
        }
        
        newCarObj.SetActive(false);
        carPool.Enqueue(newCar);
    }
    
    public void ReturnCarToPool(CarLogic car){
        activeCars.Remove(car);
        car.gameObject.SetActive(false);
        car.transform.position = Vector3.zero;
        carPool.Enqueue(car);
    }
    
    float GetCurrentSpawnInterval(){
        if(GameManager.Instance != null){
            float gameTime = GameManager.Instance.GetCurrentShiftTime();
            float multiplier = spawnRateCurve.Evaluate(gameTime);
            return Mathf.Max(minSpawnInterval, baseSpawnInterval * multiplier);
        }
        return baseSpawnInterval;
    }
    
    int GetCurrentMaxCars(){
        if(GameManager.Instance != null){
            float gameTime = GameManager.Instance.GetCurrentShiftTime();
            float multiplier = maxCarsCurve.Evaluate(gameTime);
            return Mathf.CeilToInt(maxActiveCars * multiplier);
        }
        return maxActiveCars;
    }
    
    Vector3 GetSpawnPositionOnRoad(Vector3 originalPosition){
        float roadY = northSouthRoad.bounds.max.y + eastWestRoad.bounds.max.y / 2f;
        return new Vector3(originalPosition.x, roadY + spawnHeightOffset, originalPosition.z);
    }
}