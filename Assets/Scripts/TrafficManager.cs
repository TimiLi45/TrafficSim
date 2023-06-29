using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrafficGameObjectTypes
{
    None,
    Street,
    Node,
    CarSpawner,
    Car,
    TrafficSign
}

public class TrafficManager : MonoBehaviour
{
    public GameObject trafficSignPrefab;

    [SerializeField]
    float nodeMergeDistance = 2.0f;
    [SerializeField]
    float wayPointSpacing = .3f;

    [SerializeField]
    float wayPointSphereSize = .2f;
    [SerializeField]
    float nodeSphereSize = 1f;

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
    public float NodeMergeDistance
    {
        get { return nodeMergeDistance; }
    }
    public float WayPointDistance
    {
        get { return wayPointSpacing; }
    }
    public float WayPointSphereSize
    {
        get { return wayPointSphereSize; }
    }
    public float NodeSphereSize
    {
        get { return nodeSphereSize; }
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
        GameObject street = new("Street");
        street.transform.position = Vector3.Lerp(startPoint, endPoint, .5f);
        street.AddComponent<Street>().SetData(this, startPoint, endPoint);
        street.transform.SetParent(transform.Find("Streets").transform, true);
        streetList.Add(street);
        DetectAndGenerateIntersectionsOnStreet(street.GetComponent<Street>());
    }

    public void AddCarSpawner(Vector3 position)
    {
        GameObject carSpawner = new("CarSpawner");
        carSpawner.transform.position = position;
        carSpawner.AddComponent<CarSpawner>().SetData(gameObject, position);
        carSpawner.transform.SetParent(transform.Find("CarSpawner").transform, true);
        carSpawnerList.Add(carSpawner);
    }

    public void AddTrafficSign(Vector3 position, TrafficSignTypes type, int trafficSignValue, Quaternion rotation)
    {
        GameObject trafficSign = Instantiate(trafficSignPrefab, new(position.x,0.4f,position.z), Quaternion.identity);
        trafficSign.transform.rotation = rotation;
        trafficSign.GetComponent<TrafficSign>().SetData(this, type, trafficSignValue);
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

    public void DeleteGameObjectByType(GameObject gameObject, TrafficGameObjectTypes type)
    {
        switch(type)
        {
            case TrafficGameObjectTypes.None:
                break;
            case TrafficGameObjectTypes.Street:
                DeleteStreet(gameObject);
                break;
            case TrafficGameObjectTypes.Node:
                DeleteNode(gameObject);
                break;
            case TrafficGameObjectTypes.CarSpawner:
                DeleteCarSpawner(gameObject);
                break;
            case TrafficGameObjectTypes.Car:
                DeleteCar(gameObject);
                break;
            case TrafficGameObjectTypes.TrafficSign:
                DeleteTrafficSign(gameObject);
                break;
        }
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
        for (int i = 0; i < node.GetComponent<Node>().ConnectedStreets.Count; i++)
        {
            DeleteStreet(node.GetComponent<Node>().ConnectedStreets[i].gameObject);
            i--;
        }
        nodeList.Remove(node);
        Destroy(node);
    }

    public void DeleteCarSpawner(GameObject carSpawner)
    {
        CarSpawnerList.Remove(carSpawner);
        Destroy(carSpawner);
    }

    public void DeleteCar(GameObject car)
    {
        Destroy(car);
    }

    public void DeleteTrafficSign(GameObject trafficSign)
    {
        Destroy(trafficSign);
    }

    public GameObject FindNodeWithPosition(Vector3 position)
    {
        foreach (GameObject street in streetList)
        {
            if (IsInDistance(street.GetComponent<Street>().StartNode.GetComponent<Node>().Position, position, nodeMergeDistance))
                return street.GetComponent<Street>().StartNode;
            if (IsInDistance(street.GetComponent<Street>().EndNode.GetComponent<Node>().Position, position, nodeMergeDistance))
                return street.GetComponent<Street>().EndNode;
        }
        return null;
    }

    public GameObject FindCarSpawnerInRange(Vector3 position, float range)
    {
        foreach (GameObject carSpawner in carSpawnerList)
        {
            if (IsInDistance(carSpawner.GetComponent<CarSpawner>().Position,position,range)) return carSpawner;
        }
        return null;
    }

    public GameObject FindStreetInRange(Vector3 position, float range)
    {
        foreach(GameObject street in streetList)
        {
            foreach(Vector3 wayPoint in street.GetComponent<Street>().WayPoints)
            {
                if(IsInDistance(wayPoint, position, range)) return street;
            }
        }
        return null;
    }

    public GameObject FindClosestStreetInRange(Vector3 position, float range)
    {
        List<Tuple<GameObject, Vector3>> foundStreets = new();
        foreach (GameObject street in streetList) 
            if(FindClosestWayPointOfStreetInRange(position, range, street) != null) 
                foundStreets.Add(FindClosestWayPointOfStreetInRange(position, range, street));

        if(foundStreets.Count <= 0) return null;

        Tuple<GameObject, Vector3> closestStreet = foundStreets[0];
        foreach (Tuple<GameObject, Vector3> street in foundStreets)
            if(Vector3.Distance(street.Item2, position) < Vector3.Distance(closestStreet.Item2, position))
                closestStreet = street;

        return closestStreet.Item1;
    }

    public TrafficGameObjectTypes GetGameObjectType(GameObject gameObject)
    {
        if (gameObject.GetComponent<Street>() != null) return TrafficGameObjectTypes.Street;
        if (gameObject.GetComponent<Node>() != null) return TrafficGameObjectTypes.Node;
        if (gameObject.GetComponent<CarSpawner>() != null) return TrafficGameObjectTypes.CarSpawner;
        if (gameObject.GetComponent<Car>() != null) return TrafficGameObjectTypes.Car;
        if (gameObject.GetComponent<TrafficSign>() != null) return TrafficGameObjectTypes.TrafficSign;
        return TrafficGameObjectTypes.Node;
    }

    private Tuple<GameObject, Vector3> FindClosestWayPointOfStreetInRange(Vector3 position, float range, GameObject street)
    {
        Vector3 closestPoint = Vector3.zero;
        foreach (Vector3 wayPoint in street.GetComponent<Street>().WayPoints)
            if (IsInDistance(wayPoint, position, range) && Vector3.Distance(wayPoint, position) < Vector3.Distance(closestPoint, wayPoint))
                closestPoint = wayPoint;

        if(closestPoint == Vector3.zero) return null;
        return Tuple.Create(street, closestPoint);
    }

    public bool IsInDistance(Vector3 a, Vector3 b, float distance)
    {
        if (Vector3.Distance(a, b) < distance)
            return true;
        return false;
    }
}
