using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CarSpawner  : MonoBehaviour 
{
    TrafficManager trafficManager;
    Node connectedNode;
    bool active = false;
    Vector3 position;
    private float timeRemaining = 3;



    public void GetData(TrafficManager trafficManager, Vector3 position)
    {
        this.trafficManager = trafficManager;
        this.position = position;


        connectedNode = trafficManager.FindNodeWithPosition(position);
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = connectedNode.Position;



    }



    private void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            timeRemaining = 3;
            SpawnCar();
        }
    }   

    private void SpawnCar()
    {
        Debug.Log("Spawn car");
     

        GameObject car = new GameObject("Car");
        car.AddComponent<Car>().GetData(trafficManager, connectedNode);
        //Car carSkript = GetComponent<Car>();
        //carSkript.GetData(trafficManager, connectedNode);

    }


}
