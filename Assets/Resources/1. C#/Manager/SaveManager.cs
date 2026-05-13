using UnityEngine;

public static class SaveManager
{
    // =========================
    // SAVE KEYS
    // =========================

    private const string MoneyKey = "Money";
    private const string BestScoreKey = "BestScore";

    private const string StaminaLevelKey = "StaminaLevel";
    private const string CharismaLevelKey = "CharismaLevel";
    private const string EnduranceLevelKey = "EnduranceLevel";

    private const string TotalCarsPassedKey = "TotalCarsPassed";
    private const string TotalCollisionsKey = "TotalCollisions";

    // =========================
    // MONEY
    // =========================

    public static int GetMoney()
    {
        return PlayerPrefs.GetInt(MoneyKey, 0);
    }

    public static void SetMoney(int amount)
    {
        PlayerPrefs.SetInt(MoneyKey, amount);
        PlayerPrefs.Save();
    }

    public static void AddMoney(int amount)
    {
        int currentMoney = GetMoney();
        SetMoney(currentMoney + amount);
    }

    public static bool SpendMoney(int amount)
    {
        int currentMoney = GetMoney();

        if (currentMoney >= amount)
        {
            SetMoney(currentMoney - amount);
            return true;
        }

        return false;
    }

    // =========================
    // BEST SCORE
    // =========================

    public static int GetBestScore()
    {
        return PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    public static void SaveBestScore(int score)
    {
        int currentBestScore = GetBestScore();

        if (score > currentBestScore)
        {
            PlayerPrefs.SetInt(BestScoreKey, score);
            PlayerPrefs.Save();
        }
    }

    // =========================
    // UPGRADE LEVELS
    // =========================

    public static int GetStaminaLevel()
    {
        return PlayerPrefs.GetInt(StaminaLevelKey, 1);
    }

    public static void SetStaminaLevel(int level)
    {
        PlayerPrefs.SetInt(StaminaLevelKey, level);
        PlayerPrefs.Save();
    }

    public static int GetCharismaLevel()
    {
        return PlayerPrefs.GetInt(CharismaLevelKey, 1);
    }

    public static void SetCharismaLevel(int level)
    {
        PlayerPrefs.SetInt(CharismaLevelKey, level);
        PlayerPrefs.Save();
    }

    public static int GetEnduranceLevel()
    {
        return PlayerPrefs.GetInt(EnduranceLevelKey, 1);
    }

    public static void SetEnduranceLevel(int level)
    {
        PlayerPrefs.SetInt(EnduranceLevelKey, level);
        PlayerPrefs.Save();
    }

    // =========================
    // TOTAL CARS PASSED
    // =========================

    public static int GetTotalCarsPassed()
    {
        return PlayerPrefs.GetInt(TotalCarsPassedKey, 0);
    }

    public static void SetTotalCarsPassed(int amount)
    {
        PlayerPrefs.SetInt(TotalCarsPassedKey, amount);
        PlayerPrefs.Save();
    }

    public static void AddTotalCarsPassed(int amount)
    {
        int currentTotal = GetTotalCarsPassed();
        SetTotalCarsPassed(currentTotal + amount);
    }

    // =========================
    // TOTAL COLLISIONS
    // =========================

    public static int GetTotalCollisions()
    {
        return PlayerPrefs.GetInt(TotalCollisionsKey, 0);
    }

    public static void SetTotalCollisions(int amount)
    {
        PlayerPrefs.SetInt(TotalCollisionsKey, amount);
        PlayerPrefs.Save();
    }

    public static void AddTotalCollisions(int amount)
    {
        int currentTotal = GetTotalCollisions();
        SetTotalCollisions(currentTotal + amount);
    }

    // =========================
    // RESET SAVE
    // =========================

    public static void ResetSave()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("Save data has been reset.");
    }
}