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

    private bool isBlinking = false;

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

        if(testAllLanesGreen){
            north.straightActive = true;
            north.rightActive = true;
            south.straightActive = true;
            south.rightActive = true;
            east.straightActive = true;
            east.rightActive = true;
            west.straightActive = true;
            west.rightActive = true;
        }
        
        UpdateAllLights();
    }

    public void OnTrafficLightClicked(TrafficLight clickedLight, ClickType clickType){
        if(isBlinking) return;

        if(AudioManager.Instance != null) AudioManager.Instance.PlayTrafficLightChange();

        TrafficLight.RoadDirection clickedDirection = clickedLight.GetRoadDirection();
        switch(clickType){
            case ClickType.Straight:
                ToggleStraight(clickedDirection);
                break;
            case ClickType.Right:
                ToggleRightTurn(clickedDirection);
                break;
            case ClickType.Pedestrian:
                Debug.Log("Pedestrian");
                break;

            
        }
        
        LaneStatus lane = GetLaneStatus(clickedDirection);
        TrafficUI.Instance.UpdateUI(lane);
        StartCoroutine(BlinkThenUpdate(clickedDirection));
    }

    void ToggleStraight(TrafficLight.RoadDirection direction){
        LaneStatus lane = GetLaneStatus(direction);
        if(lane != null) lane.straightActive = !lane.straightActive;
    }

    void ToggleRightTurn(TrafficLight.RoadDirection direction){
        LaneStatus lane = GetLaneStatus(direction);
        if(lane != null) lane.rightActive = !lane.rightActive;
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

    IEnumerator BlinkThenUpdate(TrafficLight.RoadDirection direction){
        isBlinking = true;
        
        TrafficLight targetLight = GetLightByDirection(direction);
        if(targetLight != null){
            targetLight.SetYellow();
            yield return new WaitForSeconds(yellowLightDuration);
        }
        
        UpdateLight(direction);
        isBlinking = false;
    }

    void UpdateLight(TrafficLight.RoadDirection direction){
        LaneStatus lane = GetLaneStatus(direction);
        TrafficLight targetLight = GetLightByDirection(direction);
        if(lane == null || targetLight == null) return;
        
        if(lane.straightActive || lane.rightActive) targetLight.SetGreen();
        else targetLight.SetRed();
    }

    void UpdateAllLights(){
        UpdateLight(TrafficLight.RoadDirection.North);
        UpdateLight(TrafficLight.RoadDirection.South);
        UpdateLight(TrafficLight.RoadDirection.East);
        UpdateLight(TrafficLight.RoadDirection.West);
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