using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class ShipController : MonoBehaviour
{
    public List<GameObject> _firstLevelLandingContainers;
    public List<GameObject> _secondLevelLandingContainers;

    public List<GameObject> _containers;
    
    public List<GameObject> _firstLevelBuildingContainers;
    public List<GameObject> _secondLevelBuildingContainers;

    public GameObject containerLocationStart;
    
    public int _priority;
}