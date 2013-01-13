using UnityEngine;
using System.IO;
using System.Collections;

public class EditorGUI : MonoBehaviour {
	
	public GUIContent[]	tiles;	//gui content tile tooltip must be the same name as the tile prefab 
	
	private GameObject	currentTile;
	
	public EditorController controller;
	
	//button variables
	private int			buttonWidth = 100;
	private int			buttonHeight = 100;
	private int 		boxHeight = 100;
	private int			boxWidth = 100;
	
	private bool		selectionActive = false;
	private int 		selectionGrid = 0;
	private int			newSelectionGrid = 0;
	
	private string 		mapName = "Map Name";
	private string		mapPath = "";
	private bool		mapExported = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (selectionActive){
			if (selectionGrid != newSelectionGrid){
				selectionGrid = newSelectionGrid;
				selectionActive = false;
			}
		}			
	}
	
	void OnGUI() {
		switch (controller.buildMode){
			case buildMode_t.REGULAR:
				SelectionGUI();
				ExportGUI();
				break;
			case buildMode_t.PATROL_POINTS:
				PatrolPointGUI();
				break;			
		}			
	}
	
	private void PatrolPointGUI(){
		GUI.TextArea (new Rect (Screen.width/2-70, Screen.height-75, 150, 25), "Placing Patrol Points");
		if (GUI.Button (new Rect (Screen.width/2-70, Screen.height-30, 150, 30), "Finish")){
			controller.SwitchModes();
		}
	}
	
	private void SelectionGUI(){
		if (!selectionActive){
			if (GUI.Button (new Rect (Screen.width/2-70, Screen.height-50, 150, 50), "Switch Building Type")) {
				// This code is executed every frame that the Button is clicked
				selectionActive = true;
				selectionGrid = newSelectionGrid = -1;
			}
		}
		else{
			newSelectionGrid = GUI.SelectionGrid (new Rect (Screen.width/2-110, Screen.height-100, 230, 100), newSelectionGrid, tiles, 2);
		}	
	}
	
	private void ExportGUI(){
		if (mapExported)
			GUI.Label(new Rect(10, 120, 150, 100),"Map Exported to " + mapPath);
		
		mapName = GUI.TextField(new Rect(10,10,100,30),mapName);
		if (GUI.Button (new Rect (10, 45, 100, 75), "Export Map")){
			if (mapName.Length < 1)
				return;
			if (!Directory.Exists(Application.persistentDataPath + "/MyMaps/"))
				Directory.CreateDirectory(Application.persistentDataPath + "/MyMaps");
			mapPath = Application.persistentDataPath + "/MyMaps/" + mapName + ".sgmg";
			if (mapPath.Length != 0){
        		TextWriter f = new StreamWriter(mapPath);
				
				foreach (Transform child in controller.mapParent.transform){
					
					if (child.name.Contains("(Clone)")){
						child.name = child.name.Replace("(Clone)","");
					}
					f.WriteLine(child.position.x + "\t" + child.position.y + "\t" + child.position.z + "\t" + child.name);
					if (child.name.Equals("MovingTile")){
						MovingTile movingTile = child.gameObject.GetComponent<MovingTile>();
						//if moving tile has any patrol points - write them to a new line
						if (movingTile.patrolPoints.Count > 1){
							for (int i = 0; i < movingTile.patrolPoints.Count; i++){							
								f.Write(movingTile.patrolPoints[i].x + "," + movingTile.patrolPoints[i].y + "," + movingTile.patrolPoints[i].z);
								if (i != movingTile.patrolPoints.Count-1){
									f.Write("\t");	
								}							
							}
						}
						//else if it hasn't any set - just put it's original place as the one point
						else{
							f.Write(child.position.x + "," + child.position.y + "," + child.position.z);
						}
						f.WriteLine();
					}
				}
				
				f.Close();
				f = null;
					
				Debug.Log("Map successfuly exported as: " + mapPath);
				mapExported = true;
				
			}	
		}
	}
	
	public GameObject GetCurrentTile(){
		GameObject tile = Resources.Load(tiles[selectionGrid].tooltip) as GameObject;
		return tile;
	}
}
