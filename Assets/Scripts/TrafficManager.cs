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

    List<Street> streetList = new();

    public float WayPointDistance
    {
        get { return wayPointDistance; }
    }
    public float WayPointSphereSize
    {
        get { return wayPointSphereSize; }
    }
    public List<Street> StreetList
    {
        get { return streetList; }
    }

    public void AddStreet(Vector3 startPoint, Vector3 endPoint)
    {
        streetList.Add(new Street(this, startPoint, endPoint));
    }

    public void AddCarSpawner(Vector3 position)
    {
        bool found = false;
        foreach (Street street in StreetList)
        {
            if (IsInDistance(position, street.StartNode.Position, 5f))
            {
                GameObject carSpawner = new GameObject();
                carSpawner.AddComponent<CarSpawner>().GetData(this, position);
                carSpawner.name = "CarSpawner";
                found = true;
                return;
            }
        }
        if (!found)
        {
            Debug.Log("No Location Found for Car Spawner");
        }
    }

    public void GenerateIntersection(Street firstStreet, Street secondStreet, Vector3 intersectionPosition)
    {
        DeleteStreet(firstStreet);
        DeleteStreet(secondStreet);
        streetList.Add(new Street(this, firstStreet.StartNode.Position, intersectionPosition));
        streetList.Add(new Street(this, firstStreet.EndNode.Position, intersectionPosition));
        streetList.Add(new Street(this, secondStreet.StartNode.Position, intersectionPosition));
        streetList.Add(new Street(this, secondStreet.EndNode.Position, intersectionPosition));
    }

    public void DeleteStreet(Street street)
    {
        street.DeleteLine();
        street.DeleteNodes();
        streetList.Remove(street);
    }

    public Node FindNodeWithPosition(Vector3 Position)
    {
        foreach (Street street in streetList)
        {
            if (IsInDistance(street.StartNode.Position, Position, nodeMergeDistance))
                return street.StartNode;
            if (IsInDistance(street.EndNode.Position, Position, nodeMergeDistance))
                return street.EndNode;
        }
        return null;
    }

    public bool IsNodeOnStreet(Node node, Street street)
    {
        return (Math.Abs(Vector3.Distance(street.StartNode.Position, node.Position)) + Math.Abs(Vector3.Distance(street.EndNode.Position, node.Position))) == Math.Abs(Vector3.Distance(street.StartNode.Position, street.EndNode.Position));
    }

    public bool IsInDistance(Vector3 a, Vector3 b, float distance)
    {
        if (Vector3.Distance(a, b) < distance)
            return true;
        return false;
    }

    public void DeleteCarSpawner(CarSpawner carSpawner)
    {
        carSpawner = null;
    }

}
