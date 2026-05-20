using UnityEngine;
using System.Collections;

public class CarRageHandler
{
    private float xRotDefault;
    public void Initialize(float xRot) => xRotDefault = xRot;

    public void HandlePatient(CarLogic car, float patienceThreshold, Transform visualTransform, ref float patientTimer, ref bool isRoadRage, ref bool hasRaged){
        if(!car.isStopped) ResetPatient(car, visualTransform);

        patientTimer += Time.deltaTime;
        float fill = Mathf.Clamp01(patientTimer / patienceThreshold);
        
        if(fill >= 1f && !isRoadRage && !hasRaged){
            car.TriggerRoadRage();
            return;
        }
        
        if(visualTransform != null){
            float shakeIntensity = Mathf.Sin(Time.time * 20f) * (fill * 0.05f);
            float shakeX = Random.Range(-shakeIntensity, shakeIntensity) * 0.01f;
            float shakeZ = Random.Range(-shakeIntensity, shakeIntensity) * 0.01f;
            visualTransform.localPosition = new Vector3(shakeX, 0, shakeZ);
            
            float tiltAngle = Mathf.Sin(Time.time * 15f) * (fill * 3f);
            visualTransform.localRotation = Quaternion.Euler(xRotDefault + tiltAngle, 0f, 0f);
        }
    }

    public void ResetPatient(CarLogic car, Transform visualTransform){
        car.SetPatientTimer(0f);
        if(visualTransform != null){
            visualTransform.localPosition = Vector3.zero;
            visualTransform.localRotation = Quaternion.Euler(xRotDefault, 0f, 0f);
        }
    }

    public void TriggerRoadRage(CarLogic car, CarLogic.TurnIntent turnIntent, Transform visualTransform, Renderer carRenderer, Color originalColor, ref Coroutine currentRageCoroutine, ref bool isRoadRage, ref bool isStopped, ref bool isWaitingForGreen, Rigidbody rb,ref bool isTurning, ref bool hasReachedTurnPoint, ref float turnProgress, MonoBehaviour monoBehaviour){
        if(currentRageCoroutine != null){
            monoBehaviour.StopCoroutine(currentRageCoroutine);
            currentRageCoroutine = null;
        }

        ResetPatient(car, visualTransform);
        isRoadRage = true;
        isStopped = false;
        isWaitingForGreen = false;
        rb.linearVelocity = Vector3.zero;

        if(turnIntent == CarLogic.TurnIntent.Right){
            isTurning = false;
            hasReachedTurnPoint = false;
            turnProgress = 0f;
        }

        currentRageCoroutine = monoBehaviour.StartCoroutine(RoadRageCoroutine(car, visualTransform, carRenderer, 
            originalColor, xRotDefault, turnIntent, car.laneID));
    }

    public IEnumerator RoadRageCoroutine(CarLogic car, Transform visualTransform, Renderer carRenderer, Color originalColor, float xRotDefault, CarLogic.TurnIntent turnIntent, int laneID){
        if(visualTransform == null) yield break;
        float rageDuration = 2f;
        float elapsed = 0f;
        
        while(elapsed < rageDuration){
            if(car == null || car.gameObject == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / rageDuration;
            
            if(carRenderer != null) carRenderer.material.color = Color.Lerp(originalColor, Color.red, t);
            
            float rotX = xRotDefault + (Mathf.Sin(Time.time * 30f) * (15f * (1f - t)));
            float rotZ = Mathf.Cos(Time.time * 28f) * (10f * (1f - t));
            float rotY = Mathf.Sin(Time.time * 8f) * (3f * (1f - t));
            
            visualTransform.localRotation = Quaternion.Euler(rotX, rotY, rotZ);
            yield return null;
        }
        
        if(carRenderer != null) carRenderer.material.color = originalColor;
        car.isRoadRage = false;
        car.hasRaged = true;
        
        if(visualTransform != null) visualTransform.localRotation = Quaternion.Euler(xRotDefault, 0f, 0f);
        
        if(turnIntent == CarLogic.TurnIntent.Straight){
            car.SetTargetLaneRotation(car.GetRotationForLane(laneID));
            car.isAligningAfterRage = true;
        }
    }
}