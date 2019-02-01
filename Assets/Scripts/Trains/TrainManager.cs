using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainManager : MonoBehaviour {

	public GameObject[] trains;
	public GameObject platform;

	private Vector3 platformPos;

	public float spawnTime = 1.0f;

	public Transform[] spawnPoints;
	private Transform stationTarget;

	// Use this for initialization
	void Start () {
		InvokeRepeating ("Spawn", spawnTime, spawnTime);
	}

	void Spawn() {
		int spawnPointIndex = Random.Range (0, spawnPoints.Length);
		int trainIndex = Random.Range (0, trains.Length);

		Instantiate (trains[trainIndex], spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation); 

		platformPos = spawnPoints [spawnPointIndex].position + new Vector3 (0.0f, 1.0f, 222.0f);

		Instantiate (platform, platformPos, spawnPoints[spawnPointIndex].rotation);

	}
}