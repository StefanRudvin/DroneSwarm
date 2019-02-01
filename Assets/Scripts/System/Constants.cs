using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : MonoBehaviour {

	public static readonly string PlayerTag = "Player";
	public static readonly string AnimationStarted = "started";
	public static readonly string AnimationJump = "jump";
	public static readonly string WidePathBorderTag = "WidePathBorder";

	public static readonly string StatusTapToStart = "Tap to start";
	public static readonly string StatusDeadTapToStart = "Dead. Tap to start";

	public enum GameState
	{
		Start,
		Playing,
		Dead
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
