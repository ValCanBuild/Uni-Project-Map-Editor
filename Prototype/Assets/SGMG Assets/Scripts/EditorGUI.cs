using UnityEngine;
using System.Collections;

public class EditorGUI : MonoBehaviour {
	
	public GUIContent[]	tiles;	//gui content tile tooltip must be the same name as the tile prefab 
	
	private GameObject	currentTile;
	
	//button variables
	private int			buttonWidth = 100;
	private int			buttonHeight = 100;
	private int 		boxHeight = 100;
	private int			boxWidth = 100;
	
	private bool		selectionActive = false;
	private int 		selectionGrid = 0;
	private int			newSelectionGrid = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (selectionGrid != newSelectionGrid){
			selectionGrid = newSelectionGrid;
			selectionActive = false;
		}
	}
	
	void OnGUI() {
		if (!selectionActive){
			if (GUI.Button (new Rect (Screen.width/2-70, Screen.height-50, 150, 50), "Switch Building Type")) {
				// This code is executed every frame that the Button is clicked
				selectionActive = !selectionActive;
			}
		}
		else{
			newSelectionGrid = GUI.SelectionGrid (new Rect (Screen.width/2-110, Screen.height-100, 230, 100), newSelectionGrid, tiles, 2);
		}
	}
	
	public GameObject GetCurrentTile(){
		GameObject tile = Resources.Load(tiles[selectionGrid].tooltip) as GameObject;
		return tile;
	}
}
