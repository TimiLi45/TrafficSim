using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;





public class CarSpawner  : MonoBehaviour 
{
    TrafficManager trafficManager;
    Node connectedNode;
    bool active = false;
    Vector3 position;
    private float _timer = 0f;

    [SerializeField]
    float CarSpanRate = 2f;

    // Konstructor
    public CarSpawner(TrafficManager trafficManager, Vector3 position)
    {
        this.trafficManager = trafficManager;
        this.position = position;

    }


    void Start()
    {
        foreach (Street street in trafficManager.StreetList)
        {
            if (trafficManager.IsInDistance( position, street.StartNode.Position, 5f))
            {
                active = true;
                connectedNode = street.StartNode;
            }
        }

        if (!active)
            Debug.Log(this + " self delet becouse no Node in area");
            trafficManager.DeleteCarSpawner(this);

    }

    private void Update()
    {
        _timer =+Time.deltaTime;
        if(_timer >= CarSpanRate)
        {
            SpawnCar();
            _timer = 0f;
        }


    }   

    private void SpawnCar()
    {
        GameObject car = new GameObject();
        car.AddComponent<Car>();
        car.GetComponent<Car>().Awake(trafficManager,connectedNode);
    }


}
