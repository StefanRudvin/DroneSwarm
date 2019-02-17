using System.Collections.Generic;
using UnityEngine;

public class DroneCollection
{
    public List<Task> _tasks;
    public Vector3 _currentLocation;
    public float _currentWeight;
    public List<GameObject> _drones;

    public DroneCollection(Vector3 currentLocation, List<GameObject> drones)
    {
        _drones = drones;
        _currentLocation = currentLocation;
        _tasks = new List<Task>();
    }
}