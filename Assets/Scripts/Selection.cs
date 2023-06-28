using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Selection : MonoBehaviour
{
    [SerializeField, HideInInspector]
    TrafficManager trafficManager;
    [SerializeField, HideInInspector]
    Camera mainCamera;

    [SerializeField, HideInInspector]
    bool doSelection;

    [SerializeField, HideInInspector]
    List<GameObject> outlinedGameObjects;

    [SerializeField, HideInInspector]
    Vector3 mousePositionInGame;

    [SerializeField, HideInInspector]
    Ray ray;
    [SerializeField, HideInInspector]
    RaycastHit hit;

    [SerializeField, HideInInspector]
    Plane plane = new Plane(Vector3.up, 0);

    public bool DoSelection{
        get { return doSelection; }
        set { doSelection = value; }
    }

    void Awake()
    {
        mainCamera = Camera.main;
        trafficManager = this.GetComponent<TrafficManager>();
    }

    void Update()
    {
        if (!doSelection) return;
        Debug.Log("test");
        GenerateOutlineOnMouseOver();
    }

    private void GenerateOutlineOnMouseOver()
    {
        GameObject clickedObject = GetHoveredObject();
        Debug.Log(clickedObject);
    }

    private GameObject GetHoveredObject()
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
        return hit.collider.gameObject;
    }
}
