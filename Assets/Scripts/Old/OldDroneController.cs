using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class OldDroneController : AController {

	public bool isLifting = false;
	public bool isAtTarget = false;
	public bool isMovingToLanding = false;
	private GameObject startPosition;
	public List<GameObject> props = new List<GameObject>();
	public GameObject Target;

	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
	}

	void OnCollisionEnter(Collision otherObject)
	{
		if (otherObject.gameObject == Target && isMovingToLanding) {
			Target = startPosition;
			isAtTarget = true;
			isLifting = false;
		}

		if (otherObject.gameObject == Target && !otherObject.gameObject.GetComponent<FixedJoint>()) {
			var joint = Target.gameObject.AddComponent<FixedJoint>();
 			joint.connectedBody = this.rb;
			isAtTarget = true;
			isLifting = true;
		}
	}

	void spinProps() {
		int counter = 1;
		foreach (GameObject prop in props) {
			if (counter <= 2) {
				prop.transform.Rotate(new Vector3 (0, 0, 20), 250 * Time.deltaTime);
			} else {
				prop.transform.Rotate(new Vector3 (0, 0, -20), 250 * Time.deltaTime);
			}
			counter ++ ;
			
		}
	}
	
	// Update is called once per frame
	void Update () {
		spinProps();
	}
}
