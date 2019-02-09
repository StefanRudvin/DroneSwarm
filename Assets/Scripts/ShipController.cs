using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public List<GameObject> firstLevelLandingContainers;
    public List<GameObject> secondLevelLandingContainers;

    public List<GameObject> containers;
    
    public List<GameObject> firstLevelBuildingContainers;
    public List<GameObject> secondLevelBuildingContainers;

    public List<int> placedFirstLevelContainers;

    public List<GameObject> getOpenLandingContainers()
    {
        List<GameObject> availableContainers = new List<GameObject>();
        
        for (int i = 0; i < 10; i++)
        {
            availableContainers.Add(placedFirstLevelContainers.Contains(i)
                ? secondLevelBuildingContainers[i]
                : firstLevelBuildingContainers[i]);
        }
        return availableContainers;
    }
    
    public void RemoveLandingContainer(GameObject landingContainer)
    {
        if (firstLevelLandingContainers.Contains(landingContainer))
        {
            placedFirstLevelContainers.Add(firstLevelLandingContainers.IndexOf(landingContainer));
            
            //firstLevelLandingContainers.Remove(landingContainer);
        }
        else
        {
            //secondLevelLandingContainers.Remove(landingContainer);
        }
    }
}