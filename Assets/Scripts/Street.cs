using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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

    GameObject streetLine;
  
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
    public Street(TrafficManager _trafficManager, Vector3 startPoint, Vector3 endPoint)
    {
        this._trafficManager = _trafficManager;
        this.startPoint = startPoint;
        this.endPoint = endPoint;
        wayPoints = new List<Vector3>();
        streetLine = new GameObject();
        streetID = currentStreetID++;

        bool generateStartWayPoint = false;
        bool generateEndWayPoint = false;

        if (_trafficManager.FindNodeWithPosition(startPoint) == null)
        {
            startNode = new Node(_trafficManager, startPoint);
            generateStartWayPoint = true;
        }
        else { startNode = _trafficManager.FindNodeWithPosition(startPoint); }

        if (_trafficManager.FindNodeWithPosition(endPoint) == null)
        {
            endNode = new Node(_trafficManager, endPoint);
            generateEndWayPoint = true;
        }
        else { endNode = _trafficManager.FindNodeWithPosition(endPoint); }

        GenerateWayPoints(_trafficManager.WayPointDistance, generateStartWayPoint, generateEndWayPoint);

        this.startPoint = startNode.Position;
        this.endPoint = endNode.Position;

        startNode.MakeConnection(endNode, this);
        endNode.MakeConnection(startNode, this);

        LineRenderer renderedLine = streetLine.AddComponent<LineRenderer>();
        renderedLine.SetPosition(0, this.startPoint);
        renderedLine.SetPosition(1, this.endPoint);
        renderedLine.name = "Street";

        DetectIntersectionsInArea();
    }

    private void GenerateWayPoints(float spacing, bool generateStartWayPoint = true, bool generateEndWayPoint = true)
    {
        Vector3 previousPoint = startPoint;
        if(generateStartWayPoint) wayPoints.Add(startPoint);
        for (float i = spacing; i < Vector3.Distance(startPoint, endPoint) - spacing; i += spacing)
        { 
            Vector3 direction = (endPoint - startPoint).normalized;
            wayPoints.Add(previousPoint + direction * spacing);
            previousPoint = previousPoint + direction * spacing;
        }
        if (generateEndWayPoint) wayPoints.Add(endPoint);
    }

    private void DetectIntersectionsInArea()
    {
        foreach (Street street in _trafficManager.StreetList)
        {
            if(street.Equals(this)) continue;
            foreach (Vector3 wayPoint in street.WayPoints)
            {
                foreach (Vector3 thisWayPoint in wayPoints)
                {
                    if (Vector3.Distance(wayPoint, thisWayPoint) <= .2f && 
                        !_trafficManager.IsInDistance(wayPoint, startPoint, 2f) && 
                        !_trafficManager.IsInDistance(wayPoint, endPoint, 2f))
                    {
                        Debug.Log("Connecting "+this.streetID+" and " + street.StreetID + " at " + wayPoint);
                        _trafficManager.GenerateIntersection(this, street, wayPoint);
                        return;
                    }
                }
            }
        }
    }

    public void DeleteStreetContents()
    {
        DeleteWayPoints();
        DeleteNodes();
        DeleteLine();
    }
    private void DeleteNodes()
    {
        if (startNode.Connections.Count <= 1)
        {
            startNode.DeleteSphere();
        }
        if (endNode.Connections.Count <= 1)
        {
            endNode.DeleteSphere();
        }
    }
    private void DeleteWayPoints()
    {
        wayPoints.Clear();
    }
    private void DeleteLine()
    {
        UnityEngine.Object.Destroy(streetLine);
    }
}
