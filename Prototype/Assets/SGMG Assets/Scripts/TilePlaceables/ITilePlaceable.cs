using UnityEngine;
using System.Collections;

/// <summary>
/// TilePlaceable interface - exposes functionality for any object that can be
/// placed on a tile.
/// </summary>
public class ITilePlaceable : MonoBehaviour {
	
	protected bool	selected;
	
	// Use this for initialization
	void Start () {
		selected = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public virtual void Select(){
		selected = true;	
	}
	
	public virtual void Deselect(){
		selected = false;	
	}
}
