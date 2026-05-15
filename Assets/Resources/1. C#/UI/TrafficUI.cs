using UnityEngine;
using Nova;

public class TrafficUI : MonoBehaviour
{
    public static TrafficUI Instance {get; private set;}

    [Header("REFERENCES")]
    [SerializeField] private UIBlock2D panel;
    [SerializeField] private TextBlock trafficDestination;
    [SerializeField] private Sprite[] trafficSprite = new Sprite[3];
    [Space(10)]
    [SerializeField] private UIBlock2D straightBlock;
    [SerializeField] private UIBlock2D[] straightIndicators = new UIBlock2D[3];
    [Space(10)]
    [SerializeField] private UIBlock2D rightBlock;
    [SerializeField] private UIBlock2D[] rightIndicators = new UIBlock2D[3];

    public enum TrafficColor{Green, Yellow, Red};
    public enum TrafficDirection{Straight, Right};

    private int currentSelection = 0;
    private TrafficLight selectedTrafficLight;
    private TrafficLightManager.ClickType clickType;

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        foreach(UIBlock2D block in straightIndicators) block.AddGestureHandler<Gesture.OnPress>(OnStraightButtonPressed);
        foreach(UIBlock2D block in rightIndicators) block.AddGestureHandler<Gesture.OnPress>(OnRightButtonPressed);
    }

    void Start(){
        HidePrompt();
    }

    public void ShowPrompt(TrafficLight tl){
        selectedTrafficLight = tl;
        panel.gameObject.SetActive(true);

        TrafficLightManager.LaneStatus laneStatus = TrafficLightManager.Instance.GetLaneStatus(tl.GetRoadDirection());
        UpdateUI(laneStatus);
    }

    public void HidePrompt(){
        panel.gameObject.SetActive(false);
        selectedTrafficLight = null;
    }

    public void UpdateUI(TrafficLightManager.LaneStatus laneStatus){
        if(selectedTrafficLight == null) return;
        trafficDestination.Text = $"{selectedTrafficLight.GetRoadDirection().ToString()}";
        
        if(laneStatus.straightActive) UpdateSprite(TrafficColor.Green, TrafficDirection.Straight);
        else UpdateSprite(TrafficColor.Red, TrafficDirection.Straight);
        
        if(laneStatus.rightActive) UpdateSprite(TrafficColor.Green, TrafficDirection.Right);
        else UpdateSprite(TrafficColor.Red, TrafficDirection.Right);
    }

    public void UpdateSprite(TrafficColor tc, TrafficDirection td){
        ClearAllShadows();
        
        if(td == TrafficDirection.Straight){
            switch(tc){
                case TrafficColor.Red:
                    if(trafficSprite.Length > 0) straightBlock.SetImage(trafficSprite[0]);
                    straightIndicators[0].Shadow.Enabled = true;
                    break;
                case TrafficColor.Yellow:
                    if(trafficSprite.Length > 1) straightBlock.SetImage(trafficSprite[1]);
                    straightIndicators[1].Shadow.Enabled = true;
                    break;
                case TrafficColor.Green:
                    if(trafficSprite.Length > 2) straightBlock.SetImage(trafficSprite[2]);
                    straightIndicators[2].Shadow.Enabled = true;
                    break;
            }
        }
        else if(td == TrafficDirection.Right){
            switch(tc){
                case TrafficColor.Red:
                    if(trafficSprite.Length > 0) rightBlock.SetImage(trafficSprite[0]);
                    rightIndicators[0].Shadow.Enabled = true;
                    break;
                case TrafficColor.Yellow:
                    if(trafficSprite.Length > 1) rightBlock.SetImage(trafficSprite[1]);
                    rightIndicators[1].Shadow.Enabled = true;
                    break;
                case TrafficColor.Green:
                    if(trafficSprite.Length > 2) rightBlock.SetImage(trafficSprite[2]);
                    rightIndicators[2].Shadow.Enabled = true;
                    break;
            }
        }
    }
    
    void ClearAllShadows(){
        foreach(UIBlock2D block in straightIndicators)
            if(block != null) block.Shadow.Enabled = false;
        foreach(UIBlock2D block in rightIndicators)
            if(block != null) block.Shadow.Enabled = false;
    }

    void OnStraightButtonPressed(Gesture.OnPress evt){
        UIBlock2D pressedButton = evt.Receiver as UIBlock2D;
        if(pressedButton == null) return;

        for(int i = 0; i < straightIndicators.Length; i++){
            if(straightIndicators[i] == pressedButton){
                currentSelection = i;
                break;
            }
        }

        if(TrafficLightManager.Instance == null) return;
        bool isGreen = currentSelection == 2;

        clickType = TrafficLightManager.ClickType.Straight;
        TrafficLightManager.Instance.OnTrafficLightClicked(selectedTrafficLight, clickType, isGreen);
    }

    void OnRightButtonPressed(Gesture.OnPress evt){
        UIBlock2D pressedButton = evt.Receiver as UIBlock2D;
        if(pressedButton == null) return;

        for(int i = 0; i < rightIndicators.Length; i++){
            if(rightIndicators[i] == pressedButton){
                currentSelection = i;
                break;
            }
        }

        if(TrafficLightManager.Instance == null) return;
        bool isGreen = currentSelection == 2;

        clickType = TrafficLightManager.ClickType.Right;
        TrafficLightManager.Instance.OnTrafficLightClicked(selectedTrafficLight, clickType, isGreen);
    }
}