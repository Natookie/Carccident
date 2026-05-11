using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    [Header("REFERENCES")]
    public MeshRenderer lightRenderer;
    public int lightID;
    
    private TrafficLightManager manager;
    private RoadDirection roadDirection;
    private bool isGreen = false;
    
    public enum RoadDirection
    {
        North,
        South,
        East,
        West
    }
    
    void Start(){
        manager = TrafficLightManager.Instance;
        if(lightRenderer == null) lightRenderer = GetComponent<MeshRenderer>();
        if(GetComponent<Collider>() == null) gameObject.AddComponent<BoxCollider>();
        
        switch(lightID){
            case 0: roadDirection = RoadDirection.North; break;
            case 1: roadDirection = RoadDirection.South; break;
            case 2: roadDirection = RoadDirection.East; break;
            case 3: roadDirection = RoadDirection.West; break;
            default: 
                Debug.LogError($"Invalid lightID {lightID}! Must be 0-3");
                roadDirection = RoadDirection.North;
                break;
        }

        SetRed();
    }
    
    void OnMouseDown(){
        if(manager != null){
            bool isRightClick = Input.GetMouseButtonDown(1);
            manager.OnTrafficLightClicked(this, isRightClick);
        }
    }
    
    public void SetGreen(){
        isGreen = true;
        if(lightRenderer != null && manager != null) 
            lightRenderer.material = manager.greenMaterial;
    }
    
    public void SetRed(){
        isGreen = false;
        if(lightRenderer != null && manager != null) 
            lightRenderer.material = manager.redMaterial;
    }
    
    public void SetYellow(){
        if(lightRenderer != null && manager != null) 
            lightRenderer.material = manager.yellowMaterial;
    }
    
    public bool IsGreen() => isGreen;
    public int GetLightID() => lightID;
    public RoadDirection GetRoadDirection() => roadDirection;
}