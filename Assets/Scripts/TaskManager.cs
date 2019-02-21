using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class TaskManager
{
    private readonly Chromosome _chromosome;

    public TaskManager(Chromosome chromosome)
    {
        _chromosome = chromosome;
    }

    public void ResetDrones(List<GameObject> drones)
    {
        foreach (var drone in drones)
        {
            DroneController droneController = drone.GetComponent<DroneController>();
            droneController.ResetTarget();
        }
    }

    public void Run()
    {
        /*
         * Check if task is completed by drones.
         * Attach drones to container for task.
         */

        foreach (var droneCollection in _chromosome._droneCollection)
        {
            if (droneCollection._tasks.Count == 0)
            {
                ResetDrones(droneCollection._drones);
                break;
            }
            Task currentTask = droneCollection._tasks.First();
            
            if (currentTask.isCompleted)
            {
                droneCollection._tasks.Remove(currentTask);
                // This could/should look for the next task, but we can just wait for the next game tick instead.
                continue;
            }
            
            if (currentTask.isUnderWay) continue;
            
            // Assign drones to task.
            var containerController = currentTask._ContainerObject.GetComponent<ContainerController>();
            containerController.assignDronesToContainer(droneCollection._drones, currentTask);
            currentTask.isUnderWay = true;
        }
    }
}