using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public class Car : MonoBehaviour
{
    TrafficManager _trafficManager = null;
    enum Verhalten
    {
        none,
        drive,
        deceleration

    }
    Verhalten _verhalten = Verhalten.drive;

    bool brake = false;
    float acceleration = .02f;
    float deceleration = 0.05f;
    int maxSpeed = 120;
    float speed = 0f;

<<<<<<< Updated upstream
=======

    bool    brake = false;
    float     acceleration = .02f;
    float     deceleration = 0.05f;
    int     maxSpeed = 120;
    float   speed = 0f;

>>>>>>> Stashed changes
    float maxDistanceFront = 3f;

    int targetNodeID = -1;
    int currentArrayPosition = -1;
    Node targetNode;
    Node currentNode;
<<<<<<< Updated upstream
    Vector3 targetLocation = new(100, 0, 0);
    List<Vector3> Waypoints = new List<Vector3>();

=======
    Vector3 targetLocation = new (100,0,0);
    List<Vector3> Waypoints = new List<Vector3>();
>>>>>>> Stashed changes

    Street currentStreet;


    public void Awake(TrafficManager trafficManager, Node startNode)
    {
<<<<<<< Updated upstream
        _trafficManager = trafficManager;
        currentNode = startNode;
=======
        Debug.Log("Star");
        Rotate();
        Waypoints.Add(new Vector3(100,0,20));
        Waypoints.Add(new Vector3(33, 0, 0));
        Waypoints.Add(new Vector3(0, 0, 0));

>>>>>>> Stashed changes
    }



    // Recplaces Konstructor
    private void Start()
    {
        FindPath();
        Rotate();
    }

    private void Update()
    {

        /*RaycastHit hit;
        Debug.DrawRay(transform.position, transform.forward * maxDistanceFront , Color.yellow);
        if (Physics.Raycast(transform.position, transform.position * maxDistanceFront, out hit))
        {
            _verhalten = Verhalten.deceleration;
           
        }
        */
        Rotate();
        switch (_verhalten)
        {
            case Verhalten.none: break;
            case Verhalten.drive:
                {
                    Acceleration();
                    break;
                }
            case Verhalten.deceleration:
                {
                    Deceleration();
                    break;
                }
        }

        Debug.Log(speed);

        transform.position = Vector3.MoveTowards(transform.position, targetLocation, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetLocation) < maxDistanceFront)
        {
            /*if (currentArrayPosition + 1 > Waypoints.Count)
            {
                Waypoints.Add(new Vector3(0, 0, 0));
                currentArrayPosition = -1;
                Debug.Log("not found");
            }
            else
            {
                targetLocation = Waypoints[currentArrayPosition + 1];
                currentArrayPosition++;
                Debug.Log("next");
            }
            */
            Random random = new();
            targetLocation = new Vector3(random.Next(-100, 100), 0, random.Next(-100, 100));
            maxSpeed = random.Next(50, 200);


        }
    }


    private void FindPath()
    {
        if (currentNode.Connetions.Count == 0)
        {
            Destroy(this);
        }
        else
        {
            Waypoints.Clear();
            Random random = new();
            int selectetRoad = random.Next(currentNode.Connetions.Count);
            //Waypoints.Add( currentNode.Connetions[selectetRoad].)
        }
        
    }
    private void RandomStreet()
    {
        Random random = new();
        List<Connection> possibleConnections = currentNode.Connetions;
        foreach (Connection connection in possibleConnections)
        {
            if (connection.EndNode.NodeID == currentNode.NodeID)
                possibleConnections.Remove(connection);
        }
        if (possibleConnections.Count == 0)
        {
            GameObject.Destroy(this);
            return;
        }
        targetNode = possibleConnections[random.Next(possibleConnections.Count)].EndNode;
    }

    private void Acceleration()
    {
        if (speed < maxSpeed)
        {
            brake = false;
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

    private void Rotate()
    {
        Vector3 direction = targetLocation - transform.position;
<<<<<<< Updated upstream
        Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = rotation;
=======
        Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);   
        transform.rotation = rotation;

        
    }

>>>>>>> Stashed changes


    }



}