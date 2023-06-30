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

    [SerializeField, HideInInspector]
    GameObject cylinder;

    public Vector3 Position
    {
        get { return position; }
    }
    public void SetData(GameObject trafficManager, Vector3 position)
    {
        this.trafficManager = trafficManager;
        connectedNode = trafficManager.GetComponent<TrafficManager>().FindNodeWithPosition(position).GetComponent<Node>();
        this.position = connectedNode.Position;
        gameObject.transform.position = connectedNode.Position;
        GenerateRayCastHitCylinder();
        // debug model, may be replaced later, or removed
        cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = connectedNode.Position;
        cylinder.name = "CarSpawnerCylinder";
        cylinder.transform.parent = gameObject.transform;
        cylinder.GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV();
        cylinder.GetComponent<Collider>().enabled = false;
    }

    private void GenerateRayCastHitCylinder()
    {
        gameObject.AddComponent<CapsuleCollider>();
        gameObject.GetComponent<CapsuleCollider>().isTrigger = true;
        gameObject.GetComponent<CapsuleCollider>().radius = 1.01f;
        gameObject.GetComponent<CapsuleCollider>().height = 3.5f;
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
    
    // If this works you are lucky!
    private void SpawnCar()
    {
        if (connectedNode == null) return;
        
        GameObject car = new("Car");
        car.transform.position = gameObject.transform.position;
        car.AddComponent<Car>().SetData(trafficManager, connectedNode, cylinder.GetComponent<Renderer>().material.color);
        car.transform.SetParent(trafficManager.transform.Find("Cars"), true);
        car.tag = "Car";
    }
}
