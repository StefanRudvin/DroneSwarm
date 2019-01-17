using System;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;

public class MoveService : MonoBehaviour
{

    private readonly DroneController _controller;

    public MoveService(DroneController controller)
    {
        _controller = controller;
    }

    public void move()
    {
        var targetController = _controller.Target.GetComponent<TargetController>();
        var targetLevel = targetController.level;

        if (targetLevel != _controller.level)
        {
            _controller.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            moveTowardsTargetLevel(targetLevel);
            return;
        }

        _controller.gameObject.GetComponent<NavMeshAgent>().enabled = true;

        float vector3distanceToTarget = Vector3.Distance(_controller.Target.transform.position, _controller.transform.position);

        // If close to object, just set it to it.
        if (vector3distanceToTarget < 1.5)
        {
            AttachToTarget();
            _controller.waitingAtTarget = true;
        }
        else
        {
            NavMeshAgent agent = _controller.GetComponent<NavMeshAgent>();
            agent.destination = _controller.Target.transform.position;
        }
        
    }

    void AttachToTarget()
    {
        var joint = _controller.Target.gameObject.AddComponent<FixedJoint>();
        joint.massScale = 100;
        joint.connectedMassScale = 100f;
        joint.connectedBody = _controller._rigidbody;
    }
    
    void moveTowardsTargetLevel(int targetLevel)
    {
        // If close to the y axis
        if (Math.Abs(_controller.transform.position.y - Levels.levelsDict[targetLevel]) < 0.05 ||
            _controller.transform.position.y > Levels.levelsDict[targetLevel])
        {
            _controller.level = targetLevel;
            Vector3 targetTransform = _controller.transform.position;
            targetTransform.y = Levels.levelsDict[targetLevel];
            _controller.transform.position = targetTransform;
        }
        // Moving towards next level.
        else
        {
            if (_controller.transform.position.y < Levels.levelsDict[targetLevel])
            {
                _controller.transform.Translate(Vector3.up * Time.deltaTime * 5, Space.World);
            }
            else
            {
                _controller.transform.Translate(Vector3.down * Time.deltaTime * 5, Space.World);
            }
        }
    }
}