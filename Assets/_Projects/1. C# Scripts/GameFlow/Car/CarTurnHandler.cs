using Unity.VisualScripting;
using UnityEngine;

public class CarTurnHandler
{
    public void HandleTurning(CarLogic car, Transform turnTarget, TrafficLightManager trafficManager, int laneID, bool hasRaged, Vector3 originalMoveDirection, float actualDeceleration, ref bool isWaitingForGreen, ref float currentSpeed, ref bool isStopped, ref bool hasReachedTurnPoint){
        bool isRightAllowed = trafficManager != null && trafficManager.IsLaneGreen(laneID);
        float distanceToTurnPoint = GetDistanceAlongMoveDirection(car, turnTarget.position, originalMoveDirection);
        if(hasRaged) isRightAllowed = true;
        
        if(distanceToTurnPoint < 1f){
            if(!isRightAllowed){
                isWaitingForGreen = true;
                currentSpeed = Mathf.Max(0, currentSpeed - actualDeceleration * Time.deltaTime);
                
                if(currentSpeed < 0.1f){
                    currentSpeed = 0f;
                    isStopped = true;
                }
                return; 
            }
            else if(!hasReachedTurnPoint){
                currentSpeed = Mathf.Min(currentSpeed, 5f);
                car.StartTurn();
                isWaitingForGreen = false;
            }
        }
    }

    float GetDistanceAlongMoveDirection(CarLogic car, Vector3 target, Vector3 originalMoveDirection){
        if(originalMoveDirection == Vector3.back) return Mathf.Abs(target.z - car.transform.position.z);
        if(originalMoveDirection == Vector3.forward) return Mathf.Abs(car.transform.position.z - target.z);
        if(originalMoveDirection == Vector3.left) return Mathf.Abs(target.x - car.transform.position.x);
        if(originalMoveDirection == Vector3.right) return Mathf.Abs(car.transform.position.x - target.x);
        return Vector3.Distance(car.transform.position, target);
    }

    public void StartTurn(CarLogic car, Transform turnTarget, Vector3 originalMoveDirection, float exitOffset, float bezierOffset, float roadLength, CarManager carManager, RoadSpawnerManager roadSpawnerManager, int laneID, ref bool isTurning, ref bool hasReachedTurnPoint, Rigidbody rb, ref float currentSpeed, ref Vector3 turnStartPoint, ref Vector3 turnControlPoint, ref Vector3 turnEndPoint, ref Vector3 turnFinalDestination, ref float turnProgress, ref float actualTurnSpeed){
        isTurning = true;
        hasReachedTurnPoint = true;
        rb.isKinematic = true;

        currentSpeed = Mathf.Min(currentSpeed, 5f);

        Vector3 turnCenter = turnTarget.position;
        Vector3 turnDirection = GetTurnDirection(originalMoveDirection);
        
        Vector3 finalDest = GetFinalDestination(car, roadLength, carManager, roadSpawnerManager, laneID);
        
        Vector3 turnExit = GetTurnExitPoint(car, turnCenter, turnDirection, exitOffset, finalDest);
        Vector3 apex = GetTurnApexPoint(car, turnCenter, turnDirection, originalMoveDirection, bezierOffset);

        turnStartPoint = car.transform.position;
        turnControlPoint = apex;
        turnEndPoint = turnExit;
        turnFinalDestination = finalDest;
        turnProgress = 0f;
        
        float turnDistance = Vector3.Distance(turnStartPoint, turnExit);
        float turnDuration = Mathf.Max(0.6f, turnDistance / 6f);
        actualTurnSpeed = 1f / turnDuration;
    }

    Vector3 GetTurnDirection(Vector3 originalMoveDirection){
        if(originalMoveDirection == Vector3.forward) return Vector3.right;
        if(originalMoveDirection == Vector3.back) return Vector3.left;
        if(originalMoveDirection == Vector3.right) return Vector3.back;
        if(originalMoveDirection == Vector3.left) return Vector3.forward;
        return originalMoveDirection;
    }

    Vector3 GetTurnExitPoint(CarLogic car, Vector3 center, Vector3 exitDirection, float exitOffset, Vector3 turnFinalDestination){
        Vector3 exitPoint = center + exitDirection * exitOffset;
        
        if(exitDirection == Vector3.right) exitPoint.z = turnFinalDestination.z;
        if(exitDirection == Vector3.left) exitPoint.z = turnFinalDestination.z;
        if(exitDirection == Vector3.forward) exitPoint.x = turnFinalDestination.x;
        if(exitDirection == Vector3.back) exitPoint.x = turnFinalDestination.x;
        
        exitPoint.y = car.transform.position.y;
        return exitPoint;
    }

    Vector3 GetTurnApexPoint(CarLogic car, Vector3 center, Vector3 exitDirection, Vector3 originalMoveDirection, float bezierOffset){
        Vector3 startDirection = originalMoveDirection;
        Vector3 cornerPoint = center + startDirection * bezierOffset + exitDirection * bezierOffset;
        cornerPoint.y = car.transform.position.y;
        return cornerPoint;
    }

    Vector3 GetFinalDestination(CarLogic car, float roadLength, CarManager carManager, RoadSpawnerManager roadSpawnerManager, int laneID){
        Transform spawnPoint = car.GetSpawnPointFromLaneID();
        
        if(spawnPoint == null) return car.transform.position + car.moveDirection * 20f;
        
        float absX = Mathf.Abs(spawnPoint.position.x);
        float absZ = Mathf.Abs(spawnPoint.position.z);
        
        int baseID = laneID & ~1;
        switch(baseID){
            case 0: return new Vector3(-roadLength, car.transform.position.y, absX);
            case 2: return new Vector3(roadLength, car.transform.position.y, -absX);
            case 4: return new Vector3(absZ, car.transform.position.y, -roadLength);
            case 6: return new Vector3(-absZ, car.transform.position.y, roadLength);
            default: return car.transform.position + car.moveDirection * 10f;
        }
    }

    public void UpdateTurn(CarLogic car, float actualTurnSpeed, ref float turnProgress, ref Vector3 turnStartPoint, ref Vector3 turnControlPoint, ref Vector3 turnEndPoint, ref bool isTurning, ref Rigidbody rb, ref float currentSpeed, ref bool isStopped, ref Vector3 moveDirection){
        turnProgress += Time.deltaTime * actualTurnSpeed;
        if(turnProgress >= 1f){
            car.transform.position = turnEndPoint;
            
            Vector3 finalTangent = 2f * (turnEndPoint - turnControlPoint);
            finalTangent.y = 0f;
            
            if(finalTangent != Vector3.zero){
                moveDirection = finalTangent.normalized;
                car.transform.rotation = Quaternion.LookRotation(moveDirection);
            }
            
            isTurning = false;
            turnProgress = 0f;
            rb.isKinematic = false;
            currentSpeed = 5f;
            isStopped = false;
            
            car.UpdateAfterTurn();
            return;
        }
        
        float t = turnProgress;
        
        Vector3 p0 = turnStartPoint;
        Vector3 p1 = turnControlPoint;
        Vector3 p2 = turnEndPoint;
        
        Vector3 newPos = (1f - t) * (1f - t) * p0 + 2f * (1f - t) * t * p1 + t * t * p2;
        newPos.y = turnStartPoint.y;
        car.transform.position = newPos;
        
        Vector3 tangent = 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
        tangent.y = 0f;
        
        if(tangent != Vector3.zero){
            Quaternion targetRot = Quaternion.LookRotation(tangent.normalized);
            car.transform.rotation = Quaternion.Slerp(car.transform.rotation, targetRot, 12f * Time.deltaTime);
        }
    }

    public void UpdateAfterTurn(CarLogic car, ref int laneID, ref CarLogic.TurnIntent turnIntent, ref Transform turnTarget, ref Vector3 moveDirection, ref Quaternion targetLaneRotation, ref bool isAligningAfterTurn){
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
            turnIntent = CarLogic.TurnIntent.Straight;
            turnTarget = null;
            
            car.SetMovementDirection();
            
            targetLaneRotation = car.GetRotationForLane(laneID);
            isAligningAfterTurn = true;
        }
    }
}