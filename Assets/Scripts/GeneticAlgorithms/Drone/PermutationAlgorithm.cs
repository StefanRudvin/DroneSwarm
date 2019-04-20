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
    public class PermutationAlgorithm
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

        private int _permutationCount = 0;

        private Dictionary<int, float> _resultsDictionary = new Dictionary<int, float>();

        public PermutationAlgorithm()
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
            _permutationCount = 0;

            foreach (var droneCollection in _droneCollections)
            {
                droneCollection._tasks.Clear();
            }
        }

        private void WriteResultsToFile(string thing)
        {
            StreamWriter writer = new StreamWriter(filePath, true);

            writer.WriteLine("{0}", thing);

            writer.Close();
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

            Tasker currentTasker = GetTasker();

            List<Task> tasks = currentTasker._tasks;
            
//            DateTime dateTime = DateTime.Now;
            
            Permute(tasks, 0, tasks.Count - 1);
            
//            TimeSpan permutationTime = DateTime.Now.Subtract(dateTime);            

//            WriteResultsToFile(GetBestChromosome()._weight.ToString());

            return GetBestChromosome();
        }

        private void setTasksToNewChromosome(List<Task> tasks)
        {
            Chromosome chromosome = CreateEmptyChromosome();

            int tasksPerDroneCollection = tasks.Count / chromosome._droneCollection.Count;
            int runningCount = 0;

            if (tasksPerDroneCollection < 1)
            {
                foreach (var droneCollection in chromosome._droneCollection)
                {
                    droneCollection._tasks.Clear();
                }

                foreach (var droneCollection in chromosome._droneCollection)
                {
                    if (runningCount == tasks.Count) break;
                    droneCollection._tasks = tasks.GetRange(runningCount, 1);
                    runningCount += 1;
                }
            }
            else
            {
                foreach (var droneCollection in chromosome._droneCollection)
                {
                    droneCollection._tasks.Clear();
                    droneCollection._tasks = tasks.GetRange(runningCount, tasksPerDroneCollection);
                    runningCount += tasksPerDroneCollection;
                }
            }

            ProcessChromosome(chromosome);
        }


        private void Permute(List<Task> tasks, int l, int r)
        {
            if (l == r)
            {
                _permutationCount++;
                setTasksToNewChromosome(tasks);
            }
            else
            {
                for (int i = l; i <= r; i++)
                {
                    tasks = swap(tasks, l, i);
                    Permute(tasks, l + 1, r);
                    tasks = swap(tasks, l, i);
                }
            }
        }

        private static List<Task> swap(List<Task> tasks, int i, int j)
        {
            Task temp;
            temp = tasks[i];
            tasks[i] = tasks[j];
            tasks[j] = temp;

            List<Task> newTasks = new List<Task>(tasks);
            return newTasks;
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

        public void AddContainers(List<GameObject> containers)
        {
            _containers.AddRange(containers);
        }

        public void SetContainers(List<GameObject> containers)
        {
            _containers = containers;
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

            if (chromosome._weight > _fittestChromosomeWeight) return;

            // We have a new fittest chromosome.
            _fittestChromosome = chromosome;
            _fittestChromosomeWeight = chromosome._weight;
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