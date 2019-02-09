using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

/*
 *	This class serves as the backbone for the genetic algorithm.
 * 	
 */
public class DroneGeneticAlgorithm
{
    // List of current chromosomes
    private List<List<DroneCollection>> _population = new List<List<DroneCollection>>();
    private List<List<DroneCollection>> _matingPool = new List<List<DroneCollection>>();

    private const int PopulationSize = 100;
    private int _generationCount = 1000;

    private List<GameObject> firstLevelContainers;
    public List<GameObject> secondLevelContainers;

    private List<GameObject> _droneGameObjects;

    private List<DroneCollection> _droneCollections;

    public void Run()
    {
        InitializeDrones();
        CreateInitialPopulation();
        EvolvePopulation();
        GetBestChromosome();
    }
    
    private void InitializeDrones()
    {
        // Make this a collection instead.
        for (int i = 0; i < _droneGameObjects.Count; i += 4)
        {
            // Get location from first drone.
            _droneCollections.Add(new DroneCollection(
                _droneGameObjects[i].transform.position,
                _droneGameObjects.GetRange(i, Math.Min(4, _droneGameObjects.Count - i))
            ));
        }
    }
    
    private void CreateInitialPopulation()
    {
        /*
         * Create a random mix of 100 chromosomes
         */
        for (int i = 0; i < PopulationSize; i++)
        {
            _population.Add(GenerateRandomChromosome());
        }
    }

    private void EvolvePopulation()
    {
        for (var i = 0; i < _generationCount; i++)
        {
            CreateMatingPool();

            NaturalSelection();

            Mutation();
        }
    }
    
    private List<DroneCollection> GenerateRandomChromosome()
    {
        var random = new Random();

        // Create a random population.
        List<DroneCollection> chromosome = new List<DroneCollection>(_droneCollections);
        Tasker currentTasker = getTasker();

        for (int j = 0; j < firstLevelContainers.Count + secondLevelContainers.Count; j++)
        {
            int droneIntToPick = random.Next(0, chromosome.Count - 1);
            var currentDrone = chromosome[droneIntToPick];

            int taskIntToPick = random.Next(0, chromosome.Count - 1);

            var currentTask = currentTasker._tasks[taskIntToPick];

            // Drone distance to task
            currentDrone._currentWeight += Vector3.Distance(
                currentDrone._currentLocation, currentTask._startLocation);

            // Add weight to task for future reference.
            currentTask._initialWeight = currentDrone._currentWeight;

            // Add task distance to drone
            currentDrone._currentWeight += currentTask._weight;

            currentDrone.tasks.Add(currentTask);

            currentDrone._currentLocation = currentTask._endLocation;

            Task task = currentTasker._tasks[taskIntToPick];

            // Add next task i.e. 2nd layer of containers.
            if (task._nextTask != null)
            {
                Task nextTask = task._nextTask;
                currentTasker._tasks.Add(nextTask);
            }
            currentTasker._tasks.RemoveAt(taskIntToPick);
        }

        return addWaitTasksToChromosome(chromosome);
    }
    
    
    private List<DroneCollection> addWaitTasksToChromosome(List<DroneCollection> chromosome)
    {
        foreach (var drone in chromosome)
        {
            foreach (var task in drone.tasks)
            {
                if (task.isFirstLevel()) continue;
                
                // Need to add a waiting task before this next task.
                foreach (var nextDrone in chromosome)
                {
                    for (int i = 0; i < nextDrone.tasks.Count; i++)
                    {
                        if (nextDrone.tasks[i] == task._nextTask)
                        {
                            nextDrone.tasks.Insert(i, new Task(task._initialWeight));
                            break;
                        }
                    }
                }
            }
        }
        return chromosome;
    }

    private Task CreateTaskAtIndex(int index)
    {
        var container = firstLevelContainers[index];
        var containerController = container.GetComponent<ContainerController>();

        var nextContainer = secondLevelContainers[index];
        var nextContainerController = container.GetComponent<ContainerController>();

        Task nextTask = new Task(
            nextContainer,
            container.transform.position,
            nextContainerController.LandingContainerController.transform.position,
            Vector3.Distance(
                container.transform.position,
                containerController.LandingContainerController.transform.position
            ));

        return new Task(container,
            container.transform.position,
            containerController.LandingContainerController.transform.position,
            Vector3.Distance(
                container.transform.position,
                containerController.LandingContainerController.transform.position
            ),
            nextTask);
    }

    private Tasker getTasker()
    {
        var tasker = new Tasker();

        for (int i = 0; i < firstLevelContainers.Count; i++)
        {
            tasker.addTask(CreateTaskAtIndex(i));
        }

        return tasker;
    }

    private void GetBestChromosome()
    {
    }

    private void Mutation()
    {
    }

    private void NaturalSelection()
    {
        var newPopulation = new List<List<DroneCollection>>();
        var random = new Random();

        foreach (var chromosome in _population)
        {
            // Select two random chromosome from mating pool
            var parentA = _matingPool[random.Next(0, _matingPool.Count)];
            var parentB = _matingPool[random.Next(0, _matingPool.Count)];

            // Add their crossover to the new population
            newPopulation.Add(crossOver(parentA, parentB));
        }
        _population = newPopulation;
    }

    private List<DroneCollection> crossOver(List<DroneCollection> chromosomeA, List<DroneCollection> chromosomeB)
    {
        /*
         * Now, how do you mix them up?
         */
        
        
        // Line em up bois
        
        

        return chromosomeA;
//
//        var newGene = getEmptyGene();
//        var random = new Random();
//
//        int randomPivot = random.Next(0, _length);
//
//        for (int i = 0; i < _length; i++)
//        {
//            if (i < randomPivot)
//            {
//                newGene[0][i] = geneA[0][i];
//                newGene[1][i] = geneA[1][i];
//            }
//            else
//            {
//                newGene[0][i] = geneB[0][i];
//                newGene[1][i] = geneB[1][i];
//            }
//        }
//        return newGene;
    }

    private float GetFitness(List<DroneCollection> chromosome)
    {
        float maxWeight = 0;
        foreach (var drone in chromosome)
        {
            if (drone._currentWeight > maxWeight)
            {
                maxWeight = drone._currentWeight;
            }
        }

        return maxWeight;
    }


    private void CreateMatingPool()
    {
        _matingPool.Clear();

        var fitnesses = new List<float>();
        // Add chromosomes to mating pool according to their fitness
        foreach (var chromosome in _population)
        {
            float fitness = GetFitness(chromosome);
            fitnesses.Add(fitness);

            for (var k = 0; k < fitness; k++)
            {
                _matingPool.Add(chromosome);
            }
        }

        Debug.Log(string.Format("Average fitness for population: {0}",
            (fitnesses.Sum() / fitnesses.Count).ToString()));
    }
}