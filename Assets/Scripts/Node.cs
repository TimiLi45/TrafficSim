using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class Node : MonoBehaviour
{
    TrafficManager trafficManager;

    private static int currentNodeID = 0;

    int nodeID;

    List<Street> connectedStreets;

    Vector3 position;

    GameObject sphere;

    public int NodeID
    {
        get { return nodeID; }
    }
    public List<Street> ConnectedStreets
    {
        get { return connectedStreets; }

    }
    public Vector3 Position
    {
        get { return position; }
    }
    public void GetData(TrafficManager trafficManager ,Vector3 position)
    {
        connectedStreets = new List<Street>();
        this.trafficManager = trafficManager;
        nodeID = currentNodeID++;
        this.position = position;
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.layer = LayerMask.NameToLayer("Ignore Raycast");
        sphere.name = "NodeSphere";
        sphere.transform.parent = gameObject.transform;
        sphere.GetComponent<SphereCollider>().enabled = false;
    }

    public void AddConnectedStreet(Street connectedStreet)
    {
        connectedStreets.Add(connectedStreet);
    }

    public void DeleteSphere()
    {
        Destroy(sphere);
    }

    public void RemoveConnectedStreet(Street connectedStreet)
    {
        connectedStreets.Remove(connectedStreet);
    }
}
