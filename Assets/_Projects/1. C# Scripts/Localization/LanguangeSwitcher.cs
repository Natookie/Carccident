using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageSwitcher : MonoBehaviour
{
    public static LanguageSwitcher Instance {get; private set;}

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetLanguage(string localeCode){
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        if(locale != null) LocalizationSettings.SelectedLocale = locale;
        else Debug.LogWarning("Locale not found: " + localeCode);
    }
}