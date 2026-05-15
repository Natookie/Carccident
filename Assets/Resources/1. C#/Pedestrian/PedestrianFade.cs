using UnityEngine;
using System.Collections;

public class PedestrianFade : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;

    private Renderer[] renderers;
    private Material[] materials;
    private bool isFading = false;

    void Awake(){
        renderers = GetComponentsInChildren<Renderer>();

        int totalMaterials = 0;

        foreach(Renderer rend in renderers) totalMaterials += rend.materials.Length;
        materials = new Material[totalMaterials];

        int index = 0;

        foreach(Renderer rend in renderers){
            Material[] rendererMaterials = rend.materials;

            for(int i = 0; i < rendererMaterials.Length; i++){
                materials[index] = rendererMaterials[i];
                index++;
            }
        }
    }

    public void SetFade(float value){
        foreach(Material mat in materials){
            if(mat == null) continue;
            if(mat.HasProperty("_Fade")) mat.SetFloat("_Fade", value);
        }
    }

    public void ResetFade(){
        if(isFading){
            StopAllCoroutines();
            isFading = false;
        }
        
        SetFade(1f);
    }

    public IEnumerator FadeIn(){
        if(isFading) yield break;
        
        isFading = true;
        float timer = 0f;

        SetFade(0f);

        while(timer < fadeDuration){
            timer += Time.deltaTime;
            float fadeValue = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            SetFade(fadeValue);
            yield return null;
        }

        SetFade(1f);
        isFading = false;
    }

    public IEnumerator FadeOut(){
        if(isFading) yield break;
        
        isFading = true;
        float timer = 0f;

        SetFade(1f);

        while(timer < fadeDuration){
            timer += Time.deltaTime;
            float fadeValue = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            SetFade(fadeValue);
            yield return null;
        }

        SetFade(0f);
        isFading = false;
    }

    public bool IsFading() => isFading;
    public float GetFadeDuration() => fadeDuration;
    public void SetFadeDuration(float duration) => fadeDuration = duration;
}