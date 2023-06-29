using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    void Awake()
    {
        mainCamera = Camera.main;
        trafficManager = gameObject.GetComponent<TrafficManager>();
    }

    private void Update()
    {
        RenderWaypoints();
        RenderStreetIDs();
        RenderNodeIDsAndPositions();

        RotateStreetIDs();
        RotateNodeIDsAndPositions();
    }

    private void RenderWaypoints()
    {
        if (!Keyboard.current.f3Key.isPressed)
        {
            foreach (GameObject street in trafficManager.StreetList)
                foreach (GameObject wayPointSphere in street.GetComponent<Street>().WayPointSpheres)
                    wayPointSphere.SetActive(false);
            renderingWayPoints = false;
            return;
        }

        if(renderingWayPoints) return;
        renderingWayPoints = true;
        foreach (GameObject street in trafficManager.StreetList)
            foreach (GameObject wayPointSphere in street.GetComponent<Street>().WayPointSpheres)
                wayPointSphere.SetActive(true);
    }

    private void RenderStreetIDs()
    {
        if (!Keyboard.current.f3Key.isPressed && !renderingStreetIDs) return;
        if (!Keyboard.current.f3Key.isPressed && renderingStreetIDs)
        {
            foreach (GameObject street in trafficManager.StreetList)
            {
                if (street.transform.Find("streetDebug") == null) continue;
                Destroy(street.transform.Find("streetDebug").gameObject);
            }
            renderingStreetIDs = false;
            return;
        }

        if (renderingStreetIDs) return;
        renderingStreetIDs = true;

        foreach (GameObject street in trafficManager.StreetList)
        {
            GameObject streetDebug = new("streetDebug");
            streetDebug.transform.SetParent(street.transform);
            streetDebug.transform.position = Vector3.Lerp(street.GetComponent<Street>().StartPoint, street.GetComponent<Street>().EndPoint, .5f);
            streetDebug.AddComponent<TextMesh>();
            streetDebug.GetComponent<TextMesh>().text = street.GetComponent<Street>().StreetID.ToString()+"\n";
            streetDebug.GetComponent<TextMesh>().color = Color.magenta;
            streetDebug.GetComponent<TextMesh>().characterSize = .1f;
            streetDebug.GetComponent<TextMesh>().fontSize = 150;
            streetDebug.GetComponent<TextMesh>().anchor = TextAnchor.LowerCenter;
            streetDebug.GetComponent<TextMesh>().alignment = TextAlignment.Center;
            streetDebug.GetComponent<TextMesh>().lineSpacing = .9f;
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
        
        foreach (GameObject node in trafficManager.NodeList)
        {
            node.AddComponent<TextMesh>();
            node.GetComponent<TextMesh>().text = node.GetComponent<Node>().NodeID.ToString() +"\n"+ node.GetComponent<Node>().Position.ToString()+ "\n";
            node.GetComponent<TextMesh>().characterSize = .1f;
            node.GetComponent<TextMesh>().fontSize = 100;
            node.GetComponent<TextMesh>().anchor = TextAnchor.LowerCenter;
            node.GetComponent<TextMesh>().alignment = TextAlignment.Center;
            node.GetComponent<TextMesh>().lineSpacing = .9f;
        }
    }


    private void RotateStreetIDs()
    {
        if (!renderingStreetIDs) return;
        foreach (GameObject street in trafficManager.StreetList)
        {
            if (street.transform.Find("streetDebug") == null) continue;
            GameObject streetDebug = street.transform.Find("streetDebug").gameObject;
            if (streetDebug.GetComponent<TextMesh>() == null) continue;
            streetDebug.GetComponent<TextMesh>().transform.rotation = Quaternion.LookRotation((streetDebug.GetComponent<TextMesh>().transform.position-mainCamera.transform.position).normalized);
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
