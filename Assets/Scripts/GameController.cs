﻿using System;
using System.Collections;
using System.Collections.Generic;
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

    public GameObject sampleContainer;

    public DroneGeneticAlgorithm _droneGeneticAlgorithm;

    private Chromosome _dronePlan;
    
    private int maxPriority = 100;

    public List<DroneCollection> _droneCollections = new List<DroneCollection>();

    public TaskManager _taskManager;

    private void Start()
    {
        _droneGeneticAlgorithm = new DroneGeneticAlgorithm();
        createContainersAndMapToShip();
        instantiateDroneCollections();
        
        _droneGeneticAlgorithm.SetDroneCollections(_droneCollections);
        
        RunGeneticAlgorithm();
        
        _taskManager = new TaskManager(_dronePlan);

        _taskManager._containers = _droneGeneticAlgorithm._containers;
        
        if (_taskManager == null)
        {
            Debug.Log("TaskManager not loaded.");
        }
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
            
            var containerModels = createRandomizedContainerModels(shipController._priority);

            ContainerGeneticAlgorithm containerGeneticAlgorithm = new ContainerGeneticAlgorithm(containerModels, 2, 1, 10);

            // List of containers in the right order.
            ContainerPlan containerPlan = containerGeneticAlgorithm.CreateOptimalContainerPlan();

            Transform starting_transform = shipController.containerLocationStart.transform;
            
            // Before this, add containers from the ship to move to another ship.
            // Perhaps a container with another container built in!
            // When container is placed at a target, instead of destroying,
            // create a new container with a new target.
            // How would this work with the task system?
            // Create a task for one, then task for 2.

            for (int i = 0; i < 10; i++)
            {
                // Create both levels of container.
                Vector3 temp = starting_transform.position;
    			temp.z += i * 20;
                
                GameObject firstLevelContainer = Instantiate(sampleContainer, temp, starting_transform.rotation);
                ContainerController firstLevelContainerController = firstLevelContainer.GetComponent<ContainerController>();
                
                firstLevelContainerController.LandingContainerController = shipController._firstLevelLandingContainers[i]
                    .GetComponent<LandingContainerController>();
                firstLevelContainerController._ShipController = shipController;
                firstLevelContainerController._shipPriority = shipController._priority;
                firstLevelContainerController._containerModel = containerPlan.firstRow[i];
                
//                GameObject  ChildGameObject1 = firstLevelContainer.transform.GetChild (0).gameObject;
//                ChildGameObject1.GetComponent<Renderer>().material.color = Utility.createColorFromWeight(firstLevelContainerController._containerModel._weight);
                
                temp.x += 20;
                
                var secondLevelContainer = Instantiate(sampleContainer, temp, starting_transform.rotation);
                var secondLevelContainerController = secondLevelContainer.GetComponent<ContainerController>();

                secondLevelContainerController.LandingContainerController = shipController._secondLevelLandingContainers[i]
                    .GetComponent<LandingContainerController>();
                secondLevelContainerController._ShipController = shipController;
                secondLevelContainerController._shipPriority = shipController._priority;
                secondLevelContainerController._containerModel = containerPlan.secondRow[i];
                
//                GameObject  ChildGameObject2 = secondLevelContainer.transform.GetChild (0).gameObject;
//                ChildGameObject2.GetComponent<Renderer>().material.color = Utility.createColorFromWeight(secondLevelContainerController._containerModel._weight);
//              
                firstLevelContainerController._nextContainer = secondLevelContainer;
                
                shipController._firstLevelBuildingContainers.Add(firstLevelContainer);
                shipController._secondLevelBuildingContainers.Add(secondLevelContainer);
                shipController._containers.Add(firstLevelContainer);
                shipController._containers.Add(secondLevelContainer);
            }
            _droneGeneticAlgorithm.AddContainers(shipController._firstLevelBuildingContainers);
        }
    }

    List<ContainerModel> createRandomizedContainerModels(int shipPriority)
    {
        var containerModels = new List<ContainerModel>();
        var random = new Random();

        for (var i = 0; i < 20; i++)
        {
            var containerModel = new ContainerModel(random.Next(100, 1000), random.Next(1, maxPriority), shipPriority);
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
            Debug.Log("ok then.");
        }
        // Return the list of new tasks from here.
        _taskManager._containers = _droneGeneticAlgorithm._containers;
        if (_taskManager.Run())
        {
            _droneGeneticAlgorithm.SetContainers(_taskManager._containers);
            _droneGeneticAlgorithm.SetDroneCollections(_taskManager._chromosome._droneCollection);
            RunGeneticAlgorithm();
            _droneCollections = _dronePlan._droneCollection;
            _taskManager.setChromosome(_dronePlan);
        }
    }

    private void RunGeneticAlgorithm()
    {
        var plan = _droneGeneticAlgorithm.Run();
        if (plan != null) _dronePlan = plan;
    }
}