using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Street
{
    readonly TrafficManager _trafficManager;

    private static int currentStreetID = 0;

    int streetID;

    Vector3 startPoint;
    Vector3 endPoint;
    List<Vector3> wayPoints;

    Node startNode;
    Node endNode;

    public int StreetID
    {
        get { return streetID; }
    }
    public List<Vector3> WayPoints
    {
        get { return wayPoints; }
    }
    public Node StartNode
    {
        get { return startNode; }
    }
    public Node EndNode
    {
        get { return endNode; }
    }

    public Street(TrafficManager _trafficManager ,Vector3 startPoint, Vector3 endPoint) {
        this._trafficManager = _trafficManager;
        this.startPoint = startPoint;   
        this.endPoint = endPoint;

        streetID = currentStreetID++;

        if (_trafficManager.FindNodeWithPosition(startPoint) == null)
            startNode = new Node(_trafficManager, startPoint);
        else
            startNode = _trafficManager.FindNodeWithPosition(startPoint);

        if (_trafficManager.FindNodeWithPosition(endPoint) == null)
            endNode = new Node(_trafficManager, endPoint);
        else
            endNode = _trafficManager.FindNodeWithPosition(endPoint);

        startNode.MakeConnection(endNode, this);
        endNode.MakeConnection(startNode, this);

        GenerateWayPoints(_trafficManager.WayPointDistance);
    }

    private void GenerateWayPoints(float spacing)
    {
        wayPoints = new List<Vector3>();
        Vector3 previousPoint = startPoint;

        wayPoints.Add(startPoint);
        for(float i = spacing; i < Vector3.Distance(startPoint, endPoint) - spacing; i += spacing)
        { 
            Vector3 direction = (endPoint - startPoint).normalized;
            wayPoints.Add(previousPoint + direction * spacing);
            previousPoint = previousPoint + direction * spacing;
        }
        wayPoints.Add(endPoint);
    }

    public void DeleteNodes()
    {
        //eventuell kein if statement nötig, requires testing
        Warning.Info("If statement nötigkeit überprüfen");
        if (startNode.Connetions.Count <= 1)
        {
            startNode = null;
        }
        if(endNode.Connetions.Count <= 1)
        {
            endNode = null;
        }
    }
}
