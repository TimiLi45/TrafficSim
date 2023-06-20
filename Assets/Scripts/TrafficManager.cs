using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class TrafficManager : MonoBehaviour
{
    [SerializeField]
    float nodeMergeDistance = 2.0f;

    List<Street> streetList = new();

    public void AddStreet(Vector3 startPoint, Vector3 endPoint)
    {
        streetList.Add(new Street(this, startPoint, endPoint));
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

    private bool IsInDistance(Vector3 a, Vector3 b, float distance)
    {
        if (Vector3.Distance(a, b) < distance)
            return true;
        return false;
    }

}
