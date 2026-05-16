using UnityEngine;
using Nova;
using System.Collections;
using System.Collections.Generic;

public class TrafficUI : MonoBehaviour
{
    public static TrafficUI Instance {get; private set;}

    [Header("REFERENCES")]
    [SerializeField] private UIBlock2D panel;
    [SerializeField] private TextBlock trafficDirection;
    [SerializeField] private DestinationText[] destinationTexts;
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
    private bool shouldBeActive = false;

    private Dictionary<TrafficLight.RoadDirection, DestinationData[]> destinationMap;

    [System.Serializable]
    private class DestinationText{
        public TextBlock nameText;
        public TextBlock rangeText;
    }

    private class DestinationData{
        public string Name;
        public int Distance;

        public DestinationData(string name, int distance){
            Name = name;
            Distance = distance;
        }
    }

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        InitializeDestinationMap();
        
        foreach(UIBlock2D block in straightIndicators) block.AddGestureHandler<Gesture.OnPress>(OnStraightButtonPressed);
        foreach(UIBlock2D block in rightIndicators) block.AddGestureHandler<Gesture.OnPress>(OnRightButtonPressed);
    }

    void InitializeDestinationMap(){
        destinationMap = new Dictionary<TrafficLight.RoadDirection, DestinationData[]>();
        destinationMap[TrafficLight.RoadDirection.North] = new DestinationData[]{
            new DestinationData("^ LOGIC HQ", 12),
            new DestinationData("> Petir Habor", 48)
        };
        destinationMap[TrafficLight.RoadDirection.South] = new DestinationData[]{
            new DestinationData("^ Seed Biji", 9),
            new DestinationData("> aGATe", 31)
        };
        destinationMap[TrafficLight.RoadDirection.East] = new DestinationData[]{
            new DestinationData("^ Projek Impek", 15),
            new DestinationData("> Dead Signal", 67)
        };
        destinationMap[TrafficLight.RoadDirection.West] = new DestinationData[]{
            new DestinationData("^ Galih Square", 11),
            new DestinationData("> Fajar District", 54)
        };
    }

    void Start(){
        HidePrompt();
    }

    void Update(){
        if(!panel.gameObject.activeSelf) return;

        if(Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0)){
            StartCoroutine(DelayedHidePrompt());
        }
    }

    public void ShowPrompt(TrafficLight tl){
        selectedTrafficLight = tl;
        panel.gameObject.SetActive(true);

        shouldBeActive = true;
        TrafficLightManager.LaneStatus laneStatus = TrafficLightManager.Instance.GetLaneStatus(tl.GetRoadDirection());
        UpdateUI(laneStatus);
    }

    public void HidePrompt(){
        panel.gameObject.SetActive(false);
        selectedTrafficLight = null;
    }

    IEnumerator DelayedHidePrompt(){
        yield return new WaitForSeconds(0.15f);
        if(!shouldBeActive){
            HidePrompt();
            yield break;
        }
        shouldBeActive = false;
    }

    public void UpdateUI(TrafficLightManager.LaneStatus laneStatus){
        if(selectedTrafficLight == null) return;
        
        TrafficLight.RoadDirection direction = selectedTrafficLight.GetRoadDirection();
        trafficDirection.Text = $"{direction.ToString()} Road";
        
        if(destinationMap.TryGetValue(direction, out var destinations)){
            for(int i = 0; i < destinations.Length && i < destinationTexts.Length; i++){
                destinationTexts[i].nameText.Text = destinations[i].Name;
                destinationTexts[i].rangeText.Text = $"{destinations[i].Distance} km";
            }
        }
        
        if(laneStatus.straightActive) UpdateSprite(TrafficColor.Green, TrafficDirection.Straight);
        else UpdateSprite(TrafficColor.Red, TrafficDirection.Straight);
        
        if(laneStatus.rightActive) UpdateSprite(TrafficColor.Green, TrafficDirection.Right);
        else UpdateSprite(TrafficColor.Red, TrafficDirection.Right);
    }

    public void UpdateSprite(TrafficColor tc, TrafficDirection td){
        if(td == TrafficDirection.Straight){
            foreach(UIBlock2D block in straightIndicators)
                if(block != null) block.Shadow.Enabled = false;

            switch(tc){
                case TrafficColor.Red:
                    straightBlock.SetImage(trafficSprite[0]);
                    straightIndicators[0].Shadow.Enabled = true;
                    break;
                case TrafficColor.Yellow:
                    straightBlock.SetImage(trafficSprite[1]);
                    straightIndicators[1].Shadow.Enabled = true;
                    break;
                case TrafficColor.Green:
                    straightBlock.SetImage(trafficSprite[2]);
                    straightIndicators[2].Shadow.Enabled = true;
                    break;
            }
        }
        else if(td == TrafficDirection.Right){
            foreach(UIBlock2D block in rightIndicators)
                if(block != null) block.Shadow.Enabled = false;

            switch(tc){
                case TrafficColor.Red:
                    rightBlock.SetImage(trafficSprite[0]);
                    rightIndicators[0].Shadow.Enabled = true;
                    break;
                case TrafficColor.Yellow:
                    rightBlock.SetImage(trafficSprite[1]);
                    rightIndicators[1].Shadow.Enabled = true;
                    break;
                case TrafficColor.Green:
                    rightBlock.SetImage(trafficSprite[2]);
                    rightIndicators[2].Shadow.Enabled = true;
                    break;
            }
        }
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

        shouldBeActive = true;
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

        shouldBeActive = true;
        if(TrafficLightManager.Instance == null) return;
        bool isGreen = currentSelection == 2;

        clickType = TrafficLightManager.ClickType.Right;
        TrafficLightManager.Instance.OnTrafficLightClicked(selectedTrafficLight, clickType, isGreen);
    }
}