using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float walkSpeed = 3f;
	public float runSpeed = 6f;

	private float movementSpeed;

	private int turnCounter = 0;
	private int jumpCounter = 0;

	private Animator anim;
	private Rigidbody rigidbody;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		rigidbody = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		// var z = Input.GetAxis("Vertical") * Time.deltaTime * movementSpeed;
		// transform.Translate(0, 0, z);

		anim.SetFloat ("Speed_f", 0.4f);
		anim.SetBool ("Jump_b", false);
		movementSpeed = walkSpeed;

		if (Input.GetKey ("x")) {
			anim.SetFloat ("Speed_f", 3f);
			movementSpeed = runSpeed;
			if (Input.GetKey ("space") && jumpCounter <= 0) {
				Jump ();
			}
		}

		var x = Input.GetAxis("Horizontal");

		if (x > 0 && turnCounter <= 0) {
			transform.Rotate (0, 90, 0);
			turnCounter = 30;
		}

		if (x < 0 && turnCounter <= 0) {
			transform.Rotate (0, -90, 0);
			turnCounter = 30;
		}

		if (turnCounter > 0) {
			turnCounter -= 1;
		}

		if (jumpCounter > 0) {
			jumpCounter -= 1;
		}

		if (!(Input.GetKey ("c"))) {
			transform.position += transform.forward * Time.deltaTime * movementSpeed;
		}

	}

	void Jump() {
		anim.SetBool ("Jump_b", true);
		rigidbody.AddForce (Vector3.up * 1000);

		// transform.Translate (0, JumpSpeed, 0);
		// jumpCounter = 60;
	}
}
