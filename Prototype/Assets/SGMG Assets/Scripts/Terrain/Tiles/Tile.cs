using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {
	public bool				canBeBuiltOn;
	public string			hint;
	public bool				hintEnabled{get;set;}
	
	private GameObject 		placedObject;
	private bool		  	isCracked;
	private TileController	tileController;
	
	// Use this for initialization
	void Start () {
		if (this.tag.Equals("CrackedTile")){
			SetCracked(true);	
		}
		else
			isCracked = false;
		
		//get a reference to the tile controller - it is this tile's parent
		tileController = transform.parent.GetComponent<TileController>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnDestroy() {
		if (placedObject != null){
			Destroy(placedObject.gameObject);	
			placedObject = null;
		}
	}
	
	public bool TrySelectObject(){
		if (placedObject == null)
			return false;
		ITilePlaceable tilePlaceable = placedObject.GetComponent<ITilePlaceable>();
		if (tilePlaceable == null){
			return false;
		}
		tilePlaceable.Select();
		return true;
	}
	
	public bool TryDeselectObject(){
		if (placedObject == null)
			return false;
		ITilePlaceable tilePlaceable = placedObject.GetComponent<ITilePlaceable>();
		if (tilePlaceable == null){
			return false;
		}
		tilePlaceable.Deselect();
		return true;
	}
	
	public bool CanBeBuiltOn(){
		return canBeBuiltOn && placedObject == null;
	}
	
	public GameObject GetPlacedObject(){
		return placedObject;	
	}
	
	public bool PlaceObject(GameObject obj){
		if (!canBeBuiltOn)
			return false;
		placedObject = obj;
		if (isCracked){
			SetCracked(false);	
		}
		this.name = "UnWalkableTile";//mark this tile as unwalkable anymore
		if (tileController == null){
			tileController = transform.parent.GetComponent<TileController>();
		}
		this.renderer.material.mainTexture = tileController.unwalkable;
		return true;
	}
	
	public void DestroyPlacedObject(){
		if (!canBeBuiltOn)
			return;
		if (tileController == null){
			tileController = transform.parent.GetComponent<TileController>();
		}
		this.renderer.material.mainTexture = tileController.regular;	
		if (placedObject != null){
			Destroy(placedObject.gameObject);	
			placedObject = null;
		}
		this.name = "RegularTile";//mark this tile as unwalkable anymore
	}
	
	/// <summary>
	/// Use this if an object that was on this tile was moved and not necessarily destroyed
	/// </summary>
	public void ObjectMoved(){
		if (!canBeBuiltOn)
			return;
		if (tileController == null){
			tileController = transform.parent.GetComponent<TileController>();
		}
		this.renderer.material.mainTexture = tileController.regular;
		placedObject = null;
		this.name = "RegularTile";//mark this tile as unwalkable anymore
	}
	
	public bool GetCracked(){
		return isCracked;	
	}
	
	public bool ContainsHint(){
		return (hintEnabled && hint != null && hint.Length	> 0);
	}
	
	/// <summary>
	/// Sets the cracked state of this tile. WARNING: If this tile has an object on it, it cannot be cracked
	/// </summary>
	/// <param name='cracked'>
	/// Cracked.
	/// </param>
	public void SetCracked(bool cracked){
		if (placedObject != null){
			cracked = false;	
		}
		isCracked = cracked;
		if (isCracked){
			this.renderer.material.mainTextureScale = new Vector2(2.0f,2.0f);
			this.tag = "CrackedTile";
		}
		else{
			this.renderer.material.mainTextureScale = new Vector2(1.0f,1.0f);
			this.tag = "Untagged";
		}
	}
}
