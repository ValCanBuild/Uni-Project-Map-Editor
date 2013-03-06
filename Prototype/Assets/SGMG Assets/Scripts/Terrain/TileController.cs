using UnityEngine;
using System.Collections;

//public enum tileType_t{INNER,EDGE_N,EDGE_S,EDGE_E,EDGE_W,EDGE_NS,EDGE_WE,CORNER_NE,CORNER_NW,CORNER_SE,CORNER_SW}

public class TileController : MonoBehaviour {
	
	public static int 	TileEditLayer = 1 << 13;
	
	public Transform	ActiveObjectLight;
	
	public Texture		regular;	
	public Texture		unwalkable;
	
	public GameObject	startTileIndicator;	
	private Tile		activeTile;
	
	GameObject 			startTile;
	
	// Use this for initialization
	void Start () {
		startTile = GameObject.FindGameObjectWithTag("StartTile");
		PlaceIndicators();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0)) {
			if (GUIUtility.hotControl != 0)
				return;
			RaycastHit hit;
	        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Debug.DrawRay(ray.origin,ray.direction,Color.white);
	        if (Physics.Raycast(ray, out hit,float.MaxValue,TileEditLayer)){      
	            activeTile = hit.transform.GetComponent<Tile>();
			}
			else{
				activeTile = null;	
			}
			if (activeTile != null){
				ActiveObjectLight.position = activeTile.transform.position + new Vector3(0,1,0);
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
		PlaceIndicators();
	}
	
	private void PlaceIndicators(){	
		startTileIndicator.transform.position = startTile.transform.position + new Vector3(0,2.2f,0);
	}

	void OnGUI(){
		if (activeTile == null || activeTile.hazardObject != null){
			return;
		}
		GUI.Label(new Rect(Screen.width-100,20,80,30),"Regular Tile");
		bool isCracked = activeTile.GetCracked();
		isCracked = GUI.Toggle(new Rect(Screen.width-100,60,50,30),isCracked,"Crack Tile");
		if (isCracked != activeTile.GetCracked()){
			activeTile.SetCracked(isCracked);	
		}
		if (GUI.Button(new Rect(Screen.width-100,100,80,40),"Make Start")){
			startTile.tag = "Untagged";
			activeTile.tag = "StartTile";
			startTile = activeTile.gameObject;
			PlaceIndicators();
		}
	}

}