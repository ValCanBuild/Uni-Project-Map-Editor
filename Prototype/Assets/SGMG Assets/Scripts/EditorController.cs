using UnityEngine;
using System.Collections;

public enum buildMode_t{REGULAR,PATROL_POINTS};

public class EditorController : MonoBehaviour
{
	public float		size = 1.0f;	
	
	public Transform 	mapParent;
	
	public EditorGUI	guiController;
	
	public buildMode_t	buildMode;
	
	private GameObject	currentTile;
	private MovingTile	movingTile;
	private Hashtable 	table;
	// Use this for initialization
	void Start ()
	{		
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
				if (table.Contains(pos))
					return;
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
}

