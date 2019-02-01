using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenController : MonoBehaviour {

	private Animator anim;

	public bool Smoking;
	public bool Dancing;
	public bool Leaning;
	public bool Dying;
	public bool Waving;
	public bool Falling;
	public bool CheckingTime;
	public bool WipingMouth;
	public bool Sitting;

	private int wait_time;


	// Use this for initialization
	void Start () {
		StartCoroutine (waiter());
		anim = GetComponent<Animator> ();
	}

	IEnumerator waiter()
	{
		wait_time = Random.Range (0, 4);
		yield return new WaitForSeconds (wait_time);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
		if (Smoking) {
			anim.SetBool ("Smoking", true);
		} else {
			anim.SetBool ("Smoking", false);
		}

		if (Waving) {
			anim.SetBool ("Waving", true);
		} else {
			anim.SetBool ("Waving", false);
		}

		if (Dying) {
			anim.SetBool ("Death_b", true);
		} else {
			anim.SetBool ("Death_b", false);
		}

		if (Dancing) {
			anim.SetBool ("Dancing", true);
		} else {
			anim.SetBool ("Dancing", false);
		}

		if (Falling) {
			anim.SetBool ("Grounded_b", false);
		} else {
			anim.SetBool ("Grounded_b", true);
		}

		if (CheckingTime) {
			anim.SetBool ("CheckingTime", true);
		} else {
			anim.SetBool ("CheckingTime", false);
		}

		if (Leaning) {
			anim.SetBool ("Leaning", true);
		} else {
			anim.SetBool ("Leaning", false);
		}

		if (WipingMouth) {
			anim.SetBool ("WipingMouth", true);
		} else {
			anim.SetBool ("WipingMouth", false);
		}

		if (Sitting) {
			anim.SetBool ("Sitting", true);
		} else {
			anim.SetBool ("Sitting", false);
		}

	}


}
