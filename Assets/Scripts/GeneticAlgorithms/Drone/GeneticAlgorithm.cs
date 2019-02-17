using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

/*
 *	This class serves as the backbone for the genetic algorithm.
 * 	
 */
public class DroneGeneticAlgorithm
{
    // List of current chromosomes
    private List<Chromosome> _population;
    private List<Chromosome> _matingPool;

    private const int PopulationSize = 100;
    private const int GenerationCount = 2;

    private float _fittestChromosomeFitness = 1000000000;
    private Chromosome _fittestChromosome;

    private List<GameObject> _firstLevelContainers;
    private List<GameObject> _secondLevelContainers;

    private List<GameObject> _droneGameObjects;

    public int taskIndex = 0;

    public DroneGeneticAlgorithm()
    {
        _matingPool = new List<Chromosome>();
        _population = new List<Chromosome>();
        
        _firstLevelContainers = new List<GameObject>();
        _secondLevelContainers = new List<GameObject>();
    }

    public void AddDrones(List<GameObject> drones)
    {
        _droneGameObjects = drones;
    }

    public Chromosome Run()
    {
        CreateInitialPopulation();
        ProcessPopulation();
        EvolvePopulation();
        return GetBestChromosome();
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
        for (int i = 0; i < GenerationCount; i++)
        {
            Debug.Log(string.Format("Generation: {0}", i));

            CreateMatingPool();

            Debug.Log(string.Format("Mating pool count: {0}", _matingPool.Count.ToString()));

            NaturalSelection();

            Mutation();

            ProcessPopulation();
        }
    }

    private void ProcessPopulation()
    {
        float minWeight = 9999f;
        float maxWeight = 0;

        foreach (Chromosome chromosome in _population)
        {
            ProcessChromosome(chromosome);
            minWeight = Math.Min(minWeight, chromosome._weight);
            maxWeight = Math.Max(maxWeight, chromosome._weight);
        }

        foreach (Chromosome chromosome in _population)
        {
            float normalized = (chromosome._weight - minWeight) / (maxWeight - minWeight);

            float fitness = (1 - normalized) * 100;

            chromosome._fitness = fitness;

            if (!(fitness < _fittestChromosomeFitness)) continue;
            
            _fittestChromosome = chromosome;
            _fittestChromosomeFitness = chromosome._weight;
        }
    }

    /*
     * Processes chromosome to add tasks and weights accordingly.
     *
     * This is done after initial population is created, AND after doing natural selection
     *
     * Chromosome already has the order at which tasks are completed. We just need to
     * add wait tasks and adjust initial weights accordingly.
     */
    private void ProcessChromosome(Chromosome chromosome)
    {
        /*
         * Approach 1: Loop through all DroneCollections and tasks.
         * If a lower level task is found, add the upper level to the 'available' list.
         * If an upper level is found and is in the 'available' list, add the waiting time before it.
         * Keep a count of available lists.
         */

        List<Task> placedTasks = new List<Task>();
        
        List<Task> availableTasks = new List<Task>();
        List<float> availableTaskWeights = new List<float>();

        /**
         *
         * Problem: Not all tasks are placed in 'availableTasks'
         *
         * Something wrong with _task._nextTask?
         */

        while (placedTasks.Count < _firstLevelContainers.Count + _secondLevelContainers.Count)
        {
            foreach (DroneCollection droneCollection in chromosome._droneCollection)
            {
                for (int index = 0; index < droneCollection._tasks.Count; index++)
                {
                    Task task = droneCollection._tasks[index];
                    
                    if (placedTasks.Contains(task)) continue;
                    
                    if (task.isUpperLevel())
                    {
                        bool cont = false;
                        foreach (Task availableTask in availableTasks)
                        {
                            if (availableTask._startLocation == task._startLocation && availableTask._endLocation == task._endLocation)
                            {
                                cont = true;
                            }
                        }
                        if (!cont) continue;
                        
                        int taskIndex = availableTasks.FindIndex(availableTask => availableTask._startLocation == task._startLocation && availableTask._endLocation == task._endLocation);
                        
                        float timeToWait = availableTaskWeights[taskIndex] - droneCollection._currentWeight;

                        if (timeToWait > 0)
                        {
                            Task waitingTask = new Task(timeToWait);
                            placedTasks.Add(waitingTask);
                            droneCollection._tasks.Insert(index, waitingTask);
                            droneCollection._currentWeight += timeToWait;
                        }
                    }
                    else
                    {
                        availableTasks.Add(task._nextTask);
                        availableTaskWeights.Add(task._initialWeight);
                    }

                    SetDroneToTask(droneCollection, task);
                    placedTasks.Add(task);

                    chromosome._weight = Math.Max(chromosome._weight, droneCollection._currentWeight);
                }
            }
        }
    }

    private void SetDroneToTask(DroneCollection droneCollection, Task task)
    {
        float weight = droneCollection._currentWeight;

        // Drone distance to task
        weight += Vector3.Distance(
            droneCollection._currentLocation, task._startLocation);

        // Add weight to task for future reference.
        task._initialWeight = weight;

        // Add task distance to drone
        weight += task._weight;

        droneCollection._currentLocation = task._endLocation;

        droneCollection._currentWeight = weight;
    }

    private Chromosome CreateEmptyChromosome()
    {
        Chromosome chromosome = new Chromosome();
        
        // Make this a collection instead.
        for (int i = 0; i < _droneGameObjects.Count; i += 4)
        {
            // Get location from first drone.
            chromosome._droneCollection.Add(new DroneCollection(
                _droneGameObjects[i].transform.position,
                _droneGameObjects.GetRange(i, Math.Min(4, _droneGameObjects.Count - i))
            ));
        }
        return chromosome;
    }

    private Chromosome GenerateRandomChromosome()
    {
        Random random = new Random();
        
        Chromosome chromosome = CreateEmptyChromosome();
        Tasker currentTasker = GetTasker();

        for (int j = 0; j < _firstLevelContainers.Count + _secondLevelContainers.Count; j++)
        {
            int droneIntToPick = random.Next(0, chromosome._droneCollection.Count);
            DroneCollection droneCollection = chromosome._droneCollection[droneIntToPick];

            int taskIntToPick = random.Next(0, currentTasker._tasks.Count);
            Task currentTask = currentTasker._tasks[taskIntToPick];

            droneCollection._tasks.Add(currentTask);
            droneCollection._currentLocation = currentTask._endLocation;

            // Add next task i.e. 2nd layer of containers.
            if (currentTask._nextTask != null)
            {
                Task nextTask = currentTask._nextTask;
                currentTasker._tasks.Add(nextTask);
            }
            currentTasker._tasks.RemoveAt(taskIntToPick);
        }
        return chromosome;
    }

    private Task CreateTaskAtIndex(int index)
    {
        GameObject container = _firstLevelContainers[index];
        ContainerController containerController = container.GetComponent<ContainerController>();

        GameObject nextContainer = _secondLevelContainers[index];
        ContainerController nextContainerController = nextContainer.GetComponent<ContainerController>();
        
        taskIndex += 1;
        
        Task nextTask = new Task(
            nextContainer,
            nextContainer.transform.position,
            nextContainerController.LandingContainerController._bottomLeftTarget.transform.position,
            Vector3.Distance(
                nextContainer.transform.position,
                nextContainerController.LandingContainerController.transform.position
            ),
            taskIndex);

        taskIndex += 1;
        
        return new Task(
            container,
            container.transform.position,
            containerController.LandingContainerController._bottomLeftTarget.transform.position,
            Vector3.Distance(
                container.transform.position,
                containerController.LandingContainerController.transform.position
            ),
            taskIndex,
            nextTask);
    }

    private Tasker GetTasker()
    {
        Tasker tasker = new Tasker();
        taskIndex = 0;

        for (int i = 0; i < _firstLevelContainers.Count; i++)
        {
            tasker.addTask(CreateTaskAtIndex(i));
        }
        return tasker;
    }

    private Chromosome GetBestChromosome()
    {
        return _fittestChromosome;
    }

    private void Mutation()
    {
    }

    private void NaturalSelection()
    {
        List<Chromosome> newPopulation = new List<Chromosome>();
        Random random = new Random();

        foreach (Chromosome unused in _population)
        {
            // Select two random chromosome from mating pool
            Chromosome parentA = _matingPool[random.Next(0, _matingPool.Count - 1)];
            Chromosome parentB = _matingPool[random.Next(0, _matingPool.Count - 1)];

            // Add their crossover to the new population
            newPopulation.Add(CrossOver(parentA, parentB));
        }

        _population.Clear();
        _population = newPopulation;
    }

    private Chromosome CrossOver(Chromosome chromosomeA, Chromosome chromosomeB)
    {
        List<Task> lineupA = new List<Task>();
        List<Task> lineupB = new List<Task>();

        foreach (DroneCollection droneCollection in chromosomeA._droneCollection)
        {
            List<Task> tasks = droneCollection._tasks.Where(task => !task.isWaitingTask()).ToList();
            lineupA.AddRange(tasks);
        }

        foreach (DroneCollection droneCollection in chromosomeB._droneCollection)
        {
            List<Task> tasks = droneCollection._tasks.Where(task => !task.isWaitingTask()).ToList();
            lineupB.AddRange(tasks);
        }

        Random random = new Random();

        // CROSSOVER BABY

        int sectionStart = random.Next(0, lineupA.Count);
        int sectionEnd = random.Next(sectionStart, lineupA.Count);

        List<Task> section = lineupA.GetRange(sectionStart, sectionEnd - sectionStart);

        List<Task> newLineUp = new List<Task>();
        int currentInt = 0;


        // XO crossover algorithm.
        for (int i = 0; i < lineupA.Count; i++)
        {
            if (i >= sectionStart && i < sectionEnd)
            {
                newLineUp.Add(lineupA[i]);
            }
            else
            {
                while (true)
                {
                    // TODO Refactor the following..
                    bool found = false;
                    foreach (var item in section)
                    {
                        if (item._index == lineupB[currentInt]._index)
                        {
                            found = true;
                        }
                    }
                    
                    if (!found)
                    {
                        newLineUp.Add(lineupB[currentInt]);
                        currentInt++;
                        break;
                    }
                    currentInt++;
                }
            }
        }

        // Map new lineup to drones.
        int tasksPerDroneCollection = newLineUp.Count / (_droneGameObjects.Count / 4);
        int runningCount = 0;

        foreach (var droneCollection in chromosomeA._droneCollection)
        {
            droneCollection._tasks.Clear();
            droneCollection._tasks = newLineUp.GetRange(runningCount, tasksPerDroneCollection);
            runningCount += tasksPerDroneCollection;
        }

        return chromosomeA;
    }

    private void CreateMatingPool()
    {
        _matingPool.Clear();

        List<float> fitnesses = new List<float>();
        // Add chromosomes to mating pool according to their fitness
        foreach (Chromosome chromosome in _population)
        {
            float fitness = chromosome._fitness;
            fitnesses.Add(fitness);

            for (int k = 0; k < fitness; k++)
            {
                _matingPool.Add(chromosome);
            }
        }

        Debug.Log(string.Format("Average fitness for population: {0}",
            (fitnesses.Sum() / fitnesses.Count).ToString()));
    }

    public void AddFirstLevelContainers(List<GameObject> containers)
    {
        _firstLevelContainers.AddRange(containers);
    }

    public void AddSecondLevelContainers(List<GameObject> containers)
    {
        _secondLevelContainers.AddRange(containers);
    }
}