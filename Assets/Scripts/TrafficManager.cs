using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
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
        DetectAndGenerateIntersectionsOnStreet(street.GetComponent<Street>());
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
    public void DetectAndGenerateIntersectionsOnStreet(Street streetToSearch)
    {
        foreach (GameObject street in streetList)
        {
            if (street.Equals(streetToSearch.gameObject)) continue;
            foreach (Vector3 wayPoint in street.GetComponent<Street>().WayPoints)
            {
                foreach (Vector3 thisWayPoint in streetToSearch.WayPoints)
                {
                    if (Vector3.Distance(wayPoint, thisWayPoint) <= .2f &&
                        !IsInDistance(wayPoint, streetToSearch.StartPoint, 2f) &&
                        !IsInDistance(wayPoint, streetToSearch.EndPoint, 2f))
                    {
                        GenerateIntersection(streetToSearch.gameObject, street, wayPoint);
                        return;
                    }
                }
            }
        }
    }

    public void GenerateIntersection(GameObject firstStreet, GameObject secondStreet, Vector3 intersectionPosition)
    {
        Vector3 positionA = firstStreet.GetComponent<Street>().StartNode.GetComponent<Node>().Position;
        Vector3 positionB = firstStreet.GetComponent<Street>().EndNode.GetComponent<Node>().Position;
        Vector3 positionC = secondStreet.GetComponent<Street>().StartNode.GetComponent<Node>().Position;
        Vector3 positionD = secondStreet.GetComponent<Street>().EndNode.GetComponent<Node>().Position;
        DeleteStreet(firstStreet, false);
        DeleteStreet(secondStreet, false);
        AddStreet(positionA, intersectionPosition);
        AddStreet(positionB, intersectionPosition);
        AddStreet(positionC, intersectionPosition);
        AddStreet(positionD, intersectionPosition);
    }

    public void DeleteStreet(GameObject street, bool doDeleteNodes = false)
    {
        street.GetComponent<Street>().DeleteStreetContents(doDeleteNodes);
        streetList.RemoveAll(streetThis => streetThis.Equals(street));
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
