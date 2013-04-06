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
	/// Exports the map with the given map name and mapHolder and propHolder parents - returns the file path exported to
	/// </summary>
	/// <returns>
	/// The map.
	/// </returns>
	/// <param name='mapName'>
	/// Map name.
	/// </param>
	/// <param name='mapHolder'>
	/// Map holder.
	/// </param>
	/// <param name='propHolder'>
	/// Prop holder.
	/// </param>
	public static string ExportMap(string mapName, Transform mapHolder, Transform propHolder){
		//Get map folder path and create folder if it does not exist
		string mapFolderPath = Application.dataPath + "/MyMaps/";
		if (!Directory.Exists(mapFolderPath))
			Directory.CreateDirectory(mapFolderPath);
		
		if (mapName.Length < 1)
			return null;
		
		string mapExportPath = mapFolderPath + mapName + ".txt";
		if (mapExportPath.Length != 0){
       		TextWriter f = new StreamWriter(mapExportPath);	
			//go through tiles and hazards
			foreach (Transform child in mapHolder){				
				if (child.name.Contains("(Clone)")){
					child.name = child.name.Replace("(Clone)","");
				}
				//Go through the tile properties of the game object(if it has any) and export them if necessary
				Tile tile = child.GetComponent<Tile>();
				if (tile != null){
					//export tile hint if any
					if (tile.ContainsHint()){
						child.tag = "HintTile";//add tag appropriately
					}
				}	
				//if child has a tag(e.g. StartTile or EndTile - write it at the end)
				if (!child.tag.Equals("Untagged")){
					f.WriteLine(child.position.x + "\t" + child.position.y + "\t" + child.position.z + "\t" + child.name + "\t" + child.tag);
					//if a hint tile, write out hint as well
					if (tile.ContainsHint()){
						f.WriteLine(tile.hint);	
					}
				}
				else{
					f.WriteLine(child.position.x + "\t" + child.position.y + "\t" + child.position.z + "\t" + child.name);
				}
				
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
			f.WriteLine("PROPS");//indicate that we are switching to props
			//go through props
			foreach (Transform	prop in propHolder){
				if (prop.name.Contains("(Clone)")){
					prop.name = prop.name.Replace("(Clone)","");
				}
				//for each prop write position, rotation and scale
				f.WriteLine(prop.position.x + "\t" + prop.position.y + "\t" + prop.position.z + "\t" + 
							prop.rotation.eulerAngles.x + "\t" + prop.rotation.eulerAngles.y + "\t" + prop.rotation.eulerAngles.z + "\t" + 
							prop.localScale.x + "\t" + prop.localScale.y + "\t" + prop.localScale.z + "\t" + prop.name);
			}
			
			f.Close();
			f = null;
				
			Debug.Log("Map successfuly exported as: " + mapExportPath);	
			
			return mapExportPath;
		}
		return null;
	}
	
	/// <summary>
	/// Imports the map from the given map path and populates the mapHolder and propHolder parents - returns success value
	/// </summary>
	/// <returns>
	/// The map.
	/// </returns>
	/// <param name='mapPath'>
	/// Where to stream the map from
	/// </param>
	/// <param name='mapHolder'>
	/// MapHolder transform to put the tiles into
	/// </param>
	/// <param name='propHolder'>
	/// PropHolder transform to put the props into
	/// </param>
	public static bool ImportMap(string mapPath, Transform mapHolder, Transform propHolder){
		TextReader r = new StreamReader(mapPath);
		
		if (r == null){
			return false;
		}		
		bool tileMode = true; //indicates what mode we are on - tiles or props
		while (true){
			string line = r.ReadLine();
			if (line != null){
				if (line.Equals("PROPS")){
					tileMode = false;					
				}
				//tile mode
				else if (tileMode){
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
					//if hint tile - add hint to it
					if (gameObject.tag.Equals("HintTile")){
						Tile tile = gameObject.GetComponent<Tile>();
						tile.hintEnabled = true;
						tile.hint = r.ReadLine();
					}
					
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
				//prop mode
				else{
					string[] text = line.Split(new char[]{'\t'});
					//GET PROP POSITION,ROTAION AND SCALE AND ADD TO PARENT
					Vector3 position = new Vector3(float.Parse(text[0]),float.Parse(text[1]),float.Parse(text[2]));
					Vector3 rotation = new Vector3(float.Parse(text[3]),float.Parse(text[4]),float.Parse(text[5]));
					Vector3 scale 	 = new Vector3(float.Parse(text[6]),float.Parse(text[7]),float.Parse(text[8]));
					string propType = text[9];
					
					GameObject propObject;
					Quaternion rot = Quaternion.identity;
					rot.eulerAngles = rotation;
					propObject = Resources.Load("2DAsset") as GameObject;
					propObject = MonoBehaviour.Instantiate(propObject,position,rot) as GameObject;
					propObject.transform.parent = propHolder;
					propObject.transform.localScale = scale;
					propObject.name = propType;
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
