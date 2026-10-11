using UnityEngine;
using NaughtyAttributes;

public class SkillManager : MonoBehaviour
{
    [Header("SKILL LEVELS")]
    public int sleepyModeLevel = 1;
    public int greenWaveAddictionLevel = 1;
    public int oopsieRecoverySystemLevel = 1;
    
    [Header("COST SETTINGS")]
    [SerializeField] private int baseCost = 80;
    [SerializeField] private float costMultiplier = 1.45f;
    [SerializeField] private int maxLevel = 15;
    
    public enum SkillType{
        SleepyMode,
        GreenWaveAddiction,
        OopsieRecoverySystem
    }
    
    public int GetSkillLevel(SkillType type){
        switch(type){
            case SkillType.SleepyMode: return sleepyModeLevel;
            case SkillType.GreenWaveAddiction: return greenWaveAddictionLevel;
            case SkillType.OopsieRecoverySystem: return oopsieRecoverySystemLevel;
            default: return 1;
        }
    }
    
    public int GetUpgradeCost(SkillType type){
        int currentLevel = GetSkillLevel(type);
        if(currentLevel >= maxLevel) return int.MaxValue;
        
        float multiplier = 1f;
        switch(type){
            case SkillType.SleepyMode: multiplier = 0.7f; break;
            case SkillType.GreenWaveAddiction: multiplier = 1.4f; break;
            case SkillType.OopsieRecoverySystem: multiplier = 1.0f; break;
        }
        
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel - 1) * multiplier);
    }
    
    public bool CanAffordUpgrade(SkillType type, int playerMoney) => playerMoney >= GetUpgradeCost(type);
    public void AddLevel(SkillType type){
        switch(type){
            case SkillType.SleepyMode:
                if(sleepyModeLevel < maxLevel) sleepyModeLevel++;
                break;
            case SkillType.GreenWaveAddiction:
                if(greenWaveAddictionLevel < maxLevel) greenWaveAddictionLevel++;
                break;
            case SkillType.OopsieRecoverySystem:
                if(oopsieRecoverySystemLevel < maxLevel) oopsieRecoverySystemLevel++;
                break;
        }
    }
}