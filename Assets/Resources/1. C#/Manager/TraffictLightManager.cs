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
    public float yellowLightDuration = 2f;

    [Header("TRAFFIC LIGHTS")]
    [SerializeField] private TrafficLight northLight;
    [SerializeField] private TrafficLight southLight;
    [SerializeField] private TrafficLight eastLight;
    [SerializeField] private TrafficLight westLight;

    [Header("LEFT TURN STATUS")]
    [ReadOnly] public bool northLeftActive = false;
    [ReadOnly] public bool southLeftActive = false;
    [ReadOnly] public bool eastLeftActive = false;
    [ReadOnly] public bool westLeftActive = false;

    [SerializeField] private bool testAllLanesGreen = false;

    private TrafficLight.RoadDirection currentActiveDirection = TrafficLight.RoadDirection.East;
    private bool isChangingPhase = false;

    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    IEnumerator Start(){
        yield return null;
        currentActiveDirection = TrafficLight.RoadDirection.North;
        SetDirectionGreen(currentActiveDirection);

        if(testAllLanesGreen){
            northLeftActive = true;
            southLeftActive = true;
            eastLeftActive = true;
            westLeftActive = true;
        }
    }

    public void OnTrafficLightClicked(TrafficLight clickedLight, bool isRightClick){
        if(isChangingPhase) return;

        TrafficLight.RoadDirection clickedDirection = clickedLight.GetRoadDirection();

        if(!isRightClick){
            if(currentActiveDirection == clickedDirection){
                Debug.Log($"{clickedDirection} already active!");
                return;
            }
            StartCoroutine(SwitchToDirection(clickedDirection));
        }
        else{
            if(currentActiveDirection != clickedDirection){
                Debug.Log($"Cannot enable left turn. {clickedDirection} is not active. Current active: {currentActiveDirection}");
                return;
            }
            ToggleLeftTurn(clickedDirection);
        }
    }

    void ToggleLeftTurn(TrafficLight.RoadDirection direction){
        switch(direction){
            case TrafficLight.RoadDirection.North: northLeftActive = !northLeftActive; break;
            case TrafficLight.RoadDirection.South: southLeftActive = !southLeftActive; break;
            case TrafficLight.RoadDirection.East: eastLeftActive = !eastLeftActive; break;
            case TrafficLight.RoadDirection.West: westLeftActive = !westLeftActive; break;
        }

        RefreshLight(direction);
    }

    void RefreshLight(TrafficLight.RoadDirection direction){
        StartCoroutine(BlinkYellow(direction));
    }

    IEnumerator BlinkYellow(TrafficLight.RoadDirection direction){
        SetDirectionYellow(direction);
        yield return new WaitForSeconds(0.3f);
        SetDirectionGreen(direction);
    }

    IEnumerator SwitchToDirection(TrafficLight.RoadDirection targetDirection){
        isChangingPhase = true;

        SetDirectionYellow(currentActiveDirection);
        yield return new WaitForSeconds(yellowLightDuration);

        SetAllRed();
        ResetLeftTurnStatus(currentActiveDirection);
        SetDirectionGreen(targetDirection);
        currentActiveDirection = targetDirection;

        isChangingPhase = false;
        Debug.Log($"Switched to {targetDirection}");
    }

    void ResetLeftTurnStatus(TrafficLight.RoadDirection direction){
        switch(direction){
            case TrafficLight.RoadDirection.North: northLeftActive = false; break;
            case TrafficLight.RoadDirection.South: southLeftActive = false; break;
            case TrafficLight.RoadDirection.East: eastLeftActive = false; break;
            case TrafficLight.RoadDirection.West: westLeftActive = false; break;
        }
    }

    void SetDirectionGreen(TrafficLight.RoadDirection direction){
        SetAllRed();
        switch(direction){
            case TrafficLight.RoadDirection.North:
                if(northLight != null) northLight.SetGreen();
                break;
            case TrafficLight.RoadDirection.South:
                if(southLight != null) southLight.SetGreen();
                break;
            case TrafficLight.RoadDirection.East:
                if(eastLight != null) eastLight.SetGreen();
                break;
            case TrafficLight.RoadDirection.West:
                if(westLight != null) westLight.SetGreen();
                break;
        }
    }

    void SetDirectionYellow(TrafficLight.RoadDirection direction){
        switch(direction){
            case TrafficLight.RoadDirection.North:
                if(northLight != null && northLight.IsGreen()) northLight.SetYellow();
                break;
            case TrafficLight.RoadDirection.South:
                if(southLight != null && southLight.IsGreen()) southLight.SetYellow();
                break;
            case TrafficLight.RoadDirection.East:
                if(eastLight != null && eastLight.IsGreen()) eastLight.SetYellow();
                break;
            case TrafficLight.RoadDirection.West:
                if(westLight != null && westLight.IsGreen()) westLight.SetYellow();
                break;
        }
    }

    void SetAllRed(){
        if(northLight != null) northLight.SetRed();
        if(southLight != null) southLight.SetRed();
        if(eastLight != null) eastLight.SetRed();
        if(westLight != null) westLight.SetRed();
    }

    public bool IsLaneGreen(int laneID){
        if(testAllLanesGreen) return true;

        switch(laneID){
            case 0: return currentActiveDirection == TrafficLight.RoadDirection.North && northLeftActive;
            case 1: return currentActiveDirection == TrafficLight.RoadDirection.North;
            case 2: return currentActiveDirection == TrafficLight.RoadDirection.South && southLeftActive;
            case 3: return currentActiveDirection == TrafficLight.RoadDirection.South;
            case 4: return currentActiveDirection == TrafficLight.RoadDirection.East && eastLeftActive;
            case 5: return currentActiveDirection == TrafficLight.RoadDirection.East;
            case 6: return currentActiveDirection == TrafficLight.RoadDirection.West && westLeftActive;
            case 7: return currentActiveDirection == TrafficLight.RoadDirection.West;
            default:
                return false;
        }
    }
}