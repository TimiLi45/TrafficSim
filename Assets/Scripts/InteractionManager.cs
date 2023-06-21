using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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
        trafficManager = this.GetComponent<TrafficManager>();
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
                GameObject.Destroy(sphere);
            }
            renderingWaypoints = false;
            return;
        }

        if(renderingWaypoints) return;
        renderingWaypoints = true;
        foreach (Street street in trafficManager.StreetList)
        {
            foreach (Vector3 wayPoint in street.WayPoints)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.localScale = new(trafficManager.WayPointSphereSize, trafficManager.WayPointSphereSize, trafficManager.WayPointSphereSize);
                sphere.transform.position = wayPoint;
                wayPointSpheres.Add(sphere);
            }
        }
    }
}
