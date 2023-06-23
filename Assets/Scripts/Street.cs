using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public class Street : MonoBehaviour
{
    TrafficManager trafficManager;

    private static int currentStreetID = 0;

    int streetID;

    Vector3 startPoint;
    Vector3 endPoint;
    List<Vector3> wayPoints;

    GameObject startNode;
    GameObject endNode;

    GameObject streetLine;
  
    public int StreetID
    {
        get { return streetID; }
    }
    public List<Vector3> WayPoints
    {
        get { return wayPoints; }
    }
    public GameObject StartNode
    {
        get { return startNode; }
    }
    public GameObject EndNode
    {
        get { return endNode; }
    }
    public void GetData(TrafficManager trafficManager, Vector3 startPoint, Vector3 endPoint)
    {
        this.trafficManager = trafficManager;
        this.startPoint = startPoint;
        this.endPoint = endPoint;
        wayPoints = new List<Vector3>();
        streetLine = new GameObject();
        streetID = currentStreetID++;

        bool generateStartWayPoint = false;
        bool generateEndWayPoint = false;

        GenerateNodes(ref generateStartWayPoint, ref generateEndWayPoint);

        GenerateWayPoints(trafficManager.WayPointDistance, generateStartWayPoint, generateEndWayPoint);

        this.startPoint = startNode.GetComponent<Node>().Position;
        this.endPoint = endNode.GetComponent<Node>().Position;

        startNode.GetComponent<Node>().AddConnectedStreet(this);
        endNode.GetComponent<Node>().AddConnectedStreet(this);

        LineRenderer renderedLine = streetLine.AddComponent<LineRenderer>();
        renderedLine.SetPosition(0, this.startPoint);
        renderedLine.SetPosition(1, this.endPoint);
        renderedLine.name = "StreetLine";
        streetLine.transform.parent = gameObject.transform;
        DetectIntersectionsInArea();
    }

    private void GenerateNodes(ref bool generateStartWayPoint, ref bool generateEndWayPoint)
    {
        if (trafficManager.FindNodeWithPosition(startPoint) == null)
        {
            startNode = new GameObject("Node");
            startNode.AddComponent<Node>().GetData(trafficManager, startPoint);
            startNode.transform.SetParent(trafficManager.transform.Find("Nodes").transform, true);
            generateStartWayPoint = true;
        }
        else { startNode = trafficManager.FindNodeWithPosition(startPoint); }

        if (trafficManager.FindNodeWithPosition(endPoint) == null)
        {
            endNode = new GameObject("Node");
            endNode.AddComponent<Node>().GetData(trafficManager, endPoint);
            endNode.transform.SetParent(trafficManager.transform.Find("Nodes").transform, true);
            generateEndWayPoint = true;
        }
        else { endNode = trafficManager.FindNodeWithPosition(endPoint); }
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
        foreach (GameObject street in trafficManager.GetComponent<TrafficManager>().StreetList)
        {
            if(street.Equals(this)) continue;
            foreach (Vector3 wayPoint in street.GetComponent<Street>().WayPoints)
            {
                foreach (Vector3 thisWayPoint in wayPoints)
                {
                    if (Vector3.Distance(wayPoint, thisWayPoint) <= .2f && 
                        !trafficManager.GetComponent<TrafficManager>().IsInDistance(wayPoint, startPoint, 2f) && 
                        !trafficManager.GetComponent<TrafficManager>().IsInDistance(wayPoint, endPoint, 2f))
                    {
                        Debug.Log("Connecting "+this.streetID+" and " + street.GetComponent<Street>().StreetID + " at " + wayPoint);
                        trafficManager.GetComponent<TrafficManager>().GenerateIntersection(gameObject, street, wayPoint);
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
        if (startNode.GetComponent<Node>().ConnectedStreets.Count <= 1)
        {
            startNode.GetComponent<Node>().DeleteSphere();
        }
        if (endNode.GetComponent<Node>().ConnectedStreets.Count <= 1)
        {
            endNode.GetComponent<Node>().DeleteSphere();
        }
    }
    private void DeleteWayPoints()
    {
        wayPoints.Clear();
    }
    private void DeleteLine()
    {
        Destroy(streetLine);
    }
}
