using UnityEngine;

public class CarPerceptionHandler
{
    private const float NO_OBSTACLE = 999f;
    public void UpdateCache(CarLogic car, bool hasRaged, int laneID, Vector3 moveDirection, bool isTurning, Vector3 turnEndPoint, float raycastDistance, LayerMask obstacleLayerMask, ref CarLogic cachedCarAhead, ref float cachedDistanceToCarAhead){
        if(hasRaged){
            cachedCarAhead = null;
            cachedDistanceToCarAhead = NO_OBSTACLE;
            return;
        }

        RaycastHit hit;
        Vector3 rayOrigin = car.transform.position + (moveDirection * car.carHalfLength) + Vector3.up * 0.5f;
        Vector3 checkDirection = isTurning ? (turnEndPoint - car.transform.position).normalized : moveDirection;

        if(Physics.Raycast(rayOrigin, checkDirection, out hit, raycastDistance, obstacleLayerMask)){
            CarLogic otherCar = hit.collider.GetComponent<CarLogic>();
            if(otherCar != null && otherCar.laneID == laneID && !otherCar.isCollisionDisabled){
                cachedCarAhead = otherCar;
                cachedDistanceToCarAhead = Mathf.Max(0.1f, hit.distance);
                return;
            }
        }

        cachedCarAhead = null;
        cachedDistanceToCarAhead = NO_OBSTACLE;
    }

    public bool CheckShouldStop(CarLogic car, bool hasRaged, bool isWaitingForGreen, float distanceToStopLine, TrafficLightManager trafficManager, int laneID, float cachedDistanceToCarAhead, float actualFollowDistance, float currentSpeed, float actualDeceleration){
        if(hasRaged) return false;
        if(isWaitingForGreen) return true;
        
        if(distanceToStopLine > 0f && distanceToStopLine < 1f){
            bool isGreen = trafficManager != null && trafficManager.IsLaneGreen(laneID);
            if(!isGreen) return true;
        }
        
        if(cachedDistanceToCarAhead < NO_OBSTACLE - 1f){
            float effectiveDistance = cachedDistanceToCarAhead - actualFollowDistance;

            if(effectiveDistance <= 0f) return true;
            float stoppingDistance = currentSpeed * currentSpeed / (2f * actualDeceleration) + 1f;
            if(effectiveDistance < stoppingDistance) return true;
        }
        
        return false;
    }

    public float GetZoneBasedTargetSpeed(CarLogic car, float cachedDistanceToCarAhead, CarLogic cachedCarAhead, float actualMaxSpeed, float farZoneDistance, float mediumZoneDistance, float closeZoneDistance){
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
}