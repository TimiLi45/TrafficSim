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
    bool renderingWayPoints = false;
    [SerializeField, HideInInspector]
    bool renderingStreetIDs = false;
    [SerializeField, HideInInspector]
    bool renderingNodePositions = false;
    [SerializeField, HideInInspector]
    List<GameObject> wayPointSpheres;

    void Awake()
    {
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
        RenderNodePositions();
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
        if (!Keyboard.current.f3Key.isPressed)
        {
            renderingStreetIDs = false;
            return;
        }/*
        if (renderingStreetIDs) return;
        renderingStreetIDs = true;
        foreach (GameObject street in trafficManager.StreetList)
        {
            Debug.Log("test");
            Canvas canvas;
            street.AddComponent<Canvas>();
            canvas = street.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.localScale = new(.01f, .01f, .01f);
            canvas.transform.position += new Vector3(0, 2, 0);

            GameObject text = new GameObject();
            text.transform.parent = street.transform;
            text.name = street.GetComponent<Street>().StreetID.ToString();

            text.AddComponent<Text>();
            text.GetComponent<Text>().text = street.GetComponent<Street>().StreetID.ToString();
            text.GetComponent<Text>().fontSize = 100;

        }*/
    }

    private void RenderNodePositions()
    {
        if (!Keyboard.current.f3Key.isPressed) return;
    }
}
