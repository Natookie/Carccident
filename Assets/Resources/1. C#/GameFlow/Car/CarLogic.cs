#pragma warning disable 0414

using UnityEngine;
using System.Collections;
using NaughtyAttributes;

public class CarLogic : MonoBehaviour
{
    [SerializeField] private CarConfiguration carConfig;
    [SerializeField] private bool showData = true;
    [SerializeField] private bool showBool = true;

    [Header("CAR CONFIGURATION")]
    [ShowIf("showData"), ReadOnly] public int carID;
    [ShowIf("showData"), ReadOnly] public int laneID;
    [ShowIf("showData"), ReadOnly] public TurnIntent turnIntent;

    [Header("MOVEMENT")]
    [ShowIf("showData"), ReadOnly] public Vector3 moveDirection;
    [ShowIf("showData"), ReadOnly] public float distanceTraveled;
    [ShowIf("showData"), ReadOnly] public float distanceToStopLine;

    [Header("ACTUAL STATS")]
    [ShowIf("showData"), ReadOnly] public float actualMaxSpeed;
    [ShowIf("showData"), ReadOnly] public float actualAcceleration;
    [ShowIf("showData"), ReadOnly] public float actualDeceleration;
    [ShowIf("showData"), ReadOnly] public float actualReactionTime;
    [ShowIf("showData"), ReadOnly] public float actualTurnSpeed;
    [ShowIf("showData"), ReadOnly] public float actualFollowDistance;

    [Header("SHOW TURN VALUES")]
    [ShowIf("showData"), ReadOnly] public Vector3 turnStartPoint;
    [ShowIf("showData"), ReadOnly] public Vector3 turnControlPoint;
    [ShowIf("showData"), ReadOnly] public Vector3 turnEndPoint;
    [ShowIf("showData"), ReadOnly] public Vector3 turnFinalDestination;
    [Space(10)]
    [ShowIf("showData"), ReadOnly] public float turnProgress = 0f;
    [ShowIf("showData"), ReadOnly] public Vector3 originalMoveDirection;

    [Header("SHOW BOOLEAN")]
    [ShowIf("showBool"), ReadOnly] public bool isTurning;
    [ShowIf("showBool"), ReadOnly] public bool hasReachedTurnPoint = false;
    [Space(10)]
    [ShowIf("showBool"), ReadOnly] public bool isStopped = true;
    [ShowIf("showBool"), ReadOnly] public bool isWaitingForGreen = false;
    [ShowIf("showBool"), ReadOnly] public bool hasPassedLight = false;
    [Space(10)]
    [ShowIf("showBool"), ReadOnly] public bool isRoadRage = false;
    [ShowIf("showBool"), ReadOnly] public bool hasRaged = false;
    [Space(10)]
    [ShowIf("showBool"), ReadOnly] public bool isAligningAfterTurn = false;
    [ShowIf("showBool"), ReadOnly] public bool isAligningAfterRage = false;
    [ShowIf("showBool"), ReadOnly] public bool isCollisionDisabled = false;
    [Space(10)]
    [ShowIf("showBool"), ReadOnly] public bool pendingStopDecision = false;
    [ShowIf("showBool"), ReadOnly] public bool bufferedShouldStop = false;
    [ShowIf("showBool"), ReadOnly] public bool isHesitating = false;

    [Header("REFERENCES")]
    [ReadOnly] public Transform turnTarget;
    [ReadOnly] public Transform stopArea;
    [ReadOnly] public Transform visualTransform;

    private CarManager carManager;
    private TrafficLightManager trafficManager;
    private RoadSpawnerManager roadSpawnerManager;
    private Rigidbody rb;
    private Collider carCollider;
    private Renderer carRenderer;

    private float patientTimer = 0f;
    internal float currentSpeed = 0f;
    private Vector3 lastPosition;
    private float roadLength;
    internal float carHalfLength = 1f;

    private Quaternion targetLaneRotation;
    private const float ROTATION_ALIGN_SPEED = 6f;
    private const float ROTATION_ALIGNMENT_THRESHOLD = 0.5f;

    private float reactionTimer = 0f;
    private float hesitationTimer = 0f;
    private float hesitationDuration = 0f;

    private float cachedDistanceToCarAhead = 999f;
    internal CarLogic cachedCarAhead = null;
    private Color originalColor;

    private Coroutine currentRageCoroutine;

    private float brakingNoise = 0f;
    private float brakingNoiseTimer = 0f;
    private const float BRAKING_NOISE_INTERVAL = 0.15f;
    private const float NO_OBSTACLE = 999f;
    private const float DESPAWN_DELAY = 5f;
    private const float X_ROT_DEFAULT = -90f;

    private float exitOffset;
    private float bezierOffset;
    private float raycastDistance;
    private LayerMask obstacleLayerMask;
    private float patienceThreshold;
    private float farZoneDistance;
    private float mediumZoneDistance;
    private float closeZoneDistance;

    private CarMovementHandler movementHandler;
    private CarPerceptionHandler perceptionHandler;
    private CarTurnHandler turnHandler;
    private CarRageHandler rageHandler;
    private CarCollisionHandler collisionHandler;

    public enum TurnIntent{
        Straight,
        Right
    }

    #region INITIALIZATION
    void Awake(){
        carManager = CarManager.Instance;
        trafficManager = TrafficLightManager.Instance;
        roadSpawnerManager = FindFirstObjectByType<RoadSpawnerManager>();

        if(roadSpawnerManager != null) roadLength = roadSpawnerManager.roadLength;
        
        InitializeHandlers();
    }

    void InitializeHandlers(){
        movementHandler = new CarMovementHandler();
        perceptionHandler = new CarPerceptionHandler();
        turnHandler = new CarTurnHandler();
        rageHandler = new CarRageHandler();
        collisionHandler = new CarCollisionHandler();
    }

    public void Initialize(int id, int lane, Vector3 startPos, TurnIntent turn, Transform target, Transform stop){
        carID = id;
        laneID = lane;
        turnIntent = turn;
        turnTarget = target;
        stopArea = stop;
        transform.position = startPos;
        lastPosition = startPos;
        
        ResetAllStates();
        InitializeComponent();
        
        if(carConfig != null) LoadConfiguration();
        else Debug.LogError($"Car {carID} has no CarConfiguration assigned!", this);
        
        SetMovementDirection();
        SetLayer();
        
        transform.rotation = GetRotationForLane(laneID);
    }

   void LoadConfiguration(){
        CarRuntimeStats stats = new CarRuntimeStats();
        stats.RandomizeStats(carConfig);
        
        actualMaxSpeed = stats.actualMaxSpeed;
        actualAcceleration = stats.actualAcceleration;
        actualDeceleration = stats.actualDeceleration;
        actualReactionTime = stats.actualReactionTime;
        actualTurnSpeed = stats.actualTurnSpeed;
        actualFollowDistance = stats.actualFollowDistance;
        
        exitOffset = carConfig.exitOffset;
        bezierOffset = carConfig.bezierOffset;
        raycastDistance = carConfig.raycastDistance;
        obstacleLayerMask = carConfig.obstacleLayerMask;
        farZoneDistance = carConfig.farZoneDistance;
        mediumZoneDistance = carConfig.mediumZoneDistance;
        closeZoneDistance = carConfig.closeZoneDistance;
        patienceThreshold = carConfig.patienceThreshold;
        
        movementHandler.Initialize(ROTATION_ALIGN_SPEED, ROTATION_ALIGNMENT_THRESHOLD);
        rageHandler.Initialize(X_ROT_DEFAULT);
        collisionHandler.Initialize(DESPAWN_DELAY);
    }

    void InitializeComponent(){
        rb = GetComponent<Rigidbody>();
        if(rb == null) rb = gameObject.AddComponent<Rigidbody>();

        carCollider = GetComponent<Collider>();
        if(carCollider == null) carCollider = gameObject.AddComponent<BoxCollider>();
        if(carCollider is BoxCollider boxCollider) carHalfLength = boxCollider.size.z * 0.5f;
        else carHalfLength = carCollider.bounds.extents.z;

        visualTransform = transform.GetChild(0);
        visualTransform.localRotation = Quaternion.Euler(X_ROT_DEFAULT, 0f, 0f);
        carRenderer = visualTransform.GetComponent<Renderer>();
        
        rb.isKinematic = false;
        originalColor = carRenderer.material.color;
    }

    void SetLayer(){
        int baseID = laneID & ~1;
        switch(baseID){
            case 0: gameObject.layer = 6; break;
            case 2: gameObject.layer = 7; break;
            case 4: gameObject.layer = 8; break;
            case 6: gameObject.layer = 9; break;
            default: gameObject.layer = 0; break;
        }
        obstacleLayerMask = (1 << 6) | (1 << 7) | (1 << 8) | (1 << 9);
    }

    void ResetAllStates(){
        currentSpeed = 0f;
        isStopped = true;
        isWaitingForGreen = false;
        hasPassedLight = false;
        distanceTraveled = 0f;
        lastPosition = Vector3.zero;
        
        isTurning = false;
        hasReachedTurnPoint = false;
        turnProgress = 0f;
        turnStartPoint = Vector3.zero;
        turnControlPoint = Vector3.zero;
        turnEndPoint = Vector3.zero;
        turnFinalDestination = Vector3.zero;
        
        patientTimer = 0f;
        isRoadRage = false;
        hasRaged = false;
        isAligningAfterTurn = false;
        isAligningAfterRage = false;
        
        isCollisionDisabled = false;
        
        cachedDistanceToCarAhead = NO_OBSTACLE;
        cachedCarAhead = null;
        
        isHesitating = false;
        hesitationTimer = 0f;
        hesitationDuration = 0f;
        
        pendingStopDecision = false;
        bufferedShouldStop = false;
        reactionTimer = 0f;
        
        brakingNoise = 0f;
        brakingNoiseTimer = 0f;
        
        if(visualTransform != null){
            visualTransform.localPosition = Vector3.zero;
            visualTransform.localRotation = Quaternion.Euler(X_ROT_DEFAULT, 0f, 0f);
        }
        
        if(rb != null){
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        if(carCollider != null) carCollider.enabled = true;
        if(carRenderer != null) carRenderer.material.color = originalColor;

        if(currentRageCoroutine != null){
            StopCoroutine(currentRageCoroutine);
            currentRageCoroutine = null;
        }
    }

    internal void SetMovementDirection(){
        if(roadSpawnerManager != null){
            Transform spawnPoint = GetSpawnPointFromLaneID();
            if(spawnPoint != null){
                moveDirection = roadSpawnerManager.GetMovementDirection(spawnPoint);
                originalMoveDirection = moveDirection;
                return;
            }
        }

        switch(laneID){
            case 0: moveDirection = Vector3.back; break;
            case 2: moveDirection = Vector3.forward; break;
            case 4: moveDirection = Vector3.left; break;
            case 6: moveDirection = Vector3.right; break;
            default: moveDirection = Vector3.forward; break;
        }
        originalMoveDirection = moveDirection;
    }

    internal Transform GetSpawnPointFromLaneID(){
        if(carManager == null || carManager.spawnPoints == null) return null;

        foreach(Transform spawnPoint in carManager.spawnPoints)
            if(roadSpawnerManager.GetLaneID(spawnPoint) == laneID) return spawnPoint;

        return null;
    }

    internal Quaternion GetRotationForLane(int laneID){
        int baseID = laneID & ~1;
        switch(baseID){
            case 0: return Quaternion.Euler(0f, 180f, 0f);
            case 2: return Quaternion.Euler(0f, 0f, 0f);
            case 4: return Quaternion.Euler(0f, -90f, 0f);
            case 6: return Quaternion.Euler(0f, 90f, 0f);
            default: return Quaternion.identity;
        }
    }
    #endregion

    #region UPDATE LOOP
    void Update(){
        if(!isStopped) distanceTraveled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        if(distanceTraveled > roadLength * 2){
            ReturnToPool();
            GameManager.Instance.OnCarPassed();
            return;
        }

        UpdatePerceptionCache();
        
        if(turnIntent == TurnIntent.Right && turnTarget != null && !isCollisionDisabled){
            if(!hasReachedTurnPoint) HandleTurning();
            if(isTurning) UpdateTurn();
        }
        if(isAligningAfterTurn || isAligningAfterRage) AlignToLaneRotation();

        if(!isTurning && !isCollisionDisabled){
            HandleTrafficLight();

            bool rawShouldStop = CheckShouldStop();
            bool reactionDelayedStop = ApplyReactionDelay(rawShouldStop);

            if(isHesitating){
                HandleHesitation();
                return;
            }

            HandleReactionAndMovement(reactionDelayedStop);
        }

        if(isWaitingForGreen && !isTurning && !hasRaged){
            currentSpeed = 0f;
            isStopped = true;
        }

        if(isStopped && !isRoadRage) HandlePatient();
    }

    void FixedUpdate(){
        if(!isTurning && !isCollisionDisabled){
            rb.linearVelocity = moveDirection * currentSpeed;
            if(!hasRaged && !isCollisionDisabled) rb.angularVelocity = Vector3.zero;
        }
    }
    #endregion

    #region PERCEPTION
    void UpdatePerceptionCache() => perceptionHandler.UpdateCache(this, hasRaged, laneID, moveDirection, isTurning, turnEndPoint, raycastDistance, obstacleLayerMask, ref cachedCarAhead, ref cachedDistanceToCarAhead);
    bool ApplyReactionDelay(bool rawShouldStop){
        if(rawShouldStop != pendingStopDecision){
            pendingStopDecision = rawShouldStop;
            reactionTimer = 0f;
        }

        reactionTimer += Time.deltaTime;
        if(reactionTimer >= actualReactionTime) bufferedShouldStop = pendingStopDecision;

        return bufferedShouldStop;
    }

    void HandleHesitation(){
        hesitationTimer += Time.deltaTime;
        currentSpeed = 0f;
        isStopped = true;

        if(hesitationTimer >= hesitationDuration){
            isHesitating = false;
            hesitationTimer = 0f;
        }
    }

    internal void TriggerHesitation(){
        isHesitating = true;
        hesitationTimer = 0f;
        hesitationDuration = actualReactionTime * Random.Range(0.8f, 1.6f);
    }
    #endregion

    #region TRAFFIC LIGHT
    void HandleTrafficLight(){
        bool isGreen = trafficManager != null && trafficManager.IsLaneGreen(laneID);
        distanceToStopLine = GetDistanceToStopLine();
        
        if(isGreen){
            if(isWaitingForGreen){
                isWaitingForGreen = false;
                hasPassedLight = true;
            }
            return;
        }
        
        if(distanceToStopLine > 0f && distanceToStopLine < 2f){
            isWaitingForGreen = true;
            return;
        }
        
        if(distanceToStopLine < 3f && !hasPassedLight) isWaitingForGreen = true;
        if(distanceToStopLine < -5f){
            hasPassedLight = false;
            isWaitingForGreen = false;
        }
    }

    float GetDistanceToStopLine(){
        if(stopArea == null) return NO_OBSTACLE;

        if(originalMoveDirection == Vector3.back) return transform.position.z - stopArea.position.z;
        if(originalMoveDirection == Vector3.forward) return stopArea.position.z - transform.position.z;
        if(originalMoveDirection == Vector3.left) return transform.position.x - stopArea.position.x;
        if(originalMoveDirection == Vector3.right) return stopArea.position.x - transform.position.x;

        return NO_OBSTACLE;
    }
    #endregion

    #region STOPPING
    bool CheckShouldStop() => perceptionHandler.CheckShouldStop(this, hasRaged, isWaitingForGreen, distanceToStopLine, trafficManager, laneID, cachedDistanceToCarAhead, actualFollowDistance, currentSpeed, actualDeceleration);
    void HandleReactionAndMovement(bool shouldStop){
        movementHandler.HandleReactionAndMovement(this, shouldStop, hasRaged, actualMaxSpeed, actualAcceleration, 
            actualDeceleration, cachedDistanceToCarAhead, actualFollowDistance, isWaitingForGreen, distanceToStopLine,
            ref currentSpeed, ref isStopped, ref isHesitating, ref brakingNoise, ref brakingNoiseTimer, 
            BRAKING_NOISE_INTERVAL, NO_OBSTACLE, farZoneDistance, mediumZoneDistance, closeZoneDistance);
    }
    #endregion

    #region TURNING
    void HandleTurning() => turnHandler.HandleTurning(this, turnTarget, trafficManager, laneID, hasRaged, originalMoveDirection, actualDeceleration, ref isWaitingForGreen, ref currentSpeed, ref isStopped, ref hasReachedTurnPoint);
    internal float GetDistanceAlongMoveDirection(Vector3 target){
        if(originalMoveDirection == Vector3.back) return Mathf.Abs(target.z - transform.position.z);
        if(originalMoveDirection == Vector3.forward) return Mathf.Abs(transform.position.z - target.z);
        if(originalMoveDirection == Vector3.left) return Mathf.Abs(target.x - transform.position.x);
        if(originalMoveDirection == Vector3.right) return Mathf.Abs(transform.position.x - target.x);
        return Vector3.Distance(transform.position, target);
    }

    internal void StartTurn() => turnHandler.StartTurn(this, turnTarget, originalMoveDirection, exitOffset, bezierOffset, roadLength, carManager, roadSpawnerManager, laneID, ref isTurning, ref hasReachedTurnPoint, rb, ref currentSpeed, ref turnStartPoint, ref turnControlPoint, ref turnEndPoint, ref turnFinalDestination, ref turnProgress, ref actualTurnSpeed);
    internal void UpdateTurn() => turnHandler.UpdateTurn(this, actualTurnSpeed, ref turnProgress, ref turnStartPoint, ref turnControlPoint, ref turnEndPoint, ref isTurning, ref rb, ref currentSpeed, ref isStopped, ref moveDirection);
    internal void UpdateAfterTurn() => turnHandler.UpdateAfterTurn(this, ref laneID, ref turnIntent, ref turnTarget, ref moveDirection, ref targetLaneRotation, ref isAligningAfterTurn);
    void AlignToLaneRotation() => movementHandler.AlignToLaneRotation(this, targetLaneRotation, isCollisionDisabled, ref isAligningAfterTurn, ref isAligningAfterRage);
    #endregion

   #region PATIENT LOGIC
    void HandlePatient() => rageHandler.HandlePatient(this, patienceThreshold, visualTransform, ref patientTimer, ref isRoadRage, ref hasRaged);
    internal void TriggerRoadRage() => rageHandler.TriggerRoadRage(this, turnIntent, visualTransform, carRenderer, originalColor, ref currentRageCoroutine, ref isRoadRage, ref isStopped, ref isWaitingForGreen, rb, ref isTurning, ref hasReachedTurnPoint, ref turnProgress, this);
    internal IEnumerator RoadRageCoroutine(){
        yield return rageHandler.RoadRageCoroutine(this, visualTransform, carRenderer, originalColor, X_ROT_DEFAULT, turnIntent, laneID);
    }
    #endregion

    #region COLLISION
    void OnTriggerEnter(Collider other) => collisionHandler.OnTriggerEnter(this, other, isCollisionDisabled, hasRaged, laneID);
    #endregion

    #region INTERNAL METHODS FOR HANDLERS
    internal void SetCurrentSpeed(float speed) => currentSpeed = speed;
    internal void SetIsTurning(bool turning) => isTurning = turning;
    internal void SetIsStopped(bool stopped) => isStopped = stopped;
    internal void SetIsWaitingForGreen(bool waiting) => isWaitingForGreen = waiting;
    internal void SetIsCollisionDisabled(bool disabled) => isCollisionDisabled = disabled;
    internal Rigidbody GetRigidbody() => rb;
    internal Collider GetCarCollider() => carCollider;
    internal float GetCurrentSpeed() => currentSpeed;
    internal CarLogic GetCachedCarAhead() => cachedCarAhead;
    internal float GetPatientTimer() => patientTimer;
    internal void SetPatientTimer(float timer) => patientTimer = timer;
    internal Quaternion GetTargetLaneRotation() => targetLaneRotation;
    internal void SetTargetLaneRotation(Quaternion rotation) => targetLaneRotation = rotation;
    internal float GetCachedDistanceToCarAhead() => cachedDistanceToCarAhead;
    #endregion

    internal void ReturnToPool(){
        if(currentRageCoroutine != null){
            StopCoroutine(currentRageCoroutine);
            currentRageCoroutine = null;
        }
        
        StopAllCoroutines();
        ResetAllStates();
        
        if(carManager != null) carManager.ReturnCarToPool(this);
        else Destroy(gameObject);
    }

    void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawRay(rayOrigin, moveDirection * raycastDistance);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, farZoneDistance);
        Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, mediumZoneDistance);
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, closeZoneDistance);

        if(turnTarget != null){
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(turnTarget.position, 0.5f);
        }

        if(stopArea != null){
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(stopArea.position, new Vector3(3f, 0.5f, 1f));
        }
    }

    void OnDrawGizmos(){
        if(!Application.isPlaying) return;
        
        if(isTurning){
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(turnStartPoint, 0.5f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(turnControlPoint, 0.5f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(turnEndPoint, 0.5f);
            
            Gizmos.color = Color.white;
            Gizmos.DrawLine(turnStartPoint, turnControlPoint);
            Gizmos.DrawLine(turnControlPoint, turnEndPoint);
        }
    }
}

#pragma warning restore 0414