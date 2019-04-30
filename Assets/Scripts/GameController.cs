using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeneticAlgorithms.Container;
using GeneticAlgorithms.Drone;
using UnityEngine;
using Chromosome = GeneticAlgorithms.Drone.Chromosome;
using Random = System.Random;

public class GameController : MonoBehaviour
{
    public List<GameObject> _availableDrones = new List<GameObject>();

    public List<GameObject> _ships = new List<GameObject>();

    public GameObject startPad;
    public GameObject sampleDrone;

    bool DebugDroneAlgo = false;
    bool DebugContainerAlgo = false;

    public GameObject sampleContainer;

    public DroneGeneticAlgorithm _droneGeneticAlgorithm;

    private Chromosome _dronePlan;

    private const int MaxPriority = 100;

    public List<DroneCollection> _droneCollections = new List<DroneCollection>();

    public TaskManager _taskManager;

    private void Start()
    {
        try
        {
            _droneGeneticAlgorithm = new DroneGeneticAlgorithm();
            createContainersAndMapToShip();
            instantiateDroneCollections();

            _droneGeneticAlgorithm.SetDroneCollections(_droneCollections);

            // Set initial drone run before update()
            RunDroneGeneticAlgorithm();

            if (DebugContainerAlgo || DebugDroneAlgo) ResetResults();

            if (DebugDroneAlgo)
            {
                for (int i = 0; i < 101; i++)
                {
                    RunDroneGeneticAlgorithm();
                }
                // Used for statistics, don't remove!
                float averageFitness = _droneGeneticAlgorithm.lastGenFitnesses.Average();
            }

            // Set new task manager with the drone plan from the GA. 
            _taskManager = new TaskManager(_dronePlan)
            {
                _containers = _droneGeneticAlgorithm._containers
            };

            // Make sure above task manager works
            if (_taskManager == null)
            {
                Debug.Log("TaskManager not loaded due to a previous error. Exiting.");
                
                // These do not work in Mac OSX
                //UnityEditor.EditorApplication.isPlaying = false;
                Application.Quit();
            }
        }
        catch (Exception e)
        {
            Debug.Log(string.Format("Game Controller Startup failed. Error: {0}", e));
            
            // These do not work in Mac OSX
            //UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
    }

    /*
     * Reset results files used for evaluation.
     */
    private void ResetResults()
    {
        string path = "Assets/Results/droneResults.csv";
        string[] arrLine = File.ReadAllLines(path);

        for (int i = 0; i < 100; i++)
        {
            arrLine[i] = "100";
        }

        File.WriteAllLines(path, arrLine);

        string containerPath = "Assets/Results/containerResults.csv";
        string[] containerContents = File.ReadAllLines(containerPath);

        for (int i = 0; i < 100; i++)
        {
            containerContents[i] = "100";
        }

        File.WriteAllLines(containerPath, containerContents);
    }

    /*
     * Per ship, create 20 boxes
     * Set landingBlockController for each box.
     */
    private void createContainersAndMapToShip()
    {
        foreach (var ship in _ships)
        {
            ShipController shipController = ship.GetComponent<ShipController>();

            if (DebugContainerAlgo)
            {
                for (int i = 0; i < 101; i++)
                {
                    var testContainerModels = createRandomizedContainerModels(shipController._priority);

                    ContainerGeneticAlgorithm testContainerGeneticAlgorithm =
                        new ContainerGeneticAlgorithm(testContainerModels, 2, 1, 10);

                    testContainerGeneticAlgorithm.CreateOptimalContainerPlan();
                }
            }

            var containerModels = createRandomizedContainerModels(shipController._priority);

            ContainerGeneticAlgorithm containerGeneticAlgorithm =
                new ContainerGeneticAlgorithm(containerModels, 2, 1, 10);

            // List of containers in the right order.
            ContainerPlan containerPlan = containerGeneticAlgorithm.CreateOptimalContainerPlan();

            Transform starting_transform = shipController.containerLocationStart.transform;

            // Before this, add containers from the ship to move to another ship.
            // Perhaps a container with another container built in!
            // When container is placed at a target, instead of destroying,
            // create a new container with a new target.
            // How would this work with the task system?
            // Create a task for one, then task for 2.

            // Loop through every container, place them in the correct ship, ship controller and level accordingly.
            for (int i = 0; i < 10; i++)
            {
                // Create both levels of container.
                Vector3 temp = starting_transform.position;
                temp.z += i * 20;

                GameObject firstLevelContainer = Instantiate(sampleContainer, temp, starting_transform.rotation);
                ContainerController firstLevelContainerController =
                    firstLevelContainer.GetComponent<ContainerController>();

                firstLevelContainerController.LandingContainerController = shipController
                    ._firstLevelLandingContainers[i]
                    .GetComponent<LandingContainerController>();
                firstLevelContainerController._ShipController = shipController;
                firstLevelContainerController._shipPriority = shipController._priority;
                firstLevelContainerController._containerModel = containerPlan.firstRow[i];
                
                // Move 2nd layer container above first one.
                temp.x += 20;

                var secondLevelContainer = Instantiate(sampleContainer, temp, starting_transform.rotation);
                var secondLevelContainerController = secondLevelContainer.GetComponent<ContainerController>();

                secondLevelContainerController.LandingContainerController = shipController
                    ._secondLevelLandingContainers[i]
                    .GetComponent<LandingContainerController>();
                secondLevelContainerController._ShipController = shipController;
                secondLevelContainerController._shipPriority = shipController._priority;
                secondLevelContainerController._containerModel = containerPlan.secondRow[i];

                firstLevelContainerController._nextContainer = secondLevelContainer;

                shipController._firstLevelBuildingContainers.Add(firstLevelContainer);
                shipController._secondLevelBuildingContainers.Add(secondLevelContainer);
                shipController._containers.Add(firstLevelContainer);
                shipController._containers.Add(secondLevelContainer);
            }
            // Add the first level containers to the drone genetic algorithm.
            _droneGeneticAlgorithm.AddContainers(shipController._firstLevelBuildingContainers);
        }
    }

    List<ContainerModel> createRandomizedContainerModels(int shipPriority)
    {
        var containerModels = new List<ContainerModel>();
        var random = new Random();

        for (var i = 0; i < 20; i++)
        {
            var containerModel = new ContainerModel(random.Next(100, 1000), random.Next(1, MaxPriority), shipPriority);
            containerModels.Add(containerModel);
        }

        return containerModels;
    }

    void instantiateDroneCollections()
    {
        var startPads = startPad.GetComponent<StartPadController>().startPads;

        foreach (var pad in startPads)
        {
            var drone = Instantiate(sampleDrone, pad.transform);
            drone.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            var droneController = drone.GetComponent<DroneController>();
            droneController.Target = pad;
            droneController.startPosition = pad;
            _availableDrones.Add(drone);
        }

        // Make this a collection instead.
        for (int i = 0; i < _availableDrones.Count; i += 4)
        {
            _droneCollections.Add(new DroneCollection(
                _availableDrones[i].transform.position,
                _availableDrones.GetRange(i, 4),
                null
            ));
        }
    }

    private void Update()
    {
        if (_taskManager == null)
        {
            Debug.Log("TaskManager not loaded due to a previous error. Pausing.");
            
            // These do not work in Mac OSX
            //UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
        
        // Return the list of new tasks from here.
        _taskManager._containers = _droneGeneticAlgorithm._containers;
        if (_taskManager.Run())
        {
            _droneGeneticAlgorithm.SetContainers(_taskManager._containers);
            _droneGeneticAlgorithm.SetDroneCollections(_taskManager._chromosome._droneCollection);
            RunDroneGeneticAlgorithm();
            _droneCollections = _dronePlan._droneCollection;
            _taskManager.setChromosome(_dronePlan);
        }
    }

    private void RunDroneGeneticAlgorithm()
    {
        try
        {
            _dronePlan = _droneGeneticAlgorithm.Run();
        }
        catch (Exception e)
        {
            Debug.Log(string.Format("Drone Genetic Algorithm experienced an error: {0}", e));
            
            // These do not work in Mac OSX
            //UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
    }
}