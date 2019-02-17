using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class GameController : MonoBehaviour
{
    public List<GameObject> _availableDrones = new List<GameObject>();

    public List<GameObject> _ships = new List<GameObject>();

    public GameObject startPad;
    public GameObject sampleDrone;

    public GameObject sampleContainer;
    public GameObject containerLocationStart;

    public DroneGeneticAlgorithm _droneGeneticAlgorithm;

    private void Start()
    {
        _droneGeneticAlgorithm = new DroneGeneticAlgorithm();
        createContainersAndMapToShip();
        instantiateDrones();
        _droneGeneticAlgorithm.AddDrones(_availableDrones);
        
        // TODO Map drones with tasks from Genetic Algorithm.
        RunGeneticAlgorithm();
    }

    /*
     * Per ship, create 20 boxes
     * Set landingBlockController for each box.
     */
    private void createContainersAndMapToShip()
    {
        foreach (var ship in _ships)
        {
            var containerModels = createRandomizedContainerModels();

            ContainerGeneticAlgorithm containerGeneticAlgorithm = new ContainerGeneticAlgorithm(containerModels, 2, 1, 10);

            ShipController shipController = ship.GetComponent<ShipController>();
            
            // List of containers in the right order.
            ContainerPlan containerPlan = containerGeneticAlgorithm.CreateOptimalContainerPlan();

            Transform starting_transform = containerLocationStart.transform;
            
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
                
                firstLevelContainerController.LandingContainerController = shipController.firstLevelLandingContainers[i]
                    .GetComponent<LandingContainerController>();
                firstLevelContainerController._ShipController = shipController;
                firstLevelContainerController._containerModel = containerPlan.firstRow[i];
                
//                GameObject  ChildGameObject1 = firstLevelContainer.transform.GetChild (0).gameObject;
//                ChildGameObject1.GetComponent<Renderer>().material.color = Utility.createColorFromWeight(firstLevelContainerController._containerModel._weight);
                
                temp.x += 20;
                
                var secondLevelContainer = Instantiate(sampleContainer, temp, starting_transform.rotation);
                var secondLevelContainerController = secondLevelContainer.GetComponent<ContainerController>();

                secondLevelContainerController.LandingContainerController = shipController.secondLevelLandingContainers[i]
                    .GetComponent<LandingContainerController>();
                secondLevelContainerController._ShipController = shipController;
                secondLevelContainerController._containerModel = containerPlan.secondRow[i];
                
//                GameObject  ChildGameObject2 = secondLevelContainer.transform.GetChild (0).gameObject;
//                ChildGameObject2.GetComponent<Renderer>().material.color = Utility.createColorFromWeight(secondLevelContainerController._containerModel._weight);
//                
                shipController.firstLevelBuildingContainers.Add(firstLevelContainer);
                shipController.secondLevelBuildingContainers.Add(secondLevelContainer);
                shipController.containers.Add(firstLevelContainer);
                shipController.containers.Add(secondLevelContainer);
            }
            
            _droneGeneticAlgorithm.AddFirstLevelContainers(shipController.firstLevelBuildingContainers);
            _droneGeneticAlgorithm.AddSecondLevelContainers(shipController.secondLevelBuildingContainers);

        }
    }

    static List<ContainerModel> createRandomizedContainerModels()
    {
        var containerModels = new List<ContainerModel>();
        var random = new Random();

        for (var i = 0; i < 20; i++)
        {
            var containerModel = new ContainerModel(random.Next(100, 1000), random.Next(1, 10));
            containerModels.Add(containerModel);
        }
        return containerModels;
    }

    void instantiateDrones()
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
    }

    private void Update()
    {
        assignDronesToBuildingBlocks();
    }

    private void RunGeneticAlgorithm()
    {
        _droneGeneticAlgorithm.Run();
    }

    /*
     * Find free building blocks and assign free drones to them.
     */
    void assignDronesToBuildingBlocks()
    {
        //RunGeneticAlgorithm();
        foreach (var ship in _ships)
        {
            ShipController shipController = ship.GetComponent<ShipController>();

            foreach (var buildingBlock in shipController.getOpenLandingContainers())
            {
                var containerController = buildingBlock.GetComponent<ContainerController>();

                if (containerController._isTargeted || _availableDrones.Count < containerController._requiredDrones) continue;

                Debug.Log("found a building block, assigning drones to it...");

                List<GameObject> extractedDrones = getNAvailableDrones(containerController);
                containerController.assignDronesToContainer(extractedDrones);
            }
        }
    }

    private List<GameObject> getNAvailableDrones(ContainerController containerController)
    {
        List<GameObject> extractedDrones = new List<GameObject>();

        for (var i = 0; i < containerController._requiredDrones; i++)
        {
            GameObject drone = _availableDrones[0];
            extractedDrones.Add(drone);
            _availableDrones.Remove(drone);
        }

        return extractedDrones;
    }

    public void receiveFreeDrones(List<GameObject> drones)
    {
        foreach (var drone in new List<GameObject>(drones)) _availableDrones.Add(drone);
    }
}