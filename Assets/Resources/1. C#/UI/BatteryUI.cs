using Unity;
using UnityEngine;
using NaughtyAttributes;
using Nova;

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

    [ReadOnly] public float battery = 0f;
    [ReadOnly] public float maxBattery = 0f;
    private int lastSpriteIndex = -1;

    void Start(){
        maxBattery = GameManager.Instance.maxBattery;
        battery = maxBattery;
        systemStatus.Color = systemColor[0];
        systemStatusIcon.Color = systemColor[0];
    }

    void Update(){
        if(battery > 0 && GameManager.Instance.isGameInitialized) HandleBattery();    
    }

    void HandleBattery(){
        if(batteryBlock == null) return;
        battery = GameManager.Instance.currentBattery;
        
        if(battery <= 0){
            systemStatus.Text = "System Offline";
            systemStatusIcon.Color = systemColor[1];
            systemStatus.Color = systemColor[1];
            return;
        }
        
        float percentage = battery / maxBattery;
        int spriteIndex = Mathf.Clamp(Mathf.FloorToInt(percentage * 5f), 0, 4);
        if(spriteIndex != lastSpriteIndex){
            UpdateBatterySprite(batterySprite[spriteIndex]);
            UpdateBatteryTextColor(batteryColor[spriteIndex]);
            lastSpriteIndex = spriteIndex;
        }
        batteryText.Text = $"{(int)(percentage * 100)}%";
    }

    void UpdateBatterySprite(Sprite sprite) => batteryBlock.SetImage(sprite);
    void UpdateBatteryTextColor(Color color) => batteryText.Color = color;
}