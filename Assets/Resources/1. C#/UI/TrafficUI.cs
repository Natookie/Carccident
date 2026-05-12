using UnityEngine;
using Nova;

public class TrafficUI : MonoBehaviour
{
    public static TrafficUI Instance {get; private set;}

    [Header("REFERENCES")]
    [SerializeField] private UIBlock2D panel;
    [SerializeField] private TextBlock trafficDestination;
    [Space(10)]
    [SerializeField] private UIBlock2D[] indicators;

    private int currentSelection = 0;
    private TrafficLight selectedTrafficLight;
    private TrafficLightManager.ClickType clickType;

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        foreach(UIBlock2D block in indicators){
            block.AddGestureHandler<Gesture.OnPress>(OnButtonPressed);
            block.AddGestureHandler<Gesture.OnHover>(OnButtonHovered);
            block.AddGestureHandler<Gesture.OnUnhover>(OnButtonUnhovered);
        }
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
        
        trafficDestination.Text = selectedTrafficLight.GetRoadDirection().ToString();

        bool straightActive = laneStatus.straightActive;
        bool rightActive = laneStatus.rightActive;
        bool pedestrianActive = false;

        indicators[0].Color = straightActive ? Color.green : Color.red;
        indicators[1].Color = rightActive ? Color.green : Color.red;
        indicators[2].Color = pedestrianActive ? Color.green : Color.red;
    }

    void OnButtonPressed(Gesture.OnPress evt){
        UIBlock2D pressedButton = evt.Receiver as UIBlock2D;
        if(pressedButton == null) return;

        for(int i = 0; i < indicators.Length; i++){
            if(indicators[i] == pressedButton){
                currentSelection = i;
                break;
            }
        }

        if(TrafficLightManager.Instance == null) return;
        switch(currentSelection){
            case 0: clickType = TrafficLightManager.ClickType.Straight; break;
            case 1: clickType = TrafficLightManager.ClickType.Right; break;
            case 2: clickType = TrafficLightManager.ClickType.Pedestrian; break;
        }
        
        TrafficLightManager.Instance.OnTrafficLightClicked(selectedTrafficLight, clickType);
    }

    void OnButtonHovered(Gesture.OnHover evt){
        UIBlock2D hoveredButton = evt.Receiver as UIBlock2D;
        if(hoveredButton != null){
        }
    }

    void OnButtonUnhovered(Gesture.OnUnhover evt){
        UIBlock2D unhoveredButton = evt.Receiver as UIBlock2D;
        if(unhoveredButton != null){
        }
    }
}