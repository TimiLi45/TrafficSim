using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
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
    bool hitOtherCar;
    [SerializeField, HideInInspector]
    float timeBevoreSelfDeleteAfterHit = 10f;
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
    int currentWayPointListPosition;
    [SerializeField, HideInInspector]
    int forcedStreetID;
    [SerializeField, HideInInspector]
    int pathListPalce = 0;

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
    [SerializeField, HideInInspector]
    Path[] pathList = new Path[500]; //Needs to be changed to list, rather difficult. Will do later.

    [SerializeField, HideInInspector]
    float pathCost = 0;
    [SerializeField, HideInInspector]
    bool dijkstraMode = false;

    List<GameObject> pathsToVisit = new();
    List<GameObject> vistetPaths = new();

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

    public void SetData(GameObject trafficManager, Node startNode, Color color)
    {
        if (startNode == null) DeleteSelf();
        this.trafficManager = trafficManager;

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

        GenerateModel(color);
        currentBehaviour = CarBehaviour.drive;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Car")
        {
            hitOtherCar = true;
            currentBehaviour = CarBehaviour.none;
            int[] randomX = new int[2] { UnityEngine.Random.Range(10, 30), UnityEngine.Random.Range(-10, -30) };
            int[] randomY = new int[2] { UnityEngine.Random.Range(10, 30), UnityEngine.Random.Range(-10, -30) };
            int[] randomZ = new int[2] { UnityEngine.Random.Range(10, 30), UnityEngine.Random.Range(-10, -30) };
            Vector3 randompoint = new Vector3(randomX[UnityEngine.Random.Range(0, 2)], randomY[UnityEngine.Random.Range(0, 2)], randomZ[UnityEngine.Random.Range(0, 2)]);
            gameObject.GetComponent<Rigidbody>().velocity = randompoint;
        }
    }


    void Update()
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

        if (hitOtherCar) return;
        if (cube != null) cube.transform.position = new(transform.position.x, boxCollider.transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetLocation, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetLocation) < maxDistanceFromNextNodeBeforeSwitching)
            SetTargetToNextWaypointOrStreet();

        // replace later with model
        if (targetLocation != null && cube != null)
        {
            cube.transform.position = new(transform.position.x,boxCollider.transform.position.y,transform.position.z);
            Quaternion rotation = Quaternion.LookRotation((targetLocation - cube.transform.position).normalized);
            gameObject.transform.rotation = new(rotation.x,0,rotation.z,0);
        }
    }

    void FixedUpdate()
    {
        if(!hitOtherCar) return;
        timeBevoreSelfDeleteAfterHit--;
        if (timeBevoreSelfDeleteAfterHit > 0f) return;
        DeleteSelf();
    }

    private void GenerateModel(Color color)
    {
        // Replace later with actual model.
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "CarCube";
        cube.transform.parent = gameObject.transform;
        cube.transform.position = gameObject.transform.position;
        cube.GetComponent<Renderer>().material.color = color;
        cube.transform.localScale = new Vector3(1f, .9f, 1.5f);
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
            {
                currentWayPointListPosition = 0;
            }
        }
    }

    private bool FindNextStreet()
    {
        if (dijkstraMode) { DeleteSelf(); }
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
        return availableStreets[randomIndex];
    }

    // for the fast boys
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
        dijkstraMode = true;
        int safty = 100;
        int startNode = currentStreet.GetComponent<Street>().EndNode.GetComponent<Node>().NodeID;
        // the current sStreet
        GameObject currentViewdStreet = currentStreet;
        // Create Start Index
        Path startEntry = new Path();
        startEntry.TargetNode = currentStreet.GetComponent<Street>().EndNode;
        startEntry.PreviousNode = null;
        startEntry.Cost = 0;
        VisitStreet(currentViewdStreet);
        Debug.ClearDeveloperConsole(); // sometimes
        do
        {
            currentViewdStreet = FindNewStreet();

            if (currentViewdStreet == null) break;
            VisitStreet(currentViewdStreet);
            safty--;

        } while (nodeID != currentViewdStreet.GetComponent<Street>().EndNode.GetComponent<Node>().NodeID && safty != 0);

        if (safty == 0)
        {
            Debug.Log("Error No Path found");
            dijkstraMode = false;
        }
        else
        {
            FindPath(nodeID, startNode);
        }
    }

    private void FindPath(int targetNode,int startNode)
    {
        //for(int i = 0; i < pathList.Length; i++)
        //{
        //    Waypoints.Clear();
        //    if (pathList[i].Cost != 0.0f)
        //    {
        //        //Debug.Log( "TargetNode: "+pathList[i].TargetNode.GetComponent<Node>().NodeID + " Cost: " + pathList[i].Cost + "StartNode: " +pathList[i].PreviousNode.GetComponent<Node>().NodeID);
        //        Waypoints.Add(pathList[i].TargetNode.GetComponent<Node>().Position);
        //    }
        //}
        // ################### This Code is the finest Cope
        Waypoints.Clear();
        currentWayPointListPosition = -1;
        int currentTarget = targetNode;
        bool errorNull = false;
        do
        {
            for (int i = 0; i < pathList.Length; i++)
            {
                // WHYYY????
                if (pathList[i].TargetNode == null) 
                {
                    errorNull = true;
                    break; 
                }
                // Ah thats why!
                if (pathList[i].TargetNode.GetComponent<Node>().NodeID == currentTarget) 
                {
                    currentTarget = pathList[i].PreviousNode.GetComponent<Node>().NodeID;
                    Waypoints.Add(pathList[i].TargetNode.GetComponent<Node>().Position);
                    break;
                }

            }
        } while (currentTarget != startNode && !errorNull);

        Waypoints.Reverse();

    }

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
        

        pathList[pathListPalce] = entry;
        pathListPalce++;
        



    }

    private void FindPathsOnNode(GameObject Node)
    {
        
        foreach (GameObject street in Node.GetComponent<Node>().ConnectedStreets)
        {
            UpdateIndexForPathList(street);
            bool found = false;
            foreach (var item in pathsToVisit)
            {
                if(street == item)
                {
                   found = true;
                   break;
                }
            }
            if (!found)
            {
                pathsToVisit.Add(street);
            }
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
                if (currentSelectedStreet.GetComponent<Street>().cost > pathToVisit.GetComponent<Street>().cost)
                {
                    currentSelectedStreet = pathToVisit;
                    // Erhöhe Cost
                    pathCost = pathCost + currentSelectedStreet.GetComponent<Street>().cost;

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