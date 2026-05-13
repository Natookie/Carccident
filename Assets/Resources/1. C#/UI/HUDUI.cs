using UnityEngine;
using Nova;

public class HUDUI : MonoBehaviour
{
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

    [Header("MAIN BUTTON REFERENCES")]
    [SerializeField] private UpgradeButton upgradeButton;
    [SerializeField] private SettingButton settingButton;
    [SerializeField] private ExitButton exitButton;
    [SerializeField] private PlayButton playButton;

    private float fpsAccumulator = 0f;
    private int fpsFrames = 0;
    private float fpsTimer = 0f;
    private float currentFPS = 0f;
    
    void Awake(){
        upgradeButton.Initialize(this);
        settingButton.Initialize(this);
        exitButton.Initialize(this);
        playButton.Initialize(this);
    }

    void Start(){
        fpsTimer = fpsUpdateInterval;
        if(redCircle != null && redCircle.Shadow == null)
            Debug.LogWarning("Red Circle doesn't have a Shadow component!");
        
        CloseAllPanels();
    }
    
    void Update(){
        CalculateFPS();
        UpdatePulseEffect();
    }
    
    #region BORDER
    void CalculateFPS(){
        fpsAccumulator += Time.unscaledDeltaTime;
        fpsFrames++;
        
        fpsTimer -= Time.unscaledDeltaTime;
        if(fpsTimer <= 0f){
            currentFPS = fpsFrames / fpsAccumulator;
            if(cctvText != null) cctvText.Text = $"{Mathf.RoundToInt(currentFPS)} FPS";
            
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
    #endregion

    #region MAIN BUTTON
    public void OpenUpgradePanel(){
        CloseAllPanels();
        upgradeButton.Panel.SetActive(true);
    }
    
    public void OpenSettingPanel(){
        CloseAllPanels();
        settingButton.Panel.SetActive(true);
    }
    
    public void OpenExitPanel() => GameManager.Instance.ExitGame();
    public void OpenPlayPanel(){
        GameManager.Instance.PlayGame();
        //HideMainMenuPanel();
    }
    
    void CloseAllPanels(){
        if(upgradeButton.Panel != null) upgradeButton.Panel.SetActive(false);
        if(settingButton.Panel != null) settingButton.Panel.SetActive(false);
    }
    #endregion
}

[System.Serializable]
public class UpgradeButton
{
    [SerializeField] private UIBlock2D button;
    [SerializeField] private GameObject panel;
    
    public GameObject Panel => panel;
    
    public void Initialize(HUDUI hud){
        if(button != null) button.AddGestureHandler<Gesture.OnPress>((evt) => hud.OpenUpgradePanel());
    }
}

[System.Serializable]
public class SettingButton
{
    [SerializeField] private UIBlock2D button;
    [SerializeField] private GameObject panel;
    
    public GameObject Panel => panel;
    
    public void Initialize(HUDUI hud){
        if(button != null) button.AddGestureHandler<Gesture.OnPress>((evt) => hud.OpenSettingPanel());
    }
}

[System.Serializable]
public class ExitButton
{
    [SerializeField] private UIBlock2D button;
    
    public void Initialize(HUDUI hud){
        if(button != null) button.AddGestureHandler<Gesture.OnPress>((evt) => hud.OpenExitPanel());
    }
}

[System.Serializable]
public class PlayButton
{
    [SerializeField] private UIBlock2D button;
    
    public void Initialize(HUDUI hud){
        if(button != null) button.AddGestureHandler<Gesture.OnPress>((evt) => hud.OpenPlayPanel());
    }
}