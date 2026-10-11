using UnityEngine;
using Nova;
using System;
using System.Collections;
using System.Collections.Generic;

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

    [Header("ANIMATION BLOCKS")]
    [SerializeField] private UIBlock2D deductionScoreBlock;
    [SerializeField] private UIBlock2D bonusScoreBlock;
    [SerializeField] private UIBlock2D multiplierScoreBlock;
    [SerializeField] private UIBlock2D elapsedTimeBlock;
    [SerializeField] private UIBlock2D carPassedBlock;
    [SerializeField] private UIBlock2D carCollidedBlock;

    [Header("ANIMATION SETTINGS")]
    [SerializeField] private float popDuration = 0.2f;
    [SerializeField] private float sequenceDelay = 0.1f;
    [SerializeField] private float shakeAmount = 5f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float clockUpdateInterval = 1f;

    private Vector3 originalScorePanelScale;
    private Dictionary<UIBlock2D, Vector3> originalBlockScales = new Dictionary<UIBlock2D, Vector3>();
    private Dictionary<UIBlock2D, Vector3> originalBlockPositions = new Dictionary<UIBlock2D, Vector3>();
    private Dictionary<TextBlock, Vector3> originalTextPositions = new Dictionary<TextBlock, Vector3>();
    private Dictionary<TextBlock, Vector3> originalTextScales = new Dictionary<TextBlock, Vector3>();
    private Coroutine currentAnimation;
    private Coroutine clockCoroutine;
    
    private Vector3 originalBaseScorePos;
    private Vector3 originalBaseScoreScale;

    private float baseMoneyMultiplier = 0f;
    ScoreCalculator.ScoreResult result;
    
    private GameManager gm;

    void Start(){
        gm = GameManager.Instance;
        ValidateReferences();
        CacheOriginalValues();
        DisableAllBlocks();
        EnableDisplay(false);
        
        StartClock();
    }

    void OnDestroy(){
        if(clockCoroutine != null) StopCoroutine(clockCoroutine);
        if(currentAnimation != null) StopCoroutine(currentAnimation);
    }

    void ValidateReferences(){
        if(baseScoreText == null) Debug.LogError("[ScoreUI] Base score is missing!");
        if(deductionScoreText == null) Debug.LogError("[ScoreUI] Deduction score is missing!");
        if(bonusScoreText == null) Debug.LogError("[ScoreUI] Bonus score is missing!");
        if(multiplierScoreText == null) Debug.LogError("[ScoreUI] Multiplier score is missing!");
    }

    void StartClock(){
        if(clockCoroutine != null) StopCoroutine(clockCoroutine);
        clockCoroutine = StartCoroutine(UpdateClockRoutine());
    }

    IEnumerator UpdateClockRoutine(){
        while(true){
            if(timeText != null) timeText.Text = GetFormattedDateTime();
            yield return new WaitForSeconds(clockUpdateInterval);
        }
    }

    string GetFormattedDateTime(){
        DateTime now = DateTime.Now;
        string formattedDate = now.ToString("dd-MM-yy");
        string formattedTime = now.ToString("HH:mm:ss");
        return $"({formattedDate}) {formattedTime}";
    }

    void CacheOriginalValues(){
        if(scorePanel != null) originalScorePanelScale = scorePanel.transform.localScale;
        
        CacheBlock(elapsedTimeBlock);
        CacheBlock(carPassedBlock);
        CacheBlock(carCollidedBlock);
        CacheBlock(deductionScoreBlock);
        CacheBlock(bonusScoreBlock);
        CacheBlock(multiplierScoreBlock);
        
        CacheTextBlock(baseScoreText);
        CacheTextBlock(deductionScoreText);
        CacheTextBlock(bonusScoreText);
        CacheTextBlock(multiplierScoreText);
        CacheTextBlock(elapsedTimeText);
        CacheTextBlock(carPassedText);
        CacheTextBlock(carCollidedText);
        
        if(baseScoreText != null){
            RectTransform rect = baseScoreText.GetComponent<RectTransform>();
            if(rect != null){
                originalBaseScorePos = rect.anchoredPosition3D;
                originalBaseScoreScale = rect.localScale;
            }
        }
    }

    void CacheBlock(UIBlock2D block){
        if(block != null){
            originalBlockScales[block] = block.transform.localScale;
            originalBlockPositions[block] = block.Position.Value;
        }
    }
    
    void CacheTextBlock(TextBlock text){
        if(text != null){
            RectTransform rect = text.GetComponent<RectTransform>();
            if(rect != null){
                originalTextPositions[text] = rect.anchoredPosition3D;
                originalTextScales[text] = rect.localScale;
            }
        }
    }

    void DisableAllBlocks(){
        if(elapsedTimeBlock != null) elapsedTimeBlock.gameObject.SetActive(false);
        if(carPassedBlock != null) carPassedBlock.gameObject.SetActive(false);
        if(carCollidedBlock != null) carCollidedBlock.gameObject.SetActive(false);
        if(deductionScoreBlock != null) deductionScoreBlock.gameObject.SetActive(false);
        if(bonusScoreBlock != null) bonusScoreBlock.gameObject.SetActive(false);
        if(multiplierScoreBlock != null) multiplierScoreBlock.gameObject.SetActive(false);
    }

    string FormatTime(float seconds){
        if(seconds < 60) return $"{Mathf.FloorToInt(seconds)}s";
        
        int minutes = Mathf.FloorToInt(seconds / 60);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60);
        
        if(minutes < 60) return $"{minutes}m {remainingSeconds}s";
        
        int hours = Mathf.FloorToInt(minutes / 60);
        int remainingMinutes = minutes % 60;
        return $"{hours}h {remainingMinutes}m {remainingSeconds}s";
    }

    public void DisplayScore(){
        baseScoreText.Text = "[???]";
        EnableDisplay(true);
        
        if(currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(ScoreDisplaySequence());
    }

    IEnumerator ScoreDisplaySequence(){
        int carsPassed = gm.carPassed;
        int carsCollided = gm.carCollided;
        float shiftTime = gm.shiftTime;

        int finalScore = result.finalScore;
        int finalPrize = result.finalPrize;
        int passedBonus = result.passedBonus;
        int totalPenalty = result.totalPenalty;        
        yield return StartCoroutine(AnimatePop(scorePanel, originalScorePanelScale));
        
        //STEP 1: Elapsed Time
        elapsedTimeText.Text = FormatTime(shiftTime);
        yield return StartCoroutine(ActivateAndPop(elapsedTimeBlock));
        yield return new WaitForSeconds(sequenceDelay);
        
        //STEP 2: Cars Passed
        carPassedText.Text = carsPassed.ToString();
        yield return StartCoroutine(ActivateAndPop(carPassedBlock));
        yield return new WaitForSeconds(sequenceDelay);
        
        //STEP 3: Cars Collided
        carCollidedText.Text = carsCollided.ToString();
        yield return StartCoroutine(ActivateAndPop(carCollidedBlock));
        yield return new WaitForSeconds(sequenceDelay);
        
        //STEP 4: Bonus Score Block
        bonusScoreText.Text = $"+{passedBonus}";
        yield return StartCoroutine(ActivateAndPop(bonusScoreBlock));
        yield return new WaitForSeconds(sequenceDelay);
        
        baseScoreText.Text = $"[{finalScore + totalPenalty}]";
        yield return StartCoroutine(ShakeBaseScore());
        yield return new WaitForSeconds(sequenceDelay);
        
        //STEP 5: Deduction Score Block (Penalty)
        deductionScoreText.Text = $"-{totalPenalty}";
        yield return StartCoroutine(ActivateAndPop(deductionScoreBlock));
        yield return new WaitForSeconds(sequenceDelay);
        
        baseScoreText.Text = $"[{finalScore}]";
        yield return StartCoroutine(ShakeBaseScore());
        yield return new WaitForSeconds(sequenceDelay);
        
        //STEP 6: Multiplier Score Block (Money Multiplier)
        multiplierScoreText.Text = $"x{baseMoneyMultiplier}";
        yield return StartCoroutine(ActivateAndPop(multiplierScoreBlock));
        yield return new WaitForSeconds(sequenceDelay);
        
        baseScoreText.Text = $"${finalPrize}";
        yield return StartCoroutine(ShakeBaseScore());
        yield return new WaitForSeconds(sequenceDelay);
        
        //STEP 7: Save and cleanup
        CheckBestScore();
        yield return new WaitForSeconds(1.5f);
        EnableDisplay(false);
        
        currentAnimation = null;
        
        if(HUDUI.Instance != null) HUDUI.Instance.ShowAllMenuUI();
        if(GameManager.Instance != null) GameManager.Instance.ResetState();
    }

    void CheckBestScore(){
        if(gm == null) return;
        
        int bestScore = SaveManager.GetBestScore();
        int currentScore = result.finalScore;
        
        if(bestScore < currentScore) SaveManager.SaveBestScore(currentScore); 
    }

    IEnumerator ShakeBaseScore(){
        if(baseScoreText == null) yield break;
        
        RectTransform rect = baseScoreText.GetComponent<RectTransform>();
        if(rect == null) yield break;
        
        float shakeElapsed = 0f;
        
        while(shakeElapsed < shakeDuration){
            shakeElapsed += Time.deltaTime;
            float t = shakeElapsed / shakeDuration;
            float intensity = (1f - t) * shakeAmount;
            
            float shakeX = originalBaseScorePos.x + UnityEngine.Random.Range(-intensity, intensity);
            float shakeY = originalBaseScorePos.y + UnityEngine.Random.Range(-intensity, intensity);
            rect.anchoredPosition3D = new Vector3(shakeX, shakeY, originalBaseScorePos.z);
            
            float scaleMultiplier = 1f + (1f - t) * 0.15f;
            rect.localScale = originalBaseScoreScale * scaleMultiplier;
            
            yield return null;
        }
        
        rect.anchoredPosition3D = originalBaseScorePos;
        rect.localScale = originalBaseScoreScale;
        
        float elapsed = 0f;
        float popTime = popDuration / 2f;
        
        while(elapsed < popTime){
            elapsed += Time.deltaTime;
            float t = elapsed / popTime;
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
            rect.localScale = originalBaseScoreScale * scale;
            yield return null;
        }
        
        rect.localScale = originalBaseScoreScale;
    }

    IEnumerator ActivateAndPop(UIBlock2D block){
        if(block == null) yield break;
        
        block.gameObject.SetActive(true);
        if(!originalBlockScales.ContainsKey(block)) originalBlockScales[block] = Vector3.one;
        
        Vector3 originalScale = originalBlockScales[block];
        block.transform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        while(elapsed < popDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            
            float scale = 0f;
            if(t < 0.5f) scale = Mathf.Lerp(0f, 1.5f, t * 2f);
            else scale = Mathf.Lerp(1.5f, 1f, (t - 0.5f) * 2f);
            
            block.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        block.transform.localScale = originalScale;
    }

    IEnumerator AnimatePop(UIBlock2D target, Vector3 originalScale){
        if(target == null) yield break;
        
        float elapsed = 0f;
        target.transform.localScale = Vector3.zero;
        
        while(elapsed < popDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            
            float scale = 0f;
            if(t < 0.5f) scale = Mathf.Lerp(0f, 1.5f, t * 2f);
            else scale = Mathf.Lerp(1.5f, 1f, (t - 0.5f) * 2f);
            
            target.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        target.transform.localScale = originalScale;
    }

    void EnableDisplay(bool value){
        if(scorePanel != null) scorePanel.gameObject.SetActive(value);
        
        if(!value && currentAnimation != null){
            StopCoroutine(currentAnimation);
            currentAnimation = null;
            DisableAllBlocks();
        }
    }

    public void SetBaseMoneyMultiplier(float value) => baseMoneyMultiplier = value;
    public void SetScoreResult(ScoreCalculator.ScoreResult value) => result = value; 
}