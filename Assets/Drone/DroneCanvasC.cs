using UnityEngine;
using System.Collections;

public class DroneCanvasC : MonoBehaviour {
	


	private Vector2 LstartButton;//start position of left joystick

	private Vector2 RButton; //Right joystick
	private Vector2 LButton;//left joystick
	private Vector2 RstartButton; //start position of right joystick

	public bool RL; //RL==true means that it is right joysctick, RL=false means that it is left joystick

	public float Rx;
	public float Ry;
	public float Lx;
	public float Ly; 

	private int ForwardBackward; 
	private int Tilt; 
	private int FlyLeftRight;  
	private int UpDown; 
	public GameObject Drone;



	void Start()
	{ 
		LButton = transform.position;
		RButton = transform.position;
		LstartButton = transform.position;
		RstartButton = transform.position;
		if(RL==true){
			RstartButton=RButton;}if(RL==false){
			LstartButton=LButton;}

	}
	void Update()
	{
		LButton = transform.position;
		RButton = transform.position;
		ForwardBackward = Drone.GetComponent<DroneControlC> ().ForwardBackward;
		Tilt = Drone.GetComponent<DroneControlC> ().Tilt;
		FlyLeftRight = Drone.GetComponent<DroneControlC> ().FlyLeftRight;
		UpDown = Drone.GetComponent<DroneControlC> ().UpDown;

	}
    public void Drag()
	{
		if(RL==true){

			Rx=RButton.x-RstartButton.x;//send difference between start position and current position of right joystick(X)
            if(Rx>0){Rx=Rx+FlyLeftRight+50;}
			if(Rx<0){Rx=Rx-FlyLeftRight-50;}
			Ry=RButton.y-RstartButton.y;//send difference between start position and current position of right joystick(Y)
			if(Ry>0){Ry=Ry+UpDown+50;}
			if(Ry<0){Ry=Ry-UpDown-50;}
		
		}
		if(RL==false){

			Lx=LButton.x-LstartButton.x;//send difference between start position and current position of left joystick(X)
			if(Lx>0){Lx=Lx+Tilt+40;}
			if(Lx<0){Lx=Lx-Tilt-40;}
			Ly=LButton.y-LstartButton.y;//send difference between start position and current position of left joystick(Y)
			if(Ly>0){Ly=Ly+ForwardBackward+40;}
			if(Ly<0){Ly=Ly-ForwardBackward-40;}
		}
	}

	public void endDrag()
	{
		if(RL==true){
			RButton=RstartButton;
			Rx=Ry=0;
		}
		if(RL==false){
			LButton=LstartButton;
			Lx=Ly=0;
		}

	}

}
