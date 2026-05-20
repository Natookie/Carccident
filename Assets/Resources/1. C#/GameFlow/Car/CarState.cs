using UnityEngine;

public class CarState
{
    public Vector3 LastPosition { get; private set; }
    public void UpdateLastPosition(Vector3 position) => LastPosition = position;
    public void Reset() => LastPosition = Vector3.zero;
    
}