using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Selection : MonoBehaviour
{
    [SerializeField]
    Shader outlineShader;

    [SerializeField]
    Canvas editCanvas;

    [SerializeField, HideInInspector]
    TrafficManager trafficManager;
    [SerializeField, HideInInspector]
    Camera mainCamera;

    [SerializeField, HideInInspector]
    bool doSelection;
    
    [SerializeField, HideInInspector]
    GameObject hoveredObject;
    [SerializeField, HideInInspector]
    GameObject selectedObject;
    [SerializeField, HideInInspector]
    List<Tuple<Renderer, Shader>> selectedObjectPreviousShaders = new();
    [SerializeField, HideInInspector]
    List<Tuple<Renderer,Shader>> outlinedGameObjects = new();

    [SerializeField, HideInInspector]
    Vector3 mousePositionInGame;

    [SerializeField, HideInInspector]
    Ray ray;
    [SerializeField, HideInInspector]
    RaycastHit hit;

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
        if (!doSelection || EventSystem.current.IsPointerOverGameObject()) return;
        RemoveOutlineOnNotHoveredObjects();
        GenerateOutlineOnMouseOver();
        SelectObject();

        if (selectedObject != null) editCanvas.GetComponent<Canvas>().enabled = true;
        else editCanvas.GetComponent<Canvas>().enabled = false;
    }

    private void RemoveOutlineOnNotHoveredObjects()
    {
        hoveredObject = GetHoveredObject();
        outlinedGameObjects.RemoveAll(gameObject => gameObject.Item1 == null);
        foreach (Tuple<Renderer, Shader> rendererAndShader in outlinedGameObjects)
            if (rendererAndShader.Item1.gameObject != hoveredObject)
                rendererAndShader.Item1.material.shader = rendererAndShader.Item2;
        outlinedGameObjects.RemoveAll(rendererAndShader => rendererAndShader.Item1.material.shader == rendererAndShader.Item2);
    }

    private void GenerateOutlineOnMouseOver()
    {
        hoveredObject = GetHoveredObject();
        if (hoveredObject == null) return;

        Renderer[] hoveredObjectRenderers = hoveredObject.GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in hoveredObjectRenderers) 
        {
            outlinedGameObjects.Add(Tuple.Create(renderer, renderer.material.shader));
            renderer.material.shader = outlineShader; 
        }
    }

    private GameObject GetHoveredObject()
    {
        if(Mouse.current.position == null) return null;
        ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        Physics.Raycast(ray, out hit, 1000f);
        
        mousePositionInGame = hit.point;
        if (hit.collider.gameObject.tag == "Ground") return null;
        return hit.collider.gameObject;
    }

    private void SelectObject()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        GetHoveredObject();
        selectedObject = hoveredObject;

        // Reset all shaders if nothing is selected or other object is selected
        if (selectedObjectPreviousShaders.Count>0 &&
            (selectedObject == null || 
            (!selectedObjectPreviousShaders[0].Item1.gameObject.Equals(selectedObject) && 
            !selectedObjectPreviousShaders[0].Item1.gameObject.transform.parent.gameObject.Equals(selectedObject))))
        {
            foreach (Tuple<Renderer, Shader> rendererAndShader in selectedObjectPreviousShaders)
                rendererAndShader.Item1.material.shader = rendererAndShader.Item2;
            selectedObjectPreviousShaders.Clear();
        }

        // Safe all old shaders of selected object
        foreach (Tuple<Renderer, Shader> rendererAndShader in outlinedGameObjects)
            if(rendererAndShader.Item1.gameObject.Equals(selectedObject) || rendererAndShader.Item1.gameObject.transform.parent.gameObject.Equals(selectedObject))
                selectedObjectPreviousShaders.Add(rendererAndShader);
        outlinedGameObjects.RemoveAll(rendererAndShader => (rendererAndShader.Item1.gameObject.Equals(selectedObject) || rendererAndShader.Item1.gameObject.transform.parent.gameObject.Equals(selectedObject)));
    }

    public void DeleteSelectedObject()
    {
        foreach(Tuple<Renderer, Shader> rendererAndShader in outlinedGameObjects)
            rendererAndShader.Item1.material.shader = rendererAndShader.Item2;
        outlinedGameObjects.Clear();
        selectedObjectPreviousShaders.Clear();
        TrafficGameObjectTypes selectedType = trafficManager.GetGameObjectType(selectedObject);
        trafficManager.DeleteGameObjectByType(selectedObject, selectedType);
    }

    public void MoveSelectedObject()
    {

    }
}
