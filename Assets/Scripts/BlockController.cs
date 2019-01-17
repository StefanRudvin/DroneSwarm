using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    public int requiredDrones = 4;
    public bool isTargeted = false;
    public bool isMovingToLandingBlock = false;

    public GameObject topLeftTarget;
    public GameObject topRightTarget;
    public GameObject bottomLeftTarget;
    public GameObject bottomRightTarget;

    public GameObject topLeftDrone;
    public GameObject topRightDrone;
    public GameObject bottomLeftDrone;
    public GameObject bottomRightDrone;

    private GameController gameController;

    // Use this for initialization
    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        if (gameController == null)
        {
            throw new Exception("Could not find");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMovingToLandingBlock && isTargeted)
        {
            checkIfDronesAreAttached();
        }
    }

    void checkIfDronesAreAttached()
    {
        if (!topLeftDrone.GetComponent<DroneController>().waitingAtTarget ||
            !topRightDrone.GetComponent<DroneController>().waitingAtTarget ||
            !bottomLeftDrone.GetComponent<DroneController>().waitingAtTarget ||
            !bottomRightDrone.GetComponent<DroneController>().waitingAtTarget)
        {
            return;
        }

        tryMoveToLandingBlock();
    }

    void tryMoveToLandingBlock()
    {
        Debug.Log("Trying to find a free landing block.");
        // All drones are here. Find a landing block.
        LandingBlockController maybeLandingBlockController = gameController.getFreeLandingBlock(requiredDrones);

        if (maybeLandingBlockController == null)
        {
            Debug.Log("Couldn't find a free landing block. Waiting for next round.");
        }

        Debug.Log("Found a free landing block. Moving to it.");
        moveToLandingBlock(maybeLandingBlockController);
    }


    public void assignDronesToBuildingBlock(List<GameObject> drones)
    {
        topLeftDrone = drones[0];
        topLeftDrone.GetComponent<DroneController>().Target = topLeftTarget;

        topRightDrone = drones[1];
        topRightDrone.GetComponent<DroneController>().Target = topRightTarget;

        bottomRightDrone = drones[2];
        bottomRightDrone.GetComponent<DroneController>().Target = bottomRightTarget;

        bottomLeftDrone = drones[3];
        bottomLeftDrone.GetComponent<DroneController>().Target = bottomLeftTarget;

        // Target only at end so that drones are present
        isTargeted = true;
    }

    public void freeTargets()
    {
        topLeftTarget.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        topRightTarget.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        bottomLeftTarget.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        bottomRightTarget.gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    private void moveToLandingBlock(LandingBlockController thisLandingBlockController)
    {
        isMovingToLandingBlock = true;
        thisLandingBlockController.isTargeted = true;

        freeTargets();

        setLandingTargetsForDrones(thisLandingBlockController);
    }


    private void setLandingTargetsForDrones(LandingBlockController ct)
    {
        topLeftDrone.GetComponent<DroneController>().Target = ct.topLeftTarget;

        topRightDrone.GetComponent<DroneController>().Target = ct.topRightTarget;

        bottomRightDrone.GetComponent<DroneController>().Target = ct.bottomRightTarget;

        bottomLeftDrone.GetComponent<DroneController>().Target = ct.bottomLeftTarget;
    }
}