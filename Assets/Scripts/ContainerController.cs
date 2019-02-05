using System;
using System.Collections.Generic;
using UnityEngine;

public class ContainerController : MonoBehaviour
{
    public int _requiredDrones = 4;
    public bool _isTargeted = false;
    public bool _isMovingToLandingBlock = false;

    public GameObject _topLeftTarget;
    public GameObject _topRightTarget;
    public GameObject _bottomLeftTarget;
    public GameObject _bottomRightTarget;

    public GameObject _topLeftDrone;
    public GameObject _topRightDrone;
    public GameObject _bottomLeftDrone;
    public GameObject _bottomRightDrone;

    public ContainerModel _containerModel;

    private GameController _gameController;

    public LandingContainerController LandingContainerController;
    public ShipController _ShipController;

    void Start()
    {
        _gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        if (_gameController == null)
        {
            throw new Exception("Could not find GameController");
        }
    }

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
            freeDrones();
            removeBuildingBlockFromGameControllerList();
            removeLandingBlockFromGameControllerList();
            attachBlock();
            Destroy(gameObject);
        }
    }

    void removeBuildingBlockFromGameControllerList()
    {
        _ShipController.containers.Remove(gameObject);
    }
    
    void removeLandingBlockFromGameControllerList()
    {
        _ShipController.RemoveLandingContainer(LandingContainerController.gameObject);
    }

    void attachBlock()
    {
        LandingContainerController.attachBlock(_containerModel);
    }

    void freeDrones()
    {
        _topLeftDrone.GetComponent<DroneController>().waitingAtTarget = false;
        _topLeftDrone.GetComponent<DroneController>().isMovingToBuildingblock = false;
        _topLeftDrone.GetComponent<DroneController>().isMovingToLandingBlock = false;
        _topLeftDrone.GetComponent<DroneController>().resetTarget();
        
        _topRightDrone.GetComponent<DroneController>().waitingAtTarget = false;
        _topRightDrone.GetComponent<DroneController>().isMovingToBuildingblock = false;
        _topRightDrone.GetComponent<DroneController>().isMovingToLandingBlock = false;
        _topRightDrone.GetComponent<DroneController>().resetTarget();
        
        _bottomLeftDrone.GetComponent<DroneController>().waitingAtTarget = false;
        _bottomLeftDrone.GetComponent<DroneController>().isMovingToBuildingblock = false;
        _bottomLeftDrone.GetComponent<DroneController>().isMovingToLandingBlock = false;
        _bottomLeftDrone.GetComponent<DroneController>().resetTarget();
        
        _bottomRightDrone.GetComponent<DroneController>().waitingAtTarget = false;
        _bottomRightDrone.GetComponent<DroneController>().isMovingToBuildingblock = false;
        _bottomRightDrone.GetComponent<DroneController>().isMovingToLandingBlock = false;
        _bottomRightDrone.GetComponent<DroneController>().resetTarget();
        
        _gameController.receiveFreeDrones(new List<GameObject>
        {
            _topLeftDrone, _topRightDrone, _bottomLeftDrone, _bottomRightDrone
        });
    }

    bool areDronesWaitingAtTarget()
    {
        return !(!_topLeftDrone.GetComponent<DroneController>().waitingAtTarget ||
               !_topRightDrone.GetComponent<DroneController>().waitingAtTarget ||
               !_bottomLeftDrone.GetComponent<DroneController>().waitingAtTarget ||
               !_bottomRightDrone.GetComponent<DroneController>().waitingAtTarget);
    }

    public void assignDronesToContainer(List<GameObject> drones)
    {
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
        _isTargeted = true;
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