using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	protected GameManager()
	{
		GameState = Constants.GameState.Start;
		CanSwipe = false;
	}

	public Constants.GameState GameState { get; set;}

	public bool CanSwipe { get; set;}

	public void Die()
	{
		UIManager.Instance.SetStatus(Constants.StatusDeadTapToStart);
		this.GameState = Constants.GameState.Dead;
	}

	private static GameManager instance;

	public static GameManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameManager();
			}
			return instance;
		}
	}


	void Awake() {
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			DestroyImmediate(this);
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
