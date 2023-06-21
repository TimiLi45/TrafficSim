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

    public void GenerateIntersection(Street firstStreet, Street secondStreet)
    {
        Vector2 A = new(firstStreet.StartNode.Position.x, firstStreet.StartNode.Position.z);
        Vector2 B = new(firstStreet.EndNode.Position.x, firstStreet.EndNode.Position.z);
        Vector2 C = new(secondStreet.StartNode.Position.x, secondStreet.StartNode.Position.z);
        Vector2 D = new(secondStreet.EndNode.Position.x, secondStreet.EndNode.Position.z);
        float a1 = B.y - A.y;
        float b1 = A.x - B.x;
        float c1 = a1 * A.x + b1 * A.y;

        float a2 = D.y - C.y;
        float b2 = C.x - D.x;
        float c2 = a2 * C.x + b2 * C.y;

        float determinant = a1 * b2 - a2 * b1;

        if (determinant == 0) return;

        float x = (b2 * c1 - b1 * c2) / determinant;
        float z = (a1 * c2 -a2 * c1) / determinant;

        Vector3 nodeAPosition = firstStreet.StartNode.Position;
        Vector3 nodeBPosition = firstStreet.EndNode.Position;
        Vector3 nodeCPosition = secondStreet.StartNode.Position;
        Vector3 nodeDPosition = secondStreet.EndNode.Position;
        Vector3 middleNodePosition = new(x, (nodeAPosition.y+ nodeBPosition.y+ nodeCPosition.y+ nodeDPosition.y)/4, z);

        streetList.Add(new Street(this, nodeAPosition, middleNodePosition));
        streetList.Add(new Street(this, nodeBPosition, middleNodePosition));
        streetList.Add(new Street(this, nodeCPosition, middleNodePosition));
        streetList.Add(new Street(this, nodeDPosition, middleNodePosition));

        DeleteStreet(firstStreet);
        DeleteStreet(secondStreet);
    }

    public void DeleteStreet(int streetID)
    {
        foreach (Street street in streetList)
        {
            if (street.StreetID == streetID)
            {
                street.DeleteNodes();
                streetList.Remove(street);
                break;
            }            
        }
    }

    public void DeleteStreet(Street street)
    {
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
