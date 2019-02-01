using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public List<GameObject> firstLevelLandingBlocks;
    public List<GameObject> secondLevelLandingBlocks;

    public List<GameObject> containers;

    public void RemoveLandingBlock(GameObject landingBlock)
    {
        if (firstLevelLandingBlocks.Contains(landingBlock))
        {
            firstLevelLandingBlocks.Remove(landingBlock);
        }
        else
        {
            secondLevelLandingBlocks.Remove(landingBlock);
        }
    }
}