using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField, HideInInspector]
    TrafficManager trafficManager;

    [SerializeField, HideInInspector]
    private static int currentNodeID = 0;

    [SerializeField, HideInInspector]
    int nodeID;

    [SerializeField, HideInInspector]
    List<GameObject> connectedStreets;

    [SerializeField, HideInInspector]
    Vector3 position;

    [SerializeField, HideInInspector]
    GameObject sphere;

    [SerializeField, HideInInspector]
    Material streetMaterial;

    public int NodeID
    {
        get { return nodeID; }
    }
    public List<GameObject> ConnectedStreets
    {
        get { return connectedStreets; }

    }
    public Vector3 Position
    {
        get { return position; }
    }
    
    public void SetData(TrafficManager trafficManager, Vector3 position, Material streetMaterial)
    {
        connectedStreets = new List<GameObject>();
        this.trafficManager = trafficManager;
        nodeID = currentNodeID++;
        this.position = position;
        this.streetMaterial = streetMaterial;
        trafficManager.NodeList.Add(gameObject);
        GenerateRayCastHitSphere();
        // The sphere for rendering Nodes is generated here for now, since later it won't be done in this class at all, but in the InteractionManager as a Debug Render.
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = new Vector3(trafficManager.NodeSphereSize, trafficManager.NodeSphereSize, trafficManager.NodeSphereSize);
        sphere.layer = LayerMask.NameToLayer("Ignore Raycast");
        sphere.name = "NodeSphere";
        sphere.transform.parent = gameObject.transform;
        sphere.GetComponent<Renderer>().material = streetMaterial;
        sphere.GetComponent<SphereCollider>().enabled = false;
    }

    private void GenerateRayCastHitSphere()
    {
        gameObject.AddComponent<SphereCollider>();
        gameObject.GetComponent<SphereCollider>().isTrigger = true;
        gameObject.GetComponent<SphereCollider>().radius = (float)(trafficManager.NodeSphereSize - 0.1 * (trafficManager.NodeSphereSize));
        gameObject.GetComponent<SphereCollider>().center = new Vector3(0f,.5f,0f);
    }

    public void AddConnectedStreet(GameObject connectedStreet)
    {
        connectedStreets.Add(connectedStreet);
    }

    public void DeleteSphere()
    {
        Destroy(sphere);
    }

    public void RemoveConnectedStreet(GameObject connectedStreet)
    {
        connectedStreets.Remove(connectedStreet);
    }
}
