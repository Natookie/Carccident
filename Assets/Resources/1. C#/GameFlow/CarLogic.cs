#pragma warning disable 0414

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;

public class CarLogic : MonoBehaviour
{
    [Foldout("CAR SETTINGS")][SerializeField] private float speed = 10f;
    [Foldout("CAR SETTINGS")][SerializeField] private float speedVariation = 2f;
    [Space(5)]
    [Foldout("CAR SETTINGS")][SerializeField] private float reactionTime = 0.5f;
    [Foldout("CAR SETTINGS")][SerializeField] private float reactionTimeVariation = 0.3f;
    [Space(5)]
    [Foldout("CAR SETTINGS")][SerializeField] private float accelerationRate = 5f;
    [Foldout("CAR SETTINGS")][SerializeField] private float accelerationVariation = 1.5f;
    [Space(5)]
    [Foldout("CAR SETTINGS")][SerializeField] private float decelerationRate = 8f;
    [Foldout("CAR SETTINGS")][SerializeField] private float decelerationVariation = 2f;

    [Foldout("TURNING SETTINGS")][SerializeField] private float turnSpeed = 5f;
    [Foldout("TURNING SETTINGS")][SerializeField] private float turnSpeedVariation = 1.5f;
    [Space(5)]
    [Foldout("TURNING SETTINGS")][SerializeField] private float exitOffset = 5f;
    [Foldout("TURNING SETTINGS")][SerializeField] private float bezierOffset = 2f;

    [Foldout("RAYCAST SETTINGS")][SerializeField] private float raycastDistance = 30f;
    [Foldout("RAYCAST SETTINGS")][SerializeField] private LayerMask obstacleLayerMask;
    [Foldout("RAYCAST SETTINGS")][SerializeField] private float minFollowDistance = 4f;
    [Foldout("RAYCAST SETTINGS")][SerializeField] private float followDistanceVariation = 1.5f;

    [Header("PERCEPTION ZONES")]
    [SerializeField] private float farZoneDistance = 20f;
    [SerializeField] private float mediumZoneDistance = 12f;
    [SerializeField] private float closeZoneDistance = 6f;

    [Header("COLLISION")]
    [SerializeField] private float knockUpForce = 12f;
    [SerializeField] private float knockBackForce = 15f;
    [SerializeField] private float despawnDelay = 1f;

    [Header("PATIENT SETTINGS")]
    [SerializeField] private float patientTimer = 0f;
    [SerializeField] private float patienceThreshold = 8f;

    [Header("CAR CONFIGURATION")]
    [ReadOnly] public int carID;
    [ReadOnly] public int laneID;
    [ReadOnly] public TurnIntent turnIntent;

    [Header("MOVEMENT")]
    [ReadOnly] public Vector3 moveDirection;
    [ReadOnly] public float distanceTraveled;
    [ReadOnly] public bool isTurning;

    [Header("ACTUAL STATS")]
    [SerializeField] private bool showTrueValue = true;
    [Space(5)]
    [ReadOnly, ShowIf("showTrueValue")] public float actualMaxSpeed;
    [ReadOnly, ShowIf("showTrueValue")] public float actualAcceleration;
    [ReadOnly, ShowIf("showTrueValue")] public float actualDeceleration;
    [ReadOnly, ShowIf("showTrueValue")] public float actualReactionTime;
    [ReadOnly, ShowIf("showTrueValue")] public float actualTurnSpeed;
    [ReadOnly, ShowIf("showTrueValue")] public float actualFollowDistance;

    [Header("SHOW TURN VALUES")]
    [SerializeField] private bool showTurnValues = true;
    [ShowIf("showTurnValues"), ReadOnly] public Vector3 turnStartPoint;
    [ShowIf("showTurnValues"), ReadOnly] public Vector3 turnControlPoint;
    [ShowIf("showTurnValues"), ReadOnly] public Vector3 turnEndPoint;
    [ShowIf("showTurnValues"), ReadOnly] public Vector3 turnFinalDestination;
    [Space(10)]
    [ShowIf("showTurnValues"), ReadOnly] public float turnProgress = 0f;
    [ShowIf("showTurnValues"), ReadOnly] public bool hasReachedTurnPoint = false;
    [ShowIf("showTurnValues"), ReadOnly] public Vector3 originalMoveDirection;

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

    private float currentSpeed = 0f;
    private bool isStopped = true;
    private bool isWaitingForGreen = false;
    private bool hasPassedLight = false;
    private bool isRoadRage = false;
    private bool hasRaged = false;
    private Vector3 lastPosition;
    private float roadLength;

    private bool isAligningAfterTurn = false;
    private bool isAligningAfterRage = false;
    private Quaternion targetLaneRotation;
    private const float ROTATION_ALIGN_SPEED = 6f;
    private const float ROTATION_ALIGNMENT_THRESHOLD = 0.5f;

    private bool isCollisionDisabled = false;
    private float collisionDisableTimer = 0f;

    private bool pendingStopDecision = false;
    private bool bufferedShouldStop = false;
    private float reactionTimer = 0f;

    private bool isHesitating = false;
    private float hesitationTimer = 0f;
    private float hesitationDuration = 0f;

    private float cachedDistanceToCarAhead = 999f;
    private CarLogic cachedCarAhead = null;
    private Color originalColor;

    private Coroutine currentRageCoroutine;
    private Coroutine currentDespawnCoroutine;

    private float brakingNoise = 0f;
    private float brakingNoiseTimer = 0f;
    private const float BRAKING_NOISE_INTERVAL = 0.15f;

    private const float NO_OBSTACLE = 999f;

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
        InitializeActualValue();
        
        SetMovementDirection();
        SetLayer();
        
        transform.rotation = GetRotationForLane(laneID);
    }

    void InitializeActualValue(){
        actualMaxSpeed = speed + Random.Range(-speedVariation, speedVariation);
        actualAcceleration = accelerationRate + Random.Range(-accelerationVariation, accelerationVariation);
        actualDeceleration = decelerationRate + Random.Range(-decelerationVariation, decelerationVariation);
        actualReactionTime = reactionTime + Random.Range(-reactionTimeVariation, reactionTimeVariation);
        actualTurnSpeed = turnSpeed + Random.Range(-turnSpeedVariation, turnSpeedVariation);
        actualFollowDistance = minFollowDistance + Random.Range(-followDistanceVariation, followDistanceVariation);
        
        actualMaxSpeed = Mathf.Max(15f, actualMaxSpeed);
        actualAcceleration = Mathf.Max(3f, actualAcceleration);
        actualDeceleration = Mathf.Max(4f, actualDeceleration);
        actualReactionTime = Mathf.Max(0.3f, actualReactionTime);
        actualTurnSpeed = Mathf.Max(3f, actualTurnSpeed);
        actualFollowDistance = Mathf.Max(2f, actualFollowDistance);
    }

    void InitializeComponent(){
        rb = GetComponent<Rigidbody>();
        if(rb == null) rb = gameObject.AddComponent<Rigidbody>();

        carCollider = GetComponent<Collider>();
        if(carCollider == null) carCollider = gameObject.AddComponent<BoxCollider>();

        visualTransform = transform.GetChild(0);
        carRenderer = visualTransform.GetComponent<Renderer>();
        
        rb.isKinematic = false;
        originalColor = carRenderer.material.color;
    }

    void SetLayer(){
        int baseID = laneID & ~1;
        switch(baseID){
            case 0: gameObject.layer = 6; break; //NorthRoad
            case 2: gameObject.layer = 7; break; //SouthRoad
            case 4: gameObject.layer = 8; break; //EastRoad
            case 6: gameObject.layer = 9; break; //WestRoad
            default: gameObject.layer = 0; break;
        }
        obstacleLayerMask = (1 << 6) | (1 << 7) | (1 << 8) | (1 << 9);
    }

    void ResetAllStates(){
        //Movement states
        currentSpeed = 0f;
        isStopped = true;
        isWaitingForGreen = false;
        hasPassedLight = false;
        distanceTraveled = 0f;
        lastPosition = Vector3.zero;
        
        //Turn states
        isTurning = false;
        hasReachedTurnPoint = false;
        turnProgress = 0f;
        turnStartPoint = Vector3.zero;
        turnControlPoint = Vector3.zero;
        turnEndPoint = Vector3.zero;
        turnFinalDestination = Vector3.zero;
        
        //Rage/Patient states
        patientTimer = 0f;
        isRoadRage = false;
        hasRaged = false;
        isAligningAfterTurn = false;
        isAligningAfterRage = false;
        
        //Collision states
        isCollisionDisabled = false;
        collisionDisableTimer = 0f;
        
        //Perception states
        cachedDistanceToCarAhead = NO_OBSTACLE;
        cachedCarAhead = null;
        
        //Hesitation states
        isHesitating = false;
        hesitationTimer = 0f;
        hesitationDuration = 0f;
        
        //Reaction states
        pendingStopDecision = false;
        bufferedShouldStop = false;
        reactionTimer = 0f;
        
        //Braking states
        brakingNoise = 0f;
        brakingNoiseTimer = 0f;
        
        //Visual states
        if(visualTransform != null){
            visualTransform.localPosition = Vector3.zero;
            visualTransform.localRotation = Quaternion.identity;
        }
        
        //Rigidbody states
        if(rb != null){
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        //Collider states
        if(carCollider != null) carCollider.enabled = true;
        
        //Renderer states
        if(carRenderer != null) carRenderer.material.color = originalColor;

        //Clear coroutine references
        if(currentRageCoroutine != null){
            StopCoroutine(currentRageCoroutine);
            currentRageCoroutine = null;
        }
        
        if(currentDespawnCoroutine != null){
            StopCoroutine(currentDespawnCoroutine);
            currentDespawnCoroutine = null;
        }
    }

    void SetMovementDirection(){
        if(roadSpawnerManager != null){
            Transform spawnPoint = GetSpawnPointFromLaneID();
            if(spawnPoint != null){
                moveDirection = roadSpawnerManager.GetMovementDirection(spawnPoint);
                originalMoveDirection = moveDirection;
                return;
            }
        }

        int baseID = laneID & ~1;
        switch(laneID){
            case 0: moveDirection = Vector3.back; break;
            case 2: moveDirection = Vector3.forward; break;
            case 4: moveDirection = Vector3.left; break;
            case 6: moveDirection = Vector3.right; break;
            default: moveDirection = Vector3.forward; break;
        }
        originalMoveDirection = moveDirection;
    }

    Transform GetSpawnPointFromLaneID(){
        if(carManager == null || carManager.spawnPoints == null) return null;

        foreach(Transform spawnPoint in carManager.spawnPoints){
            if(roadSpawnerManager.GetLaneID(spawnPoint) == laneID) return spawnPoint;
        }
        return null;
    }

    Quaternion GetRotationForLane(int laneID){
        int baseID = laneID & ~1;
        switch(baseID){
            case 0: return Quaternion.Euler(0f, 180f, 0f);  //North lanes
            case 2: return Quaternion.Euler(0f, 0f, 0f);    //South lanes
            case 4: return Quaternion.Euler(0f, -90f, 0f);  //East lanes
            case 6: return Quaternion.Euler(0f, 90f, 0f);   //West lane 
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
        
        if(turnIntent == TurnIntent.Right && turnTarget != null && !isCollisionDisabled && !isRoadRage){
            if(!hasReachedTurnPoint) HandleTurning();
            if(isTurning) UpdateTurn();
        }
        if(isAligningAfterTurn || isAligningAfterRage) AlignToLaneRotation();

        if(!isTurning && !isCollisionDisabled){
            if(!isTurning) HandleTrafficLight();

            bool rawShouldStop = CheckShouldStop();
            bool reactionDelayedStop = ApplyReactionDelay(rawShouldStop);

            if(isHesitating){
                HandleHesitation();
                return;
            }

            HandleReactionAndMovement(reactionDelayedStop);
        }

        if(isCollisionDisabled){
            collisionDisableTimer -= Time.deltaTime;
            if(collisionDisableTimer <= 0f){
                isCollisionDisabled = false;
                if(carCollider != null) carCollider.enabled = true;
            }
        }

        if(isStopped && !isRoadRage) HandlePatient();
    }

    void FixedUpdate(){
        if(!isTurning && !isCollisionDisabled) rb.linearVelocity = moveDirection * currentSpeed;
    }
    #endregion

    #region PERCEPTION
    void UpdatePerceptionCache(){
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 checkDirection = isTurning ? (turnEndPoint - transform.position).normalized : moveDirection;

        if(Physics.Raycast(rayOrigin, checkDirection, out hit, raycastDistance, obstacleLayerMask)){
            CarLogic otherCar = hit.collider.GetComponent<CarLogic>();
            if(otherCar != null && otherCar.laneID == laneID && !otherCar.isCollisionDisabled){
                cachedCarAhead = otherCar;
                float rawDistance = Vector3.Distance(transform.position, otherCar.transform.position);
                cachedDistanceToCarAhead = Mathf.Max(0.1f, rawDistance - 2f);
                return;
            }
        }

        cachedCarAhead = null;
        cachedDistanceToCarAhead = NO_OBSTACLE;
    }

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

    void TriggerHesitation(){
        isHesitating = true;
        hesitationTimer = 0f;
        hesitationDuration = actualReactionTime * Random.Range(0.8f, 1.6f);
    }
    #endregion

    #region TRAFFIC LIGHT
    void HandleTrafficLight(){
        bool isGreen = trafficManager != null && trafficManager.IsLaneGreen(laneID);
        float distanceToStopLine = GetDistanceToStopLine();

        if(distanceToStopLine > 0f && distanceToStopLine < 2f && !isGreen){
            isWaitingForGreen = true;
            return;
        }

        if(distanceToStopLine < 8f){
            if(!isGreen && !hasPassedLight) isWaitingForGreen = true;
            else if(isGreen){
                bool wasWaiting = isWaitingForGreen;
                isWaitingForGreen = false;
                hasPassedLight = true;

                if(wasWaiting && isStopped) TriggerHesitation();
            }
        }

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
    bool CheckShouldStop(){
        if(hasRaged) return false;

        float effectiveDistance = cachedDistanceToCarAhead - actualFollowDistance;
        float stoppingDistance = (currentSpeed * currentSpeed) / (2f * actualDeceleration) + 1f;

        if(effectiveDistance < stoppingDistance && cachedDistanceToCarAhead < NO_OBSTACLE - 1f) return true;
        if(isWaitingForGreen) return true;

        float distanceToStopLine = GetDistanceToStopLine();
        if(distanceToStopLine > 0f && distanceToStopLine < 8f){
            bool isGreen = trafficManager != null && trafficManager.IsLaneGreen(laneID);
            if(!isGreen) return true;
        }

        return false;
    }

    float GetNoisyDeceleration(){
        brakingNoiseTimer += Time.deltaTime;
        if(brakingNoiseTimer >= BRAKING_NOISE_INTERVAL){
            brakingNoiseTimer = 0f;
            brakingNoise = Random.Range(-0.15f, 0.15f) * actualDeceleration;
        }
        return Mathf.Max(0.5f, actualDeceleration + brakingNoise);
    }

    float GetZoneBasedTargetSpeed(){
        if(cachedDistanceToCarAhead >= NO_OBSTACLE - 1f) return actualMaxSpeed;

        float aheadSpeed = cachedCarAhead != null ? cachedCarAhead.currentSpeed : 0f;

        if(cachedDistanceToCarAhead > farZoneDistance) return actualMaxSpeed;
        if(cachedDistanceToCarAhead > mediumZoneDistance){
            float t = (cachedDistanceToCarAhead - mediumZoneDistance) / (farZoneDistance - mediumZoneDistance);
            return Mathf.Lerp(aheadSpeed, actualMaxSpeed, t);
        }

        if(cachedDistanceToCarAhead > closeZoneDistance){
            float t = (cachedDistanceToCarAhead - closeZoneDistance) / (mediumZoneDistance - closeZoneDistance);
            return Mathf.Lerp(0f, aheadSpeed, t);
        }

        return 0f;
    }

    void HandleReactionAndMovement(bool shouldStop){
        if(shouldStop){
            float targetDistance = GetTargetStopDistance();
            float noisyDecel = GetNoisyDeceleration();

            if(targetDistance > 0.2f && targetDistance < NO_OBSTACLE - 1f){
                float requiredDecel = (currentSpeed * currentSpeed) / (2f * targetDistance);
                float decel = Mathf.Min(requiredDecel, noisyDecel);
                currentSpeed = Mathf.Max(0, currentSpeed - decel * Time.deltaTime);
            }
            else if(targetDistance >= NO_OBSTACLE - 1f) currentSpeed = Mathf.Max(0, currentSpeed - noisyDecel * 0.3f * Time.deltaTime);
            else currentSpeed = 0;

            bool wasMoving = !isStopped;
            isStopped = currentSpeed < 0.1f;

            if(isStopped && wasMoving && !isHesitating) TriggerHesitation();

            if(cachedDistanceToCarAhead < actualFollowDistance * 0.8f){
                currentSpeed = 0;
                isStopped = true;
            }
        }
        else{
            float zoneTarget = GetZoneBasedTargetSpeed();

            if(currentSpeed < zoneTarget){
                isStopped = false;
                currentSpeed = Mathf.Min(zoneTarget, currentSpeed + actualAcceleration * Time.deltaTime);
            }
            else if(currentSpeed > zoneTarget + 0.5f){
                float noisyDecel = GetNoisyDeceleration();
                currentSpeed = Mathf.Max(zoneTarget, currentSpeed - noisyDecel * 0.5f * Time.deltaTime);
            }

            isStopped = currentSpeed < 0.1f;
        }
    }

    float GetTargetStopDistance(){
        float distanceToCar = cachedDistanceToCarAhead;
        float distanceToStopLine = GetDistanceToStopLine();

        if(distanceToCar < distanceToStopLine) return Mathf.Max(actualFollowDistance * 0.5f, actualFollowDistance);
        if(isWaitingForGreen && distanceToStopLine > 0f) return distanceToStopLine;

        return Mathf.Min(distanceToCar, distanceToStopLine);
    }
    #endregion

    #region TURNING
    void HandleTurning(){
        if(turnIntent == TurnIntent.Right && !hasRaged){
            bool isRightAllowed = trafficManager != null && trafficManager.IsLaneGreen(laneID);
            if(!isRightAllowed) return;
        }
        
        float distanceToTurnPoint = GetDistanceAlongMoveDirection(turnTarget.position);
        if(distanceToTurnPoint < 12f && !hasReachedTurnPoint){
            currentSpeed = Mathf.Min(currentSpeed, 5f);
            StartTurn();
            hasReachedTurnPoint = true;
        }
    }

    float GetDistanceAlongMoveDirection(Vector3 target){
        if(originalMoveDirection == Vector3.back) return Mathf.Abs(target.z - transform.position.z);
        if(originalMoveDirection == Vector3.forward) return Mathf.Abs(transform.position.z - target.z);
        if(originalMoveDirection == Vector3.left) return Mathf.Abs(target.x - transform.position.x);
        if(originalMoveDirection == Vector3.right) return Mathf.Abs(transform.position.x - target.x);
        return Vector3.Distance(transform.position, target);
    }

    void StartTurn(){
        isTurning = true;
        hasReachedTurnPoint = true;
        rb.isKinematic = true;

        currentSpeed = Mathf.Min(currentSpeed, 5f);
        //Debug.Log($"Car {carID}: Turn START - DistanceTraveled: {distanceTraveled:F2}", this);

        Vector3 turnCenter = turnTarget.position;
        Vector3 turnDirection = GetTurnDirection();
        
        Vector3 turnExit = GetTurnExitPoint(turnCenter, turnDirection);
        Vector3 apex = GetTurnApexPoint(turnCenter, turnDirection);
        Vector3 finalDest = GetFinalDestination();

        turnStartPoint = transform.position;
        turnControlPoint = apex;
        turnEndPoint = turnExit;
        turnFinalDestination = finalDest;
        turnProgress = 0f;
        
        float turnDistance = Vector3.Distance(turnStartPoint, turnExit);
        float turnDuration = Mathf.Max(0.6f, turnDistance / 6f);
        actualTurnSpeed = 1f / turnDuration;
    }

    Vector3 GetTurnDirection(){
        if(originalMoveDirection == Vector3.forward) return Vector3.right;   //Turning right from south lane = east
        if(originalMoveDirection == Vector3.back) return Vector3.left;       //Turning right from north lane = west
        if(originalMoveDirection == Vector3.right) return Vector3.back;      //Turning right from west lane = south
        if(originalMoveDirection == Vector3.left) return Vector3.forward;    //Turning right from east lane = north
        return originalMoveDirection;
    }

    Vector3 GetTurnExitPoint(Vector3 center, Vector3 exitDirection){
        Vector3 exitPoint = center + exitDirection * exitOffset;
        Vector3 finalDest = GetFinalDestination();
        
        if(exitDirection == Vector3.right) exitPoint.z = finalDest.z;
        if(exitDirection == Vector3.left) exitPoint.z = finalDest.z;
        if(exitDirection == Vector3.forward) exitPoint.x = finalDest.x;
        if(exitDirection == Vector3.back) exitPoint.x = finalDest.x;
        
        exitPoint.y = transform.position.y;
        return exitPoint;
    }
    Vector3 GetTurnApexPoint(Vector3 center, Vector3 exitDirection){
        Vector3 startDirection = originalMoveDirection;
        Vector3 cornerPoint = center + startDirection * bezierOffset + exitDirection * bezierOffset;
        cornerPoint.y = transform.position.y;
        return cornerPoint;
    }
    Vector3 GetFinalDestination(){
        Transform spawnPoint = GetSpawnPointFromLaneID();
        
        if(spawnPoint == null) return transform.position + moveDirection * 20f;
        
        float absX = Mathf.Abs(spawnPoint.position.x);
        float absZ = Mathf.Abs(spawnPoint.position.z);
        
        int baseID = laneID & ~1;
        switch(baseID){
            case 0: return new Vector3(-roadLength, transform.position.y, absX);
            case 2: return new Vector3(roadLength, transform.position.y, -absX);
            case 4: return new Vector3(absZ, transform.position.y, -roadLength);
            case 6: return new Vector3(-absZ, transform.position.y, roadLength);
            default: return transform.position + moveDirection * 10f;
        }
    }

    void UpdateTurn(){
        turnProgress += Time.deltaTime * actualTurnSpeed;

        if(turnProgress >= 1f){
            transform.position = turnEndPoint;
            
            Vector3 finalTangent = 2f * (turnEndPoint - turnControlPoint);
            finalTangent.y = 0f;
            
            if(finalTangent != Vector3.zero){
                moveDirection = finalTangent.normalized;
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
            
            isTurning = false;
            turnProgress = 0f;
            rb.isKinematic = false;
            currentSpeed = 5f;
            isStopped = false;
            
            UpdateAfterTurn();
            return;
        }
        
        float t = turnProgress;
        
        Vector3 p0 = turnStartPoint;
        Vector3 p1 = turnControlPoint;
        Vector3 p2 = turnEndPoint;
        
        Vector3 newPos = (1f - t) * (1f - t) * p0 + 2f * (1f - t) * t * p1 + t * t * p2;
        newPos.y = turnStartPoint.y;
        transform.position = newPos;
        
        Vector3 tangent = 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
        tangent.y = 0f;
        
        if(tangent != Vector3.zero){
            Quaternion targetRot = Quaternion.LookRotation(tangent.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.deltaTime);
        }
    }

    void UpdateAfterTurn(){
        int newLaneID = -1;
        
        switch(laneID){
            case 0: newLaneID = 5; break;
            case 2: newLaneID = 7; break;
            case 4: newLaneID = 3; break;
            case 6: newLaneID = 1; break;
            default: newLaneID = laneID; break;
        }
        
        if(newLaneID != -1){
            laneID = newLaneID;
            turnIntent = TurnIntent.Straight;
            turnTarget = null;
            
            SetMovementDirection();
            SetLayer();
            
            targetLaneRotation = GetRotationForLane(laneID);
            isAligningAfterTurn = true;
        }
    }

    void AlignToLaneRotation(){
        float angle = Quaternion.Angle(transform.rotation, targetLaneRotation);
        
        if(angle <= ROTATION_ALIGNMENT_THRESHOLD){
            transform.rotation = targetLaneRotation;
            isAligningAfterTurn = false;
            isAligningAfterRage = false;
            return;
        }
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetLaneRotation,
            ROTATION_ALIGN_SPEED * Time.deltaTime
        );
    }
    #endregion

    #region PATIENT LOGIC
    void HandlePatient(){
        if(!isStopped){
            ResetPatient();
            return;
        }
        
        patientTimer += Time.deltaTime;
        float fill = Mathf.Clamp01(patientTimer / patienceThreshold);
        
        if(fill >= 1f && !isRoadRage && !hasRaged){
            TriggerRoadRage();
            return;
        }
        
        if(visualTransform != null){
            float shakeIntensity = Mathf.Sin(Time.time * 20f) * (fill * 0.05f);
            float shakeX = Random.Range(-shakeIntensity, shakeIntensity) * 0.01f;
            float shakeZ = Random.Range(-shakeIntensity, shakeIntensity) * 0.01f;
            visualTransform.localPosition = new Vector3(shakeX, 0, shakeZ);
            
            float tiltAngle = Mathf.Sin(Time.time * 15f) * (fill * 3f);
            visualTransform.localRotation = Quaternion.Euler(0f, 0f, tiltAngle);
        }
    }

    void ResetPatient(){
        patientTimer = 0f;
        if(visualTransform != null){
            visualTransform.localPosition = Vector3.zero;
            visualTransform.localRotation = Quaternion.identity;
        }
    }
    
    void TriggerRoadRage(){
        if(currentRageCoroutine != null){
            StopCoroutine(currentRageCoroutine);
            currentRageCoroutine = null;
        }

        ResetPatient();
        isRoadRage = true;
        hasRaged = true;
        isStopped = false;
        isWaitingForGreen = false;
        rb.linearVelocity = Vector3.zero;

        if(turnIntent == TurnIntent.Right){
            isTurning = false;
            hasReachedTurnPoint = false;
            turnProgress = 0f;
        }

        currentRageCoroutine = StartCoroutine(RoadRageCoroutine());
    }

    IEnumerator RoadRageCoroutine(){
        if(visualTransform == null){
            currentRageCoroutine = null;
            yield break;
        }
        
        float rageDuration = 2f;
        float elapsed = 0f;
        
        float originalY = visualTransform.eulerAngles.y;
        
        while(elapsed < rageDuration){
            if(this == null || gameObject == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / rageDuration;
            
            if(carRenderer != null) carRenderer.material.color = Color.Lerp(originalColor, Color.red, t);
            
            float rotX = Mathf.Sin(Time.time * 30f) * (15f * (1f - t));
            float rotZ = Mathf.Cos(Time.time * 28f) * (10f * (1f - t));
            float rotY = Mathf.Sin(Time.time * 8f) * (3f * (1f - t));
            float finalY = originalY + rotY;
            
            visualTransform.rotation = Quaternion.Euler(rotX, finalY, rotZ);
            yield return null;
        }
        
        if(carRenderer != null) carRenderer.material.color = originalColor;
        isRoadRage = false;
        
        if(visualTransform != null) visualTransform.localRotation = Quaternion.identity;
        if(turnIntent == TurnIntent.Straight){
            targetLaneRotation = GetRotationForLane(laneID);
            isAligningAfterRage = true;
        }
        
        currentRageCoroutine = null;
    }
    #endregion

    #region COLLISION
    bool IsHitFromSide(CarLogic victim, CarLogic hitter){
        if(victim == null || hitter == null) return false;
        
        Vector3 victimForward = victim.transform.forward;
        Vector3 hitDirection = (victim.transform.position - hitter.transform.position).normalized;
        
        float forwardDot = Vector3.Dot(victimForward, hitDirection);
        float rightDot = Vector3.Dot(victim.transform.right, hitDirection);
        
        bool isSideHit = Mathf.Abs(rightDot) > Mathf.Abs(forwardDot);
        
        return isSideHit;
    }

    void OnTriggerEnter(Collider other){
        CarLogic otherCar = other.GetComponent<CarLogic>();
        if(otherCar == null) return;
        if(isCollisionDisabled || otherCar.isCollisionDisabled) return;
        
        bool sameRoad = IsSameRoad(otherCar);
        if(sameRoad) return;

        if(GetInstanceID() > otherCar.GetInstanceID()) return;
        ProcessCollision(this, otherCar);
    }

    void ProcessCollision(CarLogic carA, CarLogic carB){
        bool isAVictim = IsHitFromSide(carA, carB);
        bool isBVictim = IsHitFromSide(carB, carA);
        
        if(isAVictim && isBVictim){
            ApplyKnockback(carA, carB);
            ApplyKnockback(carB, carA);
            GameManager.Instance.OnAccident();
            return;
        }
        
        if(isAVictim && !isBVictim){
            ApplyKnockback(carA, carB);
            GameManager.Instance.OnAccident();
            return;
        }
        
        if(!isAVictim && isBVictim){
            ApplyKnockback(carB, carA);
            GameManager.Instance.OnAccident();
            return;
        }
        
        GameManager.Instance.OnAccident();
    }

    void ApplyKnockback(CarLogic victim, CarLogic hitter)
    {
        if(victim == null || hitter == null) return;
        if(victim.rb == null || hitter.rb == null) return;

        Rigidbody victimRB = victim.rb;
        Rigidbody hitterRB = hitter.rb;

        victim.currentSpeed = 0f;
        victim.isStopped = true;
        victim.isTurning = false;
        victim.isWaitingForGreen = false;

        victimRB.isKinematic = false;
        victimRB.useGravity = true;

        if(victim.carCollider != null) victim.carCollider.enabled = false;
        
        victimRB.mass = 1.2f;
        victimRB.linearDamping = 0.25f;
        victimRB.angularDamping = 0.15f;

        victimRB.linearVelocity = Vector3.zero;
        victimRB.angularVelocity = Vector3.zero;

        Vector3 relativeVelocity = hitterRB.linearVelocity - victimRB.linearVelocity;
        float impactSpeed = relativeVelocity.magnitude;

        if(impactSpeed < 2f){
            relativeVelocity = hitter.transform.forward * 8f;
            impactSpeed = relativeVelocity.magnitude;
        }

        Vector3 impactDirection = relativeVelocity.normalized;
        float sideDot = Vector3.Dot(
            victim.transform.right,
            impactDirection
        );

        float sideAmount = Mathf.Abs(sideDot);

        float baseForce = Mathf.Clamp(impactSpeed * 2.5f, 10f, 45f);
        float sideBonus = Mathf.Lerp(1f, 1.8f, sideAmount);
        float finalForce = baseForce * sideBonus;

        float upwardMultiplier = Mathf.Lerp(0.35f, 1.4f, sideAmount);
        Vector3 horizontalForce = impactDirection * finalForce;
        Vector3 upwardForce = Vector3.up * (finalForce * upwardMultiplier);

        Vector3 randomForce =
            new Vector3(
                Random.Range(-2f, 2f),
                Random.Range(0f, 1.5f),
                Random.Range(-2f, 2f)
            );

        Vector3 totalForce = horizontalForce + upwardForce + randomForce;
        victimRB.AddForce(totalForce, ForceMode.Impulse);

        float spinDirection =Mathf.Sign(sideDot);
        float torqueStrength = Mathf.Clamp(finalForce * 1.1f, 8f, 40f);
        victimRB.AddTorque(Vector3.up * torqueStrength * spinDirection, ForceMode.Impulse);

        Vector3 tumbleTorque =
            new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-0.3f, 0.3f),
                Random.Range(-1f, 1f)
            ).normalized;

        victimRB.AddTorque(tumbleTorque * torqueStrength * 0.7f, ForceMode.Impulse);
        if(sideAmount > 0.75f) victimRB.AddTorque(victim.transform.forward * Random.Range(-25f, 25f), ForceMode.Impulse);

        victimRB.AddForce(Vector3.up * Random.Range(2f, 6f), ForceMode.Impulse);
        victim.StartCoroutine(victim.DelayedDespawn());
    }

    IEnumerator DelayedDespawn(){
        yield return new WaitForSeconds(despawnDelay);
        if(this != null && gameObject != null) ReturnToPool();
    }

    bool IsSameRoad(CarLogic otherCar){
        if(otherCar == null) return false;
        int baseID = laneID & ~1;
        int otherBaseID = otherCar.laneID & ~1;
        
        return baseID == otherBaseID;
    }
    #endregion

    void ReturnToPool(){
        if(currentRageCoroutine != null){
            StopCoroutine(currentRageCoroutine);
            currentRageCoroutine = null;
        }
        
        if(currentDespawnCoroutine != null){
            StopCoroutine(currentDespawnCoroutine);
            currentDespawnCoroutine = null;
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