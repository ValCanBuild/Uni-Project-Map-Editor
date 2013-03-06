using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	public GameObject 		hazardObject{get;set;}
	private bool		  	isCracked;
	
	// Use this for initialization
	void Start () {
		if (this.tag.Equals("CrackedTile")){
			SetCracked(true);	
		}
		else
			isCracked = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnDestroy() {
		if (hazardObject != null){
			Destroy(hazardObject);	
			hazardObject = null;
		}
	}
	
	public bool GetCracked(){
		return isCracked;	
	}
	
	public void SetCracked(bool cracked){
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
