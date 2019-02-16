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
    private int _generationCount = 10;
    
    private float _fittestChromosomeFitness = 1000000000;
    private List<DroneCollection> _fittestChromosome;
    
    private List<GameObject> _firstLevelContainers;
    private List<GameObject> _secondLevelContainers;

    private List<GameObject> _droneGameObjects;

    private List<DroneCollection> _droneCollections;

    public DroneGeneticAlgorithm()
    {
        _droneCollections = new List<DroneCollection>();
        _firstLevelContainers = new List<GameObject>();
        _secondLevelContainers = new List<GameObject>();
    }

    public void AddDrones(List<GameObject> drones)
    {
        _droneGameObjects = drones;
    }

    public List<DroneCollection> Run()
    {
        InitializeDrones();
        CreateInitialPopulation();
        SetWeightPerChromosome();
        EvolvePopulation();
        return GetBestChromosome();
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
            Debug.Log(String.Format("Generation: {0}", i));
            
            SetWeightPerChromosome();
            
            CreateMatingPool();
            
            Debug.Log(String.Format("Mating pool count: {0}", _matingPool.Count.ToString()));
            
            NaturalSelection();

            Mutation();
        }
    }

    private void SetWeightPerChromosome()
    {
        foreach (var chromosome in _population)
        {
            foreach (var droneCollection in chromosome)
            {
                float weight = 0;
                foreach (var task in droneCollection._tasks)
                {
                    // Drone distance to task
                    weight += Vector3.Distance(
                        droneCollection._currentLocation, task._startLocation);

                    // Add weight to task for future reference.
                    task._initialWeight = weight;

                    // Add task distance to drone
                    weight += task._weight;

                    droneCollection._currentLocation = task._endLocation;
                }
                droneCollection._currentWeight = weight;
            }
        }
    }
    
    private List<DroneCollection> GenerateRandomChromosome()
    {
        var random = new Random();

        // Create a random population.
        List<DroneCollection> chromosome = new List<DroneCollection>(_droneCollections);
        Tasker currentTasker = getTasker();
        
        for (int j = 0; j < _firstLevelContainers.Count + _secondLevelContainers.Count; j++)
        {
            int droneIntToPick = random.Next(0, chromosome.Count);
            var currentDrone = chromosome[droneIntToPick];

            int taskIntToPick = random.Next(0, currentTasker._tasks.Count);

            var currentTask = currentTasker._tasks[taskIntToPick];

            // Drone distance to task
            currentDrone._currentWeight += Vector3.Distance(
                currentDrone._currentLocation, currentTask._startLocation);

            // Add weight to task for future reference.
            currentTask._initialWeight = currentDrone._currentWeight;

            // Add task distance to drone
            currentDrone._currentWeight += currentTask._weight;

            currentDrone._tasks.Add(currentTask);

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

        return AddWaitTasksToChromosome(chromosome);
    }
    
    
    private List<DroneCollection> AddWaitTasksToChromosome(List<DroneCollection> chromosome)
    {
        foreach (var drone in chromosome)
        {
            foreach (var task in drone._tasks)
            {
                if (task.isFirstLevel()) continue;
                
                // Need to add a waiting task before this next task.
                foreach (var nextDrone in chromosome)
                {
                    for (int i = 0; i < nextDrone._tasks.Count; i++)
                    {
                        if (nextDrone._tasks[i] == task._nextTask)
                        {
                            nextDrone._tasks.Insert(i, new Task(task._initialWeight));
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
        var container = _firstLevelContainers[index];
        var containerController = container.GetComponent<ContainerController>();

        var nextContainer = _secondLevelContainers[index];
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

        for (int i = 0; i < _firstLevelContainers.Count; i++)
        {
            tasker.addTask(CreateTaskAtIndex(i));
        }

        return tasker;
    }

    private List<DroneCollection> GetBestChromosome()
    {
        return _fittestChromosome;
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
            var parentA = _matingPool[random.Next(0, _matingPool.Count - 1)];
            var parentB = _matingPool[random.Next(0, _matingPool.Count - 1)];

            // Add their crossover to the new population
            newPopulation.Add(crossOver(parentA, parentB));
        }
        _population.Clear();
        _population = newPopulation;
    }

    private List<DroneCollection> crossOver(List<DroneCollection> chromosomeA, List<DroneCollection> chromosomeB)
    {
        /*
         * Now, how do you mix them up?
         */
        
        List<Task> lineupA = new List<Task>();
        List<Task> lineupB = new List<Task>();

        foreach (var droneCollection in chromosomeA)
        {
            lineupA.AddRange(droneCollection._tasks);
        }
        
        foreach (var droneCollection in chromosomeB)
        {
            lineupB.AddRange(droneCollection._tasks);
        }
        
        var random = new Random();
        
        // CROSSOVER BABY
        
        int sectionStart = random.Next(0, lineupA.Count);
        int sectionEnd = random.Next(sectionStart, lineupA.Count);
        
        var section = lineupA.GetRange(sectionStart, sectionEnd - sectionStart);

        var newLineUp = new List<Task>();
        var currentInt = 0;
		
        for (int i = 0; i < lineupA.Count(); i ++) {
            if (i >= sectionStart && i < sectionEnd) {
                newLineUp.Add(lineupA[i]);
            } else {
                while (true) {
                    if (!section.Contains(lineupB[currentInt])) {				
                        newLineUp.Add(lineupB[currentInt]);
                        currentInt++;
                        break;
                    }
                    currentInt++;
                }
            }
        }
        // Map new lineup to drones.
        int tasksPerDroneCollection = newLineUp.Count / _droneCollections.Count;
        var runningCount = 0;

        foreach (var chromosome in chromosomeA)
        {
            chromosome._tasks = newLineUp.GetRange(runningCount, tasksPerDroneCollection);
            runningCount += tasksPerDroneCollection;
        }
        return AddWaitTasksToChromosome(chromosomeA);
    }

    private float GetFitness(List<DroneCollection> chromosome)
    {
        float maxWeight = 0;
        foreach (var droneCollection in chromosome)
        {
            if (droneCollection._currentWeight > maxWeight)
            {
                maxWeight = droneCollection._currentWeight;
            }
        }

        if (maxWeight < _fittestChromosomeFitness)
        {
            _fittestChromosome = chromosome;
            _fittestChromosomeFitness = maxWeight;
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
    
    public void AddFirstLevelContainers(List<GameObject> containers)
    {
        _firstLevelContainers.AddRange(containers);
    }
    
    public void AddSecondLevelContainers(List<GameObject> containers)
    {
        _secondLevelContainers.AddRange(containers);
    }
}