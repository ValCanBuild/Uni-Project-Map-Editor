using UnityEngine;
using System.Collections;

public enum direction_t{NORTH = 0,EAST,SOUTH,WEST = 3};
public class Wind : MonoBehaviour
{
	public static int		ContraptionLayer = 1 << 14;
	
	public direction_t 		direction;
	public int				range;
	
	public bool				selected = false;
	
	ParticleSystem			particleSystem;
	
	// Use this for initialization
	void Start ()
	{
		direction = direction_t.EAST;
		range = 5;
		GetParticleSystem();
		particleSystem.startLifetime = 0.3f*range;                                                                                             
	}
	
	void GetParticleSystem(){
		foreach (Transform child in transform){
			if (child.particleSystem != null){
				this.particleSystem = child.particleSystem;
				break;
			}
		}	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButton(0)) {
			if (GUIUtility.hotControl != 0)
				return;
			RaycastHit hit;
	        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	        if (Physics.Raycast(ray, out hit,float.MaxValue,ContraptionLayer)){      
	            if (hit.transform == this.transform)
					selected = true;
				else{
					selected = false;	
				}
			}
			else{
				selected = false;	
			}
		}
	}
	
	public void SetDirection(direction_t to){
		direction = to;
		ChangeDirection(0);
	}
	
	void ChangeDirection(int amount){
		direction += amount;
		if (direction > direction_t.WEST){
			direction = 0;	
		}
		else if (direction < 0){
			direction = direction_t.WEST;	
		}
		switch (direction){
		case direction_t.NORTH:
			transform.localRotation = Quaternion.AngleAxis(270, Vector3.up);
			break;
		case direction_t.EAST:
			transform.localRotation = Quaternion.AngleAxis(0, Vector3.up);
			break;
		case direction_t.SOUTH:
			transform.localRotation = Quaternion.AngleAxis(90, Vector3.up);
			break;
		case direction_t.WEST:
			transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
			break;
		}
	}
	
	public void SetRange(int toRange){
		this.range = toRange;
		if (particleSystem == null)
			GetParticleSystem();
		if (particleSystem == null)
			return;
		particleSystem.startLifetime = 0.3f*range;
	}
	
	void OnGUI(){
		if (selected){
			GUI.Label(new Rect(Screen.width-100,20,50,30),"Wind");	
			GUI.Label(new Rect(Screen.width-120,60,70,20),"Wind force: ");
			string rangeStr = GUI.TextField(new Rect(Screen.width-30,60,20,20),range.ToString());	
			int rangeInt;
			if (int.TryParse(rangeStr,out rangeInt)){
				if (rangeInt != range){
					SetRange(rangeInt);
				}
			}
			if (GUI.Button(new Rect(Screen.width-100,90,50,40),"Rotate")){
				ChangeDirection(1);
			}
		}
	}
}

