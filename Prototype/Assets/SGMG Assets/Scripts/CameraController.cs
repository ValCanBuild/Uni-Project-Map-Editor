using UnityEngine;
using System.Collections;

public enum cameraType_t{BUILDVIEW,PLAYERVIEW}
public class CameraController : MonoBehaviour
{
	public cameraType_t mCameraType{get;set;}
	public float 		dragSpeed = 2;
	private Vector3 	dragOrigin;	

	// Use this for initialization
	void Start ()
	{
		if (transform.position.y < 2.5f){
			Vector3 pos = transform.position;
			pos.y = 2.5f;
			transform.position = pos;
		}
		mCameraType = cameraType_t.BUILDVIEW;
		//SwitchMode();
	}
	
	// Update is called once per frame
	void Update ()
	{		
		if (Input.GetAxis("Mouse ScrollWheel") != 0.0f){
			float scrollValue = Input.GetAxis("Mouse ScrollWheel");
			this.camera.orthographicSize -= scrollValue;
		}
		
		if (Input.GetMouseButtonDown(0)){
            dragOrigin = Input.mousePosition;
            return;
        }
		
		if (!Input.GetMouseButton(0))
			return;
		
		Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
	
		switch (mCameraType){
		case cameraType_t.BUILDVIEW:
			Vector3 move = new Vector3(-pos.x * dragSpeed, 0 , -pos.y * dragSpeed);
			transform.Translate(move, Space.World);
			break;
		case cameraType_t.PLAYERVIEW:
			Vector3 moveX = new Vector3(-dragSpeed*pos.x, 0 ,dragSpeed*pos.x); 
			Vector3 moveY = new Vector3(-dragSpeed*pos.y, 0 ,-dragSpeed*pos.y); 
			Vector3 totalMove = moveX + moveY;
	        transform.Translate(totalMove, Space.World);
			break;	
		}
		 		
	}
	public void SwitchMode(){
		if (mCameraType == cameraType_t.BUILDVIEW){
			mCameraType = cameraType_t.PLAYERVIEW;	
		}
		else{
			mCameraType = cameraType_t.BUILDVIEW;	
		}
		switch (mCameraType){
		case cameraType_t.BUILDVIEW:
			Vector3 buildRot = new Vector3(90,0,0);
			this.transform.eulerAngles = buildRot;
			break;
		case cameraType_t.PLAYERVIEW:
			Vector3 playerRot = new Vector3(30,45,0);
			this.transform.eulerAngles = playerRot;
			break;	
		}
	}
}