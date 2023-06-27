using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TrafficManager : MonoBehaviour
{
    public GameObject trafficSignPrefab;

    [SerializeField]
    float nodeMergeDistance = 2.0f;
    [SerializeField]
    float wayPointSpacing = .3f;
    [SerializeField]
    float wayPointSphereSize = .2f;

    [SerializeField, HideInInspector]
    List<GameObject> streetList = new();
    [SerializeField, HideInInspector]
    List<GameObject> carSpawnerList = new();
    [SerializeField, HideInInspector]
    List<GameObject> trafficSignList = new();
    [SerializeField, HideInInspector]
    List<GameObject> nodeList = new();

    public List<GameObject> NodeList
    {
        get { return nodeList; }
    }
    public float WayPointDistance
    {
        get { return wayPointSpacing; }
    }
    public float WayPointSphereSize
    {
        get { return wayPointSphereSize; }
    }
    public List<GameObject> StreetList
    {
        get { return streetList; }
    }
    public List<GameObject> CarSpawnerList
    {
        get { return carSpawnerList;}
    }
    public List<GameObject> TrafficSignList
    {
        get { return trafficSignList; }
    }
    
    public void AddStreet(Vector3 startPoint, Vector3 endPoint)
    {
        GameObject street = new GameObject("Street");
        street.transform.position = Vector3.Lerp(startPoint, endPoint, .5f);
        street.AddComponent<Street>().SetData(this, startPoint, endPoint);
        street.transform.SetParent(transform.Find("Streets").transform, true);
        streetList.Add(street);
        DetectAndGenerateIntersectionsOnStreet(street.GetComponent<Street>());
    }

    public void AddCarSpawner(Vector3 position)
    {
        if (FindCarSpawnerInRange(position, nodeMergeDistance) != null) return;
        bool found = false;
        foreach (GameObject street in StreetList)
        {
            if (IsInDistance(position, street.GetComponent<Street>().StartNode.GetComponent<Node>().Position, nodeMergeDistance))
            {
                GameObject carSpawner = new GameObject();
                carSpawner.transform.position = position;
                carSpawner.AddComponent<CarSpawner>().GetData(gameObject, position);
                carSpawner.name = "CarSpawner";
                carSpawner.transform.SetParent(transform.Find("CarSpawner").transform, true);
                carSpawnerList.Add(carSpawner);
                found = true;
                return;
            }
        }
        if (!found)
        {
            Debug.Log("No Location Found for Car Spawner");
        }
    }

    public void AddTrafficSign(Vector3 position, TrafficSignTypes type, int trafficSignValue, Quaternion rotation)
    {
        GameObject trafficSign = Instantiate(trafficSignPrefab, new(position.x,0.4f,position.z), Quaternion.identity);
        trafficSign.transform.rotation = rotation;
        trafficSign.GetComponent<TrafficSign>().GetData(type, trafficSignValue);
        trafficSignList.Add(trafficSign);
        trafficSign.transform.SetParent(transform.Find("TrafficSigns").transform, true);
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
        // I have to save all street start/end locations, because if I don't delete the streets first,
        // they will generate new intersections with the newly generated streets. Not clean whatsoever. Don't care.
        Vector3 positionA = firstStreet.GetComponent<Street>().StartNode.GetComponent<Node>().Position;
        Vector3 positionB = firstStreet.GetComponent<Street>().EndNode.GetComponent<Node>().Position;
        Vector3 positionC = secondStreet.GetComponent<Street>().StartNode.GetComponent<Node>().Position;
        Vector3 positionD = secondStreet.GetComponent<Street>().EndNode.GetComponent<Node>().Position;
        DeleteStreet(firstStreet);
        DeleteStreet(secondStreet);
        AddStreet(positionA, intersectionPosition);
        AddStreet(positionB, intersectionPosition);
        AddStreet(positionC, intersectionPosition);
        AddStreet(positionD, intersectionPosition);
    }

    public void DeleteStreet(GameObject street)
    {
        street.GetComponent<Street>().DeleteStreetContents();
        streetList.Remove(street);
        Destroy(street);
    }

    public void DeleteNode(GameObject node)
    {
        node.GetComponent<Node>().DeleteSphere();
        nodeList.Remove(node);
        Destroy(node);
    }

    public void DeleteCarSpawner(GameObject carSpawner)
    {
        Destroy(carSpawner);
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

    public GameObject FindCarSpawnerInRange(Vector3 Position, float range)
    {
        foreach (GameObject carSpawner in carSpawnerList)
        {
            if (IsInDistance(carSpawner.GetComponent<CarSpawner>().Position,Position,range)) return carSpawner;
        }
        return null;
    }

    public GameObject FindStreetInRange(Vector3 Position, float range)
    {
        foreach(GameObject street in streetList)
        {
            foreach(Vector3 wayPoint in street.GetComponent<Street>().WayPoints)
            {
                if(IsInDistance(wayPoint, Position, range)) return street;
            }
        }
        return null;
    }


    public bool IsInDistance(Vector3 a, Vector3 b, float distance)
    {
        if (Vector3.Distance(a, b) < distance)
            return true;
        return false;
    }
}
