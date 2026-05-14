using UnityEngine;
using System.Collections;

public class PedestrianFade : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;

    private Renderer[] renderers;
    private Material[] materials;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        int totalMaterials = 0;

        foreach (Renderer rend in renderers)
        {
            totalMaterials += rend.materials.Length;
        }

        materials = new Material[totalMaterials];

        int index = 0;

        foreach (Renderer rend in renderers)
        {
            Material[] rendererMaterials = rend.materials;

            for (int i = 0; i < rendererMaterials.Length; i++)
            {
                materials[index] = rendererMaterials[i];
                index++;
            }
        }
    }

    public void SetFade(float value)
    {
        foreach (Material mat in materials)
        {
            if (mat != null && mat.HasProperty("_Fade"))
            {
                mat.SetFloat("_Fade", value);
            }
        }
    }

    public IEnumerator FadeIn()
    {
        float timer = 0f;

        SetFade(0f);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float fadeValue = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            SetFade(fadeValue);

            yield return null;
        }

        SetFade(1f);
    }

    public IEnumerator FadeOut()
    {
        float timer = 0f;

        SetFade(1f);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float fadeValue = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            SetFade(fadeValue);

            yield return null;
        }

        SetFade(0f);
    }
}