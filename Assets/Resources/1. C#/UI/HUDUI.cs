using UnityEngine;
using Nova;
using System.Collections;
using NaughtyAttributes;

public class HUDUI : MonoBehaviour
{
    public static HUDUI Instance {get; private set;}

    [Header("BORDER REFERENCES")]
    [SerializeField] private TextBlock cctvText;
    [SerializeField] private UIBlock2D redCircle;
    [SerializeField] private TextBlock bestScoreText;
    
    [Header("FPS SETTINGS")]
    [SerializeField] private float fpsUpdateInterval = 0.2f;
    
    [Header("PULSE SETTINGS")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minBlur = 0f;
    [SerializeField] private float maxBlur = 80f;

    [Header("MAIN MENU REFERENCES")]
    [SerializeField] private MenuComponent leftComponent;
    [SerializeField] private MenuComponent mainButton;
    [SerializeField] private MenuComponent prologue;

    [Header("ANIMATION SETTINGS")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float animationSpeed = 8f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private float fpsAccumulator = 0f;
    private int fpsFrames = 0;
    private float fpsTimer = 0f;
    private float currentFPS = 0f;
    
    private Coroutine currentAnimation;
    private bool isMenuVisible = true;
    
    void Awake(){
        if(Instance != null){
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CacheOriginalPositions();
    }

    void Start(){
        fpsTimer = fpsUpdateInterval;
        if(redCircle != null && redCircle.Shadow == null) Debug.LogWarning("Red Circle doesn't have a Shadow component!");
        bestScoreText.Text = $"HIGH SCORE: {SaveManager.GetBestScore().ToString()}";
    }
    
    void Update(){
        CalculateFPS();
        UpdatePulseEffect();
    }
    
    void CalculateFPS(){
        fpsAccumulator += Time.unscaledDeltaTime;
        fpsFrames++;
        
        fpsTimer -= Time.unscaledDeltaTime;
        if(fpsTimer <= 0f){
            currentFPS = fpsFrames / fpsAccumulator;
            if(cctvText != null){
                int displayFPS = Mathf.RoundToInt(currentFPS);
                
                displayFPS = Mathf.Min(displayFPS, 99);
                cctvText.Text = $"{Mathf.RoundToInt(displayFPS)} FPS";
            }
            
            fpsAccumulator = 0f;
            fpsFrames = 0;
            fpsTimer = fpsUpdateInterval;
        }
    }
    
    void UpdatePulseEffect(){
        if(redCircle == null || redCircle.Shadow == null) return;
        
        float t = Mathf.PingPong(Time.unscaledTime * pulseSpeed, 1f);
        float blurValue = Mathf.Lerp(minBlur, maxBlur, t);
        
        redCircle.Shadow.Blur = blurValue;
    }

    void CacheOriginalPositions(){
        if(leftComponent != null && leftComponent.block != null) 
            leftComponent.originalPosition = leftComponent.block.Position.Value;
        if(mainButton != null && mainButton.block != null) 
            mainButton.originalPosition = mainButton.block.Position.Value;
        if(prologue != null && prologue.block != null) 
            prologue.originalPosition = prologue.block.Position.Value;
    }

    public void ShowAllMenuUI(){
        if(currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(AnimateShowMenu());
        isMenuVisible = true;
    }

    public void HideAllMenuUI(){
        if(currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(AnimateHideMenu());
        isMenuVisible = false;
    }

    IEnumerator AnimateShowMenu(){
        float elapsed = 0f;
        
        Vector3 startLeftPos = leftComponent != null && leftComponent.block != null ? leftComponent.block.Position.Value : Vector3.zero;
        Vector3 startMainPos = mainButton != null && mainButton.block != null ? mainButton.block.Position.Value : Vector3.zero;
        Vector3 startProloguePos = prologue != null && prologue.block != null ? prologue.block.Position.Value : Vector3.zero;
        
        Vector3 targetLeftPos = leftComponent != null ? leftComponent.originalPosition : Vector3.zero;
        Vector3 targetMainPos = mainButton != null ? mainButton.originalPosition : Vector3.zero;
        Vector3 targetProloguePos = prologue != null ? prologue.originalPosition : Vector3.zero;
        
        while(elapsed < animationDuration){
            elapsed += Time.deltaTime * animationSpeed;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float easeT = easeCurve.Evaluate(t);
            
            if(leftComponent != null && leftComponent.block != null){
                Vector3 newPos = Vector3.Lerp(startLeftPos, targetLeftPos, easeT);
                leftComponent.block.Position.Value = newPos;
            }
            
            if(mainButton != null && mainButton.block != null){
                Vector3 newPos = Vector3.Lerp(startMainPos, targetMainPos, easeT);
                mainButton.block.Position.Value = newPos;
            }
            
            if(prologue != null && prologue.block != null){
                Vector3 newPos = Vector3.Lerp(startProloguePos, targetProloguePos, easeT);
                prologue.block.Position.Value = newPos;
            }
            
            yield return null;
        }
        
        if(leftComponent != null && leftComponent.block != null) 
            leftComponent.block.Position.Value = leftComponent.originalPosition;
        if(mainButton != null && mainButton.block != null) 
            mainButton.block.Position.Value = mainButton.originalPosition;
        if(prologue != null && prologue.block != null) 
            prologue.block.Position.Value = prologue.originalPosition;
        
        currentAnimation = null;
    }

    IEnumerator AnimateHideMenu(){
        float elapsed = 0f;
        
        Vector3 startLeftPos = leftComponent != null && leftComponent.block != null ? leftComponent.block.Position.Value : Vector3.zero;
        Vector3 startMainPos = mainButton != null && mainButton.block != null ? mainButton.block.Position.Value : Vector3.zero;
        Vector3 startProloguePos = prologue != null && prologue.block != null ? prologue.block.Position.Value : Vector3.zero;
        
        Vector3 targetLeftPos = leftComponent != null ? leftComponent.originalPosition + leftComponent.hideOffset : Vector3.zero;
        Vector3 targetMainPos = mainButton != null ? mainButton.originalPosition + mainButton.hideOffset : Vector3.zero;
        Vector3 targetProloguePos = prologue != null ? prologue.originalPosition + prologue.hideOffset : Vector3.zero;
        
        while(elapsed < animationDuration){
            elapsed += Time.deltaTime * animationSpeed;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float easeT = easeCurve.Evaluate(t);
            
            if(leftComponent != null && leftComponent.block != null){
                Vector3 newPos = Vector3.Lerp(startLeftPos, targetLeftPos, easeT);
                leftComponent.block.Position.Value = newPos;
            }
            
            if(mainButton != null && mainButton.block != null){
                Vector3 newPos = Vector3.Lerp(startMainPos, targetMainPos, easeT);
                mainButton.block.Position.Value = newPos;
            }
            
            if(prologue != null && prologue.block != null){
                Vector3 newPos = Vector3.Lerp(startProloguePos, targetProloguePos, easeT);
                prologue.block.Position.Value = newPos;
            }
            
            yield return null;
        }
        
        if(leftComponent != null && leftComponent.block != null) 
            leftComponent.block.Position.Value = targetLeftPos;
        if(mainButton != null && mainButton.block != null) 
            mainButton.block.Position.Value = targetMainPos;
        if(prologue != null && prologue.block != null) 
            prologue.block.Position.Value = targetProloguePos;
        
        currentAnimation = null;
    }

    public void ToggleMenu(){
        if(isMenuVisible) HideAllMenuUI();
        else ShowAllMenuUI();
    }
}

[System.Serializable]
public class MenuComponent
{
    public UIBlock2D block;
    public Vector3 hideOffset = new Vector3(-1000f, 0f, 0f);
    
    [ReadOnly] public Vector3 originalPosition;
}