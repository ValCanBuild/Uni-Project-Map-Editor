using UnityEngine;
using System.Collections;

public enum tileType_t{INNER,EDGE_N,EDGE_S,EDGE_E,EDGE_W,EDGE_NS,EDGE_WE,CORNER_NE,CORNER_NW,CORNER_SE,CORNER_SW}

public class TileController : MonoBehaviour {
	
	public Texture		regular;
	public Texture		regularEdge;
	public Texture 		regularDoubleEdge;
	public Texture		regularCorner;
	
	public Texture		unwalkable;
	public Texture		unwalkableEdge;
	public Texture 		unwalkableDoubleEdge;
	public Texture		unwalkableCorner;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void OrganizeTiles(){
		foreach (Transform tile in transform){
			if ((!tile.name.Contains("RegularTile")) && !tile.name.Contains("UnWalkableTile"))
				continue;
			
			switch (GetTileType(tile)){
			case tileType_t.INNER:
				if (tile.name.Contains("RegularTile")){
					tile.renderer.material.mainTexture = regular;
				}
				else {
					tile.renderer.material.mainTexture = unwalkable;	
				}
				break;
				
			case tileType_t.CORNER_SW:
				if (tile.name.Contains("RegularTile")){
					tile.renderer.material.mainTexture = regularCorner;
				}
				else {
					tile.renderer.material.mainTexture = unwalkableCorner;	
				}
				break;			
			case tileType_t.CORNER_SE:
				if (tile.name.Contains("RegularTile")){
					tile.renderer.material.mainTexture = regularCorner;
				}
				else {
					tile.renderer.material.mainTexture = unwalkableCorner;	
				}
				tile.rotation = Quaternion.AngleAxis(90,Vector3.up);
				break;
			case tileType_t.CORNER_NW:
				if (tile.name.Contains("RegularTile")){
					tile.renderer.material.mainTexture = regularCorner;
				}
				else {
					tile.renderer.material.mainTexture = unwalkableCorner;	
				}
				tile.rotation = Quaternion.AngleAxis(270,Vector3.up);
				break;
			case tileType_t.CORNER_NE:
				if (tile.name.Contains("RegularTile")){
					tile.renderer.material.mainTexture = regularCorner;
				}
				else {
					tile.renderer.material.mainTexture = unwalkableCorner;	
				}
				tile.rotation = Quaternion.AngleAxis(180,Vector3.up);
				break;
			
			case tileType_t.EDGE_S:
				if (tile.name.Contains("RegularTile")){
					tile.renderer.material.mainTexture = regularEdge;
				}
				else {
					tile.renderer.material.mainTexture = unwalkableEdge;	
				}
				break;
			case tileType_t.EDGE_N:
				if (tile.name.Contains("RegularTile")){
					tile.renderer.material.mainTexture = regularEdge;
				}
				else {
					tile.renderer.material.mainTexture = unwalkableEdge;	
				}
				tile.rotation = Quaternion.AngleAxis(180,Vector3.up);
				break;
			case tileType_t.EDGE_E:
				if (tile.name.Contains("RegularTile")){
					tile.renderer.material.mainTexture = regularEdge;
				}
				else {
					tile.renderer.material.mainTexture = unwalkableEdge;	
				}
				tile.rotation = Quaternion.AngleAxis(270,Vector3.up);
				break;
			case tileType_t.EDGE_W:
				if (tile.name.Contains("RegularTile")){
					tile.renderer.material.mainTexture = regularEdge;
				}
				else {
					tile.renderer.material.mainTexture = unwalkableEdge;	
				}
				tile.rotation = Quaternion.AngleAxis(90,Vector3.up);
				break;
				
			case tileType_t.EDGE_NS:
				if (tile.name.Contains("RegularTile")){
					tile.renderer.material.mainTexture = regularDoubleEdge;
				}
				else {
					tile.renderer.material.mainTexture = unwalkableDoubleEdge;	
				}
				break;	
			case tileType_t.EDGE_WE:
				if (tile.name.Contains("RegularTile")){
					tile.renderer.material.mainTexture = regularDoubleEdge;
				}
				else {
					tile.renderer.material.mainTexture = unwalkableDoubleEdge;	
				}
				tile.rotation = Quaternion.AngleAxis(90,Vector3.up);
				break;	
			}			
		}
	}
	
	private tileType_t GetTileType(Transform tile){
		//have to check north,south,west and east for a tile
		//if there is none, this is an inner tile
		tileType_t tileType = tileType_t.INNER;
		
		Vector3 tilePos = tile.position;
		Vector3 posN = tile.position + new Vector3(0,0,1);
		Vector3 posS = tile.position + new Vector3(0,0,-1);
		Vector3 posE = tile.position + new Vector3(-1,0,0);
		Vector3 posW = tile.position + new Vector3(1,0,0);
		
		bool tileN,tileS,tileE,tileW;
		tileN = TileExistsAtPos(posN);
		tileS = TileExistsAtPos(posS);
		tileE = TileExistsAtPos(posE);
		tileW = TileExistsAtPos(posW);
		
		//Inner Tile
		if (tileN && tileS && tileE && tileW){
			tileType = tileType_t.INNER;	
		}
		
		//Edge Tile with edge on east and west sides
		else if (tileN && tileS && !tileE && !tileW){
			tileType = tileType_t.EDGE_WE;
		}
		//Edge Tile with edge on east side
		else if (tileN && tileS && tileE && !tileW){
			tileType = tileType_t.EDGE_E;
		}
		//Edge Tile with edge on west side
		else if (tileN && tileS && !tileE && tileW){
			tileType = tileType_t.EDGE_W;
		}
		//Edge Tile with edge on north and south side
		else if (!tileN && !tileS && tileE && tileW){
			tileType = tileType_t.EDGE_NS;
		}
		//Edge Tile with edge on north side
		else if (!tileN && tileS && tileE && tileW){
			tileType = tileType_t.EDGE_N;	
		}
		//Edge Tile with edge on south side
		else if (tileN && !tileS && tileE && tileW){
			tileType = tileType_t.EDGE_S;	
		}
		
		//Corner Tile with corner on north-east side
		else if (!tileN && tileS && !tileE && tileW){
			tileType = tileType_t.CORNER_NE;
		}
		//Corner Tile with corner on south-east side
		else if (tileN && !tileS && !tileE && tileW){
			tileType = tileType_t.CORNER_SE;
		}
		//Corner Tile with corner on north-west side
		else if (!tileN && tileS && tileE && !tileW){
			tileType = tileType_t.CORNER_NW;
		}
		//Corner Tile with corner on south-west side
		else if (tileN && !tileS && tileE && !tileW){
			tileType = tileType_t.CORNER_SW;
		}
		
		return tileType;
	}
	
	private bool TileExistsAtPos(Vector3 pos){
		foreach (Transform tile in transform){
			if (tile.position == pos)
				return true;
		}
		return false;
	}
}
