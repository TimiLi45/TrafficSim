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
    GameObject trafficManager;
    Node connectedNode;

    Vector3 position;
    private float timeRemaining = 3;

    public void GetData(GameObject trafficManager, Vector3 position)
    {
        this.trafficManager = trafficManager;
        this.position = position;

        connectedNode = trafficManager.GetComponent<TrafficManager>().FindNodeWithPosition(position).GetComponent<Node>();
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = connectedNode.Position;
        cylinder.name = "CarSpawnerCylinder";
        cylinder.transform.parent = gameObject.transform;
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
        GameObject car = new GameObject("Car");
        car.AddComponent<Car>().GetData(trafficManager, connectedNode);
        car.transform.SetParent(trafficManager.transform.Find("Cars"), true);
    }
}
