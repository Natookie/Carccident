using UnityEngine;
using Nova;
using System.Collections;
using UnityEngine.Rendering;

public class SettingUI : MonoBehaviour
{
    [Header("VOLUME SETTINGS")]
    [SerializeField] private UIBlock2D volumeSliderLine;
    [SerializeField] private UIBlock2D volumeInnerPart;
    [SerializeField] private TextBlock volumeValueText;
    private float volumePercent = 50f;
    private Vector3 originalVolumeInnerScale;
    private Color originalVolumeInnerColor;
    private Coroutine volumeSquishCoroutine;
    private bool isVolumeDragging = false;
    private bool isVolumeBeyondBounds = false;
    
    [Header("RESOLUTION SCALE SETTINGS")]
    [SerializeField] private UIBlock2D resolutionSliderLine;
    [SerializeField] private UIBlock2D resolutionInnerPart;
    [SerializeField] private TextBlock resolutionValueText;
    private int resolutionStep = 1;
    private float[] resolutionValues = { 0.5f, 0.75f, 1.0f };
    private string[] resolutionNames = { "Low", "Medium", "High" };
    private Vector3 originalResolutionInnerScale;
    private Color originalResolutionInnerColor;
    private Coroutine resolutionSquishCoroutine;
    private bool isResolutionDragging = false;
    private bool isResolutionBeyondBounds = false;
    
    [Header("LANGUAGE SETTINGS")]
    [SerializeField] private UIBlock2D languageButton;
    [SerializeField] private TextBlock languageText;
    private string[] languages = { "English", "Indonesia" };
    private int currentLanguageIndex = 0;
    private Color originalButtonColor;
    
    [Header("ANIMATION SETTINGS")]
    [SerializeField] private float squishIntensity = 1.2f;
    [SerializeField] private float squishSpeed = 5f;
    [SerializeField] private float popDuration = 0.1f;
    [SerializeField] private float popScale = 1.1f;
    [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color sliderHoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    
    void Awake(){
        LoadSavedSettings();
        
        if(volumeInnerPart != null){
            originalVolumeInnerScale = volumeInnerPart.transform.localScale;
            originalVolumeInnerColor = volumeInnerPart.Color;
        }
        if(resolutionInnerPart != null){
            originalResolutionInnerScale = resolutionInnerPart.transform.localScale;
            originalResolutionInnerColor = resolutionInnerPart.Color;
        }
        
        if(volumeSliderLine != null && volumeInnerPart != null){
            volumeSliderLine.AddGestureHandler<Gesture.OnDrag>(OnVolumeDrag);
            volumeSliderLine.AddGestureHandler<Gesture.OnRelease>(OnVolumeRelease);
            volumeSliderLine.AddGestureHandler<Gesture.OnHover>(OnVolumeHover);
            volumeSliderLine.AddGestureHandler<Gesture.OnUnhover>(OnVolumeUnhover);
            UpdateVolumeSliderVisuals();
        }
        
        if(resolutionSliderLine != null && resolutionInnerPart != null){
            resolutionSliderLine.AddGestureHandler<Gesture.OnDrag>(OnResolutionDrag);
            resolutionSliderLine.AddGestureHandler<Gesture.OnRelease>(OnResolutionRelease);
            resolutionSliderLine.AddGestureHandler<Gesture.OnHover>(OnResolutionHover);
            resolutionSliderLine.AddGestureHandler<Gesture.OnUnhover>(OnResolutionUnhover);
            UpdateResolutionSliderVisuals();
        }
        
        if(languageButton != null){
            originalButtonColor = languageButton.Color;
            languageButton.AddGestureHandler<Gesture.OnPress>(OnLanguagePressed);
            languageButton.AddGestureHandler<Gesture.OnHover>(OnButtonHover);
            languageButton.AddGestureHandler<Gesture.OnUnhover>(OnButtonUnhover);
            UpdateLanguageText();
        }
    }

    #region SAVE/LOAD
    void LoadSavedSettings(){
        //Volume
        float savedVolume = SaveManager.GetVolume();
        volumePercent = savedVolume * 100f;
        
        //Graphic
        int savedGraphic = SaveManager.GetGraphic();
        resolutionStep = Mathf.Clamp(savedGraphic, 0, resolutionValues.Length - 1);
        ApplyResolutionScale();
        
        //Language
        string savedLanguage = SaveManager.GetLanguage();
        for(int i = 0; i < languages.Length; i++){
            if(languages[i] == savedLanguage){
                currentLanguageIndex = i;
                break;
            }
        }
        ApplyLanguage();
    }
    
    void SaveVolumeSetting(){
        float volume = volumePercent / 100f;
        SaveManager.SetVolume(volume);
    }
    
    void SaveResolutionSetting() => SaveManager.SetGraphic(resolutionStep);
    void SaveLanguageSetting() => SaveManager.SetLanguage(languages[currentLanguageIndex]);
    #endregion

    #region VOLUME
    void OnVolumeDrag(Gesture.OnDrag evt){
        if(volumeSliderLine == null || volumeInnerPart == null) return;
        
        isVolumeDragging = true;
        Vector3 localPosition = volumeSliderLine.transform.InverseTransformPoint(evt.PointerPositions.Current);
        float sliderWidth = volumeSliderLine.Size.X.Value;
        float percent = (localPosition.x + (sliderWidth / 2f)) / sliderWidth;
        
        bool wasBeyondBounds = isVolumeBeyondBounds;
        isVolumeBeyondBounds = percent < 0f || percent > 1f;
        
        percent = Mathf.Clamp01(percent);
        volumePercent = percent * 100f;
        
        SaveVolumeSetting();
        
        if(isVolumeBeyondBounds && !wasBeyondBounds){
            if(volumeSquishCoroutine != null) StopCoroutine(volumeSquishCoroutine);
            volumeSquishCoroutine = StartCoroutine(SquishGradually(volumeInnerPart, originalVolumeInnerScale, true));
        }
        else if(!isVolumeBeyondBounds && wasBeyondBounds){
            if(volumeSquishCoroutine != null) StopCoroutine(volumeSquishCoroutine);
            volumeSquishCoroutine = StartCoroutine(SquishGradually(volumeInnerPart, originalVolumeInnerScale, false));
        }
        
        UpdateVolumeSliderVisuals();
    }
    
    void OnVolumeRelease(Gesture.OnRelease evt){
        isVolumeDragging = false;
        isVolumeBeyondBounds = false;
        
        SaveVolumeSetting();
        
        if(volumeSquishCoroutine != null) StopCoroutine(volumeSquishCoroutine);
        volumeSquishCoroutine = StartCoroutine(SquishGradually(volumeInnerPart, originalVolumeInnerScale, false));
        
        if(volumeInnerPart != null && volumeInnerPart.Color != originalVolumeInnerColor) volumeInnerPart.Color = originalVolumeInnerColor;
    }
    
    void OnVolumeHover(Gesture.OnHover evt){
        if(volumeInnerPart != null && !isVolumeDragging) volumeInnerPart.Color = sliderHoverColor;
    }
    
    void OnVolumeUnhover(Gesture.OnUnhover evt){
        if(volumeInnerPart != null && !isVolumeDragging) volumeInnerPart.Color = originalVolumeInnerColor;
    }
    
    void UpdateVolumeSliderVisuals(){
        if(volumeSliderLine == null || volumeInnerPart == null) return;
        
        float sliderWidth = volumeSliderLine.Size.X.Value;
        float innerWidth = volumeInnerPart.Size.X.Value;
        float maxX = sliderWidth - innerWidth;

        float percent = volumePercent / 100f;
        float newX = percent * maxX;
        
        volumeInnerPart.Position.X = newX;
        if(volumeValueText != null) volumeValueText.Text = $"{Mathf.RoundToInt(volumePercent)}%";
        if(AudioManager.Instance != null) AudioManager.Instance.SetVolume(percent);
    }
    #endregion
    
    #region RESOLUTION SCALE
    void OnResolutionDrag(Gesture.OnDrag evt){
        if(resolutionSliderLine == null || resolutionInnerPart == null) return;
        
        isResolutionDragging = true;
        Vector3 localPosition = resolutionSliderLine.transform.InverseTransformPoint(evt.PointerPositions.Current);
        float sliderWidth = resolutionSliderLine.Size.X.Value;
        float percent = (localPosition.x + (sliderWidth / 2f)) / sliderWidth;
        
        bool wasBeyondBounds = isResolutionBeyondBounds;
        isResolutionBeyondBounds = percent < 0f || percent > 1f;
        
        int newStep = Mathf.RoundToInt(percent * (resolutionValues.Length - 1));
        newStep = Mathf.Clamp(newStep, 0, resolutionValues.Length - 1);
        
        if(newStep != resolutionStep){
            resolutionStep = newStep;
            ApplyResolutionScale();
            SaveResolutionSetting();
        }
        
        if(isResolutionBeyondBounds && !wasBeyondBounds){
            if(resolutionSquishCoroutine != null) StopCoroutine(resolutionSquishCoroutine);
            resolutionSquishCoroutine = StartCoroutine(SquishGradually(resolutionInnerPart, originalResolutionInnerScale, true));
        }
        else if(!isResolutionBeyondBounds && wasBeyondBounds){
            if(resolutionSquishCoroutine != null) StopCoroutine(resolutionSquishCoroutine);
            resolutionSquishCoroutine = StartCoroutine(SquishGradually(resolutionInnerPart, originalResolutionInnerScale, false));
        }
        
        UpdateResolutionSliderVisuals();
    }
    
    void OnResolutionRelease(Gesture.OnRelease evt){
        isResolutionDragging = false;
        isResolutionBeyondBounds = false;
        
        SaveResolutionSetting();
        
        if(resolutionSquishCoroutine != null) StopCoroutine(resolutionSquishCoroutine);
        resolutionSquishCoroutine = StartCoroutine(SquishGradually(resolutionInnerPart, originalResolutionInnerScale, false));
        
        if(resolutionInnerPart != null && resolutionInnerPart.Color != originalResolutionInnerColor)
            resolutionInnerPart.Color = originalResolutionInnerColor;
    }
    
    void OnResolutionHover(Gesture.OnHover evt){
        if(resolutionInnerPart != null && !isResolutionDragging)
            resolutionInnerPart.Color = sliderHoverColor;
    }
    
    void OnResolutionUnhover(Gesture.OnUnhover evt){
        if(resolutionInnerPart != null && !isResolutionDragging)
            resolutionInnerPart.Color = originalResolutionInnerColor;
    }
    
    void UpdateResolutionSliderVisuals(){
        if(resolutionSliderLine == null || resolutionInnerPart == null) return;
        
        float sliderWidth = resolutionSliderLine.Size.X.Value;
        float innerWidth = resolutionInnerPart.Size.X.Value;
        float maxX = sliderWidth - innerWidth;
        
        float percent = (float)resolutionStep / (resolutionValues.Length - 1);
        float newX = percent * maxX;
        
        resolutionInnerPart.Position.X = newX;
        
        if(resolutionValueText != null)
            resolutionValueText.Text = resolutionNames[resolutionStep];
    }
    
    void ApplyResolutionScale(){
        float scale = resolutionValues[resolutionStep];
        var renderPipeline = GraphicsSettings.defaultRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
        
        if(renderPipeline != null) renderPipeline.renderScale = scale;
    }
    #endregion
    
    #region LANGUAGE BUTTON
    void OnLanguagePressed(Gesture.OnPress evt){
        StartCoroutine(PopAnimation(languageButton));
        currentLanguageIndex = (currentLanguageIndex + 1) % languages.Length;
        UpdateLanguageText();
        SaveLanguageSetting();
        ApplyLanguage();
    }
    
    void OnButtonHover(Gesture.OnHover evt){
        if(languageButton != null) languageButton.Color = hoverColor;
    }
    
    void OnButtonUnhover(Gesture.OnUnhover evt){
        if(languageButton != null) languageButton.Color = originalButtonColor;
    }
    
    void UpdateLanguageText(){
        if(languageText != null) languageText.Text = languages[currentLanguageIndex];
    }
    
    void ApplyLanguage(){
        switch (currentLanguageIndex){
            case 0: LanguageSwitcher.Instance.SetLanguage("en"); break;
            case 1: LanguageSwitcher.Instance.SetLanguage("id"); break;
        }
    }
    #endregion
    
    #region ANIMATIONS
    IEnumerator SquishGradually(UIBlock2D target, Vector3 originalScale, bool squishOut){
        if(target == null) yield break;
        
        Vector3 targetScale;
        
        if(squishOut) targetScale = new Vector3(originalScale.x * squishIntensity, originalScale.y / squishIntensity, originalScale.z);
        else targetScale = originalScale;
        
        while(Vector3.Distance(target.transform.localScale, targetScale) > 0.01f){
            target.transform.localScale = Vector3.Lerp(target.transform.localScale, targetScale, Time.deltaTime * squishSpeed);
            yield return null;
        }
        
        target.transform.localScale = targetScale;
        volumeSquishCoroutine = null;
    }
    
    IEnumerator PopAnimation(UIBlock2D target){
        if(target == null) yield break;
        
        Vector3 originalScale = target.transform.localScale;
        Vector3 popScaleVec = originalScale * popScale;
        
        float elapsed = 0f;
        while(elapsed < popDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            target.transform.localScale = Vector3.Lerp(originalScale, popScaleVec, t);
            yield return null;
        }
        
        target.transform.localScale = popScaleVec;
        
        elapsed = 0f;
        while(elapsed < popDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            target.transform.localScale = Vector3.Lerp(popScaleVec, originalScale, t);
            yield return null;
        }
        
        target.transform.localScale = originalScale;
    }
    #endregion
}