using UnityEngine;

namespace GeneticAlgorithms.Drone
{
    public class Task
    {
        public readonly GameObject _containerObject;
        public float _initialWeight = 0;
        public Vector3 _startLocation;
        public Vector3 _endLocation;
        public int _shipPriority;
        public Task _nextTask;
        public float _weight;
        public int _index;

        public bool isCompleted = false;

        public bool isUpperLevel()
        {
            return _nextTask == null;
        }

        public Task(GameObject containerObject, Vector3 startLocation, Vector3 endLocation, float weight, int index, int shipPriority,
            Task nextTask = null)
        {
            if (nextTask != null) _nextTask = nextTask;
            _containerObject = containerObject;
            _startLocation = startLocation;
            _shipPriority = shipPriority;
            _endLocation = endLocation;
            _weight = weight;
            _index = index;
        }
    }
}