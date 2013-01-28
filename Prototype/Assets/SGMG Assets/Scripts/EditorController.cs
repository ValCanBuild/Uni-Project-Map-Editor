using UnityEngine;
using System.Collections;
using System.IO;

public enum buildMode_t{REGULAR,PATROL_POINTS};

public class EditorController : MonoBehaviour
{
	public float			size = 1.0f;	
							
	public Transform 		mapParent;
							
	public EditorGUI		guiController;
							
	public buildMode_t		buildMode;
							
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
		
		case buildMode_t.PATROL_POINTS:
			PatrolPointLogic();
			break;
		}
	}
	
	void PlaceTile(){		
		Vector3 pos = GetMousePosInGrid();
		if (table.Contains(pos))
			return;
		currentTile = guiController.GetCurrentTile();
		currentTile = Instantiate(currentTile,pos,Quaternion.identity) as GameObject;		
		currentTile.transform.parent = mapParent;
		table.Add(currentTile.transform.position,currentTile);		
		if (currentTile.name.Contains("MovingTile")){
			movingTile = currentTile.GetComponent<MovingTile>();
			movingTile = Instantiate(movingTile) as MovingTile;
			buildMode = buildMode_t.PATROL_POINTS;
		}
		else{
			tileController.OrganizeTiles();	
		}
	}
	
	void DeleteTile(){		
		Vector3 pos = GetMousePosInGrid();
		if (!table.Contains(pos))
			return;
		GameObject currentTile = table[pos] as GameObject;
		//do not delete start tile
		if (currentTile.tag == "StartTile")
			return;
		Destroy(currentTile);
		table.Remove(pos);
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
	
	public void SwitchModes(){
		if (buildMode == buildMode_t.PATROL_POINTS){	
			buildMode = buildMode_t.REGULAR;
			Destroy(movingTile.gameObject);
		}
		else{
			buildMode = buildMode_t.PATROL_POINTS;
		}
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
			Destroy(child.gameObject);				
		}
		
		table.Clear();
		
		while (true){
			string line = r.ReadLine();
	        if (line != null){
				string[] text = line.Split(new char[]{'\t'});
				//GET TILE POSITION AND ADD TO PARENT AND HASH TABLE
				Vector3 position = new Vector3(float.Parse(text[0]),float.Parse(text[1]),float.Parse(text[2]));
				string tileType = text[3];	
				GameObject tileObject = Resources.Load(tileType) as GameObject;
				tileObject = Instantiate(tileObject,position,Quaternion.identity) as GameObject;
				tileObject.transform.parent = mapParent;
				table.Add(tileObject.transform.position,tileObject);	
				//IF TILE HAS ANY SPECIFIC PROPERTIES - ADD THEM HERE
				//if moving tile - add patrol coordinates to it - they are on the next line
				if (tileType.Equals("MovingTile")){
					MovingTile newMovingTile = tileObject.GetComponent<MovingTile>();
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

