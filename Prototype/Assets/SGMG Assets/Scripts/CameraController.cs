using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	
	private Vector3 dragOrigin;
	public float dragSpeed = 2;

	// Use this for initialization
	void Start ()
	{
	
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

        Vector3 move = new Vector3(-pos.x * dragSpeed, 0, -pos.y * dragSpeed); 

        transform.Translate(move, Space.World);  
		
	}
	
}

