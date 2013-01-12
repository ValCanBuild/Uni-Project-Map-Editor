using UnityEngine;
using System.Collections.Generic;

public class MovingTile : MonoBehaviour {
	
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
}
