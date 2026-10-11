using UnityEngine;
using System.Text;
using NaughtyAttributes;
using UnityEngine.SocialPlatforms.Impl;


#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

public class ScoreCalculator : MonoBehaviour
{
    [Header("REWARD SETTINGS")]
    [SerializeField] private float baseMoneyMultiplier = 1.5f;
    [SerializeField] private int baseCollisionPenalty = 25;
    [SerializeField] private int baseCarPassedMultiplier = 10;
    
    [Header("REFERENCES")]
    [SerializeField] private ScoreUI scoreUI;

    [Header("DEBUG SETTINGS")]
    [SerializeField] private float debugElapsedTime = 120f;
    [SerializeField] private int debugCarsPassed = 30;
    [SerializeField] private int debugCarsCollided = 2;
    [Space(10)]
    [SerializeField][Range(1, 99)] private int debugGWLevel = 1;
    [SerializeField][Range(1, 99)] private int debugORLevel = 1;
    [Space(10)]
    [SerializeField] private bool debugEndByPaused = false;
    [ResizableTextArea, ReadOnly] public string debugLog;
    
    private const float DIVIDED_BY_SECOND = 15f;
    private const float EARLY_PENALTY = 0.6f;
    
    [System.Serializable]
    public class ScoreInput
    {
        public float shiftTime;
        public int carsPassed;
        public int carsCollided;
        public int greenWaveLevel;
        public int oopsieRecoveryLevel;
        public bool endedByPause;
    }
    
    [System.Serializable]
    public class ScoreResult
    {
        public int baseScore;
        public int passedBonus;
        public int totalPenalty;
        public int finalScore;
        public int finalPrize;
        public float greenWaveMultiplier;
        public int oopsiePenaltyPerCrash;
        public string formula;
    }
    ScoreResult result = new ScoreResult();

    void Start() => scoreUI?.SetBaseMoneyMultiplier(baseMoneyMultiplier); 

    public void CalculateReward(ScoreInput input, bool useDebugValue = false){
        float greenWaveMultiplier = GetGreenWaveMultiplier(input.greenWaveLevel, useDebugValue);
        int oopsiePenaltyPerCrash = GetOopsiePenaltyPerCrash(input.oopsieRecoveryLevel, useDebugValue);
        
        result.greenWaveMultiplier = greenWaveMultiplier;
        result.oopsiePenaltyPerCrash = oopsiePenaltyPerCrash;
        
        result.baseScore = Mathf.CeilToInt(input.shiftTime / DIVIDED_BY_SECOND);
        result.passedBonus = input.carsPassed * baseCarPassedMultiplier + Mathf.RoundToInt(input.carsPassed * greenWaveMultiplier);
        result.totalPenalty = input.carsCollided * oopsiePenaltyPerCrash;
        
        result.finalScore = result.baseScore + result.passedBonus - result.totalPenalty;
        result.finalScore = Mathf.Max(0, result.finalScore);
        
        float lateMultiplier = input.endedByPause ? 1 - EARLY_PENALTY : 1f;
        result.finalPrize = Mathf.CeilToInt(result.finalScore * baseMoneyMultiplier * lateMultiplier);
        
        if(useDebugValue) BuildDebugString(result, input, greenWaveMultiplier, oopsiePenaltyPerCrash, lateMultiplier);
        if(scoreUI != null){
            scoreUI.SetScoreResult(result);
            if(!useDebugValue) scoreUI.DisplayScore();
        }
         
        debugLog = result.formula;
        SaveManager.AddMoney(result.finalPrize);
    }
    
    float GetGreenWaveMultiplier(int gwLevel, bool useDebugValue) => 1f + ((useDebugValue) ? debugGWLevel : gwLevel * 0.015f);
    int GetOopsiePenaltyPerCrash(int orLevel, bool useDebugValue) => Mathf.Max(0, Mathf.RoundToInt(baseCollisionPenalty - (((useDebugValue) ? debugORLevel : orLevel) * (baseCollisionPenalty / 99f))));
    
    void BuildDebugString(ScoreResult result, ScoreInput input, float greenWaveMultiplier, int oopsiePenaltyPerCrash, float lateMultiplier){
        Debug.Log($"<b>UPGRADE:</b>");
        Debug.Log($"GW: {input.greenWaveLevel} → Multiplier: x<color=red>{greenWaveMultiplier:F2}</color>");
        Debug.Log($"OR: {input.oopsieRecoveryLevel} → Penalty per crash: <color=red>{oopsiePenaltyPerCrash}</color>");
        Debug.Log($"<b>POTENTIAL:</b>");
        
        if(input.shiftTime > 0) Debug.Log($"Money/s: ${result.finalPrize / input.shiftTime:F2}/s");
        if(input.carsPassed > 0) Debug.Log($"Money/cp: ${result.finalPrize / input.carsPassed:F2}/car");
        
        StringBuilder sb = new StringBuilder("");
        sb.AppendLine($"  CALCULATION STEPS:");
        sb.AppendLine($"  Base Score = [{input.shiftTime:F1} / {DIVIDED_BY_SECOND}] = {result.baseScore}");
        sb.AppendLine($"  Pass Bonus = [{input.carsPassed} x {baseCarPassedMultiplier}] + [{input.carsPassed} x {greenWaveMultiplier:F2}] = {input.carsPassed * baseCarPassedMultiplier} + {Mathf.RoundToInt(input.carsPassed * greenWaveMultiplier)} = {result.passedBonus}");
        sb.AppendLine($"  Total Penalty = [{input.carsCollided} x {oopsiePenaltyPerCrash}] = {result.totalPenalty}");
        sb.AppendLine();
        sb.AppendLine($"  Final Score = [{result.baseScore} + {result.passedBonus} - {result.totalPenalty}] = {result.finalScore}");
        sb.AppendLine($"  Final Prize = [${result.finalScore} x {baseMoneyMultiplier} x {lateMultiplier}] = ${result.finalPrize}");
        
        if(input.endedByPause) sb.AppendLine($"  EARLY EXIT PENALTY: -{EARLY_PENALTY*100}%");
        result.formula = sb.ToString();
    }
    
    #region DEBUG METHODS
    [Button("Calculate Debug Reward", EButtonEnableMode.Editor)]
    void CalculateDebugReward(){
        ScoreInput input = new ScoreInput{
            shiftTime = debugElapsedTime,
            carsPassed = debugCarsPassed,
            carsCollided = debugCarsCollided,
            greenWaveLevel = debugGWLevel,
            oopsieRecoveryLevel = debugORLevel,
            endedByPause = debugEndByPaused
        };
        
        #if UNITY_EDITOR
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
        #endif
        
        CalculateReward(input, true);
    }
    #endregion
}