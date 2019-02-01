using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeDestroyer : MonoBehaviour {

	public float LifeTime = 10f;

	// Use this for initialization
	void Start () {
		Invoke("DestroyObject", LifeTime);
	}

	void DestroyObject()
	{
		if (GameManager.Instance.GameState != Constants.GameState.Dead)
			Destroy(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
