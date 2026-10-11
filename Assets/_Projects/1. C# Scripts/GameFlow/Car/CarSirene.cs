using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class CarSirene : MonoBehaviour
{
    [Header("AUDIO")]
    [SerializeField] private AudioClip sireneClip;
    
    [Header("VISUAL")]
    [SerializeField] private List<int> materialIndices = new List<int>();
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 3f;
    
    private AudioSource audioSource;
    private CarLogic carLogic;
    private List<Material> targetMaterials = new List<Material>();
    private List<Color> originalEmissionColors = new List<Color>();
    private Coroutine sireneCoroutine;
    private bool isActive = false;
    
    void Awake(){
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        
        carLogic = GetComponent<CarLogic>();
    }
    
    void Start(){
        if(sireneClip != null) audioSource.clip = sireneClip;
        
        if(carLogic != null && carLogic.visualTransform != null){
            Renderer renderer = carLogic.visualTransform.GetComponent<Renderer>();
            if(renderer != null){
                if(materialIndices.Count == 0){
                    for(int i = 0; i < renderer.materials.Length; i++) materialIndices.Add(i);
                }
                
                foreach(int index in materialIndices){
                    if(index < renderer.materials.Length){
                        Material mat = renderer.materials[index];
                        targetMaterials.Add(mat);
                        
                        if(mat.HasProperty("_EmissionColor")) originalEmissionColors.Add(mat.GetColor("_EmissionColor"));
                        else originalEmissionColors.Add(Color.white);
                        
                        mat.EnableKeyword("_EMISSION");
                    }
                }
            }
        }
    }
    
    void Update(){
        if(carLogic == null) return;
        
        bool shouldPlay = carLogic.gameObject.activeSelf && !carLogic.isCollisionDisabled;
        if(shouldPlay && !isActive) StartSirene();
        else if(!shouldPlay && isActive) StopSirene();
    }
    
    void StartSirene(){
        isActive = true;
        
        if(sireneClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySirene(sireneClip, true);
        if(sireneCoroutine != null) StopCoroutine(sireneCoroutine);
        
        if(targetMaterials.Count >= 2) sireneCoroutine = StartCoroutine(AlternatingSireneEffect());
        else sireneCoroutine = StartCoroutine(PulseSireneEffect());
    }

    void StopSirene(){
        isActive = false;
        
        if(AudioManager.Instance != null) AudioManager.Instance.StopSirene();
        if(sireneCoroutine != null) StopCoroutine(sireneCoroutine);
        
        foreach(Material mat in targetMaterials){
            if(mat != null) mat.SetColor("_EmissionColor", Color.black);
        }
    }
    
    IEnumerator PulseSireneEffect(){
        if(targetMaterials.Count == 0) yield break;
        
        Material mat = targetMaterials[0];
        Color originalColor = originalEmissionColors[0];
        
        while(isActive){
            float time = 0f;
            while(time < pulseSpeed && isActive){
                time += Time.deltaTime;
                float t = time / pulseSpeed;
                float intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.Sin(t * Mathf.PI));
                
                Color emissionColor = originalColor * intensity;
                mat.SetColor("_EmissionColor", emissionColor);
                
                yield return null;
            }
        }
    }
    
    IEnumerator AlternatingSireneEffect(){
        if(targetMaterials.Count < 2) yield break;
        
        int currentIndex = 0;
        float halfPulse = pulseSpeed / 2f;
        
        while(isActive){
            float time = 0f;
            while(time < halfPulse && isActive){
                time += Time.deltaTime;
                float t = time / halfPulse;
                float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
                
                Color currentColor = originalEmissionColors[currentIndex] * intensity;
                targetMaterials[currentIndex].SetColor("_EmissionColor", currentColor);
                
                int prevIndex = (currentIndex - 1 + targetMaterials.Count) % targetMaterials.Count;
                float prevIntensity = Mathf.Lerp(maxIntensity, minIntensity, t);
                Color prevColor = originalEmissionColors[prevIndex] * prevIntensity;
                targetMaterials[prevIndex].SetColor("_EmissionColor", prevColor);
                
                yield return null;
            }
            
            targetMaterials[currentIndex].SetColor("_EmissionColor", originalEmissionColors[currentIndex] * maxIntensity);
            int prevIdx = (currentIndex - 1 + targetMaterials.Count) % targetMaterials.Count;
            targetMaterials[prevIdx].SetColor("_EmissionColor", Color.black);
            
            yield return new WaitForSeconds(0.05f);
            
            currentIndex = (currentIndex + 1) % targetMaterials.Count;
        }
    }

    void OnDestroy() => StopSirene();
}