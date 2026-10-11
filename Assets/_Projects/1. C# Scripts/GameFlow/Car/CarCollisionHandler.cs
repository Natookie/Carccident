using UnityEngine;
using System.Collections;

public class CarCollisionHandler
{
    private float despawnDelay;
    public void Initialize(float delay) => despawnDelay = delay;

    public bool IsHitFromSide(CarLogic victim, CarLogic hitter){
        if(victim == null || hitter == null) return false;
        
        Vector3 victimForward = victim.transform.forward;
        Vector3 hitDirection = (victim.transform.position - hitter.transform.position).normalized;
        
        float forwardDot = Vector3.Dot(victimForward, hitDirection);
        float rightDot = Vector3.Dot(victim.transform.right, hitDirection);
        
        bool isSideHit = Mathf.Abs(rightDot) > Mathf.Abs(forwardDot);
        
        return isSideHit;
    }

    public void OnTriggerEnter(CarLogic car, Collider other, bool isCollisionDisabled, bool hasRaged, int laneID){
        CarLogic otherCar = other.GetComponent<CarLogic>();
        if(otherCar == null) return;
        if(isCollisionDisabled || otherCar.isCollisionDisabled) return;
        
        bool sameRoad = IsSameRoad(car, otherCar);
        if(!hasRaged && !otherCar.hasRaged){
            if(sameRoad) return;
        }

        if(car.GetInstanceID() > otherCar.GetInstanceID()) return;
        ProcessCollision(car, otherCar);
    }

    public void ProcessCollision(CarLogic carA, CarLogic carB){
        bool isAVictim = IsHitFromSide(carA, carB);
        bool isBVictim = IsHitFromSide(carB, carA);
        
        #if UNITY_EDITOR
            Debug.Log($"A victim{isAVictim}:{carA.gameObject.name} || B victim{isBVictim}:{carB.gameObject.name}");
        #endif

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

    public void ApplyKnockback(CarLogic victim, CarLogic hitter){
        if(victim == null || hitter == null) return;
        
        Rigidbody victimRB = victim.GetRigidbody();
        Rigidbody hitterRB = hitter.GetRigidbody();
        
        if(victimRB == null || hitterRB == null) return;

        victim.SetCurrentSpeed(0f);
        victim.SetIsStopped(true);
        victim.SetIsTurning(false);
        victim.SetIsWaitingForGreen(false);

        victimRB.isKinematic = false;
        victimRB.useGravity = true;

        Collider victimCollider = victim.GetCarCollider();
        if(victimCollider != null){
            victimCollider.enabled = false;
            victim.SetIsCollisionDisabled(true);
        }
        
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

        float spinDirection = Mathf.Sign(sideDot);
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
        
        victim.StartCoroutine(DelayedDespawn(victim));
    }

    public IEnumerator DelayedDespawn(CarLogic car){
        yield return new WaitForSeconds(despawnDelay);
        if(car != null && car.gameObject != null) car.ReturnToPool();
    }

    public bool IsSameRoad(CarLogic car, CarLogic otherCar){
        if(otherCar == null) return false;
        int baseID = car.laneID & ~1;
        int otherBaseID = otherCar.laneID & ~1;
        
        return baseID == otherBaseID;
    }
}