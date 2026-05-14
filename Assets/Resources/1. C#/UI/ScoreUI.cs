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

    private Vector3 originalScorePanelScale;
    private Dictionary<UIBlock2D, Vector3> originalBlockScales = new Dictionary<UIBlock2D, Vector3>();
    private Dictionary<UIBlock2D, Vector3> originalBlockPositions = new Dictionary<UIBlock2D, Vector3>();
    private Dictionary<TextBlock, Vector3> originalTextPositions = new Dictionary<TextBlock, Vector3>();
    private Dictionary<TextBlock, Vector3> originalTextScales = new Dictionary<TextBlock, Vector3>();
    private Coroutine currentAnimation;
    private float currentBaseScore = 0f;
    private float currentDeduction = 0f;
    private float currentBonus = 0f;
    private float currentMultiplier = 1f;
    
    private Vector3 originalBaseScorePos;
    private Vector3 originalBaseScoreScale;

    void Start(){
        if(baseScoreText == null) Debug.LogError("Base score is missing!");
        if(deductionScoreText == null) Debug.LogError("Deduction score is missing!");
        if(bonusScoreText == null) Debug.LogError("Bonus score is missing!");
        if(multiplierScoreText == null) Debug.LogError("Multiplier score is missing!");

        CacheOriginalValues();
        DisableAllBlocks();
        EnableDisplay(false);

        //DisplayScore(10, 20, 30);
    }

    void Update() => DisplayNightTime();

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
        timeText.Text = $"({dateFormatted}) {displayHour:D2}:{now.Minute:D2}:{now.Second:D2} {ampm}";
    }

    public void DisplayScore(float elapsed, float accidentCounter, float passedCounter){
        EnableDisplay(true);
        
        currentBaseScore = elapsed;
        if(currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(ScoreDisplaySequence(accidentCounter, passedCounter));
    }

    IEnumerator ScoreDisplaySequence(float accidentCounter, float passedCounter){
        yield return StartCoroutine(AnimatePop(scorePanel, originalScorePanelScale));
        
        baseScoreText.Text = $"${currentBaseScore:F2}";
        yield return StartCoroutine(ShakeBaseScore());
        yield return new WaitForSeconds(sequenceDelay);
        
        elapsedTimeText.Text = $"{currentBaseScore:F2}";
        yield return StartCoroutine(ActivateAndPop(elapsedTimeBlock));
        yield return new WaitForSeconds(sequenceDelay);
        
        carPassedText.Text = passedCounter.ToString();
        yield return StartCoroutine(ActivateAndPop(carPassedBlock));
        yield return new WaitForSeconds(sequenceDelay);
        
        carCollidedText.Text = accidentCounter.ToString();
        yield return StartCoroutine(ActivateAndPop(carCollidedBlock));
        yield return new WaitForSeconds(sequenceDelay);
        
        currentDeduction = accidentCounter * 15f;
        deductionScoreText.Text = currentDeduction.ToString("F2");
        yield return StartCoroutine(ActivateAndPop(deductionScoreBlock));
        
        currentBaseScore -= currentDeduction;
        baseScoreText.Text = $"${currentBaseScore:F2}";
        yield return StartCoroutine(ShakeBaseScore());
        yield return new WaitForSeconds(sequenceDelay);

        currentBonus = passedCounter * 20f;
        bonusScoreText.Text = currentBonus.ToString("F2");
        yield return StartCoroutine(ActivateAndPop(bonusScoreBlock));
        
        currentBaseScore += currentBonus;
        baseScoreText.Text = $"${currentBaseScore:F2}";
        yield return StartCoroutine(ShakeBaseScore());
        yield return new WaitForSeconds(sequenceDelay);
        
        multiplierScoreText.Text = $"x{currentMultiplier:F1}";
        yield return StartCoroutine(ActivateAndPop(multiplierScoreBlock));
        
        currentBaseScore *= currentMultiplier;
        baseScoreText.Text = $"${currentBaseScore:F2}";
        yield return StartCoroutine(ShakeBaseScore());
        
        currentAnimation = null;
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
        scorePanel.gameObject.SetActive(value);
        if(!value && currentAnimation != null){
            StopCoroutine(currentAnimation);
            currentAnimation = null;
            DisableAllBlocks();
        }
    }
}