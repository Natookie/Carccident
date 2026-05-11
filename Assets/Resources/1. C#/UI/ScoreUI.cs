using UnityEngine;
using Nova;

public class ScoreUI : MonoBehaviour
{
    [Header("REFERENCES")]
    public TextBlock scoreText;

    public void ChangeScore(float newScore){
        if(scoreText != null) scoreText.Text = $"Score: {newScore:F2}";
    }
}