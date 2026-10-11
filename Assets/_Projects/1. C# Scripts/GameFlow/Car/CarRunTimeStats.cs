using UnityEngine;

[System.Serializable]
public class CarRuntimeStats
{
    public float actualMaxSpeed;
    public float actualAcceleration;
    public float actualDeceleration;
    public float actualReactionTime;
    public float actualTurnSpeed;
    public float actualFollowDistance;

    public void RandomizeStats(CarConfiguration config)
    {
        actualMaxSpeed = config.speed + Random.Range(-config.speedVariation, config.speedVariation);
        actualAcceleration = config.accelerationRate + Random.Range(-config.accelerationVariation, config.accelerationVariation);
        actualDeceleration = config.decelerationRate + Random.Range(-config.decelerationVariation, config.decelerationVariation);
        actualReactionTime = config.reactionTime + Random.Range(-config.reactionTimeVariation, config.reactionTimeVariation);
        actualTurnSpeed = config.turnSpeed + Random.Range(-config.turnSpeedVariation, config.turnSpeedVariation);
        actualFollowDistance = config.minFollowDistance + Random.Range(-config.followDistanceVariation, config.followDistanceVariation);
        
        actualMaxSpeed = Mathf.Max(15f, actualMaxSpeed);
        actualAcceleration = Mathf.Max(3f, actualAcceleration);
        actualDeceleration = Mathf.Max(4f, actualDeceleration);
        actualReactionTime = Mathf.Max(0.3f, actualReactionTime);
        actualTurnSpeed = Mathf.Max(3f, actualTurnSpeed);
        actualFollowDistance = Mathf.Max(2f, actualFollowDistance);
    }
}