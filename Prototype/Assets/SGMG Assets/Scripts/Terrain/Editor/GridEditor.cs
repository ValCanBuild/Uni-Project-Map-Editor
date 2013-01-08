using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

[CustomEditor (typeof(Grid))]
public class GridEditor : Editor {
	
	Grid 				grid;//reference to the grid object
	static Hashtable 	table;
	
	public void OnEnable()
    {
        grid = target as Grid;
		if (table == null)
			table = new Hashtable();
		
		
		//add delegate
		SceneView.onSceneGUIDelegate = GridUpdate;
    }

	
	public override void OnInspectorGUI()
    {
		GUILayout.BeginHorizontal();
		GUILayout.Label( " Grid Size ");
		grid.size = EditorGUILayout.FloatField(grid.size);
    	GUILayout.EndHorizontal();		

		
		if (GUILayout.Button("Open Grid Window"))
	    {   
	       GridWindow window = (GridWindow) EditorWindow.GetWindow(typeof(GridWindow));
	       window.Init();
		}
		
		if (GUILayout.Button("RePopulate HashMap"))
	    {   
			table.Clear();
	      	foreach (Transform child in grid.mapParent){
				table.Add (child.position,child.position);	
			}
		}
		
		if (GUILayout.Button("Export Map")){   
			ExportMapData();
	      	
		}
		
		if (GUILayout.Button("Import Map")){   
			ImportMapData();
		}
		
		SceneView.RepaintAll();
    }

	void GridUpdate(SceneView sceneview){
		
		Handles.color = Color.blue;
		Event e = Event.current;
		Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, -e.mousePosition.y + Camera.current.pixelHeight));
		Vector3 mousePos = r.origin;
		
		if (e.isKey && e.character == 'a')
    	{
        	GameObject obj;
			Object prefab = PrefabUtility.GetPrefabParent(Selection.activeObject);
			if (prefab)
	        {
				Vector3 pos = GetMousePosInGrid(mousePos);
				if (table.Contains(pos)){
					return;	
				}
				else{
					Undo.IncrementCurrentEventIndex();
		            obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);					
		            obj.transform.position = pos;
		        	obj.transform.parent = grid.mapParent.transform;
					table.Add(obj.transform.position,obj.transform.position);
					
					Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name); 
				}
			}
    	}
		else if (e.isKey && e.character == 'd')
		{
			Undo.IncrementCurrentEventIndex();
    		Undo.RegisterSceneUndo("Delete Selected Objects");
		    foreach (GameObject obj in Selection.gameObjects){
				table.Remove(obj.transform.position);
		        DestroyImmediate(obj);
				
			}
		}
		//if alt is held - display the coordinates of the current grid item
		else if (e.isKey && e.control){
			Vector3 pos = GetMousePosInGrid(mousePos);
			Debug.Log("Coord X: " + pos.x + ", Coord Z: " + pos.z);
		}
	}
	
	Vector3 GetMousePosInGrid(Vector3 mousePos){
		float xCoord = Mathf.Floor(mousePos.x/grid.size)*grid.size + grid.size/2.0f;
		float zCoord = Mathf.Floor(mousePos.z/grid.size)*grid.size + grid.size/2.0f;
		return new Vector3(xCoord,0.0f,zCoord);	
	}
	
	private void ExportMapData(){
		string path = EditorUtility.SaveFilePanel("Choose save location","","map","txt");
		if (path.Length != 0){
        	TextWriter f = new StreamWriter(path);
			
			foreach (Transform child in grid.mapParent.transform){
				
				f.WriteLine(child.position.x + "\t" + child.position.y + "\t" + child.position.z + "\t" + child.name);
				/*if (child.name.Equals("MovingTile")){
					MovingTile movingTile = child.gameObject.GetComponent<MovingTile>();
					//if moving tile has any patrol points - write them to a new line
					if (movingTile.movementPoints.Length > 1){
						for (int i = 0; i < movingTile.movementPoints.Length; i++){							
							f.Write(movingTile.movementPoints[i].x + "," + movingTile.movementPoints[i].y + "," + movingTile.movementPoints[i].z);
							if (i != movingTile.movementPoints.Length-1){
								f.Write("\t");	
							}							
						}
					}
					//else if it hasn't any set - just put it's original place as the one point
					else{
						f.Write(child.position.x + "," + child.position.y + "," + child.position.z);
					}
					f.WriteLine();
				}*/
			}
			
			f.Close();
			f = null;
				
			Debug.Log("Map successfuly exported as: " + path);
		}
	}
	
	private void ImportMapData(){
		string path = EditorUtility.OpenFilePanel("Choose map location","","txt");
		if (path.Length != 0){				
			TextReader r = new StreamReader(path);
			
			//clear table and destroy all tiles in the parent
			table.Clear();
			for (int i = 0; i < grid.mapParent.transform.childCount; i++){
				Transform child = grid.mapParent.transform.GetChild(i);
				table.Remove(child.position);
				DestroyImmediate(child.gameObject);				
			}
			
			table.Clear();
			
			while (true)
            {
	            string line = r.ReadLine();
	            if (line != null){
					string[] text = line.Split(new char[]{'\t'});
					//GET TILE POSITION AND ADD TO PARENT AND HASH TABLE
					Vector3 position = new Vector3(float.Parse(text[0]),float.Parse(text[1]),float.Parse(text[2]));
					string tileType = text[3];
					
					string assetName = "Assets/SGMG Assets/Prefabs/Tiles/" + tileType + ".prefab";

					GameObject tileObject = AssetDatabase.LoadAssetAtPath(assetName, typeof(Object)) as GameObject;
					tileObject = PrefabUtility.InstantiatePrefab(tileObject) as GameObject;
					tileObject.transform.position = position;
					tileObject.transform.parent = grid.mapParent.transform;
					table.Add(tileObject.transform.position,tileObject.transform.position);
					
					//IF TILE HAS ANY SPECIFIC PROPERTIES - ADD THEM HERE
					//if moving tile - add patrol coordinates to it - they are on the next line
					/*if (tileType.Equals("MovingTile")){
						MovingTile movingTile = tileObject.GetComponent<MovingTile>();
						line = r.ReadLine();
						text = line.Split(new char[]{'\t'});
						movingTile.movementPoints = new Vector3[text.Length];
						for (int i = 0; i < text.Length; i++){
							string vector = text[i];
							string[] xyz = vector.Split(new char[]{','});
							movingTile.movementPoints[i] = new Vector3(float.Parse(xyz[0]),float.Parse(xyz[1]),float.Parse(xyz[2]));
						}
					}*/
	            }
				else{
					break;	
				}
			}
		
			
			r.Close();
			r = null;
			Debug.Log("Map successfuly imported from: " + path);
		}	
	}
}
