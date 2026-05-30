using UnityEngine;
using NaughtyAttributes;
using Nova;
using UnityEngine.Localization.Components;

public class BatteryUI : MonoBehaviour
{
    [Header("BATTERY")]
    [SerializeField] private UIBlock2D batteryBlock;
    [SerializeField] private TextBlock batteryText;

    [SerializeField] private Sprite[] batterySprite = new Sprite[5];
    [SerializeField] private Color[] batteryColor = new Color[5];
    [Space(10)]
    [SerializeField] private TextBlock systemStatus;
    [SerializeField] private UIBlock2D systemStatusIcon;
    [SerializeField] private Color[] systemColor = new Color[2];

    [Header("LOCALIZATION")]
    [SerializeField] private LocalizeStringEvent systemStatusLocalizeEvent; 

    [ReadOnly] public float battery = 0f;
    [ReadOnly] public float maxBattery = 0f;
    private int lastSpriteIndex = -1;
    private bool shouldBeOffline = false;
    private bool isSystemOffline = false;

    public void Initialize(float value){
        maxBattery = value;
        battery = maxBattery;
        UpdateBatteryUI(battery);
    }

    public void SetBattery(float value){
        battery = value;
        UpdateBatteryUI(battery);
    }

    void UpdateBatteryUI(float batteryValue){
        if(batteryBlock == null) return;
        
        float percentage = batteryValue / maxBattery;
        int spriteIndex = Mathf.Clamp(Mathf.FloorToInt(percentage * 5f), 0, 4);
        
        if(spriteIndex != lastSpriteIndex){
            UpdateBatterySprite(batterySprite[spriteIndex]);
            UpdateTextColor(batteryColor[spriteIndex]);
            
            shouldBeOffline = (spriteIndex == 0 || spriteIndex == 1);
            if(shouldBeOffline != isSystemOffline){
                if(shouldBeOffline) SetSystemOffline();
                else SetSystemOnline();
                isSystemOffline = shouldBeOffline;
            }
            
            lastSpriteIndex = spriteIndex;
        }
        
        batteryText.Text = $"{(int)(percentage * 100)}%";
    }

    void UpdateBatterySprite(Sprite sprite) => batteryBlock.SetImage(sprite);
    void UpdateTextColor(Color color){
        batteryText.Color = color;
        if(!shouldBeOffline) systemStatusIcon.Color = systemStatus.Color = color;
    }

    void SetSystemOnline(){
        if(systemStatusLocalizeEvent != null){
            systemStatusLocalizeEvent.StringReference.SetReference("Carciddent Table", "Menu.StatusOnline");
            systemStatusLocalizeEvent.RefreshString();
        }
        else systemStatus.Text = "System Online";
        systemStatusIcon.Color = systemStatus.Color = systemColor[0];
    }

    void SetSystemOffline(){
        if(systemStatusLocalizeEvent != null){
            systemStatusLocalizeEvent.StringReference.SetReference("Carciddent Table", "Menu.StatusDying");
            systemStatusLocalizeEvent.RefreshString();
        }
        else systemStatus.Text = "System Offline";
        systemStatusIcon.Color = systemStatus.Color = systemColor[1];
    }
}