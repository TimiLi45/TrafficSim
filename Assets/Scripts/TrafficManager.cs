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

    public void AddStreet(Vector3 startPoint, Vector3 endPoint, bool doGenerateIntersections = true)
    {
        GameObject street = new GameObject("Street");
        street.AddComponent<Street>().GetData(this, startPoint, endPoint, doGenerateIntersections);
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
        AddStreet(firstStreet.GetComponent<Street>().StartNode.GetComponent<Node>().Position, intersectionPosition, false);
        AddStreet(firstStreet.GetComponent<Street>().EndNode.GetComponent<Node>().Position, intersectionPosition, false);
        AddStreet(secondStreet.GetComponent<Street>().StartNode.GetComponent<Node>().Position, intersectionPosition, false);
        AddStreet(secondStreet.GetComponent<Street>().EndNode.GetComponent<Node>().Position, intersectionPosition, false);
        /*
        GameObject middleNode = new GameObject("Node");
        middleNode.AddComponent<Node>().GetData(this, intersectionPosition);
        middleNode.transform.SetParent(gameObject.transform.Find("Nodes").transform, true);


        GameObject streetA = new GameObject("Street");
        GameObject streetB = new GameObject("Street");
        GameObject streetC = new GameObject("Street");
        GameObject streetD = new GameObject("Street");

        streetA.AddComponent<Street>().ConnectNodesWithNewStreet(this, firstStreet.GetComponent<Street>().StartNode, middleNode);
        streetB.AddComponent<Street>().ConnectNodesWithNewStreet(this, firstStreet.GetComponent<Street>().EndNode, middleNode);
        streetC.AddComponent<Street>().ConnectNodesWithNewStreet(this, secondStreet.GetComponent<Street>().StartNode, middleNode);
        streetD.AddComponent<Street>().ConnectNodesWithNewStreet(this, secondStreet.GetComponent<Street>().EndNode, middleNode);

        streetA.transform.SetParent(transform.Find("Streets").transform, true);
        streetB.transform.SetParent(transform.Find("Streets").transform, true);
        streetC.transform.SetParent(transform.Find("Streets").transform, true);
        streetD.transform.SetParent(transform.Find("Streets").transform, true);

        streetList.Add(streetA);
        streetList.Add(streetB);
        streetList.Add(streetC);
        streetList.Add(streetD);
        */
        DeleteStreet(firstStreet, false);
        DeleteStreet(secondStreet, false);
    }

    public void DeleteStreet(GameObject street, bool doDeleteNodes = false)
    {
        street.GetComponent<Street>().DeleteStreetContents(doDeleteNodes);
        Debug.Log("------------");
        for(int i  = 0; i < streetList.Count; i++)
        {
            Debug.Log(streetList[i].GetComponent<Street>().StreetID);
        }
        Debug.Log("list count before " + streetList.Count);
        streetList.RemoveAll(streetThis => streetThis.GetComponent<Street>().StreetID == street.GetComponent<Street>().StreetID);
        Destroy(street);
        Debug.Log("delted item "+street.GetComponent<Street>().StreetID);
        Debug.Log("list count afer "+streetList.Count);
        for (int i = 0; i < streetList.Count; i++)
        {
            Debug.Log(streetList[i].GetComponent<Street>().StreetID);
        }
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
