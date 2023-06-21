using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

        this.startPoint = startNode.Position;
        this.endPoint = endNode.Position;

        GenerateWayPoints(_trafficManager.WayPointDistance);

        DetectIntersectionsInArea();
    }

    private void GenerateWayPoints(float spacing)
    {
        wayPoints = new List<Vector3>();
        Vector3 previousPoint = startPoint;
        //START POINT NUR GENERIEREN WENN NOCH KEINER DA IST !!HINUÜFÜUGEN
        wayPoints.Add(startPoint);
        for(float i = spacing; i < Vector3.Distance(startPoint, endPoint) - spacing; i += spacing)
        { 
            Vector3 direction = (endPoint - startPoint).normalized;
            wayPoints.Add(previousPoint + direction * spacing);
            previousPoint = previousPoint + direction * spacing;
        }
        wayPoints.Add(endPoint);
    }

    private void DetectIntersectionsInArea()
    {
        List<Vector3> allIntersections = new List<Vector3>();
        List<Vector3> intersections = new List<Vector3>();
        foreach (Street street in _trafficManager.StreetList)
        {
            foreach (Vector3 wayPoint in street.WayPoints)
            {
                foreach(Vector3 thisWayPoint in wayPoints)
                {
                    if (Vector3.Distance(wayPoint, thisWayPoint) <= .2f)
                    {
                        allIntersections.Add(wayPoint);
                    }
                }
            }
        }
        //löscht manche intersections unnötig
        for (int i = 0; i < allIntersections.Count; i++)
        {
            Debug.Log("All:"+allIntersections[i]);
        }
        for (int i = 0; i < allIntersections.Count; i++)
        {
            if (i != allIntersections.Count - 1 && Vector3.Distance(allIntersections[i], allIntersections[i + 1]) > .3f)
            {
                intersections.Add(allIntersections[i]);
            }
            if(i == allIntersections.Count - 1 && Vector3.Distance(allIntersections[0], allIntersections[i])>.3f)
            {
                intersections.Add(allIntersections[i]);
            }
        }
        for(int i=0; i < intersections.Count; i++)
        {
            Debug.Log(intersections[i]);
        }
        //_trafficManager.GenerateIntersection(this, street);

    }

    private bool CCW(Vector3 A, Vector3 B, Vector3 C)
    {
        return (C.z-A.z)*(B.x-A.x)>(B.z-A.z)*(C.x-A.x);
    }

    private bool Intersects(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        return CCW(A, C, D) != CCW(B, C, D) && CCW(A, B, C) != CCW(A, B, D);
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
