using UnityEngine;
using NaughtyAttributes;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("STATUS")]
    [ReadOnly] public int carPassed;
    [ReadOnly] public int carCollided;

    [Header("STATE")]
    [ReadOnly] public bool isGameInitialized = false;
    [ReadOnly] public bool isGameOver = false;

    [Header("STAMINA")]
    [ReadOnly] public float currentStamina;
    [ReadOnly] public float maxStamina;

    [SerializeField] private float baseStamina = 100f;
    [SerializeField] private float staminaPerLevel = 10f;
    [SerializeField] private float staminaDrainPerSecond = 1f;

    [Header("GAME RULES")]
    [SerializeField] private int maxCollision = 3;

    [Header("REWARD SETTINGS")]
    [SerializeField] private int timeBonusDivider = 10;
    [SerializeField] private int baseMoneyMultiplier = 3;
    [SerializeField] private int baseCollisionPenalty = 5;

    [Header("FINAL RESULT")]
    [ReadOnly] public int finalScore;
    [ReadOnly] public int finalPrize;

    [Header("REFERENCES")]
    public ScoreUI scoreUI;
    [SerializeField] private ScoreUI scoreUI;

    private float shiftTime;

    void Awake(){
        if(Instance != null && Instance != this){
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update(){
        HandleScore();
    private void Start()
    {
        SetupStamina();
        isGameInitialized = true;
    }

    private void Update()
    {
        if (!isGameInitialized || isGameOver) return;

        shiftTime += Time.deltaTime;
        DrainStamina();
    }

    private void SetupStamina()
    {
        int staminaLevel = SaveManager.GetStaminaLevel();

        maxStamina = baseStamina + (staminaLevel * staminaPerLevel);
        currentStamina = maxStamina;
    }

    private void DrainStamina()
    {
        currentStamina -= staminaDrainPerSecond * Time.deltaTime;

        if (currentStamina <= 0)
        {
            currentStamina = 0;
            GameOver();
        }
    }

    public void OnAccident()
    {
        if (isGameOver) return;

        carCollided++;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCrash();
        }

        if (carCollided >= maxCollision)
        {
            GameOver();
        }
    }

    public void OnCarPassed()
    {
        if (isGameOver) return;

        carPassed++;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCarPass();
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        CalculateReward();

        SaveManager.AddMoney(finalPrize);
        SaveManager.SaveBestScore(finalScore);

        SaveManager.AddTotalCarsPassed(carPassed);
        SaveManager.AddTotalCollisions(carCollided);

        if (scoreUI != null)
        {
            scoreUI.DisplayScore(shiftTime, carCollided, carPassed);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOver();
        }

        Debug.Log("Game Over");
        Debug.Log("Cars Passed This Run: " + carPassed);
        Debug.Log("Collisions This Run: " + carCollided);
        Debug.Log("Final Score: " + finalScore);
        Debug.Log("Final Prize: " + finalPrize);
        Debug.Log("Total Cars Passed: " + SaveManager.GetTotalCarsPassed());
        Debug.Log("Total Collisions: " + SaveManager.GetTotalCollisions());
    }

    private void CalculateReward()
    {
        int timeBonus = Mathf.FloorToInt(shiftTime / timeBonusDivider);
        float charismaMultiplier = GetCharismaMultiplier();

        finalScore = carPassed + timeBonus;

        int collisionPenalty = GetFinalCollisionPenalty();
        int totalPenalty = carCollided * collisionPenalty;

        finalPrize = Mathf.RoundToInt(finalScore * charismaMultiplier * baseMoneyMultiplier);
        finalPrize -= totalPenalty;

        finalPrize = Mathf.Max(0, finalPrize);
    }

    private float GetCharismaMultiplier()
    {
        int charismaLevel = SaveManager.GetCharismaLevel();

        return 1f + (charismaLevel * 0.05f);
    }

    private int GetFinalCollisionPenalty()
    {
        int enduranceLevel = SaveManager.GetEnduranceLevel();

        float reduction = Mathf.Min(enduranceLevel * 0.02f, 0.6f);
        float finalPenalty = baseCollisionPenalty * (1f - reduction);

        return Mathf.RoundToInt(finalPenalty);
    }

    public float GetCurrentShiftTime()
    {
        return shiftTime;
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public float GetMaxStamina()
    {
        return maxStamina;
    }
}