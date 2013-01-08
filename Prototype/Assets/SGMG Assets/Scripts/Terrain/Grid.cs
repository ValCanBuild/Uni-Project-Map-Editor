using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour {
	
	public float 	size = 1.0f;
	
	public Transform 	mapParent;
	
	public Color 	color = Color.white;
	
	public bool		seeLines = true;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	/*void OnDrawGizmos() {
		if (seeLines){
			Vector3 pos = Camera.main.transform.position;	
			Gizmos.color = color;
			for (float z = pos.z - 200.0f; z < pos.z + 200.0f; z+= size)
		    {
		        Gizmos.DrawLine(new Vector3(-1000.0f, 0.0f , Mathf.Floor(z/size) * size),
		                        new Vector3(1000.0f, 0.0f, Mathf.Floor(z/size) * size));
		    }
	    
	    	for (float x = pos.x - 200.0f; x < pos.x + 200.0f; x+= size)
		    {
		        Gizmos.DrawLine(new Vector3(Mathf.Floor(x/size) * size, 0.0f , -1000.0f),
		                        new Vector3(Mathf.Floor(x/size) * size, 0.0f , 1000.0f));
		    }
		}
	}*/
}
