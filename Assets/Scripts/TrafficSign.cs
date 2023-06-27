using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
public enum TrafficSignTypes
{
    maxSpeed,
    forceStreet,
    STOP,
    Dijkstra
}

public class TrafficSign : MonoBehaviour
{
    [SerializeField]
    int value = 0;
    [SerializeField]
    int nodeID = -1;
    
    [SerializeField]
    TrafficSignTypes selectedType = TrafficSignTypes.maxSpeed;

    public void GetData(TrafficSignTypes type, int value)
    {
        selectedType = type;
        this.value = value;
    }

    private void OnTriggerEnter(Collider collider)
    {
        Car collidedCar = collider.gameObject.GetComponent<Car>();
        if (collidedCar == null) return;

        switch (selectedType)
        {
            case TrafficSignTypes.maxSpeed:
                collidedCar.SetMaxSpeed(value);
                break;
            case TrafficSignTypes.forceStreet:
                collidedCar.ForcesStreetID = value;
                break;
            case TrafficSignTypes.STOP:
                collidedCar.Stop();
                break;
            case TrafficSignTypes.Dijkstra:
                collidedCar.Dijkstra(value);
                break;
        }
    }

    private void Start()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
    }
}
