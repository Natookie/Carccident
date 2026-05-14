using UnityEngine;
using System.Collections;

public class SimplePedestrianWalker : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 0.1f;

    [Header("Destination Check")]
    [SerializeField] private LayerMask obstructLayer;
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private int maxTryFindDestination = 50;

    [Header("Cast Height")]
    [SerializeField] private float castHeight = 0.5f;

    private Vector3 targetPosition;
    private bool hasTarget = false;
    private PedestrianFade pedestrianFade;

    private void Awake()
    {
        pedestrianFade = GetComponent<PedestrianFade>();
    }

    public void InitDestination(BoxCollider area)
    {
        StartCoroutine(InitRoutine(area));
    }

    private IEnumerator InitRoutine(BoxCollider area)
    {
        if (pedestrianFade != null)
        {
            yield return StartCoroutine(pedestrianFade.FadeIn());
        }

        bool foundTarget = TryGetClearDestination(area, out targetPosition);

        if (!foundTarget)
        {
            Debug.LogWarning("Pedestrian failed to find clear destination/path.");

            if (pedestrianFade != null)
            {
                yield return StartCoroutine(pedestrianFade.FadeOut());
            }

            Destroy(gameObject);
            yield break;
        }

        hasTarget = true;
    }

    private void Update()
    {
        if (!hasTarget) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) <= stopDistance)
        {
            hasTarget = false;
            StartCoroutine(ReachDestinationRoutine());
        }
    }

    private IEnumerator ReachDestinationRoutine()
    {
        if (pedestrianFade != null)
        {
            yield return StartCoroutine(pedestrianFade.FadeOut());
        }

        Destroy(gameObject);
    }

    private bool TryGetClearDestination(BoxCollider area, out Vector3 destination)
    {
        for (int i = 0; i < maxTryFindDestination; i++)
        {
            Vector3 randomPoint = GetRandomPointInArea(area);

            bool targetBlocked = IsDestinationObstructed(randomPoint);
            bool pathBlocked = IsPathObstructed(transform.position, randomPoint);

            if (!targetBlocked && !pathBlocked)
            {
                destination = randomPoint;
                return true;
            }
        }

        destination = Vector3.zero;
        return false;
    }

    private Vector3 GetRandomPointInArea(BoxCollider area)
    {
        Bounds bounds = area.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(randomX, transform.position.y, randomZ);
    }

    private bool IsDestinationObstructed(Vector3 destination)
    {
        Vector3 checkPosition = new Vector3(destination.x, castHeight, destination.z);

        Collider[] hits = Physics.OverlapSphere(
            checkPosition,
            checkRadius,
            obstructLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            Debug.Log("Destination blocked by: " + hit.name);
            return true;
        }

        return false;
    }

    private bool IsPathObstructed(Vector3 start, Vector3 end)
    {
        Vector3 castStart = new Vector3(start.x, castHeight, start.z);
        Vector3 castEnd = new Vector3(end.x, castHeight, end.z);

        Vector3 direction = castEnd - castStart;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
            return false;

        direction.Normalize();

        bool hit = Physics.SphereCast(
            castStart,
            checkRadius,
            direction,
            out RaycastHit hitInfo,
            distance,
            obstructLayer,
            QueryTriggerInteraction.Collide
        );

        if (hit)
        {
            Debug.Log("Path blocked by: " + hitInfo.collider.name);
            return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            new Vector3(transform.position.x, castHeight, transform.position.z),
            checkRadius
        );

        if (hasTarget)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                new Vector3(transform.position.x, castHeight, transform.position.z),
                new Vector3(targetPosition.x, castHeight, targetPosition.z)
            );

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(
                new Vector3(targetPosition.x, castHeight, targetPosition.z),
                checkRadius
            );
        }
    }
}