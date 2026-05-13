using Nova;
using UnityEngine;

public class UpgradeUI : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private SkillButton[] skillList;
    [SerializeField] private SkillManager skillManager;

    const int MONEY = 100000;

    void Awake(){
        for(int i = 0; i < skillList.Length; i++){
            skillList[i].SetSkillIndex(i);
            skillList[i].Initialize(this);
        }            
    }

    void Update(){
        foreach(SkillButton skill in skillList){
            skill.UpdateUpgradeableStatus(skillManager, MONEY);
        }
    }

    public void OnSkillPressed(int skillIndex){
        switch(skillIndex){
            case 0: skillManager.AddLevel(SkillManager.SkillType.SleepyMode); break;
            case 1: skillManager.AddLevel(SkillManager.SkillType.GreenWaveAddiction); break;
            case 2: skillManager.AddLevel(SkillManager.SkillType.OopsieRecoverySystem); break;
        }
    }
}

[System.Serializable]
public class SkillButton
{
    [SerializeField] private UIBlock2D button;
    [SerializeField] private TextBlock costText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color upgradeableColor = Color.green;
    [SerializeField] private Color notUpgradeableColor = Color.gray;
    
    private int skillIndex;
    
    public void Initialize(UpgradeUI ui){
        if(button != null){
            button.AddGestureHandler<Gesture.OnPress>((evt) => ui.OnSkillPressed(skillIndex));
            button.AddGestureHandler<Gesture.OnHover>(OnButtonHovered);
            button.AddGestureHandler<Gesture.OnUnhover>(OnButtonUnhovered);
        }
    }
    
    public void SetSkillIndex(int index) => skillIndex = index;
    public void UpdateUpgradeableStatus(SkillManager manager, int playerMoney){
        if(button == null || manager == null) return;
        
        bool canUpgrade = false;
        int cost = 0;
        
        switch(skillIndex){
            case 0:
                cost = manager.GetUpgradeCost(SkillManager.SkillType.SleepyMode);
                canUpgrade = manager.CanUpgrade(SkillManager.SkillType.SleepyMode) && playerMoney >= cost;
                break;
            case 1:
                cost = manager.GetUpgradeCost(SkillManager.SkillType.GreenWaveAddiction);
                canUpgrade = manager.CanUpgrade(SkillManager.SkillType.GreenWaveAddiction) && playerMoney >= cost;
                break;
            case 2:
                cost = manager.GetUpgradeCost(SkillManager.SkillType.OopsieRecoverySystem);
                canUpgrade = manager.CanUpgrade(SkillManager.SkillType.OopsieRecoverySystem) && playerMoney >= cost;
                break;
        }
        
        if(costText != null) costText.Text = FormatMoney(cost);
        
        if(canUpgrade) button.Color = upgradeableColor;
        else button.Color = notUpgradeableColor;
    }

    public static string FormatMoney(int amount){
        if(amount >= 10000){
            int rounded = Mathf.RoundToInt(amount / 1000f);
            return $"{rounded}K";
        }
        
        return amount.ToString();
    }
        
    void OnButtonHovered(Gesture.OnHover evt){
        Debug.Log($"Hover over skill {skillIndex}");
    }
    
    void OnButtonUnhovered(Gesture.OnUnhover evt){
        Debug.Log($"Unhover from skill {skillIndex}");
    }
}