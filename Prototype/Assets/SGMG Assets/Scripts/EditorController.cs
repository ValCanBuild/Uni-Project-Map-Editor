using UnityEngine;
using System.Collections;

public class EditorController : MonoBehaviour
{
	public float		size = 1.0f;	
	
	public Transform 	mapParent;
	
	public EditorGUI	guiController;
	
	private GameObject	tileToPlace;
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
		if (Input.GetKey(KeyCode.A)){
			PlaceTile();	
		}
		if (Input.GetKey(KeyCode.D)){
			DeleteTile();	
		}
	}
	
	void PlaceTile(){		
		Vector3 pos = GetMousePosInGrid();
		if (table.Contains(pos))
			return;
		GameObject newTile = guiController.GetCurrentTile();
		newTile = Instantiate(newTile,pos,Quaternion.identity) as GameObject;
		newTile.transform.parent = mapParent;
		table.Add(newTile.transform.position,newTile);
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
	
	Vector3 GetMousePosInGrid(){		
		Ray r = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
		Vector3 mousePos = r.origin;
		float xCoord = Mathf.Floor(mousePos.x/size)*size + size/2.0f;
		float zCoord = Mathf.Floor(mousePos.z/size)*size + size/2.0f;
		return new Vector3(xCoord,0.0f,zCoord);	
	}
}

