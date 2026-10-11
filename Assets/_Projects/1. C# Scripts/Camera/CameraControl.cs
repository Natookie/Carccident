using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraControl : MonoBehaviour
{
    [Header("ROTATION SETTINGS")]
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private Vector2 horizontalClamp = new Vector2(-30f, 30f);
    [SerializeField] private Vector2 verticalClamp = new Vector2(-20f, 20f);
    [SerializeField] private float edgeThreshold = 30f;
    
    [Header("KEYBOARD MOVEMENT")]
    [SerializeField] private float keyboardRotationSpeed = 25f;
    [SerializeField] private KeyCode moveLeft = KeyCode.A;
    [SerializeField] private KeyCode moveRight = KeyCode.D;
    [SerializeField] private KeyCode moveUp = KeyCode.W;
    [SerializeField] private KeyCode moveDown = KeyCode.S;
    
    [Header("ZOOM SETTINGS")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private Vector2 zoomClamp = new Vector2(-15f, 15f);
    
    [Header("DEPTH OF FIELD")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private float blurAmount = 15f;
    [SerializeField] private float blurDuration = 0.15f;
    [SerializeField] private float refocusDuration = 0.3f;
    
    [Header("INPUT")]
    [SerializeField] private string zoomAxis = "Mouse ScrollWheel";

    [Header("CAMERA SHAKE")]
    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeDecay = 0.95f;

    private float currentShakeIntensity = 0f;
    private float currentShakeDuration = 0f;
    private Quaternion originalLocalRotation;

    [Header("REFERENCES")]
    [SerializeField] private Camera thisCam;
    
    private Vector3 originalRotation;
    private float originalFOV;
    private float originalFocusDistance;
    
    private float targetX = 0f;
    private float targetY = 0f;
    private float accumulatedZoom = 0f;
    private float currentX = 0f;
    private float currentY = 0f;
    
    private DepthOfField dof;
    private bool hasDOF;
    private Coroutine activeZoomCoroutine;
    private bool isZooming = false;
    
    void Start(){
        originalRotation = transform.eulerAngles;
        originalFOV = thisCam.fieldOfView;
        
        thisCam.fieldOfView = originalFOV;
        transform.eulerAngles = originalRotation;
        
        targetX = 0f;
        targetY = 0f;
        currentX = 0f;
        currentY = 0f;
        accumulatedZoom = 0f;
        
        if(postProcessVolume != null && postProcessVolume.profile.TryGet<DepthOfField>(out dof)){
            hasDOF = true;
            originalFocusDistance = dof.focusDistance.value;
            dof.focusDistance.value = originalFocusDistance;
        }

        originalLocalRotation = transform.localRotation;
    }
    
    void Update(){
        if(GameManager.Instance.isGameInitialized){
            HandleInput();
            HandleZoom();
            ApplyRotation();
        }
    }

    void LateUpdate(){
        if(!GameManager.Instance.isGameInitialized) return;
        
        if(currentShakeDuration > 0f){
            currentShakeDuration -= Time.deltaTime;
            
            Vector3 shakeRotation = new Vector3(
                Random.Range(-1f, 1f) * currentShakeIntensity,
                Random.Range(-1f, 1f) * currentShakeIntensity,
                Random.Range(-1f, 1f) * currentShakeIntensity * 0.5f
            );
            
            transform.localRotation = originalLocalRotation * Quaternion.Euler(shakeRotation);
            currentShakeIntensity *= shakeDecay;
        }
    }

    void HandleInput(){
        float targetSpeedX = 0f;
        float targetSpeedY = 0f;
        
        bool hasKeyboardInput = false;
        
        if(Input.GetKey(moveLeft)){
            targetSpeedX = -1f;
            hasKeyboardInput = true;
        }
        else if(Input.GetKey(moveRight)){
            targetSpeedX = 1f;
            hasKeyboardInput = true;
        }
        
        if(Input.GetKey(moveDown)){
            targetSpeedY = -1f;
            hasKeyboardInput = true;
        }
        else if(Input.GetKey(moveUp)){
            targetSpeedY = 1f;
            hasKeyboardInput = true;
        }
        
        if(!hasKeyboardInput){
            Vector2 mousePos = Input.mousePosition;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            if(mousePos.x < edgeThreshold) targetSpeedX = -1f;
            else if(mousePos.x > screenWidth - edgeThreshold) targetSpeedX = 1f;
            
            if(mousePos.y < edgeThreshold) targetSpeedY = -1f;
            else if(mousePos.y > screenHeight - edgeThreshold) targetSpeedY = 1f;
        }
        
        float currentRotSpeed = hasKeyboardInput ? keyboardRotationSpeed : rotationSpeed;
        
        targetX -= targetSpeedY * currentRotSpeed * Time.deltaTime;
        targetY += targetSpeedX * currentRotSpeed * Time.deltaTime;
        
        targetX = Mathf.Clamp(targetX, verticalClamp.x, verticalClamp.y);
        targetY = Mathf.Clamp(targetY, horizontalClamp.x, horizontalClamp.y);
        
        currentX = Mathf.Lerp(currentX, targetX, Time.deltaTime * 5f);
        currentY = Mathf.Lerp(currentY, targetY, Time.deltaTime * 5f);
    }
    
    void ApplyRotation(){
        transform.eulerAngles = new Vector3(
            originalRotation.x + currentX,
            originalRotation.y + currentY,
            originalRotation.z
        );
        
        originalLocalRotation = transform.localRotation;
    }
    
    void HandleZoom(){
        float scroll = Input.GetAxis(zoomAxis);
        
        if(Mathf.Abs(scroll) > 0.01f){
            float newZoom = accumulatedZoom - scroll * zoomSpeed;
            newZoom = Mathf.Clamp(newZoom, zoomClamp.x, zoomClamp.y);
            
            bool isAtMinZoom = (scroll > 0 && newZoom >= zoomClamp.x && accumulatedZoom == zoomClamp.x);
            bool isAtMaxZoom = (scroll < 0 && newZoom <= zoomClamp.y && accumulatedZoom == zoomClamp.y);
            
            if(isAtMinZoom || isAtMaxZoom) return;
            accumulatedZoom = newZoom;
            thisCam.fieldOfView = originalFOV + accumulatedZoom;
            
            if(!isZooming){
                if(activeZoomCoroutine != null) StopCoroutine(activeZoomCoroutine);
                activeZoomCoroutine = StartCoroutine(BlurThenRefocus());
            }
        }
    }
    
    IEnumerator BlurThenRefocus(){
        if(!hasDOF) yield break;
        
        isZooming = true;
        
        float targetFocus = originalFocusDistance;
        
        dof.focusDistance.value = originalFocusDistance - blurAmount;
        yield return new WaitForSeconds(blurDuration);
        
        float elapsed = 0f;
        float startFocus = dof.focusDistance.value;
        
        while(elapsed < refocusDuration){
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / refocusDuration);
            t = Mathf.SmoothStep(0f, 1f, t);
            dof.focusDistance.value = Mathf.Lerp(startFocus, targetFocus, t);
            yield return null;
        }
        
        dof.focusDistance.value = targetFocus;
        activeZoomCoroutine = null;
        isZooming = false;
    }
    
    public void ResetCamera(){
        if(activeZoomCoroutine != null){
            StopCoroutine(activeZoomCoroutine);
            activeZoomCoroutine = null;
        }
        
        isZooming = false;
        
        targetX = 0f;
        targetY = 0f;
        currentX = 0f;
        currentY = 0f;
        accumulatedZoom = 0f;
        
        thisCam.fieldOfView = originalFOV;
        transform.eulerAngles = originalRotation;
        
        if(hasDOF) dof.focusDistance.value = originalFocusDistance;
    }

    public void ShakeCamera(){
        ShakeCamera(shakeIntensity, shakeDuration);
    }

    public void ShakeCamera(float intensity, float duration){
        currentShakeIntensity = intensity;
        currentShakeDuration = duration;
    }

    public void StopShake(){
        currentShakeDuration = 0f;
        currentShakeIntensity = 0f;
        transform.localRotation = originalLocalRotation;
    }
}