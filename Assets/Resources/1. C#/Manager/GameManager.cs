using UnityEngine;
using NaughtyAttributes;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    
    [Header("STATUS")]
    [ReadOnly] public int carPassed;
    [ReadOnly] public int carCollided;
    [ReadOnly] public float shiftTime;
    [Space(10)]
    [ReadOnly] private int GWLevel = 1;
    [ReadOnly] private int ORLevel = 1;

    [Header("STATE")]
    [ReadOnly] public bool isGameInitialized = false;
    [ReadOnly] public bool isGameOver = false;
    [ReadOnly] public bool endByPaused = false;

    [Header("REFERENCES")]
    [SerializeField] private CameraControl cameraControl;
    [SerializeField] private MainButtonUI MBUI;
    [SerializeField] private BatterySystem batterySystem;
    [SerializeField] private ScoreCalculator scoreCalculator;

    [Header("GAME RULES")]
    [SerializeField] private int maxCollision = 3;

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start(){
        isGameInitialized = false;
        StartCoroutine(batterySystem.SetUpBattery(true));
    }

    void Update(){
        HandleDebugHotKey();
        
        bool canPause = Input.GetKeyDown(KeyCode.Escape) && !TrafficUI.Instance.panel.gameObject.activeSelf;
        if(canPause && IsGameActive()) PauseUI.Instance.TogglePause();

        if(!isGameInitialized || isGameOver) return;
        if(carCollided >= maxCollision) GameOver();

        shiftTime += Time.deltaTime;
    }

    #region GAME STATE LOGIC
    public void PlayGame(){
        isGameInitialized = true;
        AudioManager.Instance.PlayStartButton();
        HUDUI.Instance.HideAllMenuUI();

        ResetValue();
        batterySystem.Initialize();
    }

    public void OnAccident(){
        if(isGameOver) return;

        carCollided++;
        if(AudioManager.Instance != null) AudioManager.Instance.PlayCrash();
        if(cameraControl != null) cameraControl.ShakeCamera();
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
        
        CalculateAndApplyReward();
        StartCoroutine(batterySystem.SetUpBattery(false));
        
        if(AudioManager.Instance != null) AudioManager.Instance.PlayGameOver();
        HUDUI.Instance.HideAllMenuUI();
    }

    void CalculateAndApplyReward(){
        ScoreCalculator.ScoreInput input = new ScoreCalculator.ScoreInput{
            shiftTime = shiftTime,
            carsPassed = carPassed,
            carsCollided = carCollided,
            greenWaveLevel = GWLevel,
            oopsieRecoveryLevel = ORLevel,
            endedByPause = endByPaused
        };
        
        scoreCalculator.CalculateReward(input, false);
        SaveManager.AddTotalCarsPassed(carPassed);
        SaveManager.AddTotalCollisions(carCollided);
    }

    public void ResetState(){
        isGameInitialized = false;
        isGameOver = false;
        
        if(CarManager.Instance != null) CarManager.Instance.ClearAllCars();
    }

    public void ExitGame(){
        if(AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    #endregion

    void ResetValue(){
        shiftTime = 0f;
        carPassed = 0;
        carCollided = 0;

        GWLevel = SaveManager.GetGreenWaveLevel();
        ORLevel = SaveManager.GetOopsieRecoveryLevel();

        endByPaused = false;
    }

    #region DEBUG
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
            Debug.Log("[GM]Money set to 99999");
        }
        #endif
    }
    #endregion

    public bool IsGameActive() => isGameInitialized && !isGameOver;
}