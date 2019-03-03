using System.Collections.Generic;
using UnityEngine;

public class DroneCollection
{
    public List<Task> _tasks;
    public Vector3 _currentLocation;
    public float _currentWeight;
    public List<GameObject> _drones;
    public Vector3 _startLocation;

    public Task _currentTask = null;
    
    public DroneCollection(Vector3 currentLocation, List<GameObject> drones, Task currentTask)
    {
        _currentTask = currentTask;
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