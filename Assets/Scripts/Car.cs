using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public class Car : MonoBehaviour
{
    GameObject trafficManager;
    enum Verhalten
    {
        none,
        drive,
        deceleration

    }
    Verhalten _verhalten = Verhalten.none;

    GameObject cube;

    bool lastRound = false;
    float acceleration = .05f;
    float deceleration = 0.05f;
    int maxSpeed = 120;
    float speed = 0f;
    float maxDistanceFront = 0.4f;

    int currentListPosition = -1;
    Node nextNode;
    Node targetNode;

    Vector3 targetLocation;
    List<Vector3> Waypoints = new List<Vector3>();
    Street currentStreet;

    public void GetData(GameObject trafficManager, Node startNode)
    {

        if (startNode == null)
        {
            DeletCar();
        }
        this.trafficManager = trafficManager;

        if (startNode.ConnectedStreets[0].GetComponent<Street>() != null)
        {
            currentStreet = startNode.ConnectedStreets[0].GetComponent<Street>();
            transform.position = currentStreet.StartNode.GetComponent<Node>().Position;
            targetLocation = currentStreet.EndNode.GetComponent<Node>().Position;
        }
        else
        {
            DeletCar();
        }
        AddMesh();
        _verhalten = Verhalten.drive;
    }

    private void Update()
    {
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

        //Sp�ter durch model ersetzen
        if (cube != null)
            cube.transform.position = transform.position;


        transform.position = Vector3.MoveTowards(transform.position, targetLocation, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetLocation) < maxDistanceFront)
        {
            NextWaypoint();
        }
    }

    private void FindPath()
    {
        if (lastRound)
        {
            DeletCar();
        }
        else
        {
            if (currentStreet != null)
            {
                Waypoints.Add(currentStreet.StartNode.GetComponent<Node>().Position);
                Waypoints.Add(currentStreet.EndNode.GetComponent<Node>().Position);
                currentStreet = FindStreetOnCurrentStreet();
            }
            else
            {
                DeletCar();
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

    private void Rotate()
    {
        Vector3 direction = targetLocation - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = rotation;
        }
    }

    private void AddMesh()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "CarCube";
        cube.transform.parent = gameObject.transform;
    }

    private Street FindStreetOnCurrentStreet()
    {
        List<Street> availableStreet = new List<Street>();

        foreach (Street connectedStreet in currentStreet.EndNode.GetComponent<Node>().ConnectedStreets)
        {
            if (connectedStreet.GetComponent<Street>().StreetID != currentStreet.StreetID)
            {
                availableStreet.Add(connectedStreet.GetComponent<Street>());
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

    private void DeletCar()
    {
        Destroy(this.gameObject);
        Destroy(cube);
    }
}