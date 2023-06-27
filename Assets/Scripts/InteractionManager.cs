using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class InteractionManager : MonoBehaviour
{
    [SerializeField, HideInInspector]
    TrafficManager trafficManager;
    [SerializeField, HideInInspector]
    Camera mainCamera;

    [SerializeField, HideInInspector]
    bool renderingWayPoints = false;
    [SerializeField, HideInInspector]
    bool renderingStreetIDs = false;
    [SerializeField, HideInInspector]
    bool renderingNodeIDsAndPositions = false;
    [SerializeField, HideInInspector]
    List<GameObject> wayPointSpheres;

    void Awake()
    {
        mainCamera = Camera.main;
        trafficManager = gameObject.GetComponent<TrafficManager>();
    }

    void Start()
    {
        wayPointSpheres = new List<GameObject>();
    }

    private void Update()
    {
        RenderWaypoints();
        RenderStreetIDs();
        RenderNodeIDsAndPositions();

        RotateStreetIDs();
        //RotateNodeIDsAndPositions();
    }

    private void RenderWaypoints()
    {
        if (!Keyboard.current.f3Key.isPressed)
        {
            foreach (GameObject sphere in wayPointSpheres)
            {
                Destroy(sphere);
            }
            renderingWayPoints = false;
            return;
        }

        if(renderingWayPoints) return;
        renderingWayPoints = true;
        foreach (GameObject street in trafficManager.StreetList)
        {
            foreach (Vector3 wayPoint in street.GetComponent<Street>().WayPoints)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.localScale = new(trafficManager.WayPointSphereSize, trafficManager.WayPointSphereSize, trafficManager.WayPointSphereSize);
                sphere.transform.position = wayPoint;
                sphere.layer = LayerMask.NameToLayer("Ignore Raycast");
                sphere.GetComponent<SphereCollider>().enabled = false;
                wayPointSpheres.Add(sphere);
            }
        }
    }

    private void RenderStreetIDs()
    {
        if (!Keyboard.current.f3Key.isPressed && !renderingStreetIDs) return;
        if (!Keyboard.current.f3Key.isPressed && renderingStreetIDs)
        {
            foreach (GameObject street in trafficManager.StreetList)
            {
                    Destroy(street.GetComponent<TextMesh>());
            }
            renderingStreetIDs = false;
            return;
        }

        if (renderingStreetIDs) return;
        renderingStreetIDs = true;

        foreach (GameObject street in trafficManager.StreetList)
        {
            street.AddComponent<TextMesh>();
            street.GetComponent<TextMesh>().text = street.GetComponent<Street>().StreetID.ToString();
            street.GetComponent<TextMesh>().characterSize = .1f;
            street.GetComponent<TextMesh>().fontSize = 150;
            street.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            street.GetComponent<TextMesh>().alignment = TextAlignment.Center;
            street.GetComponent<TextMesh>().transform.position = Vector3.Lerp(street.GetComponent<Street>().StartPoint, street.GetComponent<Street>().EndPoint, .5f);
            street.GetComponent<TextMesh>().transform.position += new Vector3(0, 3, 0);
        }
    }

    private void RenderNodeIDsAndPositions()
    {
        if (!Keyboard.current.f3Key.isPressed && !renderingNodeIDsAndPositions) return;
        if (!Keyboard.current.f3Key.isPressed && renderingNodeIDsAndPositions)
        {
            foreach (GameObject node in trafficManager.NodeList)
            {
                Destroy(node.GetComponent<TextMesh>());
            }
            renderingNodeIDsAndPositions = false;
            return;
        }

        if (renderingNodeIDsAndPositions) return;
        renderingNodeIDsAndPositions = true;
        /*
        foreach (GameObject node in trafficManager.NodeList)
        {
            node.AddComponent<TextMesh>();
            node.GetComponent<TextMesh>().text = node.GetComponent<Node>().NodeID.ToString() + node.GetComponent<Node>().Position.ToString();
            node.GetComponent<TextMesh>().characterSize = .1f;
            node.GetComponent<TextMesh>().fontSize = 150;
            node.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            node.GetComponent<TextMesh>().alignment = TextAlignment.Center;
            node.GetComponent<TextMesh>().transform.position = node.GetComponent<Node>().Position;
            node.GetComponent<TextMesh>().transform.position += new Vector3(0, 3, 0);
        }*/
    }


    private void RotateStreetIDs()
    {
        if (!renderingStreetIDs) return;
        foreach (GameObject street in trafficManager.StreetList)
        {
            if (street.GetComponent<TextMesh>() == null) continue;
            street.GetComponent<TextMesh>().transform.rotation = Quaternion.LookRotation((street.GetComponent<TextMesh>().transform.position-mainCamera.transform.position).normalized);
        }
    }

    private void RotateNodeIDsAndPositions()
    {
        if (!renderingNodeIDsAndPositions) return;
        foreach (GameObject node in trafficManager.NodeList)
        {
            if (node.GetComponent<TextMesh>() == null) continue;
            node.GetComponent<TextMesh>().transform.rotation = Quaternion.LookRotation((node.GetComponent<TextMesh>().transform.position - mainCamera.transform.position).normalized);
        }
    }
}
