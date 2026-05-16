using UnityEngine;

[CreateAssetMenu(fileName = "CarConfig", menuName = "Car/Car Configuration", order = 1)]
public class CarConfiguration : ScriptableObject
{
    [Header("CAR SETTINGS")]
    public float speed = 10f;
    public float speedVariation = 2f;
    [Space(5)]
    public float reactionTime = 0.5f;
    public float reactionTimeVariation = 0.3f;
    [Space(5)]
    public float accelerationRate = 5f;
    public float accelerationVariation = 1.5f;
    [Space(5)]
    public float decelerationRate = 8f;
    public float decelerationVariation = 2f;

    [Header("TURNING SETTINGS")]
    public float turnSpeed = 5f;
    public float turnSpeedVariation = 1.5f;
    [Space(5)]
    public float exitOffset = 5f;
    public float bezierOffset = 2f;

    [Header("RAYCAST SETTINGS")]
    public float raycastDistance = 30f;
    public LayerMask obstacleLayerMask;
    public float minFollowDistance = 4f;
    public float followDistanceVariation = 1.5f;

    [Header("PERCEPTION ZONES")]
    public float farZoneDistance = 20f;
    public float mediumZoneDistance = 12f;
    public float closeZoneDistance = 6f;

    [Header("COLLISION")]
    public float knockUpForce = 12f;
    public float knockBackForce = 15f;

    [Header("PATIENT SETTINGS")]
    public float patienceThreshold = 8f;
}