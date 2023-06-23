using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TrafficManager : MonoBehaviour
{
    [SerializeField]
    float nodeMergeDistance = 2.0f;
    [SerializeField]
    float wayPointDistance = .3f;
    [SerializeField]
    float wayPointSphereSize = .2f;

    List<GameObject> streetList = new();

    public float WayPointDistance
    {
        get { return wayPointDistance; }
    }
    public float WayPointSphereSize
    {
        get { return wayPointSphereSize; }
    }
    public List<GameObject> StreetList
    {
        get { return streetList; }
    }

    public void AddStreet(Vector3 startPoint, Vector3 endPoint)
    {
        GameObject street = new GameObject("Street");
        street.AddComponent<Street>().GetData(this, startPoint, endPoint);
        street.transform.SetParent(transform.Find("Streets").transform, true);
        streetList.Add(street);
    }

    public void AddCarSpawner(Vector3 position)
    {
        bool found = false;
        foreach (GameObject street in StreetList)
        {
            if (IsInDistance(position, street.GetComponent<Street>().StartNode.GetComponent<Node>().Position, 5f))
            {
                GameObject carSpawner = new GameObject();
                carSpawner.AddComponent<CarSpawner>().GetData(gameObject, position);
                carSpawner.name = "CarSpawner";
                carSpawner.transform.SetParent(transform.Find("CarSpawner").transform, true);
                found = true;
                return;
            }
        }
        if (!found)
        {
            Debug.Log("No Location Found for Car Spawner");
        }
    }

    public void GenerateIntersection(GameObject firstStreet, GameObject secondStreet, Vector3 intersectionPosition)
    {
        DeleteStreet(firstStreet);
        DeleteStreet(secondStreet);
        AddStreet(firstStreet.GetComponent<Street>().StartNode.GetComponent<Node>().Position, intersectionPosition);
        AddStreet(firstStreet.GetComponent<Street>().EndNode.GetComponent<Node>().Position, intersectionPosition);
        AddStreet(secondStreet.GetComponent<Street>().StartNode.GetComponent<Node>().Position, intersectionPosition);
        AddStreet(secondStreet.GetComponent<Street>().EndNode.GetComponent<Node>().Position, intersectionPosition);
    }

    public void DeleteStreet(GameObject street)
    {
        street.GetComponent<Street>().DeleteStreetContents();
        streetList.Remove(street);
        Destroy(street);
    }

    public GameObject FindNodeWithPosition(Vector3 Position)
    {
        foreach (GameObject street in streetList)
        {
            if (IsInDistance(street.GetComponent<Street>().StartNode.GetComponent<Node>().Position, Position, nodeMergeDistance))
                return street.GetComponent<Street>().StartNode;
            if (IsInDistance(street.GetComponent<Street>().EndNode.GetComponent<Node>().Position, Position, nodeMergeDistance))
                return street.GetComponent<Street>().EndNode;
        }
        return null;
    }

    public bool IsNodeOnStreet(GameObject node, GameObject street)
    {
        return (Math.Abs(Vector3.Distance(street.GetComponent<Street>().StartNode.GetComponent<Node>().Position, node.GetComponent<Node>().Position)) + Math.Abs(Vector3.Distance(street.GetComponent<Street>().EndNode.GetComponent<Node>().Position, node.GetComponent<Node>().Position))) == Math.Abs(Vector3.Distance(street.GetComponent<Street>().StartNode.GetComponent<Node>().Position, street.GetComponent<Street>().EndNode.GetComponent<Node>().Position));
    }

    public bool IsInDistance(Vector3 a, Vector3 b, float distance)
    {
        if (Vector3.Distance(a, b) < distance)
            return true;
        return false;
    }

    public void DeleteCarSpawner(CarSpawner carSpawner)
    {
        Destroy(carSpawner);
    }
}
