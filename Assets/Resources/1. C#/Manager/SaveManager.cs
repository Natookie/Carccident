using UnityEngine;

public static class SaveManager
{
    private const string MoneyKey = "Money";
    private const string BestScoreKey = "BestScore";

    private const string SleepyModeLevelKey = "SleepyModeLevel";
    private const string GreenWaveLevelKey = "GreenWaveLevel";
    private const string OopsieRecoveryLevelKey = "OopsieRecoveryLevel";

    private const string TotalCarsPassedKey = "TotalCarsPassed";
    private const string TotalCollisionsKey = "TotalCollisions";

    #region MONEY
    public static int GetMoney(){
        return PlayerPrefs.GetInt(MoneyKey, 0);
    }

    public static void SetMoney(int amount){
        PlayerPrefs.SetInt(MoneyKey, amount);
        PlayerPrefs.Save();
    }

    public static void AddMoney(int amount){
        int currentMoney = GetMoney();
        SetMoney(currentMoney + amount);
    }

    public static bool SpendMoney(int amount){
        int currentMoney = GetMoney();

        if(currentMoney >= amount){
            SetMoney(currentMoney - amount);
            return true;
        }

        return false;
    }
    #endregion

    #region BEST SCORE
    public static int GetBestScore(){
        return PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    public static void SaveBestScore(int score){
        int currentBestScore = GetBestScore();

        if(score > currentBestScore){
            PlayerPrefs.SetInt(BestScoreKey, score);
            PlayerPrefs.Save();
        }
    }
    #endregion

    #region UPGRADE LEVELS
    public static int GetSleepyModeLevel(){
        return PlayerPrefs.GetInt(SleepyModeLevelKey, 1);
    }

    public static void SetSleepyModeLevel(int level){
        PlayerPrefs.SetInt(SleepyModeLevelKey, level);
        PlayerPrefs.Save();
    }

    public static int GetGreenWaveLevel(){
        return PlayerPrefs.GetInt(GreenWaveLevelKey, 1);
    }

    public static void SetGreenWaveLevel(int level){
        PlayerPrefs.SetInt(GreenWaveLevelKey, level);
        PlayerPrefs.Save();
    }

    public static int GetOopsieRecoveryLevel(){
        return PlayerPrefs.GetInt(OopsieRecoveryLevelKey, 1);
    }

    public static void SetOopsieRecoveryLevel(int level){
        PlayerPrefs.SetInt(OopsieRecoveryLevelKey, level);
        PlayerPrefs.Save();
    }
    #endregion

    #region PASSED
    public static int GetTotalCarsPassed(){
        return PlayerPrefs.GetInt(TotalCarsPassedKey, 0);
    }

    public static void SetTotalCarsPassed(int amount){
        PlayerPrefs.SetInt(TotalCarsPassedKey, amount);
        PlayerPrefs.Save();
    }

    public static void AddTotalCarsPassed(int amount){
        int currentTotal = GetTotalCarsPassed();
        SetTotalCarsPassed(currentTotal + amount);
    }
    #endregion

    #region COLLISION
    public static int GetTotalCollisions(){
        return PlayerPrefs.GetInt(TotalCollisionsKey, 0);
    }

    public static void SetTotalCollisions(int amount){
        PlayerPrefs.SetInt(TotalCollisionsKey, amount);
        PlayerPrefs.Save();
    }

    public static void AddTotalCollisions(int amount){
        int currentTotal = GetTotalCollisions();
        SetTotalCollisions(currentTotal + amount);
    }
    #endregion

    public static void ResetSave(){
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("Save data has been reset.");
    }
}