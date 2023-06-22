using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection
{
    float cost;
    Node endNode;
    Street connectedStreet;

    public Street ConnectedStreet{
    get { return connectedStreet; }
    }

    public Node EndNode
    {
        get { return endNode; }
    }

    public Connection(float cost, Node endNode, Street connectedStreet)
    {
        this.cost = cost;
        this.endNode = endNode;
        this.connectedStreet = connectedStreet;
    }
}
