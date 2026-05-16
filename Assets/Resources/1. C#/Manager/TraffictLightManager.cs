using UnityEngine;
using System.Collections;
using NaughtyAttributes;

public class TrafficLightManager : MonoBehaviour
{
    public static TrafficLightManager Instance { get; private set; }

    [Header("MATERIALS")]
    public Material redMaterial;
    public Material greenMaterial;
    public Material yellowMaterial;

    [Header("TIMING")]
    public float yellowLightDuration = 0.3f;

    [Header("TRAFFIC LIGHTS")]
    [SerializeField] private TrafficLight northLight;
    [SerializeField] private TrafficLight southLight;
    [SerializeField] private TrafficLight eastLight;
    [SerializeField] private TrafficLight westLight;

    [System.Serializable]
    public class LaneStatus
    {
        public bool straightActive = false;
        public bool rightActive = false;
    }

    [Header("LANE STATUS")]
    [SerializeField] private LaneStatus north = new LaneStatus();
    [SerializeField] private LaneStatus south = new LaneStatus();
    [SerializeField] private LaneStatus east = new LaneStatus();
    [SerializeField] private LaneStatus west = new LaneStatus();

    [SerializeField] private bool testAllLanesGreen = false;
    
    public enum ClickType
    {
        Straight,
        Right,
        Pedestrian
    }

    private bool isProcessing = false;

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start(){
        if(redMaterial == null) Debug.LogError("Red material not assigned!");
        if(greenMaterial == null) Debug.LogError("Green material not assigned!");
        if(yellowMaterial == null) Debug.LogError("Yellow material not assigned!");

        if(testAllLanesGreen) SetAllLanesGreen(true);
        UpdateAllLights();
    }

    void SetAllLanesGreen(bool isGreen){
        north.straightActive = isGreen;
        north.rightActive = isGreen;
        south.straightActive = isGreen;
        south.rightActive = isGreen;
        east.straightActive = isGreen;
        east.rightActive = isGreen;
        west.straightActive = isGreen;
        west.rightActive = isGreen;
    }

    public void OnTrafficLightClicked(TrafficLight clickedLight, ClickType clickType, bool turnGreen){
        if(isProcessing) return;

        TrafficLight.RoadDirection clickedDirection = clickedLight.GetRoadDirection();
        
        LaneStatus lane = GetLaneStatus(clickedDirection);
        bool previousStraight = lane.straightActive;
        bool previousRight = lane.rightActive;
        
        switch(clickType){
            case ClickType.Straight: lane.straightActive = turnGreen; break;
            case ClickType.Right: lane.rightActive = turnGreen; break;
            case ClickType.Pedestrian: break;
        }
        
        TrafficUI.Instance.UpdateUI(lane);
        
        StartCoroutine(BlinkThenUpdate(clickedDirection, clickType, previousStraight, previousRight));
        if(AudioManager.Instance != null) AudioManager.Instance.PlayTrafficLightChange();
    }

    IEnumerator BlinkThenUpdate(TrafficLight.RoadDirection direction, ClickType clickType, bool previousStraight, bool previousRight){
        isProcessing = true;
        
        TrafficLight targetLight = GetLightByDirection(direction);
        LaneStatus currentLane = GetLaneStatus(direction);
        
        if(targetLight != null){
            if(clickType == ClickType.Straight && previousStraight != currentLane.straightActive){
                targetLight.SetStraightYellow();
                yield return new WaitForSeconds(yellowLightDuration);
            }
            else if(clickType == ClickType.Right && previousRight != currentLane.rightActive){
                targetLight.SetRightYellow();
                yield return new WaitForSeconds(yellowLightDuration);
            }
        }
        
        UpdateLight(direction);
        isProcessing = false;
    }

    void UpdateLight(TrafficLight.RoadDirection direction){
        LaneStatus lane = GetLaneStatus(direction);
        TrafficLight targetLight = GetLightByDirection(direction);
        if(lane == null || targetLight == null) return;
        
        if(lane.straightActive) targetLight.SetStraightGreen();
        else targetLight.SetStraightRed();
        
        if(lane.rightActive) targetLight.SetRightGreen();
        else targetLight.SetRightRed();
    }

    void UpdateAllLights(){
        UpdateLight(TrafficLight.RoadDirection.North);
        UpdateLight(TrafficLight.RoadDirection.South);
        UpdateLight(TrafficLight.RoadDirection.East);
        UpdateLight(TrafficLight.RoadDirection.West);
    }

    public LaneStatus GetLaneStatus(TrafficLight.RoadDirection direction){
        switch(direction){
            case TrafficLight.RoadDirection.North: return north;
            case TrafficLight.RoadDirection.South: return south;
            case TrafficLight.RoadDirection.East: return east;
            case TrafficLight.RoadDirection.West: return west;
            default: return null;
        }
    }

    TrafficLight GetLightByDirection(TrafficLight.RoadDirection direction){
        switch(direction){
            case TrafficLight.RoadDirection.North: return northLight;
            case TrafficLight.RoadDirection.South: return southLight;
            case TrafficLight.RoadDirection.East: return eastLight;
            case TrafficLight.RoadDirection.West: return westLight;
            default: return null;
        }
    }

    public bool IsLaneGreen(int laneID){
        if(testAllLanesGreen) return true;

        switch(laneID){
            case 0: return north.rightActive;
            case 1: return north.straightActive;
            case 2: return south.rightActive;
            case 3: return south.straightActive;
            case 4: return east.rightActive;
            case 5: return east.straightActive;
            case 6: return west.rightActive;
            case 7: return west.straightActive;
            default: return false;
        }
    }
}