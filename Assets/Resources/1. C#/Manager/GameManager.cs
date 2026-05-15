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

    [Header("BATTERY")]
    [ReadOnly] public float currentBattery;
    [ReadOnly] public float maxBattery;

    [SerializeField] private float baseBattery = 100f;
    [SerializeField] private float batteryPerLevel = 10f;
    [SerializeField] private float batteryDrainPerSecond = 1f;

    [Header("GAME RULES")]
    [SerializeField] private int maxCollision = 3;
    [SerializeField] private bool showDebug = false;

    [Header("REWARD SETTINGS")]
    [SerializeField] private int timeBonusDivider = 10;
    [SerializeField] private int baseMoneyMultiplier = 3;
    [SerializeField] private int baseCollisionPenalty = 5;

    [Header("FINAL RESULT")]
    [ReadOnly] public int finalScore;
    [ReadOnly] public int finalPrize;
    [ReadOnly] public int timeBonus;
    [ReadOnly] public int totalPenalty;
    [ReadOnly] public float greenWaveMultiplier;
    [ReadOnly] public int oopsiePenaltyPerCrash;

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

        if(!isGameInitialized || isGameOver) return;
        if(carCollided >= maxCollision) GameOver();

        shiftTime += Time.deltaTime;
        DrainBattery();
    }

    #region GAME STATE LOGIC
    public void PlayGame(){
        isGameInitialized = true;
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

        CalculateReward();

        SaveManager.AddMoney(finalPrize);
        SaveManager.AddTotalCarsPassed(carPassed);
        SaveManager.AddTotalCollisions(carCollided);

        if(scoreUI != null) scoreUI.DisplayScore(shiftTime, carCollided, carPassed);
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

    void CalculateReward(){
        timeBonus = Mathf.FloorToInt(shiftTime / timeBonusDivider);
        greenWaveMultiplier = GetGreenWaveMultiplier();
        oopsiePenaltyPerCrash = GetOopsiePenaltyPerCrash();

        Debug.Log("green " + greenWaveMultiplier);
        Debug.Log("oopsie " + oopsiePenaltyPerCrash);

        finalScore = carPassed + timeBonus;
        totalPenalty = carCollided * oopsiePenaltyPerCrash;

        finalPrize = Mathf.RoundToInt(finalScore * greenWaveMultiplier * baseMoneyMultiplier);
        finalPrize -= totalPenalty;
        finalPrize = Mathf.Max(0, finalPrize);
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
        return 1f + (greenWaveLevel * 0.05f);
    }

    public int GetOopsiePenaltyPerCrash(){
        int oopsieRecoveryLevel = SaveManager.GetOopsieRecoveryLevel();

        float reduction = Mathf.Min(oopsieRecoveryLevel * 0.02f, 0.6f);
        float finalPenalty = baseCollisionPenalty * (1f - reduction);

        return Mathf.RoundToInt(finalPenalty);
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
        #endif
    }
    #endregion
}