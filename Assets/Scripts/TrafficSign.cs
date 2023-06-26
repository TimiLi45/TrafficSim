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

    enum Sign
    {
        maxSpeed,
        forceStreet
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
                    


            }

        }
        
    }







    private void Start()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        Debug.Log(colliders.Length);



    }
}
