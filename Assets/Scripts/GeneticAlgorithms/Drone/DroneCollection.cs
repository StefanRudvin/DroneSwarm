using System.Collections.Generic;
using UnityEngine;

public class DroneCollection
{
    public List<Task> _tasks;
    public Vector3 _currentLocation;
    public float _currentWeight;
    public List<GameObject> _drones;
    private Vector3 _startLocation;

    public DroneCollection(Vector3 currentLocation, List<GameObject> drones)
    {
        _currentLocation = currentLocation;
        _drones = drones;
        _tasks = new List<Task>();
        _startLocation = currentLocation;
    }

    public void Reset()
    {
        _currentLocation = _startLocation;
        _currentWeight = 0f;
    }
}