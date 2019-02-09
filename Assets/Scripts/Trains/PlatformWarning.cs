using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformWarning : MonoBehaviour {

	private Color lerpedColor = Color.green;

	public Renderer rend;


	// Use this for initialization
	void Start () {
		rend = GetComponent<Renderer>();
		rend.enabled = true;
		rend.material.color = Color.green;

		// gameObject.GetComponent<Renderer>().material.color = Color.green;
	}

	// Update is called once per frame
	void Update () {

		if (rend.material.color == Color.red) {
			Destroy(this.gameObject);
		}

		lerpedColor = Color.Lerp(rend.material.color, Color.red, Mathf.PingPong((Time.time * 0.005f), 0.5f));

		rend.material.color = lerpedColor;

		if (rend.material.color == Color.red) {
			Destroy(this.gameObject);
		}


	}
}
