using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;
using Debug = UnityEngine.Debug;
using Random = System.Random;

//Dijkstra
public struct Pathlist
{
    private Node targetNode;
    private float cost;
    private Node previousNode;

    public Node TargetNode
    {
        get { return targetNode; }
        set { targetNode = value; }
    }
    public float Cost
    {
        get { return cost; }
        set { cost = value; }
    }
    public Node PreviousNode
    {
        get { return previousNode; }
        set { previousNode = value; }
    }
    
}

public class Car : MonoBehaviour
{
    GameObject trafficManager;
    enum Verhalten
    {
        none,
        drive,
        stop,

    }
    Verhalten _verhalten = Verhalten.none;

    GameObject cube;
    BoxCollider boxCollider;

    bool lastRound = false;
    float stopTimeLeft = 1f;
    float acceleration = .05f;
    float deceleration = 0.1f;
    float speed = 0f;
    float maxDistanceFront = 1f;
    int maxSpeed = 10;
    int currentListPosition = -1;
    int forcesStreetID = -1;


    public int ForcesStreetID
    {
        get { return forcesStreetID; }
        set { forcesStreetID = value; }
    }


    Vector3 targetLocation;
    List<Vector3> Waypoints = new List<Vector3>();

    //Street
    Street lastStreet;
    Street currentStreet;
    List<Street> visitedStreets = new List<Street>();
    //Nodes
    Node currentStartNode;
    Node lastStartNode;
    Node currentEndNode;
    Node lastEndNode;



    List<Node> notVisitedNodes = new List<Node>();
    List<Node> visitedNodes = new List<Node>();

    List<Pathlist> listOfPaths = new List<Pathlist>();


    public void GetData(GameObject trafficManager, Node startNode)
    {

        if (startNode == null) DeleteCar();
        this.trafficManager = trafficManager;

        for (int i = 0; i < startNode.ConnectedStreets.Count; i++)
        {
            if (startNode.ConnectedStreets[i] != null)
            {
                currentStreet = startNode.ConnectedStreets[i];
                if (currentStreet.StartNode.GetComponent<Node>().Equals(startNode))
                {
                    transform.position = currentStreet.StartNode.GetComponent<Node>().Position;
                    targetLocation = currentStreet.EndNode.GetComponent<Node>().Position;
                    break;
                }
            }
        }
        if (currentStreet == null) DeleteCar();
        AddMesh();
        _verhalten = Verhalten.drive;
    }

    private void Update()
    {




        switch (_verhalten)
        {
            case Verhalten.none: break;
            case Verhalten.drive:
                {
                    Acceleration();
                    break;
                }
            case Verhalten.stop:
                {
                    Deceleration();
                    if (stopTimeLeft <= 0f && speed <= 0f)
                    {
                        _verhalten = Verhalten.drive;
                        stopTimeLeft = 1f;
                    }
                    else
                    {
                        stopTimeLeft -= Time.deltaTime;
                    }

                    break;
                }
        }

        //Sp�ter durch model ersetzen
        if (cube != null)
        {
            cube.transform.position = transform.position;
            boxCollider.transform.position = transform.position;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetLocation, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetLocation) < maxDistanceFront)
        {
            NextWaypoint();
        }

        if (currentStreet == null)
            DeleteCar();
    }

    private void FindPath()
    {
        if (lastRound)
        {
            DeleteCar();
        }
        else
        {
            lastStreet = currentStreet;
            lastStartNode = currentStartNode;
            lastEndNode = currentEndNode;
            currentStreet = FindStreetOnCurrentStreet();
            if (currentStreet == lastStreet)
            {
                DeleteCar();
            }
            visitedStreets.Add(currentStreet);
            if (currentStreet != null)
            {
                currentStartNode = currentStreet.StartNode.GetComponent<Node>();
                currentEndNode = currentStreet.EndNode.GetComponent<Node>();
                if (currentStreet == lastStreet)
                {
                    DeleteCar();
                }

            }
            else
            {
                DeleteCar();
            }
        }

        if (currentStreet == null)
        {
            lastRound = true;
        }
        else
        {
            Waypoints.Clear();
            Waypoints.Add(currentStreet.StartNode.GetComponent<Node>().Position);
            Waypoints.Add(currentStreet.EndNode.GetComponent<Node>().Position);
        }

    }

    private void NextWaypoint()
    {
        if (currentListPosition + 1 < Waypoints.Count)
        {
            currentListPosition++;
            targetLocation = Waypoints[currentListPosition];
        }
        else
        {
            FindPath();

            if (currentStreet != null)
            {
                targetLocation = Waypoints[0];
                currentListPosition = 0;
            }
        }
    }

    private void Acceleration()
    {
        if (speed < maxSpeed)
        {
            speed = speed + acceleration;
        }
        else
        {
            speed = speed - deceleration;
        }
    }

    private void Deceleration()
    {
        if (speed > 0)
        {
            speed = speed - deceleration;
            if (speed < 0)
            {
                speed = 0;
            }
        }
    }

    private void AddMesh()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "CarCube";
        cube.transform.parent = gameObject.transform;
        boxCollider = this.AddComponent<BoxCollider>();
        this.AddComponent<Rigidbody>();
        cube.GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV();
    }

    private Street FindStreetOnCurrentStreet()
    {
        if (forcesStreetID != -1)
        {
            foreach (Street connectedStreet in currentStreet.EndNode.GetComponent<Node>().ConnectedStreets)
            {
                if (connectedStreet.StreetID == forcesStreetID)
                {
                    forcesStreetID = -1;
                    return connectedStreet;
                }
            }
        }

        List<Street> availableStreet = new List<Street>();
        foreach (Street connectedStreet in currentStreet.EndNode.GetComponent<Node>().ConnectedStreets)
        {

            if (connectedStreet.GetComponent<Street>().StreetID != currentStreet.StreetID)
            {

                // Suche on Die Jetzige Straße Bereits befahren wurde
                bool alreadyUsed = false;
                foreach (Street item in visitedStreets)
                {
                    if (item == connectedStreet)
                    {
                        alreadyUsed = true;
                        break;
                    }
                }

                if (!alreadyUsed)
                {
                    availableStreet.Add(connectedStreet.GetComponent<Street>());
                }
            }
        }

        if (availableStreet.Count > 0)
        {
            Random random = new();
            int randomIndex = random.Next(0, availableStreet.Count);
            return availableStreet[randomIndex];
        }
        return null;
    }

    private void DeleteCar()
    {
        Destroy(this.gameObject);
        Destroy(cube);
    }

    public void SetMaxSpeed(int newMaxSpeed)
    {
        maxSpeed = newMaxSpeed;
    }

    public void Stop()
    {
        _verhalten = Verhalten.stop;
    }

    public void Dijkstra(int nodeID)
    {
        Debug.Log(trafficManager.GetComponent<TrafficManager>().NodeList.Count);
        //Pathlist startEntry = new Pathlist();
        //startEntry.TargetNode = currentEndNode.GetComponent<Node>();
        //startEntry.PreviousNode = null;
        //startEntry.Cost = 0;
        //listOfPaths.Add(startEntry);

        // erstelle eine Liste mit den jeweils kürzersten routen zwischen 2 Punkten
        foreach (GameObject item in trafficManager.GetComponent<TrafficManager>().NodeList)
        {
            FindNodsFromNode(item.GetComponent<Node>());
        }
        foreach (var item in listOfPaths)
        {
            if (item.PreviousNode != null)
                Debug.Log("TaretNode: " + item.TargetNode.GetComponent<Node>().NodeID + " with Cost: " + item.Cost + "over Node: " + item.PreviousNode.GetComponent<Node>().NodeID);
        }


        // Fügt die jetzige Node den untersuchten hinzu u
        int safty = 7;
        int targetNodeID = nodeID;
        float pathCost = 0;
        Node currentViewdNode = FindNoteinPathlist(targetNodeID);
        List<Node> pathRevers = new List<Node>();
        
        FindNodsFromNode(currentEndNode.GetComponent<Node>());

        do
        {
            pathCost = pathCost + PathCost(targetNodeID);
            Debug.Log(pathCost);
            currentViewdNode = FindNoteinPathlist(targetNodeID);



            safty--;
        } while (nodeID != currentViewdNode.GetComponent<Node>().NodeID && safty > 0);


        //// Finde Die Erste Node die sich angeschaut werden sollte
        //foreach (var item in notVisitedNodes)
        //{
        //    currentViewdNode = item.GetComponent<Node>();
        //    break;
        //}


        //do
        //{
        //    foreach (Node item in notVisitedNodes)
        //    {
        //        bool found = false;
        //        foreach (Node item2 in visitedNodes)
        //        {
        //            if(item.GetComponent<Node>() == item2.GetComponent<Node>())
        //            {
        //                found = true;
        //                break;
        //            }
        //        }
        //        if (!found)
        //        {
        //            currentViewdNode = item.GetComponent<Node>();
        //            FindNodsFromNode(currentViewdNode);
        //        }
        //    }
        //    safty--;
        //} while (nodeID != currentViewdNode.GetComponent<Node>().NodeID && safty > 0);

    }

    public void FindNodsFromNode(Node node)
    {
        // Wenn die Node in der Liste der Liste besuchten Nodes auftaucht
        bool foundInVisited = false;
        foreach (var item in visitedNodes)
        {
            if (item == node.GetComponent<Node>())
            {
                foundInVisited = true;
                break;
            }
        }
        if (foundInVisited) { return; }
        visitedNodes.Add(node);

        // Schaut sich die Angeschlossenen nodes hinzu speichert die niedrigste cost
        float lowestCost = 99999999999999999; // ignore that shit
        Node nodeWithLowestCost = null;







        foreach (Street connectedStreet in node.GetComponent<Node>().ConnectedStreets)
        {
            if (connectedStreet.EndNode.GetComponent<Node>() != node.GetComponent<Node>())
            {
                // Wenn die jetzige straße einen niedrigeren Wert als die bisherige
                if (connectedStreet.cost < lowestCost)
                {
                    lowestCost = connectedStreet.cost;
                    nodeWithLowestCost = connectedStreet.EndNode.GetComponent<Node>();
                }

                // Wenn die Node in der Liste der noch Nicht Besuchten Nodes auftauch
                bool foundInNotVisited = false;
                foreach (var item in visitedNodes)
                {
                    if (item == connectedStreet.EndNode.GetComponent<Node>())
                    {
                        foundInNotVisited = true;
                        break;
                    }
                }

                // Wenn Die Node in keine der Listen auftaucht füge sie ein
                if (!foundInNotVisited)
                {
                    notVisitedNodes.Add(connectedStreet.EndNode.GetComponent<Node>());

                }
            }
        }

        if(nodeWithLowestCost != null) 
        { 
        Pathlist entry = new Pathlist();
        entry.TargetNode = nodeWithLowestCost.GetComponent<Node>();
        entry.PreviousNode = node.GetComponent<Node>();
        entry.Cost = lowestCost;
        listOfPaths.Add(entry);
        }

    }

    public Node FindNoteinPathlist(int targetNodeID)
    {
        foreach (Pathlist item in listOfPaths)
        {
            if (item.TargetNode.GetComponent<Node>().NodeID == targetNodeID)
            {
                return item.PreviousNode.GetComponent<Node>();
            }
              
        }
        return null;
    }

    public float PathCost(int targetNodeID)
    {
        foreach (var item in listOfPaths)
        {
            if (item.TargetNode.NodeID == targetNodeID)
            {
                return item.Cost;
            }

        }
        return 0f;
    }

}