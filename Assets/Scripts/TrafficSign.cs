using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class TrafficSign : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("dada");
    }

    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("adad");
    }







    private void Start()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        Debug.Log(colliders.Length);



    }
}
