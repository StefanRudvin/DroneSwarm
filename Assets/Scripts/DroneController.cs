using System;
using System.Collections;
using System.Collections.Generic;
using GeneticAlgorithms.Drone;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class DroneController : MonoBehaviour
{
    public bool isMovingToLandingBlock = false;
    public bool isMovingToBuildingblock = false;

    public int _normalSpeed = 2;
    public int _normalAngularSpeed = 60;
    
    public int _liftingSpeed = 1;
    public int _liftingAngularSpeed = 5;

    public bool waitingAtTarget;
    public int level;
    
    public GameObject Target;
    
    public GameObject startPosition;
    public List<GameObject> props = new List<GameObject>();

    private MoveService _moveService;
    
    public Rigidbody _rigidbody;

    public List<Task> tasks;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody> ();
        _moveService = new MoveService(this);
    }

    // Update is called once per frame
    private void Update()
    {
        spinProps();
        if (!waitingAtTarget)
        {
            move();
        }
        else
        {
            stayAtTarget();
        }
    }

    public void ResetTarget()
    {
        Target = startPosition;
    }

    public void setLiftingSpeed()
    {
        NavMeshAgent agent = gameObject.GetComponent<NavMeshAgent>();
        agent.speed = _liftingSpeed;
        agent.angularSpeed = _liftingAngularSpeed;
    }
    
    public void setNormalSpeed()
    {
        NavMeshAgent agent = gameObject.GetComponent<NavMeshAgent>();
        agent.speed = _normalSpeed;
        agent.angularSpeed = _normalAngularSpeed;
    }

    void stayAtTarget()
    {
        transform.position = Target.transform.position;
    }

    void move()
    {
        _moveService.move();
    }
    
    public void AttachToTarget()
    {
        var joint = Target.gameObject.AddComponent<FixedJoint>();
        joint.massScale = 100;
        joint.connectedMassScale = 100f;
        joint.connectedBody = _rigidbody;
    }
    
    public void DetachFromTarget()
    {
        var joint = Target.gameObject.GetComponent<FixedJoint>();
        joint.connectedBody = null;
    }

    void spinProps()
    {
        int counter = 1;
        foreach (GameObject prop in props)
        {
            if (counter <= 2)
            {
                prop.transform.Rotate(new Vector3(0, 0, 20), 250 * Time.deltaTime);
            }
            else
            {
                prop.transform.Rotate(new Vector3(0, 0, -20), 250 * Time.deltaTime);
            }

            counter++;
        }
    }
}