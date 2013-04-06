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
		GameObject endTile = GameObject.FindGameObjectWithTag("EndTile");
		if (startTile && endTile){
			table.Add(startTile.transform.position,startTile);	
			table.Add(endTile.transform.position,endTile);	
		}
		else{
			Debug.Log("No start or end tile found - please add the appropriate tags to a start and/or end tile");	
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
			DestroyPlacedObject();	
		}
		
	}
	
	void PlaceProp(){
		Vector3 pos = GetMousePosInGrid();
		if (!table.Contains(pos))
			return;
		GameObject tileObject = table[pos] as GameObject;
		Tile tile = tileObject.GetComponent<Tile>();
		//if the tile is built on already, prop cannot be placed
		if (!tile.CanBeBuiltOn()){
			return;
		}		
		Texture propTexture = guiController.GetCurrentPropTexture();
		
		GameObject newProp = Instantiate(basePropObject) as GameObject;
		tile.PlaceObject(newProp);
		newProp.name = propTexture.name;//make name equal to texture for ease of exporting
		Quaternion rot = Quaternion.identity;
		Vector3 scale = Vector3.one;
		//Tree
		if (propTexture.name.Equals("Tree")){
			//a tree takes up two spaces - get an adjacent tile to this one
			Tile adjacentTile = GetAdjacentTile(pos);			
			//if there is none, then this tree can't fit, destroy prop
			if (!adjacentTile.CanBeBuiltOn()){
				Destroy(newProp);
				return;
			}			
			pos.y = 1.9f;	
			if (adjacentTile.transform.position.x < pos.x){
				pos.x -= 0.5f;
				rot.eulerAngles = new Vector3(90,200,0);
			}
			else if (adjacentTile.transform.position.x > pos.x){
				pos.x += 0.5f;
				rot.eulerAngles = new Vector3(90,200,0);
			}
			else if (adjacentTile.transform.position.z < pos.z){
				pos.z -= 0.5f;
				rot.eulerAngles = new Vector3(90,60,0);
			}
			else if (adjacentTile.transform.position.z > pos.z){
				pos.z += 0.5f;
				rot.eulerAngles = new Vector3(90,60,0);
			}
			scale = new Vector3(3,1,4);
			
			adjacentTile.PlaceObject(newProp);
			newProp.AddComponent<ModifiableProp>();//tree prop can be modified	
			newProp.GetComponent<ModifiableProp>().SetTiles(tile,adjacentTile);//has to be called so it can be rotated afterwards
		}
		//Sign
		else if (propTexture.name.Equals("Sign")){
			pos.y = 1.2f;
			scale = new Vector3(2,1,2);
			rot.eulerAngles = new Vector3(90,240,0);
		}
		//Rock
		else if (propTexture.name.Equals("Rock2")){
			pos.y = 0.67f;
			scale = new Vector3(1.4f,1,1.4f);
			rot.eulerAngles = new Vector3(90,200,0);
		}		
		
		newProp.transform.position = pos;
		newProp.transform.rotation = rot;
		newProp.transform.localScale = scale;
		newProp.transform.parent = propParent;
		newProp.GetComponentInChildren<Renderer>().material.mainTexture = propTexture;
		
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
		if (tile.tag == "StartTile" || tile.tag == "EndTile")
			return;
		Destroy(tile);
		table.Remove(pos);
	}
	
	void HazardLogic(){
		if (Input.GetKeyDown(KeyCode.A)){
			PlaceHazard();
		}	
		if (Input.GetKeyDown(KeyCode.D)){
			DestroyPlacedObject();
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
		if (tile.GetPlacedObject() != null)
			return;
		
		GameObject currentHazard = guiController.GetCurrentObject();
		if (currentHazard.name.Contains("Wind")){
			pos += new Vector3(0,1,0);
			currentHazard = Instantiate(currentHazard,pos,Quaternion.identity) as GameObject;	
			currentHazard.transform.parent = mapParent;
			tile.PlaceObject(currentHazard);
		}		
	}
	
	void DestroyPlacedObject(){
		Vector3 pos = GetMousePosInGrid();
		//if there is no tile there, there is no hazard there
		if (!table.Contains(pos))
			return;
		GameObject tileObject = table[pos] as GameObject;
		Tile tile = tileObject.GetComponent<Tile>();
		//if the tile is built on, destroy the hazard
		tile.DestroyPlacedObject();
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
	
	private Vector3 GetMousePosInGrid(){		
		Ray r = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
		Vector3 mousePos = r.origin;
		float xCoord = Mathf.Floor(mousePos.x/size)*size + size/2.0f;
		float zCoord = Mathf.Floor(mousePos.z/size)*size + size/2.0f;
		return new Vector3(xCoord,0.0f,zCoord);	
	}
	
	/// <summary>
	/// Gets a Tile adjacent to the current one, returning the first from the order
	/// : lower X, lower Z, upper X, upper Z
	/// </summary>
	/// <returns>
	/// The adjacent tile.
	/// </returns>
	/// <param name='pos'>
	/// Position.
	/// </param>
	public Tile GetAdjacentTile(Vector3 pos){
		Tile tile;
		
		Vector3 adjacentPos;			
		//first try tile that is to the downward X
		if (table.Contains(pos - new Vector3(1.0f,0,0))){
			adjacentPos = pos - new Vector3(1.0f,0,0);
			GameObject tileObject = table[adjacentPos] as GameObject;
			tile = tileObject.GetComponent<Tile>();
			if (tile.CanBeBuiltOn()){
				return tile;	
			}
		}
		//then try downward Z
		if (table.Contains(pos - new Vector3(0,0,1.0f))){
			adjacentPos = pos - new Vector3(0,0,1.0f);
			GameObject tileObject = table[adjacentPos] as GameObject;
			tile = tileObject.GetComponent<Tile>();
			if (tile.CanBeBuiltOn()){
				return tile;	
			}
		}
		//then try upward X
		if (table.Contains(pos + new Vector3(1.0f,0,0))){
			adjacentPos = pos + new Vector3(1.0f,0,0);
			GameObject tileObject = table[adjacentPos] as GameObject;
			tile = tileObject.GetComponent<Tile>();
			if (tile.CanBeBuiltOn()){
				return tile;	
			}
		}
		//then try upward Z
		if (table.Contains(pos + new Vector3(0,0,1.0f))){
			adjacentPos = pos + new Vector3(0,0,1.0f);
			GameObject tileObject = table[adjacentPos] as GameObject;
			tile = tileObject.GetComponent<Tile>();
			if (tile.CanBeBuiltOn()){
				return tile;	
			}
		}
		return null;
	}
	
	public Tile	GetTileAtPos(Vector3 pos){
		GameObject tileObject = table[pos] as GameObject;
		if (tileObject == null){
			return null;
		}
		Tile tile = tileObject.GetComponent<Tile>();
		return tile;
	}
	
	public void ImportMap(string path){
		
		foreach (Transform child in mapParent){
			table.Remove(child.position);
			child.gameObject.active = false;
			Destroy(child.gameObject);
		}
		
		mapParent.DetachChildren();
		
		table.Clear();
		
		bool result = MapIO.ImportMap(path,mapParent,propParent);
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
						windTile.PlaceObject(child.gameObject);
					}	
				}
			}
			//mark prop containing tiles
			foreach (Transform prop in propParent){
				Vector3 posInMap = prop.position;
				posInMap.y = 0.0f;
				Tile adjacentPropTile = null;
				//check if this prop takes 2 tiles
				if (posInMap.z == (int)posInMap.z){
					Vector3 adjacentPosInMap = posInMap;				
					posInMap.z -= 0.5f;
					adjacentPosInMap.z += 0.5f;								
					adjacentPropTile = (table[adjacentPosInMap] as GameObject).GetComponent<Tile>();				
				}
				else if (posInMap.x == (int)posInMap.x){
					Vector3 adjacentPosInMap = posInMap;				
					posInMap.x -= 0.5f;
					adjacentPosInMap.x += 0.5f;							
					adjacentPropTile = (table[adjacentPosInMap] as GameObject).GetComponent<Tile>();
				}
												
				Tile propTile = (table[posInMap] as GameObject).GetComponent<Tile>();
				if (propTile != null){
					propTile.PlaceObject(prop.gameObject);
				}
				//if this prop takes up more than 2 spaces, add a modifiable component
				if (adjacentPropTile != null){
					adjacentPropTile.PlaceObject(prop.gameObject);	
					prop.gameObject.AddComponent<ModifiableProp>().SetTiles(propTile,adjacentPropTile);
				}
			}
			tileController.OrganizeTiles();
			FixPropTextures();
		}		
	
	}

	private void FixPropTextures(){
		foreach (Transform prop in propParent){
			Texture propTexture = guiController.GetPropTextureForName(prop.name);
			prop.GetComponentInChildren<Renderer>().material.mainTexture = propTexture;
		}
	}

}