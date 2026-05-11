using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

public class RoadSpawnerManager : MonoBehaviour
{
    [Header("SPAWNER SETTINGS")]
    public float roadLength = 15f;
    public float laneWidth = 2.5f;
    
    [Header("INTERSECTION AREA")]
    [SerializeField] private float intersectionSize = 8f;
    [SerializeField] private float turnPointPadding = 1f;
    
    [Header("STOP AREA")]
    [SerializeField] private bool enableStopArea = true;
    [SerializeField] private float stopAreaPadding = 1.5f;
    [SerializeField] private float stopAreaWidth = 1f;
    [SerializeField] private Color stopAreaColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color zebraStripeColor = Color.white;
    
    [Header("LANE POSITIONS")]
    [SerializeField] private float rightTurnOffset = 1.0f;
    [SerializeField] private float straightOffset = 0.5f;
    
    [Header("TURN POINTS")]
    [ReadOnly] public Transform northTurnPoint;
    [ReadOnly] public Transform southTurnPoint;
    [ReadOnly] public Transform eastTurnPoint;
    [ReadOnly] public Transform westTurnPoint;
    
    [Header("STOP AREAS")]
    [ReadOnly] public Transform northStopArea;
    [ReadOnly] public Transform southStopArea;
    [ReadOnly] public Transform eastStopArea;
    [ReadOnly] public Transform westStopArea;
    
    [Header("SPAWNERS")]
    [ReadOnly] public Transform northRightTurn;
    [ReadOnly] public Transform northStraight;
    [ReadOnly] public Transform southRightTurn;
    [ReadOnly] public Transform southStraight;
    [Space(10)]
    [ReadOnly] public Transform eastRightTurn;
    [ReadOnly] public Transform eastStraight;
    [ReadOnly] public Transform westRightTurn;
    [ReadOnly] public Transform westStraight;

    [Header("GIZMOS")]
    [SerializeField] private bool showGizmos = true;    
    [Space(5)]
    [ShowIf("showGizmos")] public bool showRoadBounds = true;
    [ShowIf("showGizmos")] public bool showLaneDividers = true;
    [ShowIf("showGizmos")] public bool showSpawnPoints = true;
    [ShowIf("showGizmos")] public bool showDirectionArrows = true;
    [ShowIf("showGizmos")] public bool showIntersectionArea = true;
    [ShowIf("showGizmos")] public bool showTurnPoints = true;
    [ShowIf("showGizmos")] public bool showStopAreas = true;
    [Space(10)]
    [ShowIf("showGizmos")] public Color roadColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    [ShowIf("showGizmos")] public Color dividerColor = Color.yellow;
    [ShowIf("showGizmos")] public Color rightTurnColor = Color.cyan;
    [ShowIf("showGizmos")] public Color straightColor = Color.blue;
    [ShowIf("showGizmos")] public Color intersectionColor = new Color(1f, 0.5f, 0f, 0.3f);
    [ShowIf("showGizmos")] public Color turnPointColor = Color.magenta;
    
    private const string SPAWNER_PARENT_NAME = "Car Spawner";
    private const string TURN_POINT_PARENT_NAME = "Turn Points";
    private const string STOP_AREA_PARENT_NAME = "Stop Areas";
    private Dictionary<Transform, LaneInfo> laneMapping;
    
    private float RightTurnX => -laneWidth * rightTurnOffset;
    private float StraightX => -laneWidth * straightOffset;
    private float SouthRightTurnX => laneWidth * rightTurnOffset;
    private float SouthStraightX => laneWidth * straightOffset;
    
    private float IntersectionHalfSize => intersectionSize / 2f;
    private float TurnPointOffset => IntersectionHalfSize - turnPointPadding;
    private float StopAreaOffset => IntersectionHalfSize + stopAreaPadding;
    private float StopAreaEnd => StopAreaOffset + stopAreaWidth;
    
    public class LaneInfo{
        public int laneID;
        public string laneName;
        public int roadDirection;
        public string laneType;
        public Vector3 movementDirection;
        public Transform turnTarget;
        public Transform stopArea;
        
        public LaneInfo(int id, string name, int road, string type, Vector3 direction, Transform target, Transform stop){
            laneID = id;
            laneName = name;
            roadDirection = road;
            laneType = type;
            movementDirection = direction;
            turnTarget = target;
            stopArea = stop;
        }
    }
    
    void Awake(){
        BuildLaneMapping();
    }
    
    void BuildLaneMapping(){
        laneMapping = new Dictionary<Transform, LaneInfo>();
        
        if(northRightTurn != null) laneMapping[northRightTurn] = new LaneInfo(0, "North_RightTurn", 0, "RightTurn", Vector3.back, northTurnPoint, northStopArea);
        if(northStraight != null) laneMapping[northStraight] = new LaneInfo(1, "North_Straight", 0, "Straight", Vector3.back, null, northStopArea);
        
        if(southRightTurn != null) laneMapping[southRightTurn] = new LaneInfo(2, "South_RightTurn", 1, "RightTurn", Vector3.forward, southTurnPoint, southStopArea);
        if(southStraight != null) laneMapping[southStraight] = new LaneInfo(3, "South_Straight", 1, "Straight", Vector3.forward, null, southStopArea);
        
        if(eastRightTurn != null) laneMapping[eastRightTurn] = new LaneInfo(4, "East_RightTurn", 2, "RightTurn", Vector3.left, eastTurnPoint, eastStopArea);
        if(eastStraight != null) laneMapping[eastStraight] = new LaneInfo(5, "East_Straight", 2, "Straight", Vector3.left, null, eastStopArea);
        
        if(westRightTurn != null) laneMapping[westRightTurn] = new LaneInfo(6, "West_RightTurn", 3, "RightTurn", Vector3.right, westTurnPoint, westStopArea);
        if(westStraight != null) laneMapping[westStraight] = new LaneInfo(7, "West_Straight", 3, "Straight", Vector3.right, null, westStopArea);
    }
    
    public LaneInfo GetLaneInfo(Transform spawnPoint){
        if(laneMapping == null) BuildLaneMapping();
        return laneMapping.ContainsKey(spawnPoint) ? laneMapping[spawnPoint] : null;
    }
    
    public int GetLaneID(Transform spawnPoint){
        LaneInfo info = GetLaneInfo(spawnPoint);
        return info != null ? info.laneID : -1;
    }
    
    public Vector3 GetMovementDirection(Transform spawnPoint){
        LaneInfo info = GetLaneInfo(spawnPoint);
        return info != null ? info.movementDirection : Vector3.zero;
    }
    
    public Transform GetTurnTarget(Transform spawnPoint){
        LaneInfo info = GetLaneInfo(spawnPoint);
        return info != null ? info.turnTarget : null;
    }
    
    public bool ShouldTurn(Transform spawnPoint){
        LaneInfo info = GetLaneInfo(spawnPoint);
        return info != null && info.laneType == "RightTurn";
    }
    
    public Transform GetStopArea(Transform spawnPoint){
        LaneInfo info = GetLaneInfo(spawnPoint);
        return info != null ? info.stopArea : null;
    }
    
    public Bounds GetIntersectionBounds() => new Bounds(Vector3.zero, new Vector3(intersectionSize, 2f, intersectionSize));
    
    public Bounds GetStopAreaBounds(int roadDirection){
        float halfSize = stopAreaWidth / 2f;
        switch(roadDirection){
            case 0: return new Bounds(new Vector3(0, 0, StopAreaOffset + halfSize), new Vector3(intersectionSize, 1f, stopAreaWidth));
            case 1: return new Bounds(new Vector3(0, 0, -StopAreaOffset - halfSize), new Vector3(intersectionSize, 1f, stopAreaWidth));
            case 2: return new Bounds(new Vector3(StopAreaOffset + halfSize, 0, 0), new Vector3(stopAreaWidth, 1f, intersectionSize));
            case 3: return new Bounds(new Vector3(-StopAreaOffset - halfSize, 0, 0), new Vector3(stopAreaWidth, 1f, intersectionSize));
            default: return new Bounds();
        }
    }
    
    #region EDITOR SETUP
    [Button("Auto-Setup Everything", EButtonEnableMode.Editor)]
    void AutoSetupEverything(){
        Debug.Log("=== Auto-Setup Started ===");
        ClearAll();
        CreateAllSpawners();
        CreateAllTurnPoints();
        CreateAllStopAreas();
        AutoAssignToCarManager();
        BuildLaneMapping();
        Debug.Log("=== Setup Complete ===");
    }
    
    void ClearAll(){
        Transform spawnerParent = transform.Find(SPAWNER_PARENT_NAME);
        if(spawnerParent != null) DestroyImmediate(spawnerParent.gameObject);
        
        Transform turnPointParent = transform.Find(TURN_POINT_PARENT_NAME);
        if(turnPointParent != null) DestroyImmediate(turnPointParent.gameObject);
        
        Transform stopAreaParent = transform.Find(STOP_AREA_PARENT_NAME);
        if(stopAreaParent != null) DestroyImmediate(stopAreaParent.gameObject);
        
        northRightTurn = northStraight = null;
        southRightTurn = southStraight = null;
        eastRightTurn = eastStraight = null;
        westRightTurn = westStraight = null;
        
        northTurnPoint = southTurnPoint = eastTurnPoint = westTurnPoint = null;
        northStopArea = southStopArea = eastStopArea = westStopArea = null;
        laneMapping?.Clear();
    }
    
    void CreateAllSpawners(){
        Transform spawnerParent = CreateSpawnerParent();
        
        Vector3 northPos = new Vector3(0, 0, roadLength);
        northRightTurn = CreateSpawnerPoint("North_RightTurn", northPos + new Vector3(RightTurnX, 0, 0), spawnerParent);
        northStraight = CreateSpawnerPoint("North_Straight", northPos + new Vector3(StraightX, 0, 0), spawnerParent);
        
        Vector3 southPos = new Vector3(0, 0, -roadLength);
        southRightTurn = CreateSpawnerPoint("South_RightTurn", southPos + new Vector3(SouthRightTurnX, 0, 0), spawnerParent);
        southStraight = CreateSpawnerPoint("South_Straight", southPos + new Vector3(SouthStraightX, 0, 0), spawnerParent);
        
        Vector3 eastPos = new Vector3(roadLength, 0, 0);
        eastRightTurn = CreateSpawnerPoint("East_RightTurn", eastPos + new Vector3(0, 0, SouthRightTurnX), spawnerParent);
        eastStraight = CreateSpawnerPoint("East_Straight", eastPos + new Vector3(0, 0, SouthStraightX), spawnerParent);
        
        Vector3 westPos = new Vector3(-roadLength, 0, 0);
        westRightTurn = CreateSpawnerPoint("West_RightTurn", westPos + new Vector3(0, 0, RightTurnX), spawnerParent);
        westStraight = CreateSpawnerPoint("West_Straight", westPos + new Vector3(0, 0, StraightX), spawnerParent);
        
        Debug.Log($"Created 8 spawners under '{SPAWNER_PARENT_NAME}'");
    }
    
    void CreateAllTurnPoints(){
        Transform turnPointParent = CreateTurnPointParent();
        
        float intersectionEdge = IntersectionHalfSize - turnPointPadding;
        
        if(northRightTurn != null){
            Vector3 northPos = new Vector3(northRightTurn.position.x, 0, intersectionEdge);
            northTurnPoint = CreateTurnPoint("North_TurnPoint", northPos, turnPointParent);
        }
        if(southRightTurn != null){
            Vector3 southPos = new Vector3(southRightTurn.position.x, 0, -intersectionEdge);
            southTurnPoint = CreateTurnPoint("South_TurnPoint", southPos, turnPointParent);
        }
        if(eastRightTurn != null){
            Vector3 eastPos = new Vector3(intersectionEdge, 0, eastRightTurn.position.z);
            eastTurnPoint = CreateTurnPoint("East_TurnPoint", eastPos, turnPointParent);
        }
        if(westRightTurn != null){
            Vector3 westPos = new Vector3(-intersectionEdge, 0, westRightTurn.position.z);
            westTurnPoint = CreateTurnPoint("West_TurnPoint", westPos, turnPointParent);
        }
    }
    
    void CreateAllStopAreas(){
    if(!enableStopArea) return;
    
    Transform stopAreaParent = CreateStopAreaParent();
    
    float stopAreaPos = StopAreaOffset;
    
    if(northRightTurn != null){
        Vector3 northPos = new Vector3(0, 0, stopAreaPos);
        northStopArea = CreateStopArea("North_StopArea", northPos, stopAreaParent);
    }
    
    if(southRightTurn != null){
        Vector3 southPos = new Vector3(0, 0, -stopAreaPos);
        southStopArea = CreateStopArea("South_StopArea", southPos, stopAreaParent);
    }
    
    if(eastRightTurn != null){
        Vector3 eastPos = new Vector3(stopAreaPos, 0, 0);
        eastStopArea = CreateStopArea("East_StopArea", eastPos, stopAreaParent);
    }
    
    if(westRightTurn != null){
        Vector3 westPos = new Vector3(-stopAreaPos, 0, 0);
        westStopArea = CreateStopArea("West_StopArea", westPos, stopAreaParent);
    }
}
    
    Transform CreateSpawnerParent(){
        Transform existingParent = transform.Find(SPAWNER_PARENT_NAME);
        if(existingParent != null) DestroyImmediate(existingParent.gameObject);
        
        GameObject parentObj = new GameObject(SPAWNER_PARENT_NAME);
        parentObj.transform.SetParent(transform);
        parentObj.transform.localPosition = Vector3.zero;
        return parentObj.transform;
    }
    
    Transform CreateTurnPointParent(){
        Transform existingParent = transform.Find(TURN_POINT_PARENT_NAME);
        if(existingParent != null) DestroyImmediate(existingParent.gameObject);
        
        GameObject parentObj = new GameObject(TURN_POINT_PARENT_NAME);
        parentObj.transform.SetParent(transform);
        parentObj.transform.localPosition = Vector3.zero;
        return parentObj.transform;
    }
    
    Transform CreateStopAreaParent(){
        Transform existingParent = transform.Find(STOP_AREA_PARENT_NAME);
        if(existingParent != null) DestroyImmediate(existingParent.gameObject);
        
        GameObject parentObj = new GameObject(STOP_AREA_PARENT_NAME);
        parentObj.transform.SetParent(transform);
        parentObj.transform.localPosition = Vector3.zero;
        return parentObj.transform;
    }
    
    Transform CreateSpawnerPoint(string name, Vector3 position, Transform parent){
        GameObject spawner = new GameObject(name);
        spawner.transform.SetParent(parent);
        spawner.transform.position = position;
        return spawner.transform;
    }
    
    Transform CreateTurnPoint(string name, Vector3 position, Transform parent){
        GameObject turnPoint = new GameObject(name);
        turnPoint.transform.SetParent(parent);
        turnPoint.transform.position = position;
        
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "Visual";
        visual.transform.SetParent(turnPoint.transform);
        visual.transform.localScale = Vector3.one * 0.4f;
        visual.transform.localPosition = Vector3.up * 0.3f;
        DestroyImmediate(visual.GetComponent<Collider>());
        
        Renderer renderer = visual.GetComponent<Renderer>();
        if(renderer != null){
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = turnPointColor;
            renderer.material = mat;
        }
        return turnPoint.transform;
    }
    
    Transform CreateStopArea(string name, Vector3 position, Transform parent){
        GameObject stopArea = new GameObject(name);
        stopArea.transform.SetParent(parent);
        stopArea.transform.position = position;
        return stopArea.transform;
    }
    
    void AutoAssignToCarManager(){
        CarManager carManager = FindFirstObjectByType<CarManager>();
        if(carManager == null){
            Debug.LogError("CarManager not found!");
            return;
        }
        
        Transform[] allSpawners = {
            northRightTurn, northStraight,
            southRightTurn, southStraight,
            eastRightTurn, eastStraight,
            westRightTurn, westStraight
        };
        
        var spawnPointsField = typeof(CarManager).GetField("spawnPoints", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        if(spawnPointsField != null){
            spawnPointsField.SetValue(carManager, allSpawners);
            Debug.Log($"Assigned {allSpawners.Length} spawners to CarManager");
        }
        
        var roadSpawnerField = typeof(CarManager).GetField("roadSpawnerManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if(roadSpawnerField != null) roadSpawnerField.SetValue(carManager, this);
    }
    
    [Button("Update Turn Points Position", EButtonEnableMode.Editor)]
    void UpdateTurnPointsPosition(){
        Transform turnPointParent = transform.Find(TURN_POINT_PARENT_NAME);
        if(turnPointParent == null){
            Debug.LogError("Turn Points not found! Run Auto-Setup Everything first.");
            return;
        }
        
        float intersectionEdge = IntersectionHalfSize - turnPointPadding;
        
        UpdateTurnPointPosition("North_TurnPoint", new Vector3(northRightTurn?.position.x ?? 0, 0, intersectionEdge));
        UpdateTurnPointPosition("South_TurnPoint", new Vector3(southRightTurn?.position.x ?? 0, 0, -intersectionEdge));
        UpdateTurnPointPosition("East_TurnPoint", new Vector3(intersectionEdge, 0, eastRightTurn?.position.z ?? 0));
        UpdateTurnPointPosition("West_TurnPoint", new Vector3(-intersectionEdge, 0, westRightTurn?.position.z ?? 0));
        
        BuildLaneMapping();
        Debug.Log($"Turn points updated");
    }
    
    void UpdateTurnPointPosition(string pointName, Vector3 newPosition){
        Transform turnPointParent = transform.Find(TURN_POINT_PARENT_NAME);
        if(turnPointParent != null){
            Transform point = turnPointParent.Find(pointName);
            if(point != null) point.position = newPosition;
        }
    }
    
    [Button("Debug: Print All Positions", EButtonEnableMode.Editor)]
    void DebugPrintPositions(){
        Debug.Log("=== SPAWN POSITIONS ===");
        Debug.Log($"North Right Turn: {GetSpawnPos(northRightTurn)}");
        Debug.Log($"North Straight: {GetSpawnPos(northStraight)}");
        Debug.Log($"South Right Turn: {GetSpawnPos(southRightTurn)}");
        Debug.Log($"South Straight: {GetSpawnPos(southStraight)}");
        Debug.Log($"East Right Turn: {GetSpawnPos(eastRightTurn)}");
        Debug.Log($"East Straight: {GetSpawnPos(eastStraight)}");
        Debug.Log($"West Right Turn: {GetSpawnPos(westRightTurn)}");
        Debug.Log($"West Straight: {GetSpawnPos(westStraight)}");
        
        Debug.Log("=== TURN POINTS ===");
        Debug.Log($"North Turn Point: {GetSpawnPos(northTurnPoint)}");
        Debug.Log($"South Turn Point: {GetSpawnPos(southTurnPoint)}");
        Debug.Log($"East Turn Point: {GetSpawnPos(eastTurnPoint)}");
        Debug.Log($"West Turn Point: {GetSpawnPos(westTurnPoint)}");
        
        Debug.Log("=== STOP AREAS ===");
        Debug.Log($"North Stop Area: {GetSpawnPos(northStopArea)}");
        Debug.Log($"South Stop Area: {GetSpawnPos(southStopArea)}");
        Debug.Log($"East Stop Area: {GetSpawnPos(eastStopArea)}");
        Debug.Log($"West Stop Area: {GetSpawnPos(westStopArea)}");
    }
    
    string GetSpawnPos(Transform t) => t != null ? t.position.ToString() : "Not created";
    #endregion

    #region GIZMOS LOGIC
    void OnDrawGizmos(){
        if(!showGizmos) return;
        
        DrawNorthSouthRoad();
        DrawEastWestRoad();
        
        if(showIntersectionArea) DrawIntersectionArea();
        if(showStopAreas) DrawStopAreas();
        if(showTurnPoints) DrawTurnPoints();
        
        if(showSpawnPoints){
            DrawSpawnPoint(northRightTurn, rightTurnColor);
            DrawSpawnPoint(northStraight, straightColor);
            DrawSpawnPoint(southRightTurn, rightTurnColor);
            DrawSpawnPoint(southStraight, straightColor);
            DrawSpawnPoint(eastRightTurn, rightTurnColor);
            DrawSpawnPoint(eastStraight, straightColor);
            DrawSpawnPoint(westRightTurn, rightTurnColor);
            DrawSpawnPoint(westStraight, straightColor);
        }
    }
    
    void DrawNorthSouthRoad(){
        if(!showRoadBounds && !showLaneDividers) return;
        
        float totalWidth = laneWidth * 2;
        float halfWidth = totalWidth / 2;
        
        if(showRoadBounds){
            Gizmos.color = roadColor;
            Vector3 roadSize = new Vector3(totalWidth, 0.1f, roadLength * 2);
            Gizmos.DrawCube(Vector3.zero + new Vector3(0, -0.2f, 0), roadSize);
            
            Gizmos.color = Color.white;
            Gizmos.DrawLine(new Vector3(-halfWidth, 0.1f, -roadLength), new Vector3(-halfWidth, 0.1f, roadLength));
            Gizmos.DrawLine(new Vector3(halfWidth, 0.1f, -roadLength), new Vector3(halfWidth, 0.1f, roadLength));
        }
        
        if(showLaneDividers){
            Gizmos.color = dividerColor;
            for(float z = -roadLength; z < roadLength; z += 0.8f){
                float zStart = z;
                float zEnd = Mathf.Min(z + 0.4f, roadLength);
                Gizmos.DrawLine(new Vector3(0, 0.15f, zStart), new Vector3(0, 0.15f, zEnd));
            }
        }
    }
    
    void DrawEastWestRoad(){
        if(!showRoadBounds && !showLaneDividers) return;
        
        float totalWidth = laneWidth * 2;
        float halfWidth = totalWidth / 2;
        
        if(showRoadBounds){
            Gizmos.color = roadColor;
            Vector3 roadSize = new Vector3(roadLength * 2, 0.1f, totalWidth);
            Gizmos.DrawCube(Vector3.zero + new Vector3(0, -0.2f, 0), roadSize);
            
            Gizmos.color = Color.white;
            Gizmos.DrawLine(new Vector3(-roadLength, 0.1f, -halfWidth), new Vector3(roadLength, 0.1f, -halfWidth));
            Gizmos.DrawLine(new Vector3(-roadLength, 0.1f, halfWidth), new Vector3(roadLength, 0.1f, halfWidth));
        }
        
        if(showLaneDividers){
            Gizmos.color = dividerColor;
            for(float x = -roadLength; x < roadLength; x += 0.8f){
                float xStart = x;
                float xEnd = Mathf.Min(x + 0.4f, roadLength);
                Gizmos.DrawLine(new Vector3(xStart, 0.15f, 0), new Vector3(xEnd, 0.15f, 0));
            }
        }
    }
    
    void DrawIntersectionArea(){
        Gizmos.color = intersectionColor;
        Vector3 intersectionCenter = Vector3.zero;
        Vector3 intersectionSize3D = new Vector3(intersectionSize, 0.2f, intersectionSize);
        Gizmos.DrawCube(intersectionCenter, intersectionSize3D);
        
        Gizmos.color = Color.white;
        float half = IntersectionHalfSize;
        Vector3 topLeft = new Vector3(-half, 0.2f, half);
        Vector3 topRight = new Vector3(half, 0.2f, half);
        Vector3 bottomLeft = new Vector3(-half, 0.2f, -half);
        Vector3 bottomRight = new Vector3(half, 0.2f, -half);
        
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
    
    void DrawStopAreas(){
        float stopAreaStart = StopAreaOffset;
        float stopAreaEnd = StopAreaOffset + stopAreaWidth;
        
        Gizmos.color = stopAreaColor;
        Vector3 northSize = new Vector3(intersectionSize, 0.2f, stopAreaWidth);
        Gizmos.DrawCube(new Vector3(0, 0, stopAreaStart + stopAreaWidth/2), northSize);
        
        Gizmos.color = zebraStripeColor;
        for(float x = -IntersectionHalfSize; x < IntersectionHalfSize; x += 0.8f){
            float stripeWidth = 0.4f;
            Gizmos.DrawLine(new Vector3(x, 0.3f, stopAreaStart), new Vector3(x + stripeWidth, 0.3f, stopAreaStart));
            Gizmos.DrawLine(new Vector3(x, 0.3f, stopAreaEnd), new Vector3(x + stripeWidth, 0.3f, stopAreaEnd));
        }
        
        Gizmos.color = stopAreaColor;
        Gizmos.DrawCube(new Vector3(0, 0, -stopAreaStart - stopAreaWidth/2), northSize);
        
        Gizmos.color = zebraStripeColor;
        for(float x = -IntersectionHalfSize; x < IntersectionHalfSize; x += 0.8f){
            float stripeWidth = 0.4f;
            Gizmos.DrawLine(new Vector3(x, 0.3f, -stopAreaStart), new Vector3(x + stripeWidth, 0.3f, -stopAreaStart));
            Gizmos.DrawLine(new Vector3(x, 0.3f, -stopAreaEnd), new Vector3(x + stripeWidth, 0.3f, -stopAreaEnd));
        }
        
        Gizmos.color = stopAreaColor;
        Vector3 eastSize = new Vector3(stopAreaWidth, 0.2f, intersectionSize);
        Gizmos.DrawCube(new Vector3(stopAreaStart + stopAreaWidth/2, 0, 0), eastSize);
        
        Gizmos.color = zebraStripeColor;
        for(float z = -IntersectionHalfSize; z < IntersectionHalfSize; z += 0.8f){
            float stripeWidth = 0.4f;
            Gizmos.DrawLine(new Vector3(stopAreaStart, 0.3f, z), new Vector3(stopAreaStart, 0.3f, z + stripeWidth));
            Gizmos.DrawLine(new Vector3(stopAreaEnd, 0.3f, z), new Vector3(stopAreaEnd, 0.3f, z + stripeWidth));
        }
        
        Gizmos.color = stopAreaColor;
        Gizmos.DrawCube(new Vector3(-stopAreaStart - stopAreaWidth/2, 0, 0), eastSize);
        
        Gizmos.color = zebraStripeColor;
        for(float z = -IntersectionHalfSize; z < IntersectionHalfSize; z += 0.8f){
            float stripeWidth = 0.4f;
            Gizmos.DrawLine(new Vector3(-stopAreaStart, 0.3f, z), new Vector3(-stopAreaStart, 0.3f, z + stripeWidth));
            Gizmos.DrawLine(new Vector3(-stopAreaEnd, 0.3f, z), new Vector3(-stopAreaEnd, 0.3f, z + stripeWidth));
        }
    }
    
    void DrawTurnPoints(){
        if(northTurnPoint != null){
            Gizmos.color = turnPointColor;
            Gizmos.DrawWireSphere(northTurnPoint.position, 0.5f);
        }
        if(southTurnPoint != null){
            Gizmos.color = turnPointColor;
            Gizmos.DrawWireSphere(southTurnPoint.position, 0.5f);
        }
        if(eastTurnPoint != null){
            Gizmos.color = turnPointColor;
            Gizmos.DrawWireSphere(eastTurnPoint.position, 0.5f);
        }
        if(westTurnPoint != null){
            Gizmos.color = turnPointColor;
            Gizmos.DrawWireSphere(westTurnPoint.position, 0.5f);
        }
    }
    
    void DrawSpawnPoint(Transform spawnPoint, Color color){
        if(spawnPoint == null) return;
        
        Gizmos.color = color;
        Gizmos.DrawWireSphere(spawnPoint.position, 0.4f);
        Gizmos.DrawSphere(spawnPoint.position, 0.15f);
        
        if(showDirectionArrows){
            Vector3 direction = GetMovementDirection(spawnPoint);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + direction * 1.5f);
            
            Vector3 arrowEnd = spawnPoint.position + direction * 1.5f;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized * 0.3f;
            Gizmos.DrawLine(arrowEnd, arrowEnd - direction * 0.3f + perpendicular);
            Gizmos.DrawLine(arrowEnd, arrowEnd - direction * 0.3f - perpendicular);
        }
    }
    #endregion
}