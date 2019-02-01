using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDOL : MonoBehaviour {

	public void Awake()
	{
		DontDestroyOnLoad (gameObject);
		Debug.Log ("DDOL" + gameObject.name);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
