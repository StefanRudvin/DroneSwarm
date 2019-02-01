using System;
using UnityEngine;

public class LandingContainerController : MonoBehaviour
{
    public GameObject _topLeftTarget;
    public GameObject _topRightTarget;
    public GameObject _bottomLeftTarget;
    public GameObject _bottomRightTarget;

    public GameObject _basicBlock;

    public int requiredDrones = 4;

    public bool isTargeted = false;
    
    public void attachBlock(ContainerModel containerModel)
    {
        var pos = transform.position;
        Quaternion rotation= transform.rotation;
        pos.y += 0.5f;
        GameObject newContainer = Instantiate(_basicBlock, pos, rotation);

        ContainerModel newModel = newContainer.GetComponent<ContainerModel>();
        newModel._weight = containerModel._weight;
        newModel._orderPriority = containerModel._orderPriority;
        
//        GameObject  ChildGameObject1 = newContainer.transform.GetChild (0).gameObject;
//        newContainer.GetComponent<Renderer>().material.color = Utility.createColorFromWeight(containerModel._weight);

        Destroy(gameObject);
    }
}