using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainController : MonoBehaviour {

	private Vector3 stationTarget;
	private Vector3 finalTarget;

	public float toStation = 1F;
	public float fromStation = 2.0F;

	private Vector3 velocity = Vector3.zero;


	// Use this for initialization
	void Start () {
		stationTarget = transform.TransformPoint (0, 0, 280);
		finalTarget = transform.TransformPoint (0, 0, 1000);
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.position.z > 250) {
			Destroy(this.gameObject);
		}


		if (transform.position.z < 28) {
			MoveToStation ();
		} else {
			MoveToFinal ();
		}
	}

	void MoveToStation () {
		Vector3 targetPosition = stationTarget;
		transform.position = Vector3.SmoothDamp (transform.position, targetPosition, ref velocity, toStation);
	}

	void MoveToFinal () {
		Vector3 targetPosition = finalTarget;
		transform.position = Vector3.SmoothDamp (transform.position, targetPosition, ref velocity, fromStation);
	}
}
