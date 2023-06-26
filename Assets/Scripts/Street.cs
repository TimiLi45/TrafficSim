using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public class Street : MonoBehaviour
{
    [SerializeField, HideInInspector]
    TrafficManager trafficManager;

    [SerializeField, HideInInspector]
    private static int currentStreetID = 0;

    [SerializeField, HideInInspector]
    int streetID;

    [SerializeField, HideInInspector]
    Vector3 startPoint;
    [SerializeField, HideInInspector]
    Vector3 endPoint;
    [SerializeField, HideInInspector]
    List<Vector3> wayPoints;

    [SerializeField, HideInInspector]
    GameObject startNode;
    [SerializeField, HideInInspector]
    GameObject endNode;

    [SerializeField, HideInInspector]
    GameObject streetLine;
  
    public int StreetID
    {
        get { return streetID; }
    }
    public Vector3 StartPoint
    {
        get { return startPoint; }
    }
    public Vector3 EndPoint
    {
        get { return endPoint; }
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
    public void SetData(TrafficManager trafficManager, Vector3 startPoint, Vector3 endPoint)
    {
        this.trafficManager = trafficManager;
        this.startPoint = startPoint;
        this.endPoint = endPoint;
        wayPoints = new List<Vector3>();
        streetLine = new GameObject();
        streetID = currentStreetID++;
        // I have to check if I should generate WayPoints and the start and end of the street,
        // because if the street connects to another noder, it already has a WayPoint at that location.
        // I don't want to call GenerateWayPoints from GenerateNodes, so I have to have external variables.
        // In order to set 2 variables with 1 method call, I'm setting the variables via reference.
        // This is not clean code. This is not good practice. It is what it is.
        // I don't have time to search for a better solution.
        bool generateStartWayPoint = false;
        bool generateEndWayPoint = false;

        GenerateNodes(ref generateStartWayPoint, ref generateEndWayPoint);
        GenerateWayPoints(trafficManager.WayPointDistance, generateStartWayPoint, generateEndWayPoint);

        // I'm setting the start and end Point a second time here, because the Nodes may have merged with different Nodes,
        // in turn changeing their positions a bit. It's fine like this, at the beginning these Points are just for creating
        // the Nodes, now they can be called as an alternative to the Node position. This call is shorter and therefore better.
        this.startPoint = startNode.GetComponent<Node>().Position;
        this.endPoint = endNode.GetComponent<Node>().Position;

        GenerateModel();
    }

    private void GenerateNodes(ref bool generateStartWayPoint, ref bool generateEndWayPoint)
    {
        if (trafficManager.FindNodeWithPosition(startPoint) == null)
        {
            startNode = new GameObject("Node");
            startNode.AddComponent<Node>().SetData(trafficManager, startPoint);
            startNode.transform.SetParent(trafficManager.transform.Find("Nodes").transform, true);
            generateStartWayPoint = true;
        }
        else { startNode = trafficManager.FindNodeWithPosition(startPoint); }

        if (trafficManager.FindNodeWithPosition(endPoint) == null)
        {
            endNode = new GameObject("Node");
            endNode.AddComponent<Node>().SetData(trafficManager, endPoint);
            endNode.transform.SetParent(trafficManager.transform.Find("Nodes").transform, true);
            generateEndWayPoint = true;
        }
        else { endNode = trafficManager.FindNodeWithPosition(endPoint); }
        startNode.GetComponent<Node>().AddConnectedStreet(this);
        endNode.GetComponent<Node>().AddConnectedStreet(this);
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

    private void GenerateModel()
    {
        // Currently the model is just a line, will be changed later.
        LineRenderer renderedLine = streetLine.AddComponent<LineRenderer>();
        renderedLine.SetPosition(0, this.startPoint);
        renderedLine.SetPosition(1, this.endPoint);
        renderedLine.name = "StreetLine";
        streetLine.transform.parent = gameObject.transform;
    }

    public void DeleteStreetContents()
    {
        DeleteWayPoints();
        startNode.GetComponent<Node>().RemoveConnectedStreet(this);
        endNode.GetComponent<Node>().RemoveConnectedStreet(this);
        DeleteNodes();
        DeleteLine();
    }
    private void DeleteNodes()
    {
        if (startNode.GetComponent<Node>().ConnectedStreets.Count < 1)
        {
            startNode.GetComponent<Node>().DeleteSphere();
            Destroy(startNode);
        }
        if (endNode.GetComponent<Node>().ConnectedStreets.Count < 1)
        {
            endNode.GetComponent<Node>().DeleteSphere();
            Destroy(endNode);
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
