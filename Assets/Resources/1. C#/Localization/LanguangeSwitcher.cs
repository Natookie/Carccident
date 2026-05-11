using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageSwitcher : MonoBehaviour
{
    void Update(){
        //if(Input.GetKeyDown(KeyCode.Alpha1)) SetLanguage("en");
        //if(Input.GetKeyDown(KeyCode.Alpha2)) SetLanguage("id");
        //if(Input.GetKeyDown(KeyCode.Alpha3)) SetLanguage("ru");
    }

    public void SetLanguage(string localeCode){
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        if(locale != null) LocalizationSettings.SelectedLocale = locale;
        else Debug.LogWarning("Locale not found: " + localeCode);
    }
}