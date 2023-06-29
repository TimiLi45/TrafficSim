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
    //[SerializeField, HideInInspector]
    int value = 0;
    
    //[SerializeField, HideInInspector]
    TrafficSignTypes selectedType = TrafficSignTypes.maxSpeed;

    public void SetData(TrafficSignTypes type, int value)
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
