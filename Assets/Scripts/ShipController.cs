using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public List<GameObject> firstLevelLandingContainers;
    public List<GameObject> secondLevelLandingContainers;

    public List<GameObject> containers;

    public void RemoveLandingContainer(GameObject landingContainer)
    {
        if (firstLevelLandingContainers.Contains(landingContainer))
        {
            firstLevelLandingContainers.Remove(landingContainer);
        }
        else
        {
            secondLevelLandingContainers.Remove(landingContainer);
        }
    }
}