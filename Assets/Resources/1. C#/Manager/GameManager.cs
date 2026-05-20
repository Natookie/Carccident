using UnityEngine;
using System.Collections;
using NaughtyAttributes;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    [Header("STATUS")]
    [ReadOnly] public int carPassed;
    [ReadOnly] public int carCollided;

    [Header("STATE")]
    [ReadOnly] public bool isGameInitialized = false;
    [ReadOnly] public bool isGameOver = false;

    [Header("REFERENCES")]
    [SerializeField] ScoreUI scoreUI;
    [SerializeField] CameraControl cameraControl;
    [SerializeField] MainButtonUI MBUI;

    [Header("BATTERY")]
    [ReadOnly] public float currentBattery;
    [ReadOnly] public float maxBattery;

    [SerializeField] private float baseBattery = 100f;
    [SerializeField] private float batteryPerLevel = 15f;
    [SerializeField] private float batteryDrainPerSecond = 1f;

    [Header("GAME RULES")]
    [SerializeField] private int maxCollision = 3;
    [SerializeField] private bool showDebug = false;

    [Header("REWARD SETTINGS")]
    [SerializeField] private int baseMoneyMultiplier = 3;
    [SerializeField] private int baseCollisionPenalty = 5;

    [Header("FINAL RESULT")]
    [ReadOnly] public int finalScore;
    [ReadOnly] public int finalPrize;
    [ReadOnly] public int timeBonus;
    [ReadOnly] public int totalPenalty;
    [ReadOnly] public float greenWaveMultiplier;
    [ReadOnly] public int oopsiePenaltyPerCrash;

    [Header("DEBUG SIMULATION")]
    [SerializeField] private float debugElapsedTime = 120f;
    [SerializeField] private int debugCarsPassed = 30;
    [SerializeField] private int debugCarsCollided = 2;

    private float shiftTime;

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start(){
        SetupBattery();
        isGameInitialized = false;
    }

    void Update(){
        HandleDebugHotKey();
        if(Input.GetKeyDown(KeyCode.Escape) && !TrafficUI.Instance.panel.gameObject.activeSelf){
            if(isGameInitialized && !isGameOver) PauseUI.Instance.TogglePause();
        }

        if(!isGameInitialized || isGameOver) return;
        if(carCollided >= maxCollision) GameOver();

        shiftTime += Time.deltaTime;
        DrainBattery();
    }

    #region GAME STATE LOGIC
    public void PlayGame(){
        isGameInitialized = true;
        AudioManager.Instance.PlayStartButton();
        HUDUI.Instance.HideAllMenuUI();
    }

    public void OnAccident(){
        if(isGameOver) return;

        carCollided++;
        if(AudioManager.Instance != null) AudioManager.Instance.PlayCrash();
    }

    public void OnCarPassed(){
        if(isGameOver) return;

        carPassed++;
        if(AudioManager.Instance != null) AudioManager.Instance.PlayCarPass();
    }

    public void GameOver(){
        if(isGameOver || !isGameInitialized) return;
        isGameOver = true;
        MBUI.ResetMainButton();
        CalculateReward();
        SetupBattery();

        SaveManager.AddMoney(finalPrize);
        SaveManager.AddTotalCarsPassed(carPassed);
        SaveManager.AddTotalCollisions(carCollided);

        if(scoreUI != null) scoreUI.DisplayScore();
        if(AudioManager.Instance != null) AudioManager.Instance.PlayGameOver();

        if(showDebug){
            Debug.Log("Game Over");
            Debug.Log("Cars Passed This Run: " + carPassed);
            Debug.Log("Collisions This Run: " + carCollided);
            Debug.Log("Final Score: " + finalScore);
            Debug.Log("Final Prize: " + finalPrize);
            Debug.Log("Total Cars Passed: " + SaveManager.GetTotalCarsPassed());
            Debug.Log("Total Collisions: " + SaveManager.GetTotalCollisions());
        }

        HUDUI.Instance.HideAllMenuUI();
    }

    public void ResetState(){
        isGameInitialized = false;
        isGameOver = false;
        CarManager.Instance.ClearAllCars();
    }

    void CalculateReward(){
        float greenWaveMultiplier = GetGreenWaveMultiplier();
        int oopsiePenaltyPerCrash = GetOopsiePenaltyPerCrash();
        
        int baseScore = carPassed * 10;
        int timeBonus = Mathf.FloorToInt(shiftTime / 2f);
        
        int bonus = Mathf.RoundToInt(carPassed * greenWaveMultiplier);
        int penalty = carCollided * oopsiePenaltyPerCrash;
        
        int finalScore = baseScore + timeBonus + bonus - penalty;
        finalScore = Mathf.Max(0, finalScore);
        
        finalPrize = finalScore * baseMoneyMultiplier;
        
        this.finalScore = finalScore;
        this.timeBonus = timeBonus;
        this.totalPenalty = penalty;
        
        //Debug.Log($"Cars Passed (Base): {carPassed} → {baseScore}pts, Time Bonus: +{timeBonus}, Green Wave: +{bonus}, Penalty: -{penalty}, Final: {finalScore}, Prize: ${finalPrize}");
    }

    public void ExitGame(){
        AudioManager.Instance.PlayButtonClick();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    #endregion

    #region BATTERY
    void SetupBattery(){
        int sleepyModeLevel = SaveManager.GetSleepyModeLevel();
        maxBattery = baseBattery + (sleepyModeLevel * batteryPerLevel);
        currentBattery = maxBattery;
    }

    void DrainBattery(){
        currentBattery -= batteryDrainPerSecond * Time.deltaTime;

        if(currentBattery <= 0){
            currentBattery = 0;
            GameOver();
        }
    }
    #endregion

    #region PUBLIC GETTERS
    public float GetCurrentBattery() => currentBattery;
    public float GetMaxBattery() => maxBattery;
    public float GetCurrentShiftTime() => shiftTime;
    public int GetFinalScore() => finalScore;
    public int GetFinalPrize() => finalPrize;
    public int GetTimeBonus() => timeBonus;
    public int GetTotalPenalty() => totalPenalty;

    public float GetGreenWaveMultiplier(){
        int greenWaveLevel = SaveManager.GetGreenWaveLevel();
        return 1f + (greenWaveLevel * 0.015f);
    }

    public int GetOopsiePenaltyPerCrash(){
        int oopsieRecoveryLevel = SaveManager.GetOopsieRecoveryLevel();
        
        if(oopsieRecoveryLevel >= 50) return 0;
        
        float penalty = baseCollisionPenalty - (oopsieRecoveryLevel * 0.12f);
        return Mathf.Max(0, Mathf.RoundToInt(penalty));
    }
    #endregion

    #region DEBUG SIMULATION
    [Button("Calculate Debug Reward", EButtonEnableMode.Editor)]
    void CalculateDebugReward(){
        float tempShiftTime = debugElapsedTime;
        int tempCarPassed = debugCarsPassed;
        int tempCarCollided = debugCarsCollided;
        
        float greenWaveMultiplier = GetGreenWaveMultiplier();
        int oopsiePenaltyPerCrash = GetOopsiePenaltyPerCrash();
        
        int baseScore = Mathf.FloorToInt(tempShiftTime);
        int bonus = Mathf.RoundToInt(tempCarPassed * greenWaveMultiplier);
        int penalty = tempCarCollided * oopsiePenaltyPerCrash;
        
        int finalScore = baseScore + bonus - penalty;
        finalScore = Mathf.Max(0, finalScore);
        
        int finalPrize = finalScore * baseMoneyMultiplier;
        
        Debug.Log("=== DEBUG REWARD CALCULATION ===");
        Debug.Log($"Upgrade Multipliers:");
        Debug.Log($"  Green Wave Level: {SaveManager.GetGreenWaveLevel()} → Multiplier: x{greenWaveMultiplier:F2}");
        Debug.Log($"  Oopsie Recovery Level: {SaveManager.GetOopsieRecoveryLevel()} → Penalty per crash: {oopsiePenaltyPerCrash}");
        Debug.Log($"  Sleepy Mode Level: {SaveManager.GetSleepyModeLevel()} → Max Battery: {baseBattery + (SaveManager.GetSleepyModeLevel() * batteryPerLevel)}");
        Debug.Log($"");
        Debug.Log($"Calculation:");
        Debug.Log($"  Base Score (Time): {baseScore}");
        Debug.Log($"  Bonus ({tempCarPassed} x {greenWaveMultiplier:F2}): +{bonus}");
        Debug.Log($"  Penalty ({tempCarCollided} x {oopsiePenaltyPerCrash}): -{penalty}");
        Debug.Log($"  Final Score: {baseScore} + {bonus} - {penalty} = {finalScore}");
        Debug.Log($"  Final Prize ({finalScore} x {baseMoneyMultiplier}): ${finalPrize}");
        Debug.Log($"");
        Debug.Log($"Potential Earnings:");
        Debug.Log($"  Money per second: ${finalPrize / tempShiftTime:F2}/s");
        Debug.Log($"  Money per car passed: ${finalPrize / tempCarPassed:F2}/car");
        Debug.Log("================================\n");
    }
    
    string FormatTime(float seconds){
        if(seconds < 60) return $"{Mathf.FloorToInt(seconds)} seconds";
        else if(seconds < 3600){
            int minutes = Mathf.FloorToInt(seconds / 60);
            int remainingSeconds = Mathf.FloorToInt(seconds % 60);
            return $"{minutes} minute(s) and {remainingSeconds} second(s)";
        }
        else{
            int hours = Mathf.FloorToInt(seconds / 3600);
            int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
            return $"{hours} hour(s) and {minutes} minute(s)";
        }
    }
    #endregion

    #region DEBUG HOTKEY
    void HandleDebugHotKey(){
        #if UNITY_EDITOR
        bool isCtrlClicked = Input.GetKey(KeyCode.LeftControl);
        if(!isCtrlClicked) return;

        if(Input.GetKeyDown(KeyCode.Q)){
            OnCarPassed();
            Debug.Log("[GM]Passed: " + carPassed);
        }
        if(Input.GetKeyDown(KeyCode.E)){
            OnAccident();
            cameraControl.ShakeCamera();
            Debug.Log("[GM]Collided: " + carCollided);
        }
        if(Input.GetKeyDown(KeyCode.G)){
            GameOver();
            Debug.Log("[GM]Gameover");
        }
        if(Input.GetKeyDown(KeyCode.M)){
            SaveManager.SetMoney(99999);
            Debug.Log("[GM]Money");
        }
        if(Input.GetKeyDown(KeyCode.C)){
            cameraControl.ShakeCamera();
            Debug.Log("[GM]Money");
        }
        #endif
    }
    #endregion
}