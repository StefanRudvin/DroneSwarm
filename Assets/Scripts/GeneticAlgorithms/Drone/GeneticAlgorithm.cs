using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;
using UnityEngineInternal;
using Random = System.Random;

/*
 *	This class serves as the backbone for the genetic algorithm.
 * 	
 */

namespace GeneticAlgorithms.Drone
{
    public class DroneGeneticAlgorithm
    {
        // List of current chromosomes
        private List<Chromosome> _population;
        private List<Chromosome> _matingPool;

        private const int PopulationSize = 100;
        private const int GenerationCount = 100;

        private float _fittestChromosomeWeight = 1000000000;
        private Chromosome _fittestChromosome;

        public List<GameObject> _containers;

        private List<DroneCollection> _droneCollections;

        private int taskIndex = 0;

        private string filePath;

        private Dictionary<int, float> _resultsDictionary = new Dictionary<int, float>();

        public DroneGeneticAlgorithm()
        {
            _matingPool = new List<Chromosome>();
            _population = new List<Chromosome>();

            _containers = new List<GameObject>();
        }

        public void SetDroneCollections(List<DroneCollection> droneCollections)
        {
            _droneCollections = droneCollections;
        }

        private void Reset()
        {
            _matingPool.Clear();
            _population.Clear();
            _fittestChromosome = null;
            _fittestChromosomeWeight = 1000000000;
            _resultsDictionary.Clear();

            foreach (var droneCollection in _droneCollections)
            {
                droneCollection._tasks.Clear();
            }
        }

        private void createResultsFile()
        {
//            string path = "Assets/Results/";
//            DateTime dateTime = DateTime.Now;
//            filePath = string.Format("{0}-{1}-{2}-{3}-{4}-{5}.txt",
//                path, dateTime.Day.ToString(),
//                dateTime.Hour.ToString(),
//                dateTime.Minute.ToString(),
//                dateTime.Second.ToString(),
//                dateTime.Millisecond.ToString()
//            );
//            File.Create(filePath);
            string path = "Assets/Results/results.txt";
            filePath = path;
        }

        public Chromosome Run()
        {
            Reset();
            createResultsFile();
            RemoveCompletedContainers();

            if (_containers.Count == 0)
            {
                Debug.Log("done!");
                return null;
            }

            CreateInitialPopulation();

            ProcessPopulation(0);
            //EvolvePopulation();
            WriteResultsToFile();
            return GetBestChromosome();
        }

        private void EvolvePopulation()
        {
            for (int i = 1; i < GenerationCount; i++)
            {
                if (_containers.Count == 0)
                {
                    Debug.Log("done!");
                    break;
                }

                CreateMatingPool();

                NaturalSelection();

                Mutation();

                if (ProcessPopulation(i)) break;
            }
        }

        private void RemoveCompletedContainers()
        {
            List<GameObject> newContainers = new List<GameObject>();

            foreach (var container in _containers)
            {
                ContainerController containerController = container.GetComponent<ContainerController>();

                if (!containerController._isCompleted &&
                    !containerController._isTargeted &&
                    !containerController._isMovingToLandingBlock && container.activeInHierarchy)
                {
                    newContainers.Add(container);
                }
            }

            _containers = newContainers;
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

        private Chromosome GenerateRandomChromosome()
        {
            Random random = new Random();
            Chromosome chromosome = CreateEmptyChromosome();
            Tasker currentTasker = GetTasker();
            int count = currentTasker._tasks.Count;

            if (count != _containers.Count)
            {
                Debug.Log("problem....");
            }

            for (int j = 0; j < _containers.Count; j++)
            {
                int droneIntToPick = random.Next(0, chromosome._droneCollection.Count);
                DroneCollection droneCollection = chromosome._droneCollection[droneIntToPick];

                int taskIntToPick = random.Next(0, currentTasker._tasks.Count);
                Task currentTask = currentTasker._tasks[taskIntToPick];
                droneCollection._tasks.Add(currentTask);
                currentTasker._tasks.RemoveAt(taskIntToPick);
            }

            return chromosome;
        }

        private Task CreateTaskFromContainer(GameObject container)
        {
            ContainerController containerController = container.GetComponent<ContainerController>();

            Task nextTask = null;
            taskIndex += 1;

            if (containerController._nextContainer != null)
            {
                GameObject nextContainer = containerController._nextContainer;
                ContainerController nextContainerController = nextContainer.GetComponent<ContainerController>();
                nextTask = new Task(
                    nextContainer,
                    nextContainer.transform.position,
                    nextContainerController.LandingContainerController._bottomLeftTarget.transform.position,
                    Vector3.Distance(
                        nextContainer.transform.position,
                        nextContainerController.LandingContainerController.transform.position
                    ),
                    taskIndex,
                    containerController._shipPriority);
            }

            return new Task(
                container,
                container.transform.position,
                containerController.LandingContainerController._bottomLeftTarget.transform.position,
                Vector3.Distance(
                    container.transform.position,
                    containerController.LandingContainerController.transform.position
                ),
                taskIndex,
                containerController._shipPriority,
                nextTask);
        }

        private Tasker GetTasker()
        {
            Tasker tasker = new Tasker();
            taskIndex = 0;

            foreach (var container in _containers)
            {
                tasker.addTask(CreateTaskFromContainer(container));
            }

            return tasker;
        }

        private Chromosome GetBestChromosome()
        {
            return new Chromosome(_fittestChromosome._droneCollection);
        }

        private void Mutation()
        {
        }

        private void NaturalSelection()
        {
            List<Chromosome> newPopulation = new List<Chromosome>();
            Random random = new Random();
            foreach (Chromosome chromosome in _population)
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

            Chromosome newChromosome = CreateEmptyChromosome();

            foreach (DroneCollection droneCollection in chromosomeA._droneCollection)
            {
                lineupA.AddRange(droneCollection._tasks);
            }

            foreach (DroneCollection droneCollection in chromosomeB._droneCollection)
            {
                lineupB.AddRange(droneCollection._tasks);
            }

            if (lineupA.Count != lineupB.Count || lineupA.Count == 0)
            {
                Debug.Log("Chromosome counts don't match up!");
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
            int tasksPerDroneCollection = newLineUp.Count / newChromosome._droneCollection.Count;
            int runningCount = 0;

            if (tasksPerDroneCollection < 1)
            {
                foreach (var droneCollection in newChromosome._droneCollection)
                {
                    droneCollection._tasks.Clear();
                }

                foreach (var droneCollection in newChromosome._droneCollection)
                {
                    if (runningCount == newLineUp.Count) break;
                    droneCollection._tasks = newLineUp.GetRange(runningCount, 1);
                    runningCount += 1;
                }
            }
            else
            {
                foreach (var droneCollection in newChromosome._droneCollection)
                {
                    droneCollection._tasks.Clear();
                    droneCollection._tasks = newLineUp.GetRange(runningCount, tasksPerDroneCollection);
                    runningCount += tasksPerDroneCollection;
                }
            }

            return newChromosome;
        }

        private void CreateMatingPool()
        {
            _matingPool.Clear();
            foreach (Chromosome chromosome in _population)
            {
                for (int k = 0; k < chromosome._fitness; k++)
                {
                    _matingPool.Add(chromosome);
                }
            }
        }

        public void AddContainers(List<GameObject> containers)
        {
            _containers.AddRange(containers);
        }

        public void SetContainers(List<GameObject> containers)
        {
            _containers = containers;
        }

        private bool ProcessPopulation(int generationCount)
        {
            float minWeight = 9999f;
            float maxWeight = 0;

            foreach (Chromosome chromosome in _population)
            {
                ProcessChromosome(chromosome);
                minWeight = Math.Min(minWeight, chromosome._weight);
                maxWeight = Math.Max(maxWeight, chromosome._weight);
            }

            List<float> fitnesses = new List<float>();

            foreach (Chromosome chromosome in _population)
            {
                /*
                 * Weight should be as low as possible.
                 *
                 * 1 - normalized * 100 should be creeping up to 100.
                 */
                float fitness;

                if (minWeight == maxWeight && chromosome._weight == minWeight)
                {
                    fitness = 100;
                }
                else
                {
                    float normalized = (chromosome._weight - minWeight) / (maxWeight - minWeight);

                    fitness = (1 - normalized) * 100;
                }

                chromosome._fitness = fitness;
                fitnesses.Add(fitness);

                if (chromosome._weight > _fittestChromosomeWeight) continue;

                // We have a new fittest chromosome.
                _fittestChromosome = chromosome;
                _fittestChromosomeWeight = chromosome._weight;
            }

            float averageFitness = fitnesses.Sum() / fitnesses.Count;

            AddFitnessToResultsDictionary(generationCount, _fittestChromosome._weight);

            return averageFitness == 100f;
        }

        private void AddFitnessToResultsDictionary(int generationCount, float fitness)
        {
            _resultsDictionary.Add(generationCount, fitness);
        }


        private void WriteResultsToFile()
        {
            StreamWriter writer = new StreamWriter(filePath, true);

            foreach (var entry in _resultsDictionary)
            {
                writer.WriteLine("{0}", entry.Value.ToString());
            }

            writer.Close();
        }

        /*
        * Processes chromosome to add tasks and weights accordingly.
        *
        * This is done after initial population is created, AND after doing natural selection
        *
        * Chromosome already has the order at which tasks are completed. We just need to
        * adjust initial weights accordingly.
        */
        private void ProcessChromosome(Chromosome chromosome)
        {
            /*
             *    Just go through all tasks of a drone and add the weights of each one.
             *    No need to do any waiting.
             */
            foreach (DroneCollection droneCollection in chromosome._droneCollection)
            {
                droneCollection.Reset();
                float shipPriorityPenalty = 0;

                // Set current task weight for Drone.
                if (droneCollection._currentTask != null)
                {
                    // This may need refactoring: best would be to see if drone is on the way
                    // to block or already carrying, then add weight accordingly.
                    float weight = droneCollection._currentWeight;
                    Task task = droneCollection._currentTask;

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

                for (int i = 0; i < droneCollection._tasks.Count; i++)
                {
                    Task task = droneCollection._tasks[i];

                    shipPriorityPenalty += (100 - task._shipPriority) * (i + 1);

                    SetDroneToTask(droneCollection, task);

                    float droneWeightWithShipPriority = droneCollection._currentWeight + shipPriorityPenalty;

                    chromosome._weight = Math.Max(chromosome._weight, droneWeightWithShipPriority);
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

            foreach (var droneCollection in _droneCollections)
            {
                chromosome._droneCollection.Add(new DroneCollection(droneCollection._startLocation,
                    droneCollection._drones,
                    droneCollection._currentTask));
            }

            return chromosome;
        }
    }
}