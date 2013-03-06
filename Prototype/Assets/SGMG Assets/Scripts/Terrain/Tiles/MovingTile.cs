using UnityEngine;
using System.Collections.Generic;

public class MovingTile : Tile {
	
	public List<Vector3> patrolPoints = new List<Vector3>();

	// Use this for initialization
	void Start () {
		patrolPoints.Add(transform.position);
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	public void HalfAlpha(){
		Color color = renderer.material.color;
        color.a -= 0.5f;
		renderer.material.color = color;	
	}
	
	public bool IsLegalPatrolPoint(Vector3 point){
		Vector3 lastPatrolPoint = patrolPoints[patrolPoints.Count-1];
		Vector3 referenceRight= Vector3.Cross(Vector3.up, lastPatrolPoint);
		
		float angle = Vector3.Angle(lastPatrolPoint,point);
		
		//if (angle != 90.0f && angle != 180.0f && angle != 270.0f && angle != 0.0f && angle != 360.0f){
			//return false;	
		//}
		return true;
	}
}
