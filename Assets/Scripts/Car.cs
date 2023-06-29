using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public struct Path
{
    private GameObject targetNode;
    private float cost;
    private GameObject previousNode;

    public GameObject TargetNode
    {
        readonly get { return targetNode; }
        set { targetNode = value; }
    }
    public float Cost
    {
        get { return cost; }
        set { cost = value; }
    }
    public GameObject PreviousNode
    {
        readonly get { return previousNode; }
        set { previousNode = value; }
    }



}

public enum CarBehaviour
{
    none,
    drive,
    stop
}

public class Car : MonoBehaviour
{
    [SerializeField, HideInInspector]
    GameObject trafficManager;

    [SerializeField, HideInInspector]
    CarBehaviour currentBehaviour = CarBehaviour.none;

    [SerializeField, HideInInspector]
    GameObject cube;
    [SerializeField, HideInInspector]
    BoxCollider boxCollider;

    [SerializeField, HideInInspector]
    float stopTimeLeft = 1f;
    [SerializeField, HideInInspector]
    float acceleration = .05f;
    [SerializeField, HideInInspector]
    float deceleration = 0.1f;
    [SerializeField, HideInInspector]
    float speed = 0f;
    [SerializeField, HideInInspector]
    float maxSpeed = 10f;
    [SerializeField, HideInInspector]
    float maxDistanceFromNextNodeBeforeSwitching = 1f;

    [SerializeField, HideInInspector]
    float maxRangeOfSight = 10f;
    [SerializeField, HideInInspector]
    float reactionTime = 2f;
    [SerializeField, HideInInspector]
    float distanceFromNextCar = 2f;

    [SerializeField, HideInInspector]
    int currentWayPointListPosition;
    [SerializeField, HideInInspector]
    int forcedStreetID;


    [SerializeField, HideInInspector]
    Vector3 targetLocation;
    [SerializeField, HideInInspector]
    List<Vector3> Waypoints = new();


    [SerializeField, HideInInspector]
   GameObject lastStreet;
    [SerializeField, HideInInspector]
    GameObject currentStreet;
    [SerializeField, HideInInspector]
    List<GameObject> visitedStreets = new();

    [SerializeField, HideInInspector]
    Node currentStartNode;
    [SerializeField, HideInInspector]
    Node lastStartNode;
    [SerializeField, HideInInspector]
    Node currentEndNode;
    [SerializeField, HideInInspector]
    Node lastEndNode;


    //[SerializeField, HideInInspector]
    //List<Node> notVisitedNodes = new();
    //[SerializeField, HideInInspector]
    //List<Node> visitedNodes = new();

    [SerializeField, HideInInspector]
    Path[] pathList = new Path[100];

    float pathCost = 0;

    List<GameObject> pathsToVisit = new();
    List<GameObject> vistetPaths = new();
    //List<int> vistetNodes = new();
    ////List<int> toVistNodes = new();

    public int ForcedStreetID
    {
        get { return forcedStreetID; }
        set { forcedStreetID = value; }
    }
    public float MaxSpeed
    {
        get { return maxSpeed; }
        set { maxSpeed = value; }
    }
    public CarBehaviour CurrentBehaviour
    {
        get { return currentBehaviour; }
        set { currentBehaviour = value; }
    }

    public void SetData(GameObject trafficManager, Node startNode)
    {
        if (startNode == null) DeleteSelf();
        this.trafficManager = trafficManager;
        forcedStreetID = -1;

        foreach (GameObject connectedStreet in startNode.GetComponent<Node>().ConnectedStreets)
        {
            if (connectedStreet == null) continue;
            currentStreet = connectedStreet;

            if (currentStreet.GetComponent<Street>().StartNode.Equals(startNode.gameObject))
            {
                transform.position = currentStreet.GetComponent<Street>().StartNode.GetComponent<Node>().Position;
                targetLocation = currentStreet.GetComponent<Street>().EndNode.GetComponent<Node>().Position;
                break;
            }
        }
        if (currentStreet == null) DeleteSelf();

        AddMesh();
        currentBehaviour = CarBehaviour.drive;
    }

    private void Update()
    {
        switch (currentBehaviour)
        {
            case CarBehaviour.none:
                break;
            case CarBehaviour.drive:
                Accelerate();
                break;
            case CarBehaviour.stop:
                Decelerate();
                if (stopTimeLeft <= 0f && speed <= 0f)
                {
                    currentBehaviour = CarBehaviour.drive;
                    stopTimeLeft = 1f;
                }
                else
                {
                    stopTimeLeft -= Time.deltaTime;
                }
                break;
        }

        // replace later with model
        if (cube != null)
        {
            cube.transform.position = transform.position;
            boxCollider.transform.position = transform.position;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetLocation, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetLocation) < maxDistanceFromNextNodeBeforeSwitching)
            SetTargetToNextWaypointOrStreet();
    }

    private void AddMesh()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "CarCube";
        cube.transform.parent = gameObject.transform;
        cube.GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV();
        boxCollider = this.AddComponent<BoxCollider>();
        gameObject.AddComponent<Rigidbody>();
    }

    private void SetTargetToNextWaypointOrStreet()
    {
        if (currentWayPointListPosition + 1 < Waypoints.Count)
        {
            currentWayPointListPosition++;
            targetLocation = Waypoints[currentWayPointListPosition];
        }
        else
        {
            if (FindNextStreet()) targetLocation = Waypoints[0];
            currentWayPointListPosition = 0;
        }
    }

    private bool FindNextStreet()
    {
        lastStreet = currentStreet;
        lastStartNode = currentStartNode;
        lastEndNode = currentEndNode;

        currentStreet = FindNextConnectedStreet();

        if (currentStreet == lastStreet || currentStreet == null)
        {
            DeleteSelf();
            return false;
        }

        visitedStreets.Add(currentStreet);

        currentStartNode = currentStreet.GetComponent<Street>().StartNode.GetComponent<Node>();
        currentEndNode = currentStreet.GetComponent<Street>().EndNode.GetComponent<Node>();

        Waypoints.Clear();
        Waypoints.Add(currentStartNode.Position);
        Waypoints.Add(currentEndNode.Position);
        return true;
    }

    private GameObject FindNextConnectedStreet()
    {
        if (forcedStreetID >= 0)
        {
            foreach (GameObject connectedStreet in currentStreet.GetComponent<Street>().EndNode.GetComponent<Node>().ConnectedStreets)
            {
                if (connectedStreet.GetComponent<Street>().StreetID == forcedStreetID)
                {
                    forcedStreetID = -1;
                    return connectedStreet;
                }
            }
        }

        List<GameObject> availableStreets = new();
        foreach (GameObject connectedStreet in currentStreet.GetComponent<Street>().EndNode.GetComponent<Node>().ConnectedStreets)
        {
            if (connectedStreet.GetComponent<Street>().StreetID == currentStreet.GetComponent<Street>().StreetID) continue;
            if (!visitedStreets.Contains(connectedStreet)) availableStreets.Add(connectedStreet);
        }
        
        if (availableStreets.Count <= 0) return null;

        // Can later be used, when end points or a goal exist, where a car will be deleted.
        // If this is the case, then there is no use for destroying the car when it has visited all available streets,
        // because it should be allowed to backtrack and find something else. This does need work tho, since now it
        // would just turn around and drive randomly from there, when it in fact should go back to its start,
        // or the street it visited first. The code also doesn't work, since it currently returns the street it visited
        // first, when it should return the street it visited first from the connected streets to the car's current node.
        //
        //if (availableStreets.Count <= 0)
        //{
        //    Street nextStreet = visitedStreets[0];
        //    visitedStreets.Clear();
        //    return nextStreet;
        //}

        Random random = new();
        int randomIndex = random.Next(0, availableStreets.Count - 1);
        return availableStreets[randomIndex];    }

    private void Accelerate()
    {
        if (speed < maxSpeed) speed += acceleration;
        else speed -= deceleration;
    }

    private void Decelerate()
    {
        if (speed > 0) speed -= deceleration;
        if (speed - deceleration < 0) speed = 0;
    }

    public void Dijkstra(int nodeID)
    {
        //Debug.Log(trafficManager.GetComponent<TrafficManager>().NodeList.Count);
        //Pathlist startEntry = new Pathlist();
        //startEntry.TargetNode = currentEndNode.GetComponent<Node>();
        //startEntry.PreviousNode = null;
        //startEntry.Cost = 0;
        //listOfPaths.Add(startEntry);
        //// erstelle eine Liste mit den jeweils kürzersten routen zwischen 2 Punkten
        //foreach (GameObject item in trafficManager.GetComponent<TrafficManager>().NodeList)
        //{
        //    FindNodsFromNode(item.GetComponent<Node>());
        //}
        //foreach (var item in pathList)
        //{
        //    if (item.PreviousNode != null)
        //        Debug.Log("TaretNode: " + item.TargetNode.GetComponent<Node>().NodeID + " with Cost: " + item.Cost + "over Node: " + item.PreviousNode.GetComponent<Node>().NodeID);
        //}


        // Fügt die jetzige Node den untersuchten hinzu u
        //int safty = 100;
        //FindNodsFromNode(currentEndNode.GetComponent<Node>());
        //do
        //{
        //    pathCost += FindPathCostByTargetNodeID(targetNodeID);
        //    Debug.Log(pathCost);
        //    currentlyViewedNode = FindPrevoiusNodeOfPathByTargetNodeID(targetNodeID);
        //    safty--;
        //} while (nodeID != currentlyViewedNode.GetComponent<Node>().NodeID && safty > 0);


        int safty = 100;
        List<GameObject> pathList = new();




        // Die Straße die sich gerade Angeschaut wird
        GameObject currentViewdStreet = currentStreet;
        // Create Start Index

        Path startEntry = new Path();
        startEntry.TargetNode = currentStreet.GetComponent<Street>().EndNode;
        startEntry.PreviousNode = null;
        startEntry.Cost = 0;
        VisitStreet(currentViewdStreet);

        do
        {
            currentViewdStreet = FindNewStreet();
            VisitStreet(currentViewdStreet);
            safty--;
        } while (nodeID != currentViewdStreet.GetComponent<Street>().EndNode.GetComponent<Node>().NodeID && safty > 0);

        if (safty > 0)
        {
            Debug.Log("Error No Path found");
        }
        else
        {
            Debug.Log("Create Path" + vistetPaths.Count);
        }











    }

    //private void FindNodsFromNode(Node node)
    //{
    //    // Wenn die Node in der Liste der Liste besuchten Nodes auftaucht
    //    bool foundInVisited = false;
    //    foreach (var item in visitedNodes)
    //    {
    //        if (item == node.GetComponent<Node>())
    //        {
    //            foundInVisited = true;
    //            break;
    //        }
    //    }
    //    if (foundInVisited) { return; }
    //    visitedNodes.Add(node);

    //    // Schaut sich die Angeschlossenen nodes hinzu speichert die niedrigste cost
    //    float lowestCost = 99999999999999999; // ignore that shit
    //    GameObject nodeWithLowestCost = null;


    //    foreach (Street connectedStreet in node.GetComponent<Node>().ConnectedStreets)
    //    {
    //        if (connectedStreet.EndNode.GetComponent<Node>() != node.GetComponent<Node>())
    //        {
    //            // Wenn die jetzige straße einen niedrigeren Wert als die bisherige
    //            if (connectedStreet.cost < lowestCost)
    //            {
    //                lowestCost = connectedStreet.cost;
    //                nodeWithLowestCost = connectedStreet.EndNode.GetComponent<Node>();
    //            }

    //            // Wenn die Node in der Liste der noch Nicht Besuchten Nodes auftauch
    //            bool foundInNotVisited = false;
    //            foreach (var item in visitedNodes)
    //            {
    //                if (item == connectedStreet.EndNode.GetComponent<Node>())
    //                {
    //                    foundInNotVisited = true;
    //                    break;
    //                }
    //            }

    //            // Wenn Die Node in keine der Listen auftaucht füge sie ein
    //            if (!foundInNotVisited)
    //            {
    //                notVisitedNodes.Add(connectedStreet.EndNode.GetComponent<Node>());

    //            }
    //        }
    //    }

    //    if(nodeWithLowestCost != null) 
    //    { 
    //    Path entry = new();
    //    entry.TargetNode = nodeWithLowestCost;
    //    entry.PreviousNode = node.GetComponent<Node>();
    //    entry.Cost = lowestCost;
    //    pathList.Add(entry);
    //    }

    //}

    //private Node FindPrevoiusNodeOfPathByTargetNodeID(int targetNodeID)
    //{
    //    foreach (Path path in pathList)
    //    {
    //        if (path.TargetNode.GetComponent<Node>().NodeID == targetNodeID)
    //            return path.PreviousNode.GetComponent<Node>();
    //    }
    //    return null;
    //}

    //private float FindPathCostByTargetNodeID(int targetNodeID)
    //{
    //    foreach (Path path in pathList)
    //    {
    //        if (path.TargetNode.NodeID == targetNodeID)
    //        {
    //            return path.Cost;
    //        }
    //    }
    //    return 0f;
    //}


    private GameObject FindNodeByID(int targetNodeID)
    {
        foreach (var item in trafficManager.GetComponent<TrafficManager>().NodeList)
        {
            if (item.GetComponent<Node>().NodeID == targetNodeID)
            {
                return item;
            }
        }
        return null;
    }

    private void UpdateIndexForPathList(GameObject street)
    {

        int targetNodeID = street.GetComponent<Street>().EndNode.GetComponent<Node>().NodeID;
        int startNodeID = street.GetComponent<Street>().StartNode.GetComponent<Node>().NodeID;
        float Cost = street.GetComponent<Street>().cost;


        for (int i = 0; i < pathList.Length; i++)
        {
            if (pathList[i].TargetNode != null)
            {

                if (pathList[i].TargetNode.GetComponent<Node>().NodeID == targetNodeID && pathCost + Cost < pathList[i].Cost)
                {
                    pathList[i].Cost = pathCost + Cost;
                    pathList[i].PreviousNode = FindNodeByID(startNodeID);
                    return;
                }
            }
        }

        Path entry = new Path();
        entry.TargetNode = FindNodeByID(targetNodeID);
        entry.PreviousNode = FindNodeByID(startNodeID); ;
        entry.Cost = Cost;
        vistetPaths.Add(street);




    }

    private void FindPathsOnNode(GameObject Node)
    {
        foreach (GameObject street in Node.GetComponent<Node>().ConnectedStreets)
        {
            UpdateIndexForPathList(street);
        }
    }


    private void VisitStreet(GameObject currentStreet)
    {
        // Get the Node


        int streetEndPointID = currentStreet.GetComponent<Street>().EndNode.GetComponent<Node>().NodeID;
        GameObject currentNode = FindNodeByID(streetEndPointID);
        FindPathsOnNode(currentNode);
        //Add to Visited Nodes
        vistetPaths.Add(currentStreet);
    }

    private GameObject FindNewStreet()
    {

        GameObject currentSelectedStreet = null;
        bool found = false;
        foreach (GameObject pathToVisit in pathsToVisit)
        {

            foreach (GameObject pathVisited in vistetPaths)
            {
                if (pathToVisit == pathVisited)
                {
                    found = true;
                    break;

                }
            }
            if (found)
            {
                found = false;
            }
            else
            {
                if (currentSelectedStreet == null)
                {
                    currentSelectedStreet = pathToVisit;
                }
                else
                {
                    if (currentSelectedStreet.GetComponent<Street>().cost > pathToVisit.GetComponent<Street>().cost)
                    {
                        currentSelectedStreet = pathToVisit;
                        // Erhöhe Cost
                        pathCost = pathCost + currentSelectedStreet.GetComponent<Street>().cost;

                    }
                }
            }
        }

        return currentSelectedStreet;
    }


    private void DeleteSelf()
    {
        Destroy(cube);
        Destroy(gameObject);
    }



}