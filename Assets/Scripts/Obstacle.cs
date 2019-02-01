using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider col)
	{
		//if the player hits one obstacle, it's game over
		if(col.gameObject.tag == Constants.PlayerTag)
		{
			GameManager.Instance.Die();
		}
	}
}
