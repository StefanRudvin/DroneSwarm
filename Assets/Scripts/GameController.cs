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
                
                var firstLevelContainer = Instantiate(sampleContainer, temp, starting_transform.rotation);
                var firstLevelContainerController = firstLevelContainer.GetComponent<ContainerController>();
                
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

    private void RunGeneticAlgorithm()
    {
        /*
         * Multi-Agent travelling salesman problem.
         *
         * We have n drones, or collections of 4 drones.
         *
         * We have n blocks with n destinations.
         *
         * Create a genetic algorithm for that?
         *
         * Idea:
         * Generate 1 gene:
         *
         * Take a drone, give it a target, then another, then another. Fitness function is the total time from start to finish.
         *
         * Drone 1    Building-C 1    Landing-C 1
         * Drone 2    Building-C 2    Landing-C 2
         *            Building-C 3    Landing-C 3
         *            Building-C 4    Landing-C 4
         *            Building-C 5    Landing-C 5
         *
         * Create random permutations.
         *
         * Pick a random drone + a random destination. Rinse and repeat until no more destinations.
         * In between that, program a way so that containers on the 2nd level aren't available until the first one is placed.
         *
         * Need a data structure to hold current.
         *
         * Tree Data structure? Keep leaves in an array or some shite like that.
         *
         * Graph Data Structure. Maybe keep a running count for each.
         *
         * System can also decide not to do anything for a time period.
         *
         * Once the bottom layer is reached, add the new graph node with a cost!
         *
         * Even better: Once the 'bottom' container takes off and is at the same distance from the landing as the 'top' container,
         * free the top container with the current cost.
         *
         * Each drone keeps a running 'cost' to it.
         *
         * End 'fitness' is the largest 'cost' in each drone.
         * 
         * 
         *
         * 
         * 
         * 
         * 
         */
        
        DroneGeneticAlgorithm droneGeneticAlgorithm = new DroneGeneticAlgorithm();

        droneGeneticAlgorithm.Run();
    }

    /*
     * Find free building blocks and assign free drones to them.
     */
    void assignDronesToBuildingBlocks()
    {
        //RunGeneticAlgorithm();
        foreach (var ship in ships)
        {
            ShipController shipController = ship.GetComponent<ShipController>();

            foreach (var buildingBlock in shipController.getOpenLandingContainers())
            {
                var containerController = buildingBlock.GetComponent<ContainerController>();

                if (containerController._isTargeted || availableDrones.Count < containerController._requiredDrones) continue;

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