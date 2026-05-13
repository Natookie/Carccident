using Unity;
using UnityEngine;
using NaughtyAttributes;
using Nova;

public class BatteryUI : MonoBehaviour
{
    [Header("BATTERY")]
    [SerializeField] private UIBlock2D batteryBlock;
    [SerializeField] private Sprite[] batterySprite = new Sprite[5];
    [Space(10)]
    [SerializeField] private TextBlock systemStatus;
    [SerializeField] private UIBlock2D systemStatusIcon;
    [SerializeField] private Color[] systemColor = new Color[2];

    [ReadOnly] public float battery = 0f;
    [ReadOnly] public float maxBattery = 100f;

    void Start(){
        battery = maxBattery;
        systemStatus.Color = systemColor[0];
        systemStatusIcon.Color = systemColor[0];
    }

    void Update(){
        if(battery > 0) HandleBattery();    
    }

    void HandleBattery(){
        if(batteryBlock == null) return;
        battery -= Time.deltaTime;

        float percentage = battery / maxBattery;
        if(percentage >= 0.8f) UpdateBatterySprite(batterySprite[4]); //100% - 80%
        else if(percentage >= 0.6f) UpdateBatterySprite(batterySprite[3]); //80% - 60%
        else if(percentage >= 0.4f) UpdateBatterySprite(batterySprite[2]); //60% - 40%
        else if(percentage >= 0.2f) UpdateBatterySprite(batterySprite[1]); //40% - 20%
        else UpdateBatterySprite(batterySprite[0]); //20% - 0%
        
        battery = Mathf.Max(0, battery - Time.deltaTime);
        if(battery <= 0){
            GameManager.Instance.GameOver();
            systemStatus.Text = "System Offline";

            systemStatusIcon.Color = systemColor[1];
            systemStatus.Color = systemColor[1];
        }
    }

    void UpdateBatterySprite(Sprite sprite) => batteryBlock.SetImage(sprite);
}