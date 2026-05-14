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
    
    [Header("UPGRADE SETTINGS")]
    public int maxLevel = 99;
    
    [Header("COST FORMULA")]
    [SerializeField] private int baseUpgradeCost = 50;
    [SerializeField] private float costPower = 1.3f;

    public void AddLevel(SkillType type){
        if(!CanUpgrade(type)) {
            Debug.Log($"Cannot upgrade {type} - max level reached!");
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
        return CalculateUpgradeCost(currentLevel);
    }
    
    int CalculateUpgradeCost(int currentLevel) => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(currentLevel, costPower));
    public bool CanAffordUpgrade(SkillType type, int playerMoney) => playerMoney >= GetUpgradeCost(type) && CanUpgrade(type);
    
    public int GetNextLevelCost(SkillType type){
        int nextLevel = GetSkillLevel(type) + 1;
        if(nextLevel > maxLevel) return 0;
        return CalculateUpgradeCost(nextLevel);
    }
}