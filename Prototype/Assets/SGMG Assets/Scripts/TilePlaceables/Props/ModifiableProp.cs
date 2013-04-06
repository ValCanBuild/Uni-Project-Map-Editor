using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModifiableProp : ITilePlaceable {
	
	private enum propType_t {TREE};	
	
	private EditorController	editor;
	
	private Tile				originTile;
	private Tile				adjacentTile;
	
	private Vector3	XRot = new Vector3(90,200,0);
	private Vector3	ZRot = new Vector3(90,60,0);
	
	private List<Tile>			availableTiles = new List<Tile>();
	
	private int					currentRotation = 0;

	// Use this for initialization
	void Start () {
		editor = FindObjectOfType(typeof(EditorController)) as EditorController;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public override void Select(){
		base.Select();
		Vector3 originPos = originTile.transform.position;
		
		//get all the free available tiles to rotate to
		availableTiles.Clear();
		availableTiles.Add(adjacentTile);
		//check tile at lower x
		Tile tile = editor.GetTileAtPos(originPos - new Vector3(1.0f,0,0));
		if (tile != null && tile != adjacentTile && tile.CanBeBuiltOn()){
			availableTiles.Add(tile);	
		}
		//check tile at lower z
		tile = editor.GetTileAtPos(originPos - new Vector3(0,0,1.0f));
		if (tile != null && tile != adjacentTile && tile.CanBeBuiltOn()){
			availableTiles.Add(tile);	
		}
		//check tile at upper x
		tile = editor.GetTileAtPos(originPos + new Vector3(1.0f,0,0));
		if (tile != null && tile != adjacentTile && tile.CanBeBuiltOn()){
			availableTiles.Add(tile);	
		}
		//check tile at upper z
		tile = editor.GetTileAtPos(originPos + new Vector3(0,0,1.0f));
		if (tile != null && tile != adjacentTile && tile.CanBeBuiltOn()){
			availableTiles.Add(tile);	
		}
		
		currentRotation = 0;
	}
	
	public void SetTiles(Tile origin, Tile adjacent){
		this.originTile = origin;
		this.adjacentTile = adjacent;
	}
	
	void OnGUI(){
		if (selected){
			GUI.Label(new Rect(Screen.width-100,20,50,30),"Tree");	
			if (GUI.Button(new Rect(Screen.width-100,60,50,40),"Rotate")){
				Rotate();
			}
		}
	}
	
	void Rotate(){
		currentRotation++;
		if (currentRotation > availableTiles.Count-1){
			currentRotation = 0;	
		}
		
		//clear old adjacentTile		
		adjacentTile.ObjectMoved();
		
		//get new adjacentTile
		adjacentTile = availableTiles[currentRotation];
		adjacentTile.PlaceObject(this.gameObject);
		
		Vector3 originPos = originTile.transform.position;
		Vector3	adjacentPos = adjacentTile.transform.position;
		
		Vector3 propPos = new Vector3(originPos.x,transform.position.y,originPos.z);
		
		Quaternion rot = Quaternion.identity;
		if (adjacentPos.x < originPos.x){
			propPos.x -= 0.5f;
			rot.eulerAngles = XRot;
		}
		else if (adjacentPos.x > originPos.x){
			propPos.x += 0.5f;
			rot.eulerAngles = XRot;
		}
		else if (adjacentPos.z < originPos.z){
			propPos.z -= 0.5f;
			rot.eulerAngles = ZRot;
		}
		else if (adjacentPos.z > originPos.z){
			propPos.z += 0.5f;
			rot.eulerAngles = ZRot;
		}
		
		this.transform.rotation = rot;
		this.transform.position = propPos;
	}

}
