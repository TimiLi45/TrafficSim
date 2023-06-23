using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TrafficSign: MonoBehaviour
{

    
    
    
    
    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log(collider);
    }
}
