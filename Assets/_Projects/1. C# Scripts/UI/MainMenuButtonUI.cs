using UnityEngine;
using Nova;
using System.Collections;
using Unity.VisualScripting;

public class MainButtonUI : MonoBehaviour
{
    [Header("BUTTON REFERENCES")]
    [SerializeField] private UpgradeButton upgradeButton;
    [SerializeField] private SettingButton settingButton;
    [SerializeField] private ExitButton exitButton;
    [SerializeField] private PlayButton playButton;
    
    [SerializeField] private ButtonAnimationSettings animationSettings;
    [HideInInspector] private bool isGameLoaded = false;

    private AnimatedButton currentSelectedButton;
    
    void Awake(){
        upgradeButton.Initialize(this, animationSettings);
        settingButton.Initialize(this, animationSettings);
        exitButton.Initialize(this, animationSettings);
        playButton.Initialize(this, animationSettings);
    }
    
    void Start(){
        CloseAllPanels();
        OpenUpgradePanel();
        isGameLoaded = true;
    }
    
    public void SetSelectedButton(AnimatedButton button){
        if(currentSelectedButton != null && currentSelectedButton != button) currentSelectedButton.SetSelected(false);
        
        if(isGameLoaded) AudioManager.Instance.PlayButtonClick();
        currentSelectedButton = button;
        currentSelectedButton.SetSelected(true);
    }

    public void ResetMainButton() => playButton.ResetButton();
    
    public void OpenUpgradePanel(){
        CloseAllPanels();
        upgradeButton.Panel.SetActive(true);
        SetSelectedButton(upgradeButton);
    }
    
    public void OpenSettingPanel(){
        CloseAllPanels();
        settingButton.Panel.SetActive(true);
        SetSelectedButton(settingButton);
    }
    
    public void OpenExitPanel() => GameManager.Instance.ExitGame();
    public void OpenPlayPanel() => GameManager.Instance.PlayGame();
    
    void CloseAllPanels(){
        if(upgradeButton.Panel != null) upgradeButton.Panel.SetActive(false);
        if(settingButton.Panel != null) settingButton.Panel.SetActive(false);
    }
}

[System.Serializable]
public class ButtonAnimationSettings
{
    [Header("SHARED VALUES")]
    public Color mainBlockHoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public Color elementHoverColor = new Color(1f, 0.9f, 0.9f, 1f);
    public float hoverPositionOffset = 20f;
    public float clickScaleIncrease = 1.1f;
    public float animationSpeed = 8f;
    public float clickDuration = 0.075f;
}

[System.Serializable]
public class AnimatedButton
{
    [SerializeField] protected UIBlock2D container;
    [SerializeField] protected UIBlock2D topBorder;
    [SerializeField] protected UIBlock2D mainBlock;
    [SerializeField] protected UIBlock2D icon;
    [SerializeField] protected TextBlock label;
    [SerializeField] protected Color topBorderHoverColor = new Color(1f, 0.8f, 0.8f, 1f);
    
    protected float originalMainBlockY;
    protected Color originalTopBorderColor;
    protected Color originalMainBlockColor;
    protected Color originalIconColor;
    protected Color originalLabelColor;
    protected Vector3 originalContainerScale;
    
    protected Coroutine currentAnimation;
    protected MonoBehaviour owner;
    protected ButtonAnimationSettings settings;
    protected bool isSelected = false;
    
    public virtual void Initialize(MonoBehaviour mono, ButtonAnimationSettings animSettings){
        owner = mono;
        settings = animSettings;
        
        if(topBorder != null) originalTopBorderColor = topBorder.Color;
        if(mainBlock != null){
            originalMainBlockY = mainBlock.Position.Y.Value;
            originalMainBlockColor = mainBlock.Color;
        }
        if(icon != null) originalIconColor = icon.Color;
        if(label != null) originalLabelColor = label.Color;
        
        if(container != null){
            originalContainerScale = container.transform.localScale;
            
            container.AddGestureHandler<Gesture.OnHover>(OnHover);
            container.AddGestureHandler<Gesture.OnUnhover>(OnUnhover);
        }
    }
    
    protected virtual void OnHover(Gesture.OnHover evt){
        if(owner == null || isSelected) return;
        
        if(currentAnimation != null) owner.StopCoroutine(currentAnimation);
        currentAnimation = owner.StartCoroutine(AnimateHover(true));
    }
    
    protected virtual void OnUnhover(Gesture.OnUnhover evt){
        if(owner == null || isSelected) return;
        
        if(currentAnimation != null) owner.StopCoroutine(currentAnimation);
        currentAnimation = owner.StartCoroutine(AnimateHover(false));
    }
    
    protected virtual IEnumerator AnimateHover(bool hover){
        float elapsed = 0f;
        
        float startMainBlockY = mainBlock != null ? mainBlock.Position.Y.Value : 0;
        float targetMainBlockY = hover ? originalMainBlockY + settings.hoverPositionOffset : originalMainBlockY;
        
        Color startTopBorderColor = topBorder != null ? topBorder.Color : Color.white;
        Color targetTopBorderColor = hover ? topBorderHoverColor : originalTopBorderColor;
        
        Color startMainBlockColor = mainBlock != null ? mainBlock.Color : Color.white;
        Color targetMainBlockColor = hover ? settings.mainBlockHoverColor : originalMainBlockColor;
        
        Color startIconColor = icon != null ? icon.Color : Color.white;
        Color targetIconColor = hover ? settings.elementHoverColor : originalIconColor;
        
        Color startLabelColor = label != null ? label.Color : Color.white;
        Color targetLabelColor = hover ? settings.elementHoverColor : originalLabelColor;
        
        while(elapsed < 1f){
            elapsed += Time.deltaTime * settings.animationSpeed;
            float t = Mathf.Clamp01(elapsed);
            float easeT = 1f - Mathf.Pow(1f - t, 3);
            
            if(topBorder != null) topBorder.Color = Color.Lerp(startTopBorderColor, targetTopBorderColor, easeT);
            if(mainBlock != null){
                mainBlock.Position.Y.Value = Mathf.Lerp(startMainBlockY, targetMainBlockY, easeT);
                mainBlock.Color = Color.Lerp(startMainBlockColor, targetMainBlockColor, easeT);
            }
            if(icon != null) icon.Color = Color.Lerp(startIconColor, targetIconColor, easeT);
            if(label != null) label.Color = Color.Lerp(startLabelColor, targetLabelColor, easeT);
            
            yield return null;
        }
        
        if(topBorder != null) topBorder.Color = targetTopBorderColor;
        if(mainBlock != null){
            mainBlock.Position.Y.Value = targetMainBlockY;
            mainBlock.Color = targetMainBlockColor;
        }
        if(icon != null) icon.Color = targetIconColor;
        if(label != null) label.Color = targetLabelColor;
        
        currentAnimation = null;
    }
    
    public void PlayClickAnimation(){
        if(container == null || owner == null) return;
        
        if(currentAnimation != null) owner.StopCoroutine(currentAnimation);
        currentAnimation = owner.StartCoroutine(AnimateClick());
    }
    
    protected virtual IEnumerator AnimateClick(){
        if(container == null) yield break;
        
        Vector3 originalScale = container.transform.localScale;
        Vector3 targetScale = originalScale * settings.clickScaleIncrease;
        
        float elapsed = 0f;
        while(elapsed < settings.clickDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / settings.clickDuration;
            float easeT = 1f - Mathf.Pow(1f - t, 3);
            container.transform.localScale = Vector3.Lerp(originalScale, targetScale, easeT);
            yield return null;
        }
        
        container.transform.localScale = targetScale;
        
        elapsed = 0f;
        while(elapsed < settings.clickDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / settings.clickDuration;
            float easeT = t * t * t;
            container.transform.localScale = Vector3.Lerp(targetScale, originalScale, easeT);
            yield return null;
        }
        
        container.transform.localScale = originalScale;
        currentAnimation = null;
    }
    
    public virtual void SetSelected(bool selected){
        isSelected = selected;
        
        if(selected){
            if(topBorder != null) topBorder.Color = topBorderHoverColor;
            if(mainBlock != null){
                mainBlock.Position.Y.Value = originalMainBlockY;
                mainBlock.Color = settings.mainBlockHoverColor;
            }
            if(icon != null) icon.Color = settings.elementHoverColor;
            if(label != null) label.Color = settings.elementHoverColor;
        }
        else{
            if(topBorder != null) topBorder.Color = originalTopBorderColor;
            if(mainBlock != null){
                mainBlock.Position.Y.Value = originalMainBlockY;
                mainBlock.Color = originalMainBlockColor;
            }
            if(icon != null) icon.Color = originalIconColor;
            if(label != null) label.Color = originalLabelColor;
        }
    }
}

[System.Serializable]
public class UpgradeButton : AnimatedButton
{
    [Header("UPGRADE BUTTON")]
    [SerializeField] private GameObject panel;
    public GameObject Panel => panel;
    
    public override void Initialize(MonoBehaviour mono, ButtonAnimationSettings animSettings){
        base.Initialize(mono, animSettings);
        
        if(container != null){
            container.AddGestureHandler<Gesture.OnPress>((evt) => {
                if(owner != null){
                    var hud = owner as MainButtonUI;
                    if(hud != null){
                        hud.OpenUpgradePanel();
                        hud.SetSelectedButton(this);
                    }
                }
                PlayClickAnimation();
            });
        }
    }
    
    public override void SetSelected(bool selected){
        base.SetSelected(selected);
        if(selected && panel != null) panel.SetActive(true);
    }
}

[System.Serializable]
public class SettingButton : AnimatedButton
{
    [Header("SETTING BUTTON")]
    [SerializeField] private GameObject panel;
    public GameObject Panel => panel;
    
    public override void Initialize(MonoBehaviour mono, ButtonAnimationSettings animSettings){
        base.Initialize(mono, animSettings);
        
        if(container != null){
            container.AddGestureHandler<Gesture.OnPress>((evt) => {
                if(owner != null){
                    var hud = owner as MainButtonUI;
                    if(hud != null){
                        hud.OpenSettingPanel();
                        hud.SetSelectedButton(this);
                    }
                }
                PlayClickAnimation();
            });
        }
    }
    
    public override void SetSelected(bool selected){
        base.SetSelected(selected);
        if(selected && panel != null) panel.SetActive(true);
    }
}

[System.Serializable]
public class ExitButton : AnimatedButton
{
    [Header("EXIT BUTTON")]
    [SerializeField] private Color exitTopBorderHoverColor = new Color(1f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color exitMainBlockHoverColor = new Color(0.9f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color exitElementHoverColor = new Color(1f, 0.5f, 0.5f, 1f);
    
    public override void Initialize(MonoBehaviour mono, ButtonAnimationSettings animSettings){
        base.Initialize(mono, animSettings);
        
        topBorderHoverColor = exitTopBorderHoverColor;
        
        if(container != null){
            container.AddGestureHandler<Gesture.OnPress>((evt) => {
                if(owner != null){
                    var hud = owner as MainButtonUI;
                    if(hud != null) hud.OpenExitPanel();
                }
                PlayClickAnimation();
            });
        }
    }
    
    protected override IEnumerator AnimateHover(bool hover){
        float elapsed = 0f;
        
        float startMainBlockY = mainBlock != null ? mainBlock.Position.Y.Value : 0;
        float targetMainBlockY = hover ? originalMainBlockY + settings.hoverPositionOffset : originalMainBlockY;
        
        Color startTopBorderColor = topBorder != null ? topBorder.Color : Color.white;
        Color targetTopBorderColor = hover ? exitTopBorderHoverColor : originalTopBorderColor;
        
        Color startMainBlockColor = mainBlock != null ? mainBlock.Color : Color.white;
        Color targetMainBlockColor = hover ? exitMainBlockHoverColor : originalMainBlockColor;
        
        Color startIconColor = icon != null ? icon.Color : Color.white;
        Color targetIconColor = hover ? exitElementHoverColor : originalIconColor;
        
        Color startLabelColor = label != null ? label.Color : Color.white;
        Color targetLabelColor = hover ? exitElementHoverColor : originalLabelColor;
        
        while(elapsed < 1f){
            elapsed += Time.deltaTime * settings.animationSpeed;
            float t = Mathf.Clamp01(elapsed);
            float easeT = 1f - Mathf.Pow(1f - t, 3);
            
            if(topBorder != null) topBorder.Color = Color.Lerp(startTopBorderColor, targetTopBorderColor, easeT);
            if(mainBlock != null){
                mainBlock.Position.Y.Value = Mathf.Lerp(startMainBlockY, targetMainBlockY, easeT);
                mainBlock.Color = Color.Lerp(startMainBlockColor, targetMainBlockColor, easeT);
            }
            if(icon != null) icon.Color = Color.Lerp(startIconColor, targetIconColor, easeT);
            if(label != null) label.Color = Color.Lerp(startLabelColor, targetLabelColor, easeT);
            
            yield return null;
        }
        
        if(topBorder != null) topBorder.Color = targetTopBorderColor;
        if(mainBlock != null){
            mainBlock.Position.Y.Value = targetMainBlockY;
            mainBlock.Color = targetMainBlockColor;
        }
        if(icon != null) icon.Color = targetIconColor;
        if(label != null) label.Color = targetLabelColor;
        
        currentAnimation = null;
    }
    
    public override void SetSelected(bool selected){
        isSelected = selected;
        
        if(selected){
            if(topBorder != null) topBorder.Color = exitTopBorderHoverColor;
            if(mainBlock != null){
                mainBlock.Position.Y.Value = originalMainBlockY;
                mainBlock.Color = exitMainBlockHoverColor;
            }
            if(icon != null) icon.Color = exitElementHoverColor;
            if(label != null) label.Color = exitElementHoverColor;
        }
        else{
            if(topBorder != null) topBorder.Color = originalTopBorderColor;
            if(mainBlock != null){
                mainBlock.Position.Y.Value = originalMainBlockY;
                mainBlock.Color = originalMainBlockColor;
            }
            if(icon != null) icon.Color = originalIconColor;
            if(label != null) label.Color = originalLabelColor;
        }
    }
}

[System.Serializable]
public class PlayButton
{
    [Header("PLAY BUTTON REFERENCES")]
    [SerializeField] private UIBlock2D container;
    [SerializeField] private UIBlock2D icon;
    [SerializeField] private UIBlock2D mainBlock;
    [SerializeField] private TextBlock playTextTop;
    [SerializeField] private TextBlock playTextBot;
    
    [Header("PLAY BUTTON COLORS")]
    [SerializeField] private Color mainBlockHoverColor = new Color(0.95f, 0.95f, 0.95f, 1f);
    [SerializeField] private Color mainBlockClickColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private Color componentsHoverColor = new Color(1f, 0.9f, 0.9f, 1f);
    [SerializeField] private Color componentsClickColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    
    [Header("PLAY BUTTON ANIMATION")]
    [SerializeField] private float pulseScale = 1.05f;
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float hoverPulseSpeed = 0.5f;
    [SerializeField] private float pulseRotationZ = 5f;
    [SerializeField] private float hoverPositionOffset = 10f;
    [SerializeField] private float clickDuration = 0.2f;
    
    private Vector3 originalMainBlockScale;
    private float originalMainBlockY;
    private Quaternion originalMainBlockRotation;
    private Color originalIconColor;
    private Color originalMainBlockColor;
    private Color originalTextTopColor;
    private Color originalTextBotColor;
    
    private Coroutine pulseCoroutine;
    private Coroutine hoverCoroutine;
    private MonoBehaviour owner;
    private ButtonAnimationSettings settings;
    private bool isHovering = false;
    private bool isGameInitialized = false;
    private bool isClicked = false;
    
    public void Initialize(MainButtonUI mainButtonUI, ButtonAnimationSettings animSettings){
        owner = mainButtonUI;
        settings = animSettings;
        
        if(mainBlock != null){
            originalMainBlockScale = mainBlock.transform.localScale;
            originalMainBlockY = mainBlock.Position.Y.Value;
            originalMainBlockRotation = mainBlock.transform.localRotation;
            originalMainBlockColor = mainBlock.Color;
            
            if(mainBlock.Gradient != null) mainBlock.Gradient.Color = originalMainBlockColor;
        }
        
        if(icon != null) originalIconColor = icon.Color;
        if(playTextTop != null) originalTextTopColor = playTextTop.Color;
        if(playTextBot != null) originalTextBotColor = playTextBot.Color;
        
        if(container != null){
            container.AddGestureHandler<Gesture.OnHover>(OnHover);
            container.AddGestureHandler<Gesture.OnUnhover>(OnUnhover);
            container.AddGestureHandler<Gesture.OnPress>(OnPress);
        }
        
        isGameInitialized = GameManager.Instance != null && GameManager.Instance.isGameInitialized;
        if(!isGameInitialized && owner != null) pulseCoroutine = owner.StartCoroutine(IdlePulse());
    }
    
    IEnumerator IdlePulse(){
        float pulseTimer = 0f;
        float currentPulseSpeed = isHovering ? hoverPulseSpeed : pulseSpeed;
        
        while(!isGameInitialized && !isClicked){
            pulseTimer += Time.deltaTime * currentPulseSpeed;
            
            float t = Mathf.PingPong(pulseTimer, 1f);
            float easeT = Mathf.SmoothStep(0f, 1f, t);
            
            float scaleMultiplier = 1f + (pulseScale - 1f) * easeT;
            if(mainBlock != null) mainBlock.transform.localScale = originalMainBlockScale * scaleMultiplier;
            
            float rotationZ = Mathf.Lerp(-pulseRotationZ, pulseRotationZ, easeT);
            if(mainBlock != null) mainBlock.transform.localRotation = originalMainBlockRotation * Quaternion.Euler(0, 0, rotationZ);
            
            currentPulseSpeed = isHovering ? hoverPulseSpeed : pulseSpeed;
            
            yield return null;
        }
        
        float resetTimer = 0f;
        float resetDuration = 0.2f;
        Vector3 startMainBlockScale = mainBlock != null ? mainBlock.transform.localScale : originalMainBlockScale;
        Quaternion startMainBlockRot = mainBlock != null ? mainBlock.transform.localRotation : originalMainBlockRotation;
        
        while(resetTimer < resetDuration){
            resetTimer += Time.deltaTime;
            float t = resetTimer / resetDuration;
            float easeT = Mathf.SmoothStep(0f, 1f, t);
            
            if(mainBlock != null){
                mainBlock.transform.localScale = Vector3.Lerp(startMainBlockScale, originalMainBlockScale, easeT);
                mainBlock.transform.localRotation = Quaternion.Lerp(startMainBlockRot, originalMainBlockRotation, easeT);
            }
            
            yield return null;
        }
        
        if(mainBlock != null){
            mainBlock.transform.localScale = originalMainBlockScale;
            mainBlock.transform.localRotation = originalMainBlockRotation;
        }
    }
    
    void OnHover(Gesture.OnHover evt){
        if(owner == null || isClicked) return;
        
        isHovering = true;
        
        if(hoverCoroutine != null) owner.StopCoroutine(hoverCoroutine);
        hoverCoroutine = owner.StartCoroutine(AnimateHover(true));
    }
    
    void OnUnhover(Gesture.OnUnhover evt){
        if(owner == null || isClicked) return;
        
        isHovering = false;
        
        if(hoverCoroutine != null) owner.StopCoroutine(hoverCoroutine);
        hoverCoroutine = owner.StartCoroutine(AnimateHover(false));
    }
    
    void OnPress(Gesture.OnPress evt){
        if(owner == null || isClicked) return;
        
        isClicked = true;
        isHovering = false;
        
        if(pulseCoroutine != null) owner.StopCoroutine(pulseCoroutine);
        if(hoverCoroutine != null) owner.StopCoroutine(hoverCoroutine);
        
        hoverCoroutine = owner.StartCoroutine(AnimateClick());
        
        owner.StartCoroutine(DelayedAction());
    }
    
    IEnumerator DelayedAction(){
        yield return new WaitForSeconds(clickDuration);
        var hud = owner as MainButtonUI;
        if(hud != null) hud.OpenPlayPanel();
    }
    
    IEnumerator AnimateHover(bool hover){
        if(mainBlock == null) yield break;
        
        float elapsed = 0f;
        
        float startMainBlockY = mainBlock.Position.Y.Value;
        float targetMainBlockY = hover ? originalMainBlockY + hoverPositionOffset : originalMainBlockY;
        
        Color startIconColor = icon != null ? icon.Color : Color.white;
        Color targetIconColor = hover ? componentsHoverColor : originalIconColor;
        
        Color startMainBlockColor = mainBlock.Color;
        Color targetMainBlockColor = hover ? mainBlockHoverColor : originalMainBlockColor;
        
        Color startTextTopColor = playTextTop != null ? playTextTop.Color : Color.white;
        Color targetTextTopColor = hover ? componentsHoverColor : originalTextTopColor;
        
        Color startTextBotColor = playTextBot != null ? playTextBot.Color : Color.white;
        Color targetTextBotColor = hover ? componentsHoverColor : originalTextBotColor;
        
        while(elapsed < 1f){
            elapsed += Time.deltaTime * settings.animationSpeed;
            float t = Mathf.Clamp01(elapsed);
            float easeT = 1f - Mathf.Pow(1f - t, 3);
            
            mainBlock.Position.Y.Value = Mathf.Lerp(startMainBlockY, targetMainBlockY, easeT);
            mainBlock.Color = Color.Lerp(startMainBlockColor, targetMainBlockColor, easeT);
            
            if(mainBlock.Gradient != null) mainBlock.Gradient.Color = mainBlock.Color;
            
            if(icon != null) icon.Color = Color.Lerp(startIconColor, targetIconColor, easeT);
            if(playTextTop != null) playTextTop.Color = Color.Lerp(startTextTopColor, targetTextTopColor, easeT);
            if(playTextBot != null) playTextBot.Color = Color.Lerp(startTextBotColor, targetTextBotColor, easeT);
            
            yield return null;
        }
        
        mainBlock.Position.Y.Value = targetMainBlockY;
        mainBlock.Color = targetMainBlockColor;
        
        if(mainBlock.Gradient != null) mainBlock.Gradient.Color = targetMainBlockColor;
        
        if(icon != null) icon.Color = targetIconColor;
        if(playTextTop != null) playTextTop.Color = targetTextTopColor;
        if(playTextBot != null) playTextBot.Color = targetTextBotColor;
        
        hoverCoroutine = null;
    }
    
    IEnumerator AnimateClick(){
        if(mainBlock == null) yield break;
        
        Vector3 originalMainScale = mainBlock.transform.localScale;
        Quaternion originalRot = mainBlock.transform.localRotation;
        Color originalIconCol = icon != null ? icon.Color : Color.white;
        Color originalMainBlockCol = mainBlock.Color;
        Color originalTextTopCol = playTextTop != null ? playTextTop.Color : Color.white;
        Color originalTextBotCol = playTextBot != null ? playTextBot.Color : Color.white;
        
        Vector3 popScale = originalMainScale * 1.1f;
        Vector3 squashScale = originalMainScale * 0.95f;
        
        float elapsed = 0f;
        float halfDuration = clickDuration / 3f;
        
        while(elapsed < halfDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float easeT = t * t;
            mainBlock.transform.localScale = Vector3.Lerp(originalMainScale, squashScale, easeT);
            yield return null;
        }
        
        elapsed = 0f;
        float popDuration = clickDuration / 1.5f;
        Quaternion targetRot = originalRot * Quaternion.Euler(0, 0, 360);
        
        while(elapsed < popDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            float easeT = 1f - Mathf.Pow(1f - t, 2);
            
            mainBlock.transform.localScale = Vector3.Lerp(squashScale, popScale, easeT);
            mainBlock.transform.localRotation = Quaternion.Lerp(originalRot, targetRot, easeT);
            
            if(icon != null) icon.Color = Color.Lerp(originalIconCol, componentsClickColor, easeT);
            mainBlock.Color = Color.Lerp(originalMainBlockCol, mainBlockClickColor, easeT);
            if(playTextTop != null) playTextTop.Color = Color.Lerp(originalTextTopCol, componentsClickColor, easeT);
            if(playTextBot != null) playTextBot.Color = Color.Lerp(originalTextBotCol, componentsClickColor, easeT);
            
            if(mainBlock.Gradient != null) mainBlock.Gradient.Color = mainBlock.Color;
            
            yield return null;
        }
        
        mainBlock.transform.localScale = originalMainScale;
        mainBlock.transform.localRotation = originalRot;
        mainBlock.Color = mainBlockClickColor;
        
        if(mainBlock.Gradient != null) mainBlock.Gradient.Color = mainBlockClickColor;
        
        if(icon != null) icon.Color = componentsClickColor;
        if(playTextTop != null) playTextTop.Color = componentsClickColor;
        if(playTextBot != null) playTextBot.Color = componentsClickColor;
        
        hoverCoroutine = null;
    }
    
    public void ResetButton(){
        isClicked = false;
        isHovering = false;
        
        if(pulseCoroutine != null && owner != null) owner.StopCoroutine(pulseCoroutine);
        if(hoverCoroutine != null && owner != null) owner.StopCoroutine(hoverCoroutine);
        
        if(mainBlock != null){
            mainBlock.transform.localScale = originalMainBlockScale;
            mainBlock.transform.localRotation = originalMainBlockRotation;
            mainBlock.Position.Y.Value = originalMainBlockY;
            mainBlock.Color = originalMainBlockColor;
            
            if(mainBlock.Gradient != null) mainBlock.Gradient.Color = originalMainBlockColor;
        }
        
        if(icon != null) icon.Color = originalIconColor;
        if(playTextTop != null) playTextTop.Color = originalTextTopColor;
        if(playTextBot != null) playTextBot.Color = originalTextBotColor;
        
        if(!isGameInitialized && owner != null) pulseCoroutine = owner.StartCoroutine(IdlePulse());
    }
}