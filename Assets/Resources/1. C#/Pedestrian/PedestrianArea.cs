using UnityEngine;

public class PedestrianArea : MonoBehaviour
{
    [SerializeField] private BoxCollider areaCollider;

    void Start(){
        if(areaCollider == null) areaCollider = GetComponent<BoxCollider>();
    }

    public Vector3 GetRandomPoint(){
        Bounds bounds = areaCollider.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(randomX, bounds.max.y, randomZ);
    }
}