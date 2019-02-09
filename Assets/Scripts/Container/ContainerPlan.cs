using System.Collections.Generic;
using UnityEngine;

public class ContainerPlan : MonoBehaviour
{
    public List<ContainerModel> firstRow;
    public List<ContainerModel> secondRow;

    public ContainerPlan(List<ContainerModel> firstRow, List<ContainerModel> secondRow)
    {
        this.firstRow = firstRow;
        this.secondRow = secondRow;
    }
    
    public ContainerPlan(List<List<ContainerModel>> ListOfLists)
    {
        firstRow = ListOfLists[0];
        secondRow = ListOfLists[1];
    }
}