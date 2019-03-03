using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class TaskManager
{
    public Chromosome _chromosome;
    public List<GameObject> _containers = new List<GameObject>();

    public TaskManager(Chromosome chromosome)
    {
        _chromosome = chromosome;
    }

    public void setChromosome(Chromosome chromosome)
    {
        _chromosome = chromosome;
    }

    public bool Run()
    {
        /*
         * Check if task is completed by drones.
         * Attach drones to container for task.
         */
        bool areNewContainersAdded = false;
        foreach (var droneCollection in _chromosome._droneCollection)
        {
            if (droneCollection._tasks.Count == 0)
            {
                continue;
            }

            // Drone is currently on a mission
            if (droneCollection._currentTask != null && droneCollection._currentTask.isCompleted)
            {
                areNewContainersAdded = true;
                Task currentTask = droneCollection._currentTask;
                // Add second layer task to the mix.
                if (currentTask._nextTask != null)
                {
                    _containers.Add(currentTask._nextTask._containerObject);
                }
                currentTask._containerObject.GetComponent<ContainerController>()._isCompleted = true;
                _containers.Remove(currentTask._containerObject);
                
                droneCollection._currentTask = null;
                // This could/should look for the next task, but we can just wait for the next game tick instead.
            }

            if (droneCollection._currentTask != null) continue;
            
            Task nextTask = droneCollection._tasks.First();

            // Assign drones to task.
            var containerController = nextTask._containerObject.GetComponent<ContainerController>();
            containerController.assignDronesToContainer(droneCollection._drones, nextTask);
            containerController._isTargeted = true;            

            droneCollection._currentTask = nextTask;
            droneCollection._tasks.Remove(nextTask);
            _containers.Remove(nextTask._containerObject);
        }
        return areNewContainersAdded;
    }
}