using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Car : MonoBehaviour
{
    readonly TrafficManager _trafficManager;

    bool    brake = false;
    int     acceleration = 15;
    int     deceleration = 15;
    int     maxSpeed = 120;
    float   speed = 0;

    int targetNodeID = -1;
    Node targetNode;
    Node currentNode;
    //Vector3 targetLocation;
    //Vector3[] Waypoints;

    Street currentStreet;

    private void Update()
    {
            
    }

    private void RandomStreet()
    {
        Random random = new();
        List<Connection> possibleConnections = currentNode.Connetions;
        foreach(Connection connection in possibleConnections)
        {
            if(connection.EndNode.NodeID == currentNode.NodeID)
                possibleConnections.Remove(connection);
        }
        if(possibleConnections.Count == 0)
        {
            GameObject.Destroy(this);
            return;
        }
        targetNode = possibleConnections[random.Next(possibleConnections.Count)].EndNode;
    }
}
