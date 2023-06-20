using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    TrafficManager trafficManager;

    private static int currentNodeID = 0;


    int nodeID;
    List<Connection> connections;
    Vector3 position;

    public int NodeID
    {
        get { return nodeID; }
    }
    public List<Connection> Connetions
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
    }

    public void MakeConnection(Node a, Node b)
    {
        connections.Add(new Connection(Vector3.Distance(a.position, b.position), b));
    }
}
