using UnityEngine;
using NaughtyAttributes;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    [ReadOnly] public float score = 0f;

    [Header("REFERENCES")]
    public ScoreUI scoreUI;

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update(){
        HandleScore();
    }

    void HandleScore(){
        score += Time.deltaTime;
        scoreUI.ChangeScore(score);
    }

    public void OnAccident(int carID1, int carID2){
        score = Mathf.Max(0, score - 5f);
        //scoreUI.ShowAccidentFeedback();
    }
    
    public float GetCurrentShiftTime() => score;
}