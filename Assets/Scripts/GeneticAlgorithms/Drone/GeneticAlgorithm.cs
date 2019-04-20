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

        public List<float> lastGenFitnesses = new List<float>();

        private int taskIndex = 0;

        private string filePath;

        private bool DebugAlgo = false;

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
            string path = "Assets/Results/droneResults.csv";
            filePath = path;
        }

        public Chromosome Run()
        {
            Reset();
            RemoveCompletedContainers();

            if (_containers.Count == 0)
            {
                Debug.Log("done!");
                return null;
            }

            CreateInitialPopulation();
            ProcessPopulation(0);

            if (DebugAlgo)
            {
                createResultsFile();

                DateTime dateTime = DateTime.Now;

                EvolvePopulation();

                TimeSpan permutationTime = DateTime.Now.Subtract(dateTime);

                WriteResultsToFile();
            }
            else
            {
                EvolvePopulation();
            }

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

                ProcessPopulation(i);
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

        private List<Task> MergeTasks(Chromosome chromosomeA, Chromosome chromosomeB)
        {
            List<Task> tasksA = new List<Task>();
            List<Task> tasksB = new List<Task>();

            foreach (DroneCollection droneCollection in chromosomeA._droneCollection)
            {
                tasksA.AddRange(droneCollection._tasks);
            }

            foreach (DroneCollection droneCollection in chromosomeB._droneCollection)
            {
                tasksB.AddRange(droneCollection._tasks);
            }

            if (tasksA.Count != tasksB.Count || tasksA.Count == 0)
            {
                Debug.Log("Chromosome counts don't match up!");
            }

            Random random = new Random();

            // Start crossover
            int sectionStart = random.Next(0, tasksA.Count);
            int sectionEnd = random.Next(sectionStart, tasksA.Count);
            List<Task> section = tasksA.GetRange(sectionStart, sectionEnd - sectionStart);
            List<Task> mergedTasks = new List<Task>();
            int currentInt = 0;

            // XO crossover algorithm.
            for (int i = 0; i < tasksA.Count; i++)
            {
                if (i >= sectionStart && i < sectionEnd)
                {
                    mergedTasks.Add(tasksA[i]);
                }
                else
                {
                    while (true)
                    {
                        // TODO Refactor the following..
                        bool found = false;

                        foreach (var item in section)
                        {
                            if (item._index == tasksB[currentInt]._index)
                            {
                                found = true;
                            }
                        }

                        if (!found)
                        {
                            mergedTasks.Add(tasksB[currentInt]);
                            currentInt++;
                            break;
                        }

                        currentInt++;
                    }
                }
            }

            return mergedTasks;
        }

        private Chromosome CrossOver(Chromosome chromosomeA, Chromosome chromosomeB)
        {
            Chromosome newChromosome = CreateEmptyChromosome();
            
            List<Task> mutatedLineUp = MergeTasks(chromosomeA, chromosomeB);
//            List<Task> mutatedLineUp = Mutation(newLineUp);

            // Map task list to drones.
            int tasksPerDroneCollection = mutatedLineUp.Count / newChromosome._droneCollection.Count;
            int runningCount = 0;

            // Done in case there's fewer tasks than drones
            if (tasksPerDroneCollection < 1)
            {
                foreach (var droneCollection in newChromosome._droneCollection)
                {
                    droneCollection._tasks.Clear();
                }

                foreach (var droneCollection in newChromosome._droneCollection)
                {
                    if (runningCount == mutatedLineUp.Count) break;
                    droneCollection._tasks = mutatedLineUp.GetRange(runningCount, 1);
                    runningCount += 1;
                }
            }
            else
            {
                foreach (var droneCollection in newChromosome._droneCollection)
                {
                    droneCollection._tasks.Clear();
                    droneCollection._tasks = mutatedLineUp.GetRange(runningCount, tasksPerDroneCollection);
                    runningCount += tasksPerDroneCollection;
                }
            }
            return newChromosome;
        }

        /*
         * 10% chance to swap 2 random indexes of list. 
         */
        private List<Task> Mutation(List<Task> lineUp)
        {
            Random random = new Random();

            int mutationIndex = random.Next(0, 10);

            if (mutationIndex != 0) return lineUp;

            List<Task> mutatedList = new List<Task>(lineUp);

            int index1 = random.Next(0, mutatedList.Count / 2);
            int index2 = random.Next(mutatedList.Count / 2, mutatedList.Count);

            Task ok = mutatedList[index1];
            Task temp = new Task(ok._containerObject, ok._startLocation, ok._endLocation, ok._weight, ok._index,
                ok._shipPriority, ok._nextTask);

            mutatedList[index1] = mutatedList[index2];
            mutatedList[index2] = temp;
            return mutatedList;
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

        private void ProcessPopulation(int generationCount)
        {
            float minWeight = 9999f;
            float maxWeight = 0;

            foreach (Chromosome chromosome in _population)
            {
                ProcessChromosome(chromosome);
                minWeight = Math.Min(minWeight, chromosome._weight);
                maxWeight = Math.Max(maxWeight, chromosome._weight);
            }
            
            SetFitnesses(minWeight, maxWeight);
            if (DebugAlgo)
            {
                AddFitnessToResultsDictionary(generationCount, _fittestChromosome._weight);
            }
        }

        private void SetFitnesses(float minWeight, float maxWeight)
        {
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
        }

        private void AddFitnessToResultsDictionary(int generationCount, float fitness)
        {
            _resultsDictionary.Add(generationCount, fitness);
        }

        private void WriteResultsToFile()
        {
            lastGenFitnesses.Add(_resultsDictionary[99]);

            string[] arrLine = File.ReadAllLines(filePath);

            int counter = 0;

            foreach (var entry in _resultsDictionary)
            {
                arrLine[counter] = String.Format("{0}, {1}", arrLine[counter].ToString(), entry.Value.ToString());

                counter++;
            }

            File.WriteAllLines(filePath, arrLine);
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