using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CarSpawner  : MonoBehaviour 
{
    [SerializeField, HideInInspector]
    GameObject trafficManager;
    [SerializeField, HideInInspector]
    Node connectedNode;
    [SerializeField, HideInInspector]
    Vector3 position;
    [SerializeField, HideInInspector]
    private float timeRemaining = 3;

    public Vector3 Position
    {
        get { return position; }
    }
    public void SetData(GameObject trafficManager, Vector3 position)
    {
        this.trafficManager = trafficManager;
        this.position = position;
        connectedNode = trafficManager.GetComponent<TrafficManager>().FindNodeWithPosition(position).GetComponent<Node>();
        // debug model, may be replaced later, or removed
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = connectedNode.Position;
        cylinder.name = "CarSpawnerCylinder";
        cylinder.transform.parent = gameObject.transform;
        cylinder.GetComponent<Collider>().enabled = false;
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
        if (!trafficManager.GetComponent<TrafficManager>().FindNodeWithPosition(position).Equals(connectedNode))
            connectedNode = trafficManager.GetComponent<TrafficManager>().FindNodeWithPosition(position).GetComponent<Node>();
        if (connectedNode == null) trafficManager.GetComponent<TrafficManager>().DeleteCarSpawner(gameObject);
    }   

    private void SpawnCar()
    {
        if (connectedNode == null) return;
        
        GameObject car = new("Car");
        car.AddComponent<Car>().SetData(trafficManager, connectedNode);
        car.transform.SetParent(trafficManager.transform.Find("Cars"), true);
    }
}
