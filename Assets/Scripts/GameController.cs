using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour {
	public List<GameObject> availableDrones = new List<GameObject> ();
	public List<GameObject> movingToLandingList = new List<GameObject> ();
	public List<GameObject> buildingBlocks = new List<GameObject> ();
	public List<GameObject> landingPads = new List<GameObject> ();
	public GameObject startPad;
	public GameObject sampleDrone;

	void Start () {
		instantiateDrones();
	}

	void instantiateDrones() {
		List<GameObject> startPads = startPad.GetComponent<StartPadController>().startPads;

		foreach (GameObject pad in startPads) {
			GameObject drone = Instantiate(sampleDrone, pad.transform) as GameObject;
			drone.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
			DroneController droneController = drone.GetComponent<DroneController>();
			droneController.Target = pad;
			availableDrones.Add(drone);
		}
	}

	private void Update () {
		assignDronesToBuildingBlocks();
	}
	
	/*
	 * Find free building blocks and assign free drones to them.
	 */
	void assignDronesToBuildingBlocks() {
		foreach (var buildingBlock in buildingBlocks) {
			var blockController = buildingBlock.GetComponent<BlockController>();
			if (!blockController.isTargeted && availableDrones.Count >= blockController.requiredDrones) {
				Debug.Log("found a building block, assigning drones to it...");

				List<GameObject> extractedDrones = getNAvailableDrones(blockController.requiredDrones);
				blockController.assignDronesToBuildingBlock(extractedDrones);								
			}
		}
	}

	public List<GameObject> getNAvailableDrones(int requiredDrones)
	{
		List<GameObject> extractedDrones = new List<GameObject> ();

		for (int i = 0; i < requiredDrones; i++)
		{
			GameObject drone = availableDrones[0];
			extractedDrones.Add(drone);
			availableDrones.Remove(drone);
		}
		return extractedDrones;
	}

	public LandingBlockController getFreeLandingBlock(int dronesCount)
	{
		foreach (GameObject landingPad in landingPads)
		{
			LandingBlockController landingController = landingPad.GetComponent<LandingBlockController>();
			
			if (landingController.isTargeted ||
			    landingController.requiredDrones != dronesCount) continue;

			return landingController;
		}
		Debug.Log("Couldn't find landingController for " + dronesCount.ToString() + " drones.");
		return null;
	}

	void assignFreeDronesToAvailable(){
		var dronesToMove = new List<GameObject>();
		foreach (GameObject drone in movingToLandingList) {
			if (!drone.GetComponent<DroneController>().isMovingToLanding) {
				availableDrones.Add(drone);
				
				dronesToMove.Add(drone);
			}
		}

		foreach (GameObject drone in dronesToMove) {
			movingToLandingList.Remove(drone);
		}
	}
}
