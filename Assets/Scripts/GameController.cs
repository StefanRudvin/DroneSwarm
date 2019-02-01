using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class GameController : MonoBehaviour
{
    public List<GameObject> availableDrones = new List<GameObject>();

    public List<GameObject> ships = new List<GameObject>();

    public GameObject startPad;
    public GameObject sampleDrone;

    public GameObject sampleContainer;
    public GameObject containerLocationStart;

    private void Start()
    {
        createContainersAndMapToShip();
        instantiateDrones();
    }

    /*
     * Per ship, create 20 boxes
     * Set landingBlockController for each box.
     */
    private void createContainersAndMapToShip()
    {
        foreach (var ship in ships)
        {
            var containerModels = createRandomizedContainerModels();

            GeneticAlgorithm geneticAlgorithm = new GeneticAlgorithm(containerModels, 2, 1, 10);

            ShipController shipController = ship.GetComponent<ShipController>();

            // List of containers in the right order.
            ContainerPlan containerPlan = geneticAlgorithm.CreateOptimalContainerPlan();

            Transform starting_transform = containerLocationStart.transform;

            for (int i = 0; i < 10; i++)
            {
                // Create both levels of container.
                Vector3 temp = starting_transform.position;
    			temp.z += i * 20;
                
                var firstLevelContainer = Instantiate(sampleContainer, temp, starting_transform.rotation);
                var firstLevelContainerController = firstLevelContainer.GetComponent<ContainerController>();
                
                
                firstLevelContainerController.LandingContainerController = shipController.firstLevelLandingBlocks[i]
                    .GetComponent<LandingContainerController>();
                firstLevelContainerController._ShipController = shipController;
                firstLevelContainerController._containerModel = containerPlan.firstRow[i];
                
//                GameObject  ChildGameObject1 = firstLevelContainer.transform.GetChild (0).gameObject;
//                ChildGameObject1.GetComponent<Renderer>().material.color = Utility.createColorFromWeight(firstLevelContainerController._containerModel._weight);
//                
                temp.x += 20;
                
                var secondLevelContainer = Instantiate(sampleContainer, temp, starting_transform.rotation);
                var secondLevelContainerController = secondLevelContainer.GetComponent<ContainerController>();

                secondLevelContainerController.LandingContainerController = shipController.secondLevelLandingBlocks[i]
                    .GetComponent<LandingContainerController>();
                secondLevelContainerController._ShipController = shipController;
                secondLevelContainerController._containerModel = containerPlan.secondRow[i];
                
//                GameObject  ChildGameObject2 = secondLevelContainer.transform.GetChild (0).gameObject;
//                ChildGameObject2.GetComponent<Renderer>().material.color = Utility.createColorFromWeight(secondLevelContainerController._containerModel._weight);
//                
                shipController.containers.Add(firstLevelContainer);
                shipController.containers.Add(secondLevelContainer);
            }
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
            availableDrones.Add(drone);
        }
    }

    private void Update()
    {
        assignDronesToBuildingBlocks();
    }

    /*
     * Find free building blocks and assign free drones to them.
     */
    void assignDronesToBuildingBlocks()
    {
        foreach (var ship in ships)
        {
            ShipController shipController = ship.GetComponent<ShipController>();

            foreach (var buildingBlock in shipController.containers)
            {
                var containerController = buildingBlock.GetComponent<ContainerController>();

                if (containerController._isTargeted || availableDrones.Count < containerController._requiredDrones) continue;

                Debug.Log("found a building block, assigning drones to it...");

                List<GameObject> extractedDrones = getNAvailableDrones(containerController._requiredDrones);
                containerController.assignDronesToContainer(extractedDrones);
            }
        }
    }

    private List<GameObject> getNAvailableDrones(int requiredDrones)
    {
        List<GameObject> extractedDrones = new List<GameObject>();

        for (var i = 0; i < requiredDrones; i++)
        {
            GameObject drone = availableDrones[0];
            extractedDrones.Add(drone);
            availableDrones.Remove(drone);
        }

        return extractedDrones;
    }

    public void receiveFreeDrones(List<GameObject> drones)
    {
        foreach (var drone in new List<GameObject>(drones)) availableDrones.Add(drone);
    }
}