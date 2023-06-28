using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Selection : MonoBehaviour
{
    [SerializeField]
    Shader outlineShader;

    [SerializeField, HideInInspector]
    TrafficManager trafficManager;
    [SerializeField, HideInInspector]
    Camera mainCamera;

    [SerializeField, HideInInspector]
    bool doSelection;
    
    [SerializeField, HideInInspector]
    GameObject hoveredObject;
    [SerializeField, HideInInspector]
    List<Tuple<GameObject,Shader>> outlinedGameObjects = new();

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
        GenerateOutlineOnMouseOver();
    }

    private void GenerateOutlineOnMouseOver()
    {
        hoveredObject = GetHoveredObject();

        foreach (Tuple<GameObject,Shader> gameObject in outlinedGameObjects)
            if(gameObject.Item1 != hoveredObject) { gameObject.Item1.GetComponentInChildren<Renderer>().material.shader = gameObject.Item2; }
        outlinedGameObjects.RemoveAll(gameObject => gameObject.Item1.GetComponentInChildren<Renderer>().material.shader == gameObject.Item2);

        if(hoveredObject == null) return;

        Shader previousShader = hoveredObject.GetComponentInChildren<Renderer>().material.shader;
        hoveredObject.GetComponentInChildren<Renderer>().material.shader = outlineShader;
        outlinedGameObjects.Add(Tuple.Create(hoveredObject, previousShader));
    }

    private GameObject GetHoveredObject()
    {
        ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        Physics.Raycast(ray, out hit, 1000f);
        
        mousePositionInGame = hit.point;
        if (hit.collider.gameObject.tag == "Ground") return null;
        return hit.collider.gameObject;
    }
}
