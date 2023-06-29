using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.EventSystems;

public enum BuildableTypes
{
    GrabObject,
    BezierCurve,
    Straight,
    CarSpawner,
    TrafficSign
}

public class PartBuilder : MonoBehaviour
{
    [SerializeField]
    float minStreetLength = 3f;

    [SerializeField]
    float trafficSignStreetDetectionDistance = 1.5f;

    [SerializeField, HideInInspector]
    TrafficManager trafficManager;

    [SerializeField, HideInInspector]
    Camera mainCamera;

    [SerializeField, HideInInspector]
    Ray ray;
    [SerializeField, HideInInspector]
    RaycastHit hit;

    [SerializeField, HideInInspector]
    BuildableTypes currentlySelectedType = BuildableTypes.Straight;
    [SerializeField, HideInInspector]
    TrafficSignTypes currentlySelectedTrafficSignType = TrafficSignTypes.maxSpeed;

    [SerializeField, HideInInspector]
    int currentlySelectedTrafficSignValue = 0;

    [SerializeField, HideInInspector]
    bool isDragging = false;

    [SerializeField, HideInInspector]
    Vector3 mousePositionInGame;

    [SerializeField, HideInInspector]
    Vector3 startDrag;
    [SerializeField, HideInInspector]
    Vector3 endDrag;

    [SerializeField, HideInInspector]
    Plane plane = new Plane(Vector3.up, 0);

    public BuildableTypes CurrentlySelectedType
    {
        get { return currentlySelectedType; }
    } 

    public void SetCurrentlySelectedTypebyInt(int ID)
    {
        switch (ID)
        {
            case -1:
                currentlySelectedType = BuildableTypes.GrabObject;
                break;
            case 0:
                currentlySelectedType = BuildableTypes.BezierCurve;
                break;
            case 1:
                currentlySelectedType = BuildableTypes.Straight;
                break;
            case 2:
                currentlySelectedType = BuildableTypes.CarSpawner;
                break;
            case 3:
                currentlySelectedType = BuildableTypes.TrafficSign;
                break;
        }
    }

    public void SetCurrentlySelectedTrafficSignTypeByInt(int ID)
    {
        switch (ID)
        {
            case 0:
                currentlySelectedTrafficSignType = TrafficSignTypes.maxSpeed;
                break;
            case 1:
                currentlySelectedTrafficSignType = TrafficSignTypes.forceStreet;
                break;
            case 2:
                currentlySelectedTrafficSignType = TrafficSignTypes.STOP;
                break;
            case 3:
                currentlySelectedTrafficSignType = TrafficSignTypes.Dijkstra;
                break;
        }
    }

    public void SetCurrentlySelectedTrafficSignValueByString(string value)
    {
        try
        {
            currentlySelectedTrafficSignValue = Convert.ToInt32 (value);
        }
        catch
        {
            currentlySelectedTrafficSignValue = 0;
            Debug.Log("Please input an int, not a string.");
        }
    }

    void Awake()
    {
        mainCamera = Camera.main;
        trafficManager = this.GetComponent<TrafficManager>();
    }

    void Update()
    {
        ChangeVariables();
        BuildPart();
        if (isDragging)
            DragPlaceStreet();
    }

    private void DragPlaceStreet()
    {
        if (Mouse.current.leftButton.isPressed)
            return;

        isDragging = false;
        GetMousePosition();
        endDrag = mousePositionInGame;

        if (Vector3.Distance(startDrag, endDrag) < minStreetLength)
            return;

        trafficManager.AddStreet(startDrag, endDrag);
    }

    private void BuildPart()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame || EventSystem.current.IsPointerOverGameObject())
            return;

        switch (currentlySelectedType)
        {
            case BuildableTypes.GrabObject:
                break;
            case BuildableTypes.Straight:
                GetMousePosition();
                isDragging = true;
                startDrag = mousePositionInGame;
                break;
            case BuildableTypes.CarSpawner:
                PlaceCarSpawner();
                break;
            case BuildableTypes.TrafficSign:
                PlaceTrafficSign();
                break;
        }
    }

    private Vector3 GetMousePosition()
    {
        Vector3 worldposition;
        float distance;
        Ray Mouseray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (plane.Raycast(Mouseray, out distance))
            worldposition = Mouseray.GetPoint(distance);
        else
            worldposition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        ray = new Ray(worldposition, mainCamera.transform.forward);

        Physics.Raycast(ray, out hit, 1000f);
        mousePositionInGame = hit.point;
        return mousePositionInGame;
    }

    private void ChangeVariables()
    {
        if (currentlySelectedType == BuildableTypes.GrabObject) gameObject.GetComponent<Selection>().DoSelection = true;
        else gameObject.GetComponent<Selection>().DoSelection = false;
    }

    private void PlaceCarSpawner()
    {
        Vector3 placePosition = GetMousePosition();
        trafficManager.AddCarSpawner(placePosition);
    }

    private void PlaceTrafficSign()
    {
        Vector3 placePosition = GetMousePosition();
        Quaternion rotation;
        if(trafficManager.FindClosestStreetInRange(placePosition, trafficSignStreetDetectionDistance) != null)
        {
            GameObject streetInRange = trafficManager.FindClosestStreetInRange(placePosition, trafficSignStreetDetectionDistance);
            rotation = Quaternion.LookRotation((streetInRange.GetComponent<Street>().StartPoint - streetInRange.GetComponent<Street>().EndPoint).normalized);
        }
        else
        {
            rotation = Quaternion.LookRotation((mainCamera.transform.position - placePosition).normalized);
        }
        rotation.x = 0; // I didn't find any better way of setting JUST the y rotation, that I liked better than this,
        rotation.z = 0; // so I had to reset the x and z rotation, it however does not matter, so.. too bad!
        trafficManager.AddTrafficSign(placePosition, currentlySelectedTrafficSignType, currentlySelectedTrafficSignValue, rotation);
    }
}
