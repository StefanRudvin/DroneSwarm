using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour {
	public List<GameObject> availableDrones = new List<GameObject> ();
	
	public List<GameObject> buildingBlocks = new List<GameObject> ();
	public List<GameObject> landingBlocks = new List<GameObject> ();
	
	public GameObject startPad;
	public GameObject sampleDrone;

	private void Start () {
		instantiateDrones();
	}

	void instantiateDrones() {
		var startPads = startPad.GetComponent<StartPadController>().startPads;

		foreach (var pad in startPads) {
			var drone = Instantiate(sampleDrone, pad.transform);
			drone.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
			var droneController = drone.GetComponent<DroneController>();
			droneController.Target = pad;
			droneController.startPosition = pad;
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
			
			if (blockController._isTargeted || availableDrones.Count < blockController._requiredDrones) continue;
			
			Debug.Log("found a building block, assigning drones to it...");

			List<GameObject> extractedDrones = getNAvailableDrones(blockController._requiredDrones);
			blockController.assignDronesToBuildingBlock(extractedDrones);
		}
	}

	private List<GameObject> getNAvailableDrones(int requiredDrones)
	{
		List<GameObject> extractedDrones = new List<GameObject> ();

		for (var i = 0; i < requiredDrones; i++)
		{
			GameObject drone = availableDrones[0];
			extractedDrones.Add(drone);
			availableDrones.Remove(drone);
		}
		return extractedDrones;
	}

	public LandingBlockController getFreeLandingBlock(int dronesCount)
	{
		foreach (GameObject landingPad in landingBlocks)
		{
			LandingBlockController landingController = landingPad.GetComponent<LandingBlockController>();
			
			if (landingController.isTargeted ||
			    landingController.requiredDrones != dronesCount) continue;

			return landingController;
		}
		Debug.Log("Couldn't find landingController for " + dronesCount.ToString() + " drones.");
		return null;
	}

	public void receiveFreeDrones(List<GameObject> drones)
	{
		foreach (var drone in new List<GameObject> (drones)) availableDrones.Add(drone);
	}
}
