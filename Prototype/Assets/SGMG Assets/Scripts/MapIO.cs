using System.Collections;
using System.IO;
using UnityEngine;

/// <summary>
/// Map IO - contains static functions for Map exporting/importing
/// </summary>
public class MapIO {
	
	private static string			tilePath = "Tiles/";
	private static string			hazardPath = "Hazards/";
	
	public static string GetTilePath(){
		return tilePath;	
	}
	
	public static string GetHazardPath(){
		return hazardPath;	
	}
	
	public static string GetMapFolderPath(){
		string mapFolderPath = Application.dataPath + "/MyMaps/";
			
		return mapFolderPath;
	}
	/// <summary>
	/// Exports the map with the given map name and mapHolder parent - returns the file path exported to
	/// </summary>
	/// <returns>
	/// The map.
	/// </returns>
	/// <param name='mapName'>
	/// Map name.
	/// </param>
	/// <param name='mapHolder'>
	/// Map holder
	/// </param>
	public static string ExportMap(string mapName, Transform mapHolder){
		string mapFolderPath = Application.dataPath + "/MyMaps/";
		if (!Directory.Exists(mapFolderPath))
			Directory.CreateDirectory(mapFolderPath);
		
		if (mapName.Length < 1)
			return null;
		
		string mapExportPath = mapFolderPath + mapName + ".txt";
		if (mapExportPath.Length != 0){
       		TextWriter f = new StreamWriter(mapExportPath);			
			foreach (Transform child in mapHolder){				
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
				
			Debug.Log("Map successfuly exported as: " + mapExportPath);	
			
			return mapExportPath;
		}
		return null;
	}
	
	/// <summary>
	/// Imports the map from the given map path and populates the mapHolder parent - returns success value
	/// </summary>
	/// <returns>
	/// The map.
	/// </returns>
	/// <param name='mapPath'>
	/// If set to <c>true</c> map path.
	/// </param>
	/// <param name='mapHolder'>
	/// If set to <c>true</c> map holder.
	/// </param>
	public static bool ImportMap(string mapPath, Transform mapHolder){
		TextReader r = new StreamReader(mapPath);
		
		if (r == null){
			return false;
		}		
		while (true){
			string line = r.ReadLine();
			if (line != null){
				string[] text = line.Split(new char[]{'\t'});
				//GET TILE POSITION AND ADD TO PARENT
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
				
				gameObject = MonoBehaviour.Instantiate(gameObject,position,Quaternion.identity) as GameObject;
				gameObject.transform.parent = mapHolder;
				if (objectTag != null){
					gameObject.tag = objectTag;
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
				}
			}
			else{
				break;	
			}
		}

		r.Close();
		r = null;
		Debug.Log("Map successfuly imported");
		return true;
		
	}
}
