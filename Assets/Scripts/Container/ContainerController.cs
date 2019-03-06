using System;
using System.Collections.Generic;
using GeneticAlgorithms.Drone;
using UnityEngine;

public class ContainerController : MonoBehaviour
{
    public int _requiredDrones = 4;
    public bool _isTargeted = false;
    public bool _isMovingToLandingBlock = false;
    public bool _isCompleted = false;

    public GameObject _topLeftTarget;
    public GameObject _topRightTarget;
    public GameObject _bottomLeftTarget;
    public GameObject _bottomRightTarget;

    public GameObject _topLeftDrone;
    public GameObject _topRightDrone;
    public GameObject _bottomLeftDrone;
    public GameObject _bottomRightDrone;

    public ContainerModel _containerModel;

    public int _shipPriority = 0;
    
    public GameObject _nextContainer;
    
    private Task _task;

    public LandingContainerController LandingContainerController;
    public ShipController _ShipController;

    void Update()
    {
        // If not targeted or drones not arrived yet, do nothing
        if (!_isTargeted || !areDronesWaitingAtTarget()) return;

        // All drones have arrived
        if (!_isMovingToLandingBlock)
        {
            moveToPreviouslyDesignatedLandingBlock();
        }
        else
        {
            _isCompleted = true;
            removeBuildingBlockFromShipController();
            placeLandingContainer();
            freeDrones();
            _isCompleted = true;
            gameObject.SetActive(false);
            _task.isCompleted = true;  
//            Destroy(gameObject);
//            GameObject ChildGameObject2 = transform.GetChild(0).gameObject;
//            ChildGameObject2.GetComponent<Renderer>().material.color = Color.white;
//            ChildGameObject2.GetComponent<Renderer>().material.color = Utility.createColorFromWeight(_containerModel._weight);
        }
    }

    void removeBuildingBlockFromShipController()
    {
        _ShipController._containers.Remove(gameObject);
    }

    void placeLandingContainer()
    {
        LandingContainerController.placeLandingContainer(_containerModel);
    }

    void freeDrones()
    {
        _topLeftDrone.GetComponent<DroneController>().waitingAtTarget = false;
        _topLeftDrone.GetComponent<DroneController>().isMovingToBuildingblock = false;
        _topLeftDrone.GetComponent<DroneController>().isMovingToLandingBlock = false;
        _topLeftDrone.GetComponent<DroneController>().ResetTarget();

        _topRightDrone.GetComponent<DroneController>().waitingAtTarget = false;
        _topRightDrone.GetComponent<DroneController>().isMovingToBuildingblock = false;
        _topRightDrone.GetComponent<DroneController>().isMovingToLandingBlock = false;
        _topRightDrone.GetComponent<DroneController>().ResetTarget();

        _bottomLeftDrone.GetComponent<DroneController>().waitingAtTarget = false;
        _bottomLeftDrone.GetComponent<DroneController>().isMovingToBuildingblock = false;
        _bottomLeftDrone.GetComponent<DroneController>().isMovingToLandingBlock = false;
        _bottomLeftDrone.GetComponent<DroneController>().ResetTarget();

        _bottomRightDrone.GetComponent<DroneController>().waitingAtTarget = false;
        _bottomRightDrone.GetComponent<DroneController>().isMovingToBuildingblock = false;
        _bottomRightDrone.GetComponent<DroneController>().isMovingToLandingBlock = false;
        _bottomRightDrone.GetComponent<DroneController>().ResetTarget();
    }

    bool areDronesWaitingAtTarget()
    {
        return !(!_topLeftDrone.GetComponent<DroneController>().waitingAtTarget ||
                 !_topRightDrone.GetComponent<DroneController>().waitingAtTarget ||
                 !_bottomLeftDrone.GetComponent<DroneController>().waitingAtTarget ||
                 !_bottomRightDrone.GetComponent<DroneController>().waitingAtTarget);
    }

    public void assignDronesToContainer(List<GameObject> drones, Task task)
    {
        _isTargeted = true;
        _task = task;
        
        _topLeftDrone = drones[0];
        _topLeftDrone.GetComponent<DroneController>().Target = _topLeftTarget;
        _topLeftDrone.GetComponent<DroneController>().isMovingToBuildingblock = true;

        _topRightDrone = drones[1];
        _topRightDrone.GetComponent<DroneController>().Target = _topRightTarget;
        _topRightDrone.GetComponent<DroneController>().isMovingToBuildingblock = true;

        _bottomRightDrone = drones[2];
        _bottomRightDrone.GetComponent<DroneController>().Target = _bottomRightTarget;
        _bottomRightDrone.GetComponent<DroneController>().isMovingToBuildingblock = true;

        _bottomLeftDrone = drones[3];
        _bottomLeftDrone.GetComponent<DroneController>().Target = _bottomLeftTarget;
        _bottomLeftDrone.GetComponent<DroneController>().isMovingToBuildingblock = true;

        // Target only at end so that drones are present
        
    }
    
    public void makeTargetsKinematic()
    {
        _topLeftTarget.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        _topRightTarget.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        _bottomLeftTarget.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        _bottomRightTarget.gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }

    public void makeTargetsNonKinematic()
    {
        _topLeftTarget.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        _topRightTarget.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        _bottomLeftTarget.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        _bottomRightTarget.gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    private void moveToPreviouslyDesignatedLandingBlock()
    {
        _isMovingToLandingBlock = true;
        LandingContainerController.isTargeted = true;
        makeTargetsNonKinematic();
        setLandingTargetsForDrones(LandingContainerController);
    }


    private void setLandingTargetsForDrones(LandingContainerController ct)
    {
        _topLeftDrone.GetComponent<DroneController>().Target = ct._topLeftTarget;
        _topLeftDrone.GetComponent<DroneController>().waitingAtTarget = false;
        _topLeftDrone.GetComponent<DroneController>().isMovingToBuildingblock = false;
        _topLeftDrone.GetComponent<DroneController>().isMovingToLandingBlock = true;

        _topRightDrone.GetComponent<DroneController>().Target = ct._topRightTarget;
        _topRightDrone.GetComponent<DroneController>().waitingAtTarget = false;
        _topRightDrone.GetComponent<DroneController>().isMovingToBuildingblock = false;
        _topRightDrone.GetComponent<DroneController>().isMovingToLandingBlock = true;

        _bottomRightDrone.GetComponent<DroneController>().Target = ct._bottomRightTarget;
        _bottomRightDrone.GetComponent<DroneController>().waitingAtTarget = false;
        _bottomRightDrone.GetComponent<DroneController>().isMovingToBuildingblock = false;
        _bottomRightDrone.GetComponent<DroneController>().isMovingToLandingBlock = true;

        _bottomLeftDrone.GetComponent<DroneController>().Target = ct._bottomLeftTarget;
        _bottomLeftDrone.GetComponent<DroneController>().waitingAtTarget = false;
        _bottomLeftDrone.GetComponent<DroneController>().isMovingToBuildingblock = false;
        _bottomLeftDrone.GetComponent<DroneController>().isMovingToLandingBlock = true;
    }
}