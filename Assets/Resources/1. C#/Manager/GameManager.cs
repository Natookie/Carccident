using UnityEngine;
using NaughtyAttributes;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    [Header("STATUS")]
    [ReadOnly] public int carPassed;
    [ReadOnly] public int  carCollided;

    [Header("STATE")]
    [ReadOnly] public bool isGameInitialized = false;
    [ReadOnly] public bool isGameOver = false;

    [Header("REFERENCES")]
    [SerializeField] ScoreUI scoreUI;

    private float shiftTime;

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start(){
        isGameInitialized = false;
    }

    void Update(){
        shiftTime += Time.deltaTime;
    }

    #region TRAFFIC STATUS
    public void OnAccident() => carCollided++;
    public void OnCarPassed() => carPassed++;
    #endregion

    #region GAME STATE LOGIC
    public void PlayGame(){
        isGameInitialized = true;
        Debug.Log("duh");
    }

    public void GameOver(){
        if(isGameOver) return;
        isGameOver = true;

        scoreUI.DisplayScore(shiftTime, carCollided, carPassed);
        //Save high score to txt
        //Display Main menu
    }

    public void ExitGame(){
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    #endregion

    public float GetCurrentShiftTime() => shiftTime;
}