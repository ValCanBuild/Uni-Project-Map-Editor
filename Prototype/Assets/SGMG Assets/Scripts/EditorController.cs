using UnityEngine;
using System.Collections;
using System.IO;

public enum buildMode_t{REGULAR,PATROL_POINTS,HAZARD};

public class EditorController : MonoBehaviour
{
	public float			size = 1.0f;								
	public Transform 		mapParent;							
	public EditorGUI		guiController;							
	public buildMode_t		buildMode{get;set;}
							
	private GameObject		currentTile;
	private MovingTile		movingTile;
	private Hashtable 		table;
	private TileController	tileController;
	
	private string			tilePath = "Tiles/";
	private string			hazardPath = "Hazards/";
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
		}
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
		TextReader r = new StreamReader(path);

		for (int i = 0; i < mapParent.childCount; i++){
			Transform child = mapParent.GetChild(i);
			table.Remove(child.position);
			child.gameObject.active = false;
			Destroy(child.gameObject);		
			child = null;
		}
		
		table.Clear();
		
		while (true){
			string line = r.ReadLine();
	        if (line != null){
				string[] text = line.Split(new char[]{'\t'});
				//GET TILE POSITION AND ADD TO PARENT AND HASH TABLE
				Vector3 position = new Vector3(float.Parse(text[0]),float.Parse(text[1]),float.Parse(text[2]));
				string objectType = text[3];
				string objectTag = null;
				if (text.Length > 4){
					objectTag = text[4];	
				}
				GameObject gameObject;
				if (objectType.Contains("Tile")){
					gameObject = Resources.Load(tilePath+objectType) as GameObject;
				}
				else{
					gameObject = Resources.Load(hazardPath+objectType) as GameObject;	
				}
				
				gameObject = Instantiate(gameObject,position,Quaternion.identity) as GameObject;
				gameObject.transform.parent = mapParent;
				if (objectTag != null){
					gameObject.tag = objectTag;
				}
				
				if (objectType.Contains("Tile")){
					table.Add(gameObject.transform.position,gameObject);
				}
				//IF Object HAS ANY SPECIFIC PROPERTIES - ADD THEM HERE
				//if moving tile - add patrol coordinates to it - they are on the next line
				if (objectType.Equals("MovingTile")){
					MovingTile newMovingTile = gameObject.GetComponent<MovingTile>();
					line = r.ReadLine();
					text = line.Split(new char[]{'\t'});
					for (int i = 0; i < text.Length; i++){
						string vector = text[i];
						string[] xyz = vector.Split(new char[]{','});
						Vector3 patrolP = new Vector3(float.Parse(xyz[0]),float.Parse(xyz[1]),float.Parse(xyz[2]));
						newMovingTile.patrolPoints.Add(patrolP);
						if (!table.Contains(patrolP)){
							MovingTile patrolLoc = Instantiate(newMovingTile,patrolP,Quaternion.identity) as MovingTile;
							patrolLoc.HalfAlpha();
							table.Add(patrolP,patrolLoc.gameObject);
						}
					}
				}
				//If this is a wind hazard
				else if (objectType.Equals("Wind")){
					Wind wind = gameObject.GetComponent<Wind>();
					line = r.ReadLine();
					text = line.Split(new char[]{','});
					int range;
					if (!int.TryParse(text[0],out range)){
						Debug.Log("Error parsing wind hazard range info");
					}
					direction_t dir = (direction_t)System.Enum.Parse(typeof(direction_t),text[1]);
					wind.SetRange(range);
					wind.SetDirection(dir);
					Vector3 posInMap = position;
					posInMap.y = 0.0f;
					Tile windTile = (table[posInMap] as GameObject).GetComponent<Tile>();
					if (windTile != null){
						windTile.hazardObject = wind.gameObject;
					}
				}
				
	        }
			else{
				break;	
			}
		}
					
		r.Close();
		r = null;
		Debug.Log("Map successfuly imported");
		tileController.OrganizeTiles();	
	}
}