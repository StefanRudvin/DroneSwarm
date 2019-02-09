using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerModel : MonoBehaviour {

	public int _weight = 500;
	public int _orderPriority = 5;

	public ContainerModel(int weight, int orderPriority)
	{
		_weight = weight;
		_orderPriority = orderPriority;
	}
}
