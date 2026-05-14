using Nova;
using UnityEngine;
using System.Collections;
using NaughtyAttributes;

public class UpgradeUI : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private SkillButton[] skillList;
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private UpgradeSettings settings;

    bool needsUpdate => upgradePanel.activeSelf;
    const int MONEY = 100000;

    void Awake(){
        for(int i = 0; i < skillList.Length; i++){
            skillList[i].SetSkillIndex(i);
            skillList[i].Initialize(this, settings);
        }            
    }

    void Update(){
        if(!needsUpdate) return;

        foreach(SkillButton skill in skillList){
            skill.UpdateUpgradeableStatus(skillManager, MONEY);
        }
    }

    public void OnSkillPressed(int skillIndex){
        if(!skillList[skillIndex].IsUpgradeable) return;
        
        skillList[skillIndex].PlayClickAnimation();
        StartCoroutine(DelayedUpgrade(skillIndex));
    }
    
    IEnumerator DelayedUpgrade(int skillIndex){
        yield return new WaitForSeconds(0.1f);
        
        switch(skillIndex){
            case 0: skillManager.AddLevel(SkillManager.SkillType.SleepyMode); break;
            case 1: skillManager.AddLevel(SkillManager.SkillType.GreenWaveAddiction); break;
            case 2: skillManager.AddLevel(SkillManager.SkillType.OopsieRecoverySystem); break;
        }
        
        skillList[skillIndex].ResetSizeIfNotUpgradeable();
    }
}

[System.Serializable]
public class UpgradeSettings
{
    [Header("Colors")]
    public Color upgradeableColor = Color.green;
    public Color notUpgradeableColor = Color.gray;
    public Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color clickColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    
    [Header("Animation")]
    public float hoverSizeIncrease = 20f;
    public float clickSizeIncrease = 10f;
    public float animationSpeed = 10f;
    public float clickDuration = 0.075f;
}

[System.Serializable]
public class SkillButton
{
    [SerializeField] private UIBlock2D button;
    [SerializeField] private TextBlock costText;
    
    private int skillIndex;
    private float originalWidth;
    private bool isUpgradeable = false;
    private Coroutine currentAnimation;
    private MonoBehaviour owner;
    private UpgradeSettings settings;
    private bool isHovering = false;
    
    public bool IsUpgradeable => isUpgradeable;
    
    public void Initialize(MonoBehaviour mono, UpgradeSettings upgradeSettings){
        owner = mono;
        settings = upgradeSettings;
        
        if(button != null){
            originalWidth = button.Size.X.Value;
            
            button.AddGestureHandler<Gesture.OnPress>((evt) => {
                if(isUpgradeable && owner != null){
                    var upgradeUI = owner as UpgradeUI;
                    if(upgradeUI != null) upgradeUI.OnSkillPressed(skillIndex);
                }
            });
            button.AddGestureHandler<Gesture.OnHover>(OnButtonHovered);
            button.AddGestureHandler<Gesture.OnUnhover>(OnButtonUnhovered);
        }
    }
    
    public void SetSkillIndex(int index) => skillIndex = index;
    
    public void UpdateUpgradeableStatus(SkillManager manager, int playerMoney){
        if(button == null || manager == null) return;
        
        bool canUpgrade = false;
        int cost = 0;
        int currentLevel = 0;
        
        switch(skillIndex){
            case 0:
                currentLevel = manager.GetSkillLevel(SkillManager.SkillType.SleepyMode);
                cost = manager.GetUpgradeCost(SkillManager.SkillType.SleepyMode);
                canUpgrade = manager.CanUpgrade(SkillManager.SkillType.SleepyMode) && playerMoney >= cost;
                break;
            case 1:
                currentLevel = manager.GetSkillLevel(SkillManager.SkillType.GreenWaveAddiction);
                cost = manager.GetUpgradeCost(SkillManager.SkillType.GreenWaveAddiction);
                canUpgrade = manager.CanUpgrade(SkillManager.SkillType.GreenWaveAddiction) && playerMoney >= cost;
                break;
            case 2:
                currentLevel = manager.GetSkillLevel(SkillManager.SkillType.OopsieRecoverySystem);
                cost = manager.GetUpgradeCost(SkillManager.SkillType.OopsieRecoverySystem);
                canUpgrade = manager.CanUpgrade(SkillManager.SkillType.OopsieRecoverySystem) && playerMoney >= cost;
                break;
        }
        
        isUpgradeable = canUpgrade;
        
        if(costText != null){
            string formattedCost = FormatMoney(cost);
            string levelColor = canUpgrade ? "#A3A3A3" : "#666666";
            string costColor = canUpgrade ? "#FFFFFF" : "#888888";
            costText.Text = $"<color={levelColor}>Lv.{currentLevel}</color>\n<color={costColor}>{formattedCost}</color>";
        }
        
        if(currentAnimation == null){
            if(isHovering && isUpgradeable){
                button.Color = settings.hoverColor;
                button.Size.X.Value = originalWidth + settings.hoverSizeIncrease;
            }else{
                button.Color = canUpgrade ? settings.upgradeableColor : settings.notUpgradeableColor;
                if(!canUpgrade && button.Size.X.Value != originalWidth) button.Size.X.Value = originalWidth;
            }
        }
    }
    
    void OnButtonHovered(Gesture.OnHover evt){
        if(button == null || owner == null || !isUpgradeable) return;
        
        isHovering = true;
        
        if(currentAnimation != null) owner.StopCoroutine(currentAnimation);
        currentAnimation = owner.StartCoroutine(AnimateButton(true));
    }
    
    void OnButtonUnhovered(Gesture.OnUnhover evt){
        if(button == null || owner == null) return;
        
        isHovering = false;
        
        if(currentAnimation != null) owner.StopCoroutine(currentAnimation);
        
        if(isUpgradeable) currentAnimation = owner.StartCoroutine(AnimateButton(false));
        else{
            button.Size.X.Value = originalWidth;
            button.Color = settings.notUpgradeableColor;
        }
    }
    
    public void PlayClickAnimation(){
        if(button == null || owner == null || !isUpgradeable) return;
        
        if(currentAnimation != null) owner.StopCoroutine(currentAnimation);
        currentAnimation = owner.StartCoroutine(PlayClickAnimationRoutine());
    }
    
    public void ResetSizeIfNotUpgradeable(){
        if(!isUpgradeable && button != null && button.Size.X.Value != originalWidth){
            if(currentAnimation != null) owner.StopCoroutine(currentAnimation);
            button.Size.X.Value = originalWidth;
            button.Color = settings.notUpgradeableColor;
        }
    }
    
    IEnumerator AnimateButton(bool isHover){
        if(button == null) yield break;
        
        float targetWidth = isHover ? originalWidth + settings.hoverSizeIncrease : originalWidth;
        Color targetColor = isHover ? settings.hoverColor : settings.upgradeableColor;
        
        float startWidth = button.Size.X.Value;
        Color startColor = button.Color;
        float elapsed = 0f;
        
        while(elapsed < 1f){
            elapsed += Time.deltaTime * settings.animationSpeed;
            float t = Mathf.Clamp01(elapsed);
            
            button.Size.X.Value = Mathf.Lerp(startWidth, targetWidth, t);
            button.Color = Color.Lerp(startColor, targetColor, t);
            
            yield return null;
        }
        
        button.Size.X.Value = targetWidth;
        button.Color = targetColor;
        currentAnimation = null;
    }
    
    IEnumerator PlayClickAnimationRoutine(){
        if(button == null) yield break;
        
        Color startColor = button.Color;
        float originalWid = button.Size.X.Value;
        float expandedWidth = originalWid + settings.clickSizeIncrease;
        
        float elapsed = 0f;
        while(elapsed < settings.clickDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / settings.clickDuration;
            button.Size.X.Value = Mathf.Lerp(originalWid, expandedWidth, t);
            button.Color = Color.Lerp(startColor, settings.clickColor, t);
            yield return null;
        }
        
        button.Size.X.Value = expandedWidth;
        button.Color = settings.clickColor;
        
        elapsed = 0f;
        while(elapsed < settings.clickDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / settings.clickDuration;
            button.Size.X.Value = Mathf.Lerp(expandedWidth, originalWid, t);
            button.Color = Color.Lerp(settings.clickColor, startColor, t);
            yield return null;
        }
        
        button.Size.X.Value = originalWid;
        button.Color = startColor;
        currentAnimation = null;
    }
    
    public static string FormatMoney(int amount){
        if(amount >= 10000){
            int rounded = Mathf.RoundToInt(amount / 1000f);
            return $"{rounded}K";
        }
        return amount.ToString();
    }
}