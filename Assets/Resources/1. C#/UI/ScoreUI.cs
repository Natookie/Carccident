using UnityEngine;
using Nova;
using System;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextBlock timeText;

    [Header("SCORE")]
    [SerializeField] private UIBlock2D scorePanel;
    [Space(10)]
    [SerializeField] private TextBlock baseScoreText;
    [SerializeField] private TextBlock deductionScoreText;
    [SerializeField] private TextBlock bonusScoreText;
    [SerializeField] private TextBlock multiplierScoreText;

    [Header("STATUS")]
    [SerializeField] private TextBlock elapsedTimeText;
    [SerializeField] private TextBlock carPassedText;
    [SerializeField] private TextBlock carCollidedText;

    void Start(){
        if(baseScoreText == null) Debug.LogError("Base score is missing!");
        if(deductionScoreText == null) Debug.LogError("Deduction score is missing!");
        if(bonusScoreText == null) Debug.LogError("Bonus score is missing!");
        if(multiplierScoreText == null) Debug.LogError("Multiplier score is missing!");

        EnableDisplay(false);
    }

    void Update() => DisplayNightTime();

    void DisplayNightTime(){
        DateTime now = DateTime.Now;
        
        int displayHour = now.Hour;
        string ampm = "AM";
        
        if(now.Hour >= 22 || now.Hour <= 3) displayHour = now.Hour;
        else{
            displayHour = (now.Hour + 12) % 24;
            if(displayHour < 10) displayHour += 12;
        }
        
        if(displayHour >= 22){
            ampm = "PM";
            displayHour = displayHour == 22 ? 10 : displayHour == 23 ? 11 : displayHour;
        }
        else if(displayHour <= 3){
            ampm = "AM";
            displayHour = displayHour == 0 ? 12 : displayHour == 1 ? 1 : displayHour == 2 ? 2 : 3;
        }
        
        string dateFormatted = now.ToString("dd-MM-yy");
        timeText.Text = $"{dateFormatted} - {displayHour:D2}:{now.Minute:D2} {ampm}";
    }

    public void DisplayScore(float elapsed, float accidentCounter, float passedCounter){
        EnableDisplay(true);

        float baseScore = elapsed;
        float afterDeduction = baseScore - (accidentCounter * 15f);
        float afterBonus = baseScore + (passedCounter * 20f);

        //Display Status
        carCollidedText.Text = accidentCounter.ToString();
        carPassedText.Text = passedCounter.ToString();
        elapsedTimeText.Text = $"{elapsed.ToString():F2}";

        //Display Score
        baseScoreText.Text = $"${baseScore:F2}";
        bonusScoreText.Text = afterBonus.ToString();
        deductionScoreText.Text = afterDeduction.ToString();
        multiplierScoreText.Text = $"x1.0";
    }

    void EnableDisplay(bool value) => scorePanel.gameObject.SetActive(value);
}