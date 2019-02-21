using UnityEngine;

public class Task
{
    public Vector3 _startLocation;
    public Vector3 _endLocation;
    public float _weight;
    public GameObject _ContainerObject;
    public Task _nextTask;
    public float _initialWeight = 0;
    public int _index;
    
    public bool isUnderWay = false;
    public bool isCompleted = false;

    public bool isUpperLevel()
    {
        return _nextTask == null;
    }

    public Task(GameObject containerObject, Vector3 startLocation, Vector3 endLocation, float weight, int index,
        Task nextTask = null)
    {
        if (nextTask != null) _nextTask = nextTask;

        _index = index;
        _ContainerObject = containerObject;
        _startLocation = startLocation;
        _endLocation = endLocation;
        _weight = weight;
    }
}