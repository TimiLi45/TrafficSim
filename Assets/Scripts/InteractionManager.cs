using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class InteractionManager : MonoBehaviour
{
    [SerializeField, HideInInspector]
    TrafficManager trafficManager;

    [SerializeField, HideInInspector]
    bool renderingWaypoints = false;
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
    }

    private void RenderWaypoints()
    {
        if (!Keyboard.current.tabKey.isPressed)
        {
            foreach (GameObject sphere in wayPointSpheres)
            {
                Destroy(sphere);
            }
            renderingWaypoints = false;
            return;
        }

        if(renderingWaypoints) return;
        renderingWaypoints = true;
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
}
