using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public class Car : MonoBehaviour
{
    readonly TrafficManager _trafficManager;

    enum Verhalten
    {
        none,
        acceleration,
        deceleration

    }
    Verhalten _verhalten = Verhalten.acceleration;



    bool    brake = false;
    float     acceleration = .02f;
    float     deceleration = 0.05f;
    int     maxSpeed = 120;
    float   speed = 0f;

    float maxDistanceFront = 20f;

    int targetNodeID = -1;
    Node targetNode;
    Node currentNode;
    Vector3 targetLocation = new (100,0,0);
    //Vector3[] Waypoints;

    Street currentStreet;
    private void Start()
    {
        Debug.Log("Star");
    }




    private void Update()
    {
        
        RaycastHit hit;
        Debug.DrawRay(transform.position, transform.forward * maxDistanceFront, Color.yellow);
        Debug.Log(transform.forward * maxDistanceFront);
        if (Physics.Raycast(transform.position, transform.forward * maxDistanceFront, out hit))
        {
            _verhalten = Verhalten.deceleration;
           
        }

        transform.position =  Vector3.MoveTowards(transform.position, targetLocation, speed * Time.deltaTime);
       
        switch (_verhalten)
        {
            case Verhalten.none: break;
            case Verhalten.acceleration:
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


    private void Acceleration()
    {
        if (speed < maxSpeed)
        {
            brake = false;
            speed = speed + acceleration;

        }
    }

    private void Deceleration()
    {
        if (speed > 0)
        {
            speed = speed - deceleration;
            if ( speed < 0)
            {
                speed = 0;
            }

        }
    }



}
