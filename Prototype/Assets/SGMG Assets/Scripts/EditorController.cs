using UnityEngine;
using System.Collections;
using System.IO;

public enum buildMode_t{REGULAR,PATROL_POINTS,HAZARD,PROPS};

public class EditorController : MonoBehaviour
{
	public float			size = 1.0f;								
	public Transform 		mapParent;			
	public Transform		propParent;
	public EditorGUI		guiController;							
	public buildMode_t		buildMode{get;set;}
	public GameObject		basePropObject;
							
	private GameObject		currentTile;
	private MovingTile		movingTile;
	private Hashtable 		table;
	private TileController	tileController;
	// Use this for initialization
	void Start ()
	{		
		tileController = mapParent.GetComponent<TileController>();
		table = new Hashtable();
		GameObject startTile = GameObject.FindGameObjectWithTag("StartTile");
		if (startTile){
			table.Add(startTile.transform.position,startTile);	
		}
		else{
			Debug.Log("No start tile found - please add 'StartTile' tag to a start tile");	
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		switch (buildMode){
		case buildMode_t.REGULAR:
			if (Input.GetKey(KeyCode.A)){
				PlaceTile();	
			}
			if (Input.GetKey(KeyCode.D)){
				DeleteTile();	
			}
			break;
		case buildMode_t.HAZARD:
			HazardLogic();
			break;
		case buildMode_t.PATROL_POINTS:
			PatrolPointLogic();
			break;
		case buildMode_t.PROPS:
			PropLogic();
			break;			
		}
	}
	
	void PropLogic(){
		if (Input.GetKeyDown(KeyCode.A)){
			PlaceProp();	
		}
		if (Input.GetKeyDown(KeyCode.D)){
			DeleteProp();	
		}
		
	}
	
	void PlaceProp(){
		Vector3 pos = GetMousePosInGrid();
		if (!table.Contains(pos))
			return;
		Texture propTexture = guiController.GetCurrentPropTexture();
		Quaternion rot = Quaternion.identity;
		Vector3 scale = Vector3.one;
		if (propTexture.name.Equals("Tree")){
			pos.y = 1.9f;
			pos.x = (int)pos.x;
			scale = new Vector3(3,1,4);
			rot.eulerAngles = new Vector3(90,200,0);
		}
		else if (propTexture.name.Equals("Sign")){
			pos.y = 1.2f;
			scale = new Vector3(2,1,2);
			rot.eulerAngles = new Vector3(90,200,0);
		}
		GameObject newProp = Instantiate(basePropObject,pos,rot) as GameObject;
		newProp.transform.localScale = scale;
		newProp.transform.parent = propParent;
		newProp.GetComponentInChildren<Renderer>().material.mainTexture = propTexture;
	}
	
	void DeleteProp(){
			
	}
		
	void PlaceTile(){		
		Vector3 pos = GetMousePosInGrid();
		if (table.Contains(pos))
			return;
		currentTile = guiController.GetCurrentObject();
		currentTile = Instantiate(currentTile,pos,Quaternion.identity) as GameObject;
		currentTile.transform.parent = mapParent;
		table.Add(currentTile.transform.position,currentTile);		
		if (currentTile.name.Contains("MovingTile")){
			movingTile = currentTile.GetComponent<MovingTile>();
			movingTile = Instantiate(movingTile) as MovingTile;
			buildMode = buildMode_t.PATROL_POINTS;
		}
	}
	
	void DeleteTile(){		
		Vector3 pos = GetMousePosInGrid();
		if (!table.Contains(pos))
			return;
		GameObject tile = table[pos] as GameObject;
		//do not delete start tile
		if (tile.tag == "StartTile")
			return;
		Destroy(tile);
		table.Remove(pos);
	}
	
	void HazardLogic(){
		if (Input.GetKeyDown(KeyCode.A)){
			PlaceHazard();
		}	
		if (Input.GetKeyDown(KeyCode.D)){
			DestroyHazard();
		}	
	}
	
	void PlaceHazard(){
		Vector3 pos = GetMousePosInGrid();
		//if there is no tile there, hazard cannot be placed
		if (!table.Contains(pos))
			return;
		GameObject tileObject = table[pos] as GameObject;
		Tile tile = tileObject.GetComponent<Tile>();
		//if the tile is built on already, hazard cannot be placed
		if (tile.hazardObject != null)
			return;
		
		GameObject currentHazard = guiController.GetCurrentObject();
		if (currentHazard.name.Contains("Wind")){
			pos += new Vector3(0,1,0);
			currentHazard = Instantiate(currentHazard,pos,Quaternion.identity) as GameObject;	
			currentHazard.transform.parent = mapParent;
			tile.hazardObject = currentHazard;
		}		
	}
	
	void DestroyHazard(){
		Vector3 pos = GetMousePosInGrid();
		//if there is no tile there, there is no hazard there
		if (!table.Contains(pos))
			return;
		GameObject tileObject = table[pos] as GameObject;
		Tile tile = tileObject.GetComponent<Tile>();
		//if the tile is built on, destroy the hazard
		if (tile.hazardObject != null){
			Destroy(tile.hazardObject);
		}
	}
	
	void PatrolPointLogic(){
		Vector3 pos = GetMousePosInGrid();
		movingTile.transform.position = pos;
		if (Input.GetKey(KeyCode.A)){
			if (table.Contains(pos)){
				return;
			}
			if (!currentTile.GetComponent<MovingTile>().IsLegalPatrolPoint(pos)){
				return;
			}
			currentTile.GetComponent<MovingTile>().patrolPoints.Add(pos);
			MovingTile patrolLoc = Instantiate(movingTile,pos,Quaternion.identity) as MovingTile;
			patrolLoc.HalfAlpha();
			table.Add(pos,patrolLoc.gameObject);	
		}
		if (Input.GetKey(KeyCode.D)){
			if (table.Contains(pos)){
				GameObject patrolPoint = table[pos] as GameObject;
				//do not delete start tile
				if (patrolPoint.name.Contains("MovingTile")){
					Destroy(patrolPoint);
					table.Remove(pos);	
					currentTile.GetComponent<MovingTile>().patrolPoints.Remove(pos);
				}
			}
		}
	}
	
	public void SwitchMode(buildMode_t toMode){
		if (buildMode == buildMode_t.PATROL_POINTS){	
			Destroy(movingTile.gameObject);
		}
		buildMode = toMode;
	}
	
	Vector3 GetMousePosInGrid(){		
		Ray r = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
		Vector3 mousePos = r.origin;
		float xCoord = Mathf.Floor(mousePos.x/size)*size + size/2.0f;
		float zCoord = Mathf.Floor(mousePos.z/size)*size + size/2.0f;
		return new Vector3(xCoord,0.0f,zCoord);	
	}
	
	public void ImportMap(string path){
		
		foreach (Transform child in mapParent){
			table.Remove(child.position);
			child.gameObject.active = false;
			Destroy(child.gameObject);
		}
		
		mapParent.DetachChildren();
		
		table.Clear();
		
		bool result = MapIO.ImportMap(path,mapParent);
		//if map was successfuly imported, do any post-initialization here
		if (result == true){
			foreach (Transform child in mapParent){
				if (child.name.Contains("Tile")){
					table.Add(child.position,child.gameObject);
				}
				if (child.name.Contains("MovingTile")){
					MovingTile importedMovingTile = child.GetComponent<MovingTile>();	
					foreach (Vector3 patrolPoint in importedMovingTile.patrolPoints){
						if (!table.Contains(patrolPoint)){
							MovingTile patrolLoc = Instantiate(importedMovingTile,patrolPoint,Quaternion.identity) as MovingTile;
							patrolLoc.HalfAlpha();
							table.Add(patrolPoint,patrolLoc.gameObject);
						}	
					}
				}
				//If this is a wind hazard
				if (child.name.Contains("Wind")){
					Vector3 posInMap = child.position;
					posInMap.y = 0.0f;
					Tile windTile = (table[posInMap] as GameObject).GetComponent<Tile>();
					if (windTile != null){
						windTile.hazardObject = child.gameObject;
					}	
				}
			}
			tileController.OrganizeTiles();
		}		
	}
}