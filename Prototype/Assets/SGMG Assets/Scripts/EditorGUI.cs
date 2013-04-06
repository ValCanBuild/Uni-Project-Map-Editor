using UnityEngine;
using System.IO;
using System.Collections;

public class EditorGUI : MonoBehaviour {
	
	public static int GUILayerMask = 1 << 12;
	
	public GUIContent[]	tiles;	//gui content tile tooltip must be the same name as the tile prefab 	
	public GUIContent[]	hazards;	//gui content tile tooltip must be the same name as the hazard prefab 
	public GUIContent[]	props;	
	
	private GameObject	currentTile;
	
	public EditorController controller;
	public CameraController	cameraController;
	
	//button variables	
	private bool		pickingBuildType = false;
	
	private GUIContent[] contentToDisplay;
	
	private bool		selectionActive = false;	
	private int 		selectionGrid = 0;
	private int			newSelectionGrid = 0;
	
	private string 		exportMapName = "Map Name";
	private string		exportMapPath = "";
	private bool		mapExported = false;	
	
	//For map importing
	private bool		importActive = false;
	private int			numMaps = 0;
	private Vector2 	scrollPosition = Vector2.zero;
	string [] 			importMapPaths;
	string [] 			importMapNames;
	
	// Use this for initialization
	void Start () {
		contentToDisplay = tiles;
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
		case buildMode_t.PROPS:
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
		if (GUI.Button (new Rect (Screen.width/2-70, 0, 150, 50), "Switch Camera")) {
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
				if (GUI.Button (new Rect (Screen.width/2-30, Screen.height-30, 60, 30), "Tiles")) {
					contentToDisplay = tiles;
					controller.SwitchMode(buildMode_t.REGULAR);
					pickingBuildType = false;
				}
				if (GUI.Button (new Rect (Screen.width/2-30, Screen.height-65, 60, 30), "Hazards")) {
					contentToDisplay = hazards;
					controller.SwitchMode(buildMode_t.HAZARD);
					pickingBuildType = false;
				}
				if (GUI.Button (new Rect (Screen.width/2-30, Screen.height-100, 60, 30), "Props")) {
					contentToDisplay = props;					
					controller.SwitchMode(buildMode_t.PROPS);
					pickingBuildType = false;
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
			exportMapPath = MapIO.ExportMap(exportMapName,controller.mapParent,controller.propParent);
			if (exportMapPath != null){
				mapExported = true;	
			}			
		}
	}
	
	private void ImportGUI(){
		if (GUI.Button (new Rect (95, 45, 80, 40), "Import map")){
			//get any maps in the folder
			importMapPaths = Directory.GetFiles(MapIO.GetMapFolderPath(),"*.txt");
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
		GameObject tile = Resources.Load(MapIO.GetTilePath()+contentToDisplay[selectionGrid].tooltip) as GameObject;
		if (tile == null){
			tile = Resources.Load(MapIO.GetHazardPath()+contentToDisplay[selectionGrid].tooltip) as GameObject;	
		}
		return tile;
	}
	
	public Texture GetCurrentPropTexture(){
		Texture tex = contentToDisplay[selectionGrid].image;
		return tex;
	}
	
	public Texture GetPropTextureForName(string name){
		foreach (GUIContent content in props){
			if (name.Contains(content.text)){
				return content.image;	
			}
		}
		return null;
	}
}
