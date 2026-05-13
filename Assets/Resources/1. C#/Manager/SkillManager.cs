using UnityEngine;
using NaughtyAttributes;

public class SkillManager : MonoBehaviour
{
    public enum SkillType
    {
        SleepyMode,
        GreenWaveAddiction,
        OopsieRecoverySystem
    }

    public int sleepyModeLevel;
    public int greenWaveAddictionLevel;
    public int OopsieRecoverySystemLevel;
    
    [Header("Upgrade Settings")]
    public int maxLevel = 10;
    public int[] upgradeCosts = new int[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };

    public void AddLevel(SkillType type){
        if(!CanUpgrade(type)) {
            Debug.Log($"Cannot upgrade {type} - max level reached or not enough money!");
            return;
        }
        
        switch(type){
            case SkillType.SleepyMode:
                sleepyModeLevel++;
                Debug.Log($"Sleepy Mode upgraded to level {sleepyModeLevel}");
                break;
            case SkillType.GreenWaveAddiction:
                greenWaveAddictionLevel++;
                Debug.Log($"Green Wave Addiction upgraded to level {greenWaveAddictionLevel}");
                break;
            case SkillType.OopsieRecoverySystem:
                OopsieRecoverySystemLevel++;
                Debug.Log($"Oopsie Recovery System upgraded to level {OopsieRecoverySystemLevel}");
                break;
        }
    }

    public bool CanUpgrade(SkillType type){
        int currentLevel = GetSkillLevel(type);
        return currentLevel < maxLevel;
    }
    
    public int GetSkillLevel(SkillType type){
        switch(type){
            case SkillType.SleepyMode: return sleepyModeLevel;
            case SkillType.GreenWaveAddiction: return greenWaveAddictionLevel;
            case SkillType.OopsieRecoverySystem: return OopsieRecoverySystemLevel;
            default: return 0;
        }
    }
    
    public int GetUpgradeCost(SkillType type){
        int currentLevel = GetSkillLevel(type);
        if(currentLevel >= upgradeCosts.Length) return upgradeCosts[upgradeCosts.Length - 1];
        return upgradeCosts[currentLevel];
    }
    
    public bool CanAffordUpgrade(SkillType type, int playerMoney) => playerMoney >= GetUpgradeCost(type) && CanUpgrade(type);
}