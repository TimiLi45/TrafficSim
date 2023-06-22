using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class Node
{
    TrafficManager trafficManager;

    private static int currentNodeID = 0;

    int nodeID;

    List<Connection> connections;

    Vector3 position;

    GameObject sphere;

    public int NodeID
    {
        get { return nodeID; }
    }
    public List<Connection> Connections
    {
        get { return connections; }

    }
    public Vector3 Position
    {
        get { return position; }
    }
    public Node(TrafficManager trafficManager ,Vector3 position)
    {
        connections = new List<Connection>();
        this.trafficManager = trafficManager;
        nodeID = currentNodeID++;
        this.position = position;
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.layer = LayerMask.NameToLayer("Ignore Raycast");
        sphere.name = "node";
    }

    public void MakeConnection(Node node, Street connectedStreet)
    {
        connections.Add(new Connection(Vector3.Distance(this.position, node.Position), node, connectedStreet));
    }

    public void DeleteSphere()
    {
        UnityEngine.Object.Destroy(sphere);
    }

    public void DeleteConnection(Connection connection)
    {
        connections.Remove(connection);
    }
}
