using UnityEngine;
using System.Collections;

//public enum tileType_t{INNER,EDGE_N,EDGE_S,EDGE_E,EDGE_W,EDGE_NS,EDGE_WE,CORNER_NE,CORNER_NW,CORNER_SE,CORNER_SW}

public class TileController : MonoBehaviour {
	
	public static int 	TileEditLayer = 1 << 13;
	
	public Transform	ActiveObjectLight;
	
	public Texture		regular;	
	public Texture		unwalkable;
	
	public GameObject	startTileIndicator;	
	public GameObject	endTileIndicator;	
	
	private Tile		activeTile;
	
	private GameObject 	startTile;
	private GameObject	endTile;
	// Use this for initialization
	void Start () {
		startTile = GameObject.FindGameObjectWithTag("StartTile");
		endTile = GameObject.FindGameObjectWithTag("EndTile");
		PlaceIndicators();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0)) {
			if (GUIUtility.hotControl != 0)
				return;
			RaycastHit hit;
			//deselect the previous tile, if there was one
			if (activeTile != null){
				activeTile.TryDeselectObject();	
			}
	        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Debug.DrawRay(ray.origin,ray.direction,Color.white);
	        if (Physics.Raycast(ray, out hit,float.MaxValue,TileEditLayer)){      
	            activeTile = hit.transform.GetComponent<Tile>();
			}
			else{
				activeTile = null;	
			}
			//check if we have a tile selected, then check if the tile has a selectable object on it
			if (activeTile != null){
				ActiveObjectLight.position = activeTile.transform.position + new Vector3(0,1,0);
				activeTile.TrySelectObject();
			}
			else
				ActiveObjectLight.position = new Vector3(-100,0,-100);
		}
	}
	
	public void OrganizeTiles(){
		startTile = null;
		foreach (Transform tile in transform){
			if ((!tile.name.Contains("RegularTile")) && !tile.name.Contains("UnWalkableTile"))
				continue;
			
			if (tile.name.Contains("RegularTile")){
				tile.renderer.material.mainTexture = regular;
			}
			else {
				tile.renderer.material.mainTexture = unwalkable;	
			}		
		}
		GameObject [] objects = GameObject.FindGameObjectsWithTag("StartTile");
		foreach (GameObject tileObj in objects){
			if (tileObj.active){
				startTile = tileObj;
				break;
			}
				
		}
		objects = GameObject.FindGameObjectsWithTag("EndTile");
		foreach (GameObject tileObj in objects){
			if (tileObj.active){
				endTile = tileObj;
				break;
			}
				
		}
		PlaceIndicators();
	}
	
	private void PlaceIndicators(){	
		startTileIndicator.transform.position = startTile.transform.position + new Vector3(0,2.2f,0);
		endTileIndicator.transform.position = endTile.transform.position + new Vector3(0,0.8f,0);
	}

	void OnGUI(){
		if (activeTile == null || !activeTile.CanBeBuiltOn()){
			return;
		}
		GUI.Label(new Rect(Screen.width-100,20,80,30),"Regular Tile");
		bool isCracked = activeTile.GetCracked();
		isCracked = GUI.Toggle(new Rect(Screen.width-100,60,50,30),isCracked,"Crack");
		if (isCracked != activeTile.GetCracked()){
			activeTile.SetCracked(isCracked);	
		}
		if (GUI.Button(new Rect(Screen.width-100,100,80,40),"Make Start")){
			startTile.tag = "Untagged";
			activeTile.tag = "StartTile";
			startTile = activeTile.gameObject;
			PlaceIndicators();
		}
		if (GUI.Button(new Rect(Screen.width-100,150,80,40),"Make End")){
			endTile.tag = "Untagged";
			activeTile.tag = "EndTile";
			endTile = activeTile.gameObject;
			PlaceIndicators();
		}
		activeTile.hintEnabled = GUI.Toggle(new Rect(Screen.width-100,200,80,30),activeTile.hintEnabled,"Hint");
		if (activeTile.hintEnabled)
			activeTile.hint = GUI.TextArea(new Rect(Screen.width-100,240,90,100),activeTile.hint);
	}

}