using UnityEngine;

[CreateAssetMenu(fileName = "PedestrianSettings", menuName = "Pedes/Pedestrian Settings")]
public class PedestrianSettings : ScriptableObject
{
    [Header("MOVEMENT")]
    public float moveSpeed = 2f;
    public float stopDistance = 0.1f;
    public float rotationSpeed = 10f;

    [Header("WALK ANIMATION")]
    public float bobSpeed = 8f;
    public float bobAmount = 0.05f;
    public float wobbleAmount = 5f;
    public float wobbleSpeed = 10f;
    public float tiltAmount = 8f;
    public float tiltSpeed = 12f;

    [Header("DEST CHECK")]
    public float checkRadius = 0.5f;
    public int maxTryFindDestination = 50;

    [Header("CAST HEIGHT")]
    public float castHeight = 0.5f;
}