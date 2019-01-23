using System;
using UnityEngine;

public class LandingBlockController : MonoBehaviour
{
    public GameObject _topLeftTarget;
    public GameObject _topRightTarget;
    public GameObject _bottomLeftTarget;
    public GameObject _bottomRightTarget;

    public GameObject _basicBlock;

    public int requiredDrones = 4;

    public bool isTargeted = false;
    
    public void attachBlock()
    {
        var pos = transform.position;
        Quaternion rotation= transform.rotation;
        pos.y += 0.5f;
        Instantiate(_basicBlock, pos, rotation);
        
        //GameObject.Instantiate(_basicBlock, gameObject.transform);
        // Destroy from GameController too?
        Destroy(gameObject);
    }
}