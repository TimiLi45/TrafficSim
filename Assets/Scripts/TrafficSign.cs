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
    [SerializeField, HideInInspector]
    TrafficManager trafficManager;

    [SerializeField, HideInInspector]
    int value = 0;
    
    [SerializeField, HideInInspector]
    TrafficSignTypes selectedType = TrafficSignTypes.maxSpeed;

    public void SetData(TrafficManager trafficManager, TrafficSignTypes type, int value)
    {
        this.trafficManager = trafficManager;
        selectedType = type;
        this.value = value;
        GenerateRayCastHitCylinder();
    }

    private void GenerateRayCastHitCylinder()
    {
        gameObject.AddComponent<CapsuleCollider>();
        gameObject.GetComponent<CapsuleCollider>().isTrigger = true;
        gameObject.GetComponent<CapsuleCollider>().radius = 1;
        gameObject.GetComponent<CapsuleCollider>().height = 4f;
    }

    private void OnTriggerEnter(Collider collider)
    {
        Car collidedCar = collider.gameObject.GetComponent<Car>();
        if (collidedCar == null) return;

        switch (selectedType)
        {
            case TrafficSignTypes.maxSpeed:
                collidedCar.MaxSpeed = value;
                break;
            case TrafficSignTypes.forceStreet:
                collidedCar.ForcedStreetID = value;
                break;
            case TrafficSignTypes.STOP:
                collidedCar.CurrentBehaviour = CarBehaviour.stop;
                break;
            case TrafficSignTypes.Dijkstra:
                collidedCar.Dijkstra(value);
                break;
        }
    }
}
