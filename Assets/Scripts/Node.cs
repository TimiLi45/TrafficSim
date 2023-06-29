using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.InputSystem.HID;

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
    
    public void SetData(TrafficManager trafficManager ,Vector3 position)
    {
        connectedStreets = new List<GameObject>();
        this.trafficManager = trafficManager;
        nodeID = currentNodeID++;
        this.position = position;
        trafficManager.NodeList.Add(gameObject);
        // The sphere for rendering Nodes is generated here for now, since later it won't be done in this class at all, but in the InteractionManager as a Debug Render.
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.layer = LayerMask.NameToLayer("Ignore Raycast");
        sphere.name = "NodeSphere";
        sphere.transform.parent = gameObject.transform;
        sphere.GetComponent<SphereCollider>().enabled = false;
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
