using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    [Header("REFERENCES")]
    public MeshRenderer straightLightRenderer;
    public MeshRenderer rightLightRenderer;
    public int lightID;
    
    private TrafficLightManager manager;
    private RoadDirection roadDirection;
    private bool isStraightGreen = false;
    private bool isRightGreen = false;
    
    public enum RoadDirection
    {
        North,
        South,
        East,
        West
    }
    
    void Start(){
        manager = TrafficLightManager.Instance;
        
        if(straightLightRenderer == null) Debug.LogError($"Straight light renderer not assigned for light {lightID}!");
        if(rightLightRenderer == null) Debug.LogError($"Right light renderer not assigned for light {lightID}!");
        
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

        SetStraightRed();
        SetRightRed();
        SetLayer(gameObject, 11);
    }

    #region STRAIGHT
    public void SetStraightGreen(){
        isStraightGreen = true;
        if(straightLightRenderer != null && manager != null) straightLightRenderer.material = manager.greenMaterial;
        TrafficUI.Instance.UpdateSprite(TrafficUI.TrafficColor.Green, TrafficUI.TrafficDirection.Straight);
    }
    public void SetStraightRed(){
        isStraightGreen = false;
        if(straightLightRenderer != null && manager != null) straightLightRenderer.material = manager.redMaterial;
        TrafficUI.Instance.UpdateSprite(TrafficUI.TrafficColor.Red, TrafficUI.TrafficDirection.Straight);
    }
    public void SetStraightYellow(){
        if(straightLightRenderer != null && manager != null) straightLightRenderer.material = manager.yellowMaterial;
        TrafficUI.Instance.UpdateSprite(TrafficUI.TrafficColor.Yellow, TrafficUI.TrafficDirection.Straight);
    }
    #endregion
    
    #region RIGHT
    public void SetRightGreen(){
        isRightGreen = true;
        if(rightLightRenderer != null && manager != null) rightLightRenderer.material = manager.greenMaterial;
        TrafficUI.Instance.UpdateSprite(TrafficUI.TrafficColor.Green, TrafficUI.TrafficDirection.Right);
    }
    public void SetRightRed(){
        isRightGreen = false;
        if(rightLightRenderer != null && manager != null) rightLightRenderer.material = manager.redMaterial;
        TrafficUI.Instance.UpdateSprite(TrafficUI.TrafficColor.Red, TrafficUI.TrafficDirection.Right);
    }
    public void SetRightYellow(){
        if(rightLightRenderer != null && manager != null) rightLightRenderer.material = manager.yellowMaterial;
        TrafficUI.Instance.UpdateSprite(TrafficUI.TrafficColor.Yellow, TrafficUI.TrafficDirection.Right);
    }
    #endregion
    
    public bool IsStraightGreen() => isStraightGreen;
    public bool IsRightGreen() => isRightGreen;
    public int GetLightID() => lightID;
    public RoadDirection GetRoadDirection() => roadDirection;
    public void SetLayer(GameObject obj, int index){
        if(obj == null) return; 
        obj.layer = index; 
        foreach(Transform child in obj.transform) SetLayer(child.gameObject, index);
    }

    void OnMouseDown(){
        if(!GameManager.Instance.isGameInitialized || GameManager.Instance.isGameOver) return;
        if(PauseUI.Instance.IsPaused()) return;
        TrafficUI.Instance.ShowPrompt(this);
    }
}