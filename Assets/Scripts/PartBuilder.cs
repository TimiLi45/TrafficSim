using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public enum BuildableTypes
{
    Crossroad,
    BezierCurve,
    Straight,
    CarSpawner
}

public class PartBuilder : MonoBehaviour
{
    [SerializeField]
    float minStreetLength = 1.5f;

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
    bool isDragging = false;

    [SerializeField, HideInInspector]
    Vector3 mousePositionInGame;

    Vector3 startDrag;
    Vector3 endDrag;

    Plane plane = new Plane(Vector3.up, 0);

    public BuildableTypes CurrentlySelectedType
    {
        get { return currentlySelectedType; }
        set { currentlySelectedType = value; }
    } 

    public void SetCurrentlySelectedTypebyInt(int ID)
    {
        switch (ID)
        {
            case 0:
                currentlySelectedType = BuildableTypes.BezierCurve;
                break;
            case 1:
                currentlySelectedType = BuildableTypes.Straight;
                break;
            case 2:
                currentlySelectedType = BuildableTypes.CarSpawner;
                break;
        }
    }

    void Awake()
    {
        mainCamera = Camera.main;
        trafficManager = this.GetComponent<TrafficManager>();
    }

    void Update()
    {
        BuildPart();
        if (isDragging)
            Drag();
    }
    private void Drag()
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
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        switch (currentlySelectedType)
        {
            case BuildableTypes.Crossroad:
                break;
            case BuildableTypes.BezierCurve:
                break;
            case BuildableTypes.Straight:
                GetMousePosition();
                isDragging = true;
                startDrag = mousePositionInGame;
                break;
            case BuildableTypes.CarSpawner:
                PlaceCarSpawner();
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

    private void PlaceCarSpawner()
    {
        Vector3 placePosition = GetMousePosition();
        trafficManager.AddCarSpawner(placePosition);
    }
}
