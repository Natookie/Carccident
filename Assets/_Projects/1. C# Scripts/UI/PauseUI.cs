using UnityEngine;
using Nova;
using System.Collections;

public class PauseUI : MonoBehaviour
{
    public static PauseUI Instance {get; private set;}

    [Header("REFERENCES")]
    [SerializeField] private UIBlock2D pausePanel;
    [SerializeField] private UIBlock2D resumeButton;
    [SerializeField] private UIBlock2D endGameButton;
    
    [Header("PAUSE SETTINGS")]
    [SerializeField] private float timeScale = 0.3f;
    
    [Header("BUTTON ANIMATION")]
    [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float clickScale = 0.95f;
    [SerializeField] private float animationDuration = 0.1f;
    
    private bool isPaused = false;
    private float originalTimeScale = 1f;
    private Vector3 originalResumeScale;
    private Vector3 originalEndGameScale;
    private Color originalResumeColor;
    private Color originalEndGameColor;

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if(pausePanel != null) pausePanel.gameObject.SetActive(false);
        
        if(resumeButton != null){
            originalResumeScale = resumeButton.transform.localScale;
            originalResumeColor = resumeButton.Color;
            resumeButton.AddGestureHandler<Gesture.OnPress>(OnResumePressed);
            resumeButton.AddGestureHandler<Gesture.OnHover>(OnButtonHover);
            resumeButton.AddGestureHandler<Gesture.OnUnhover>(OnButtonUnhover);
        }
        
        if(endGameButton != null){
            originalEndGameScale = endGameButton.transform.localScale;
            originalEndGameColor = endGameButton.Color;
            endGameButton.AddGestureHandler<Gesture.OnPress>(OnEndGamePressed);
            endGameButton.AddGestureHandler<Gesture.OnHover>(OnButtonHover);
            endGameButton.AddGestureHandler<Gesture.OnUnhover>(OnButtonUnhover);
        }
    }

    public void TogglePause(){
        if(isPaused) ResumeGame();
        else PauseGame();
    }
    
    public void PauseGame(){
        if(isPaused) return;
        if(GameManager.Instance == null || GameManager.Instance.isGameOver) return;
        
        isPaused = true;
        originalTimeScale = Time.timeScale;
        Time.timeScale = timeScale;
        
        if(pausePanel != null) pausePanel.gameObject.SetActive(true);
    }
    
    void OnResumePressed(Gesture.OnPress evt){
        StartCoroutine(AnimateButtonClick(resumeButton, originalResumeScale, originalResumeColor, () => {
            ResumeGame();
        }));
    }
    
    void OnEndGamePressed(Gesture.OnPress evt){
        StartCoroutine(AnimateButtonClick(endGameButton, originalEndGameScale, originalEndGameColor, () => {
            ResumeGame();
            GameManager.Instance.GameOver();
        }));
    }
    
    void OnButtonHover(Gesture.OnHover evt){
        UIBlock2D button = evt.Receiver as UIBlock2D;
        if(button != null){
            StopCoroutine("AnimateButtonHover");
            StartCoroutine(AnimateButtonHover(button, true));
        }
    }
    
    void OnButtonUnhover(Gesture.OnUnhover evt){
        UIBlock2D button = evt.Receiver as UIBlock2D;
        if(button != null){
            StopCoroutine("AnimateButtonHover");
            StartCoroutine(AnimateButtonHover(button, false));
        }
    }
    
    IEnumerator AnimateButtonHover(UIBlock2D button, bool isHover){
        Vector3 targetScale;
        Color targetColor;
        Vector3 startScale = button.transform.localScale;
        Color startColor = button.Color;
        
        if(isHover){
            targetScale = GetOriginalScale(button) * hoverScale;
            targetColor = hoverColor;
        }
        else{
            targetScale = GetOriginalScale(button);
            targetColor = GetOriginalColor(button);
        }
        
        float elapsed = 0f;
        while(elapsed < animationDuration){
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            
            button.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            button.Color = Color.Lerp(startColor, targetColor, t);
            
            yield return null;
        }
        
        button.transform.localScale = targetScale;
        button.Color = targetColor;
    }
    
    IEnumerator AnimateButtonClick(UIBlock2D button, Vector3 originalScale, Color originalColor, System.Action onComplete){
        float elapsed = 0f;
        Vector3 clickScaleVec = originalScale * clickScale;
        
        while(elapsed < animationDuration){
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            button.transform.localScale = Vector3.Lerp(originalScale, clickScaleVec, t);
            yield return null;
        }
        
        button.transform.localScale = clickScaleVec;
        
        onComplete?.Invoke();
        
        elapsed = 0f;
        while(elapsed < animationDuration){
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            button.transform.localScale = Vector3.Lerp(clickScaleVec, originalScale, t);
            yield return null;
        }
        
        button.transform.localScale = originalScale;
    }
    
    Vector3 GetOriginalScale(UIBlock2D button){
        if(button == resumeButton) return originalResumeScale;
        if(button == endGameButton) return originalEndGameScale;
        return Vector3.one;
    }
    
    Color GetOriginalColor(UIBlock2D button){
        if(button == resumeButton) return originalResumeColor;
        if(button == endGameButton) return originalEndGameColor;
        return Color.white;
    }
    
    public void ResumeGame(){
        if(!isPaused) return;
        
        isPaused = false;
        Time.timeScale = originalTimeScale;
        
        if(pausePanel != null) pausePanel.gameObject.SetActive(false);
    }
    
    public bool IsPaused() => isPaused;
    
    void OnDestroy(){
        if(isPaused) Time.timeScale = originalTimeScale;
    }
}