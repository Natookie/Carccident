using UnityEngine;

public class CarMovementHandler
{
    private float rotationAlignSpeed;
    private float rotationAlignmentThreshold;

    public void Initialize(float alignSpeed, float alignThreshold){
        rotationAlignSpeed = alignSpeed;
        rotationAlignmentThreshold = alignThreshold;
    }

    public void AlignToLaneRotation(CarLogic car, Quaternion targetLaneRotation, bool isCollisionDisabled, ref bool isAligningAfterTurn, ref bool isAligningAfterRage){
        if(isCollisionDisabled) return;

        float angle = Quaternion.Angle(car.transform.rotation, targetLaneRotation);
        if(angle <= rotationAlignmentThreshold){
            car.transform.rotation = targetLaneRotation;
            isAligningAfterTurn = false;
            isAligningAfterRage = false;
            return;
        }
        
        car.transform.rotation = Quaternion.Slerp(
            car.transform.rotation,
            targetLaneRotation,
            rotationAlignSpeed * Time.deltaTime
        );
    }

    public void HandleReactionAndMovement(CarLogic car, bool shouldStop, bool hasRaged, float actualMaxSpeed, float actualAcceleration, float actualDeceleration, float cachedDistanceToCarAhead, float actualFollowDistance, bool isWaitingForGreen, float distanceToStopLine, ref float currentSpeed, ref bool isStopped, ref bool isHesitating, ref float brakingNoise, ref float brakingNoiseTimer, float brakingNoiseInterval, float noObstacle, float farZoneDistance, float mediumZoneDistance, float closeZoneDistance){
        if(hasRaged){
            currentSpeed = actualMaxSpeed;
            isStopped = false;
            return;
        }

        if(shouldStop){
            float targetDistance = GetTargetStopDistance(cachedDistanceToCarAhead, actualFollowDistance, 
                isWaitingForGreen, distanceToStopLine, noObstacle);
            
            float noisyDecel = GetNoisyDeceleration(actualDeceleration, ref brakingNoise, ref brakingNoiseTimer, brakingNoiseInterval);

            if(targetDistance > 0.2f && targetDistance < noObstacle - 1f){
                float requiredDecel = (currentSpeed * currentSpeed) / (2f * targetDistance);
                float decel = Mathf.Min(requiredDecel, noisyDecel);
                currentSpeed = Mathf.Max(0, currentSpeed - decel * Time.deltaTime);
            }
            else if(targetDistance >= noObstacle - 1f) currentSpeed = Mathf.Max(0, currentSpeed - noisyDecel * 0.3f * Time.deltaTime);
            else currentSpeed = 0;

            bool wasMoving = !isStopped;
            isStopped = currentSpeed < 0.1f;

            if(isStopped && wasMoving && !isHesitating) car.TriggerHesitation();

            if(cachedDistanceToCarAhead < actualFollowDistance){
                currentSpeed = 0;
                isStopped = true;
            }
        }
        else{
            float zoneTarget = GetZoneBasedTargetSpeed(car, cachedDistanceToCarAhead, actualMaxSpeed, 
                farZoneDistance, mediumZoneDistance, closeZoneDistance);

            if(currentSpeed < zoneTarget){
                isStopped = false;
                currentSpeed = Mathf.Min(zoneTarget, currentSpeed + actualAcceleration * Time.deltaTime);
            }
            else if(currentSpeed > zoneTarget + 0.5f){
                float noisyDecel = GetNoisyDeceleration(actualDeceleration, ref brakingNoise, ref brakingNoiseTimer, brakingNoiseInterval);
                currentSpeed = Mathf.Max(zoneTarget, currentSpeed - noisyDecel * 0.5f * Time.deltaTime);
            }

            isStopped = currentSpeed < 0.1f;
        }
    }

    float GetTargetStopDistance(float cachedDistanceToCarAhead, float actualFollowDistance, bool isWaitingForGreen, float distanceToStopLine, float noObstacle){float distanceToCar = cachedDistanceToCarAhead;
        if(distanceToCar < distanceToStopLine) return Mathf.Max(actualFollowDistance * 0.5f, actualFollowDistance);
        if(isWaitingForGreen && distanceToStopLine > 0f) return distanceToStopLine;
        
        return Mathf.Min(distanceToCar, distanceToStopLine);
    }

    float GetNoisyDeceleration(float actualDeceleration, ref float brakingNoise, ref float brakingNoiseTimer, float brakingNoiseInterval){
        brakingNoiseTimer += Time.deltaTime;
        if(brakingNoiseTimer >= brakingNoiseInterval){
            brakingNoiseTimer = 0f;
            brakingNoise = Random.Range(-0.15f, 0.15f) * actualDeceleration;
        }
        return Mathf.Max(0.5f, actualDeceleration + brakingNoise);
    }

    float GetZoneBasedTargetSpeed(CarLogic car, float cachedDistanceToCarAhead, float actualMaxSpeed, 
        float farZoneDistance, float mediumZoneDistance, float closeZoneDistance){
        const float NO_OBSTACLE = 999f;
        
        if(cachedDistanceToCarAhead >= NO_OBSTACLE - 1f) return actualMaxSpeed;

        float aheadSpeed = car.cachedCarAhead != null ? car.cachedCarAhead.currentSpeed : 0f;

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
}