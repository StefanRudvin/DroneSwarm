using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerOld : MonoBehaviour {
	public List<GameObject> availableDrones = new List<GameObject> ();
	public List<GameObject> movingToLandingList = new List<GameObject> ();
	public List<GameObject> buildingBlocks = new List<GameObject> ();
	public List<GameObject> landingPads = new List<GameObject> ();
	public GameObject startPad;

	public GameObject sampleDrone;
	private Dictionary<GameObject, List<GameObject>> movingToBlockDict = new Dictionary<GameObject, List<GameObject>>();
	private Dictionary<GameObject, List<GameObject>> movingToLandingDict = new Dictionary<GameObject, List<GameObject>>();

	void Start () {
		instantiateDrones();
	}

	void instantiateDrones() {
		List<GameObject> startPads = startPad.GetComponent<StartPadController>().startPads;

		foreach (GameObject pad in startPads) {
			GameObject drone = Instantiate(sampleDrone, pad.transform);
			availableDrones.Add(drone);
		}
	}
	
	void Update () {/*
		assignDronesToBlocks();
		assignDronesToLandingPads();*/
		assignFreeDronesToAvailable();
	}

	void assignFreeDronesToAvailable(){
		List<GameObject> dronesToMove = new List<GameObject>();
		foreach (GameObject drone in movingToLandingList) {
			if (!drone.GetComponent<DroneController>().isMovingToBuildingblock) {
				availableDrones.Add(drone);
				dronesToMove.Add(drone);
			}
		}

		foreach (GameObject drone in dronesToMove) {
			movingToLandingList.Remove(drone);
		}


	}

	/*void assignDronesToBlocks() {
		foreach (GameObject buildingBlock in buildingBlocks) {
			BlockController blockController = buildingBlock.GetComponent<BlockController>();
			if (!blockController.isTargeted && availableDrones.Count >= blockController.requiredDrones) {

				List<GameObject> targets = new List<GameObject>();

				foreach (GameObject target in blockController.targets) {
					targets.Add(target);
				}
				
				blockController.isTargeted = true;

				int count = blockController.requiredDrones;

				movingToBlockDict[buildingBlock] = new List<GameObject>();

				while (count > 0) {
					GameObject drone = availableDrones[0];
					DroneController droneController = drone.GetComponent<DroneController>();
					GameObject target = targets[0];

					List<GameObject> drones = movingToBlockDict[buildingBlock];
					drones.Add(drone);
					movingToBlockDict[buildingBlock] = drones;
					
					droneController.Target = target;
					targets.RemoveAt(0);
					availableDrones.Remove(drone);
					count --;
				}
			}
		}
	}*/
/*

	void assignDronesToLandingPads() {
		foreach(KeyValuePair<GameObject, List<GameObject>> entry in movingToBlockDict)
		{
			bool eachAtTarget = true;

			foreach(GameObject drone in entry.Value) {
				DroneController droneController = drone.GetComponent<DroneController>();
				if (!droneController.waitingAtTarget) {
					eachAtTarget = false;
				}
			}

			if (!eachAtTarget) {
				continue;
			}

			// Each drone is at target, find new target.
			BlockController blockController = entry.Key.GetComponent<BlockController> ();
			blockController.isTargeted = true;
			//blockController.isCarried = true;

			foreach(GameObject target in blockController.targets) {
				target.gameObject.GetComponent<Rigidbody>().isKinematic = false;
			}

			foreach (GameObject landingPad in landingPads) {
				LandingBlockController landingController = landingPad.GetComponent<LandingBlockController>();
				if (!landingController.isTargeted && landingController.requiredDrones == blockController.requiredDrones) {
					landingController.isTargeted = true;	
					List<GameObject> targets = new List<GameObject>();

					foreach (GameObject target in landingController.targets) {
						targets.Add(target);
					}

					int count = blockController.requiredDrones;
					List<GameObject> drones = entry.Value;

					while (count > 0) {
						GameObject drone = drones[0];
						DroneController droneController = drone.GetComponent<DroneController>();
						GameObject target = targets[0];
						droneController.waitingAtTarget = false;
						droneController.Target = target;
						droneController.isMovingToLanding = true;
						targets.RemoveAt(0);
						movingToLandingList.Add(drone);
						drones.Remove(drone);
						count --;
					}
				}
			}
		}
	}*/
}
