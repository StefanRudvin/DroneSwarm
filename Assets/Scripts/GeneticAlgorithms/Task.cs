using UnityEngine;

public class Task
{
    public Vector3 _startLocation;
    public Vector3 _endLocation;
    public float _weight;
    public GameObject _gameObject;
    public Task _nextTask;
    public float _initialWeight;
    public float _waitingTime;
    
    public bool isFirstLevel()
    {
        return _nextTask != null;
    }

    public Task(float waitingTime)
    {
        _waitingTime = waitingTime;
    }

    public Task(GameObject gameObject, Vector3 startLocation, Vector3 endLocation, float weight,
        Task nextTask = null)
    {
        if (nextTask != null) _nextTask = nextTask;

        _gameObject = gameObject;
        _startLocation = startLocation;
        _endLocation = endLocation;
        _weight = weight;
    }
}