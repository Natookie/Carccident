using UnityEngine;
using System.Collections;

public class SimplePedestrianWalker : MonoBehaviour
{
    [SerializeField] private PedestrianSettings settings;
    [SerializeField] private LayerMask obstructLayer;

    private Vector3 targetPosition;
    private bool hasTarget = false;
    private PedestrianFade pedestrianFade;
    private PedestrianSpawner spawner;
    private float defaultYOffset = 0f;
    private float walkTimer = 0f;
    private bool isWalking = false;
    private bool isReturning = false;

    void Awake(){
        pedestrianFade = GetComponent<PedestrianFade>();
        defaultYOffset = transform.position.y;
        spawner = FindFirstObjectByType<PedestrianSpawner>();
    }

    public void InitDestination(BoxCollider area, LayerMask obstacleMask){
        obstructLayer = obstacleMask;
        StartCoroutine(InitRoutine(area));
    }

    private IEnumerator InitRoutine(BoxCollider area){
        if(pedestrianFade != null) yield return StartCoroutine(pedestrianFade.FadeIn());

        bool foundTarget = TryGetClearDestination(area, out targetPosition);
        if(!foundTarget){
            Debug.LogWarning("Pedestrian failed to find clear destination/path.");
            ReturnToPool();
            yield break;
        }

        hasTarget = true;
        isWalking = true;
    }

    void Update(){
        if(!hasTarget || settings == null || isReturning) return;

        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        Vector3 newPosition = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            settings.moveSpeed * Time.deltaTime
        );
        newPosition.y = defaultYOffset;
        transform.position = newPosition;

        if(moveDirection != Vector3.zero && distance > 0.01f){
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            
            walkTimer += Time.deltaTime * settings.wobbleSpeed;
            float wobble = Mathf.Sin(walkTimer) * settings.wobbleAmount;
            float tilt = Mathf.Sin(Time.time * settings.tiltSpeed) * settings.tiltAmount;
            Quaternion wobbleRotation = Quaternion.Euler(0, wobble, tilt);
            
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation * wobbleRotation,
                settings.rotationSpeed * Time.deltaTime
            );
        }

        AnimateWalk(isWalking && distance > settings.stopDistance);
        
        if(distance <= settings.stopDistance){
            hasTarget = false;
            isWalking = false;
            StartCoroutine(ReachDestinationRoutine());
        }
    }

    void AnimateWalk(bool walking){
        Vector3 pos = transform.position;
        if(!walking){
            pos.y = Mathf.Lerp(pos.y, defaultYOffset, Time.deltaTime * 10f);
            transform.position = pos;
            return;
        }

        float bobY = Mathf.Sin(Time.time * settings.bobSpeed) * settings.bobAmount;
        pos.y = defaultYOffset + bobY;
        transform.position = pos;
    }

    IEnumerator ReachDestinationRoutine(){
        if(pedestrianFade != null){
            yield return StartCoroutine(pedestrianFade.FadeOut());
        }

        ReturnToPool();
    }

    void ReturnToPool(){
        if(isReturning) return;
        isReturning = true;

        hasTarget = false;
        isWalking = false;
        
        if(pedestrianFade != null) pedestrianFade.ResetFade();

        if(spawner != null) spawner.ReturnToPool(gameObject);
        else Destroy(gameObject);
    }

    void OnDisable(){
        ResetState();
    }

    void ResetState(){
        hasTarget = false;
        isWalking = false;
        isReturning = false;
        walkTimer = 0f;
        targetPosition = Vector3.zero;
        
        if(transform != null){
            Vector3 pos = transform.position;
            pos.y = defaultYOffset;
            transform.position = pos;
            transform.rotation = Quaternion.identity;
        }
    }

    private bool TryGetClearDestination(BoxCollider area, out Vector3 destination){
        for(int i = 0; i < settings.maxTryFindDestination; i++){
            Vector3 randomPoint = GetRandomPointInArea(area);

            bool targetBlocked = IsDestinationObstructed(randomPoint);
            bool pathBlocked = IsPathObstructed(transform.position, randomPoint);

            if(!targetBlocked && !pathBlocked){
                destination = randomPoint;
                return true;
            }
        }

        destination = Vector3.zero;
        return false;
    }

    private Vector3 GetRandomPointInArea(BoxCollider area){
        Bounds bounds = area.bounds;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(randomX, defaultYOffset, randomZ);
    }

    private bool IsDestinationObstructed(Vector3 destination){
        Vector3 checkPosition = new Vector3(destination.x, settings.castHeight, destination.z);

        Collider[] hits = Physics.OverlapSphere(
            checkPosition,
            settings.checkRadius,
            obstructLayer,
            QueryTriggerInteraction.Collide
        );

        foreach(Collider hit in hits){
            if(hit == null) continue;
            if(hit.transform == transform) continue;
            return true;
        }

        return false;
    }

    bool IsPathObstructed(Vector3 start, Vector3 end){
        Vector3 castStart = new Vector3(start.x, settings.castHeight, start.z);
        Vector3 castEnd = new Vector3(end.x, settings.castHeight, end.z);

        Vector3 direction = castEnd - castStart;
        float distance = direction.magnitude;

        if(distance <= 0.01f) return false;

        direction.Normalize();

        RaycastHit[] hits = Physics.SphereCastAll(
            castStart,
            settings.checkRadius,
            direction,
            distance,
            obstructLayer,
            QueryTriggerInteraction.Collide
        );

        foreach(RaycastHit hit in hits){
            if(hit.collider == null) continue;
            if(hit.collider.transform == transform) continue;
            return true;
        }

        return false;
    }

    void OnDrawGizmosSelected(){
        if(settings == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            new Vector3(transform.position.x, settings.castHeight, transform.position.z),
            settings.checkRadius
        );

        if(hasTarget){
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                new Vector3(transform.position.x, settings.castHeight, transform.position.z),
                new Vector3(targetPosition.x, settings.castHeight, targetPosition.z)
            );

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(
                new Vector3(targetPosition.x, settings.castHeight, targetPosition.z),
                settings.checkRadius
            );
        }
    }
}