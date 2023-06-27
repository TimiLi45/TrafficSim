using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class TrafficSign : MonoBehaviour
{

    [SerializeField]
    int newMaxSpeed = 100;
    [SerializeField]
    int newStreet = -1;
    [SerializeField]
    int nodeID = -1;

    enum Sign
    {
        maxSpeed,
        forceStreet,
        STOP,
        dijkstra

    }

    [SerializeField]
    Sign sign = Sign.maxSpeed;

    private void OnTriggerEnter(Collider collider)
    {
        Car carComponente = collider.gameObject.GetComponent<Car>();

        if (carComponente != null ) 
        {
           switch (sign){
                case Sign.maxSpeed:
                    carComponente.SetMaxSpeed(newMaxSpeed);
                    break;
                case Sign.forceStreet:
                    carComponente.ForcesStreetID = newStreet;
                    break;
                case Sign.STOP:
                    carComponente.Stop();
                    break;
                case Sign.dijkstra:
                    carComponente.Dijkstra(nodeID);
                    


            }

        }
        
    }







    private void Start()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        Debug.Log(colliders.Length);



    }
}
