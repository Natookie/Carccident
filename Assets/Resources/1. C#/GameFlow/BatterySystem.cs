using UnityEngine;
using System.Collections;
using NaughtyAttributes;

public class BatterySystem : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private BatteryUI batteryUI;
    
    [Header("BATTERY SETTINGS")]
    [SerializeField] private float baseBattery = 100f;
    [SerializeField] private float batteryPerLevel = 15f;
    [SerializeField] private float batteryFillDuration = 1.2f;
    [ReadOnly] public float currentBattery;
    [ReadOnly] public float maxBattery;
    
    void Update() => DrainBattery();

    #region BATTERY LOGIC
    public IEnumerator SetUpBattery(bool isFirstTime){
        int sleepyModeLevel = SaveManager.GetSleepyModeLevel();
        maxBattery = baseBattery + (sleepyModeLevel * batteryPerLevel);
        
        if(isFirstTime){
            currentBattery = maxBattery;
            batteryUI?.Initialize(maxBattery);
            batteryUI?.SetBattery(currentBattery);
            yield return StartCoroutine(AnimateBatteryFill(batteryFillDuration));
        }
        else{
            currentBattery = 0;
            batteryUI?.Initialize(maxBattery);
            batteryUI?.SetBattery(currentBattery);
            yield return StartCoroutine(AnimateBatteryFill(batteryFillDuration));
        }
    }

    IEnumerator AnimateBatteryFill(float duration){
        float startBattery = currentBattery;
        float targetBattery = maxBattery;
        float elapsed = 0f;
        
        while(elapsed < duration){
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 2f);
            currentBattery = Mathf.Lerp(startBattery, targetBattery, t);
            batteryUI?.SetBattery(currentBattery);
            yield return null;
        }
        
        currentBattery = targetBattery;
        batteryUI?.SetBattery(currentBattery);
    }
    
    void DrainBattery(){
        if(!GameManager.Instance.IsGameActive()) return;
        
        currentBattery -= Time.deltaTime;
        batteryUI.SetBattery(currentBattery);
        
        if(currentBattery <= 0){
            currentBattery = 0;
            OnBatteryEmpty();
        }
    }
    
    void OnBatteryEmpty(){
        if(GameManager.Instance != null && !GameManager.Instance.isGameOver && GameManager.Instance.isGameInitialized)
            GameManager.Instance.GameOver();
    }
    
    #endregion
    
    #region PUBLIC METHODS
    public void Initialize(){
        int sleepyModeLevel = SaveManager.GetSleepyModeLevel();
        maxBattery = baseBattery + (sleepyModeLevel * batteryPerLevel);
        currentBattery = maxBattery;
        batteryUI?.Initialize(maxBattery);
        batteryUI?.SetBattery(currentBattery);
    }

    public void ResetBattery(){
        currentBattery = 0;
        maxBattery = 0;
    }
    
    [Button("Empty Current Battery", EButtonEnableMode.Playmode)]
    public void SetCurrentBattery(){
        if(!GameManager.Instance.IsGameActive()) return;
        batteryUI.SetBattery(currentBattery = 0);
    }
    
    public float GetCurrentBattery() => currentBattery;
    public float GetMaxBattery() => maxBattery;
    #endregion
}