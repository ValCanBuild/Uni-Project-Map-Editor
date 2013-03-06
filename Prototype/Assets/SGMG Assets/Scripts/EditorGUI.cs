using UnityEngine;
using System.IO;
using System.Collections;

public class EditorGUI : MonoBehaviour {
	
	public static int GUILayerMask = 1 << 12;
	
	public GUIContent[]	tiles;	//gui content tile tooltip must be the same name as the tile prefab 
	
	public GUIContent[]	hazards;	//gui content tile tooltip must be the same name as the tile prefab 
	
	private GameObject	currentTile;
	
	public EditorController controller;
	public CameraController	cameraController;
	
	private string			tilePath = "Tiles/";
	private string			hazardPath = "Hazards/";
	
	//button variables	
	private bool		pickingBuildType = false;
	
	private GUIContent[] contentToDisplay;
	
	private bool		selectionActive = false;	
	private int 		selectionGrid = 0;
	private int			newSelectionGrid = 0;
	
	private string 		exportMapName = "Map Name";
	private string		exportMapPath = "";
	private bool		mapExported = false;
	
	private string		mapFolderPath;
	
	
	//For map importing
	private bool		importActive = false;
	private int			numMaps = 0;
	private Vector2 	scrollPosition = Vector2.zero;
	string [] 			importMapPaths;
	string [] 			importMapNames;
	
	// Use this for initialization
	void Start () {
		mapFolderPath = Application.dataPath + "/MyMaps/";
		contentToDisplay = tiles;
		if (!Directory.Exists(mapFolderPath))
			Directory.CreateDirectory(mapFolderPath);
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
		CameraSwitchGUI();
		if (cameraController.mCameraType != cameraType_t.BUILDVIEW)
			return;
		switch (controller.buildMode){
			case buildMode_t.REGULAR:
			case buildMode_t.HAZARD:
				SelectionGUI();
				ExportGUI();
				ImportGUI();
				break;
			case buildMode_t.PATROL_POINTS:
				PatrolPointGUI();
				break;			
		}		
		
	}
	
	private void CameraSwitchGUI(){
		if (GUI.Button (new Rect (Screen.width/2+100, Screen.height-50, 150, 50), "Switch Camera")) {
			cameraController.SwitchMode();
		}
	}
	
	private void PatrolPointGUI(){
		GUI.Label(new Rect (Screen.width/2-70, Screen.height-75, 150, 25), "Placing Patrol Points");
		if (GUI.Button (new Rect (Screen.width/2-70, Screen.height-30, 150, 30), "Finish")){
			controller.SwitchMode(buildMode_t.REGULAR);
		}
	}
	
	private void SelectionGUI(){
		if (!selectionActive){
			if (GUI.Button (new Rect (Screen.width/2-70, Screen.height-50, 150, 50), "Switch Building Type")) {
				// This code is executed every frame that the Button is clicked
				selectionActive = true;
				pickingBuildType = true;
				selectionGrid = newSelectionGrid = -1;
			}
		}
		else{
			if (pickingBuildType){
				if (GUI.Button (new Rect (Screen.width/2-70, Screen.height-40, 60, 40), "Tiles")) {
					contentToDisplay = tiles;
					pickingBuildType = false;
					controller.SwitchMode(buildMode_t.REGULAR);
				}
				if (GUI.Button (new Rect (Screen.width/2, Screen.height-40, 60, 40), "Hazards")) {
					contentToDisplay = hazards;
					pickingBuildType = false;
					controller.SwitchMode(buildMode_t.HAZARD);
				}
			}
			else{
				newSelectionGrid = GUI.SelectionGrid (new Rect (Screen.width/2-110, Screen.height-100, 230, 100), newSelectionGrid, contentToDisplay, 2);
		
			}
		}
	}
	
	private void ExportGUI(){
		if (mapExported)
			GUI.Label(new Rect(10, 120, 150, 100),"Map Exported to " + exportMapPath);
		
		exportMapName = GUI.TextField(new Rect(10,10,100,30),exportMapName);
		if (GUI.Button (new Rect (10, 45, 80, 40), "Export Map")){
			if (exportMapName.Length < 1)
				return;
			exportMapPath = mapFolderPath + exportMapName + ".txt";
			if (exportMapPath.Length != 0){
        		TextWriter f = new StreamWriter(exportMapPath);
				
				foreach (Transform child in controller.mapParent.transform){
					
					if (child.name.Contains("(Clone)")){
						child.name = child.name.Replace("(Clone)","");
					}
					//if child has a tag(e.g. StartTile or EndTile - write it at the end)
					if (!child.tag.Equals("Untagged")){
						f.WriteLine(child.position.x + "\t" + child.position.y + "\t" + child.position.z + "\t" + child.name + "\t" + child.tag);
					}
					else
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
					else if (child.name.Equals("Wind")){
						Wind wind = child.gameObject.GetComponent<Wind>();	
						f.Write(wind.range + "," + wind.direction + "\n");
					}					
				}
				
				f.Close();
				f = null;
					
				Debug.Log("Map successfuly exported as: " + exportMapPath);
				mapExported = true;
				
			}	
		}
	}
	
	private void ImportGUI(){
		if (GUI.Button (new Rect (95, 45, 80, 40), "Import map")){
			//get any maps in the folder
			importMapPaths = Directory.GetFiles(mapFolderPath,"*.txt");
			numMaps = importMapPaths.Length;
			importMapNames = new string[numMaps];
			
			for (int i = 0; i < numMaps; i++){
				importMapNames[i] = Path.GetFileNameWithoutExtension(importMapPaths[i]);
			}       
			importActive = !importActive;
		}
		if (importActive){
			scrollPosition = GUI.BeginScrollView(new Rect(95, 90, 150, 60), scrollPosition, new Rect(0, 0, 85*numMaps, 70));
			for (int i = 0; i < numMaps; i++){
				if (GUI.Button(new Rect(80*i, 0, 80, 30), importMapNames[i])){
					controller.ImportMap (importMapPaths[i]);	
					importActive = false;
					exportMapName = importMapNames[i];
				}
			}
		    GUI.EndScrollView();
		}
	}
	
	public GameObject GetCurrentObject(){
		GameObject tile = Resources.Load(tilePath+contentToDisplay[selectionGrid].tooltip) as GameObject;
		if (tile == null){
			tile = Resources.Load(hazardPath+contentToDisplay[selectionGrid].tooltip) as GameObject;	
		}
		return tile;
	}
}
