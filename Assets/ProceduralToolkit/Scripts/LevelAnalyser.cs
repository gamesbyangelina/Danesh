using UnityEngine;
using System.Collections;

public class LevelAnalyser : MonoBehaviour {

	// [Metric("Randomness")]
	public float RandomMetric(Tile[,] map){
		return Random.Range(0f, 1f);
	}

	/*
		Density is the proportion of tiles which are solid. 
	*/
	[Metric("Density")]
	public float CalculateDensity(Tile[,] map){
		int totalTiles = map.GetLength(0) * map.GetLength(1);
		int solidTiles = 0;
		for(int i=0; i<map.GetLength(0); i++){
			for(int j=0; j<map.GetLength(1); j++){
				if(map[i,j].BLOCKS_MOVEMENT)
					solidTiles++;
			}
		}
		return (float)solidTiles/(float)totalTiles;
	}

	/*
		Openness is the proportion of tiles which have no solid neighbours. 
		For this purpose, tiles which are on the edge of the map are not considered solid.
	*/
	[Metric("Openness")]
	public float CalculateOpenness(Tile[,] map){
		int totalTiles = 0;//map.GetLength(0) * map.GetLength(1);
		int openTiles = 0;
		for(int i=0; i<map.GetLength(0); i++){
			for(int j=0; j<map.GetLength(1); j++){
				if(!map[i,j].BLOCKS_MOVEMENT && !HasBlockingNeighbour(map, i, j))
					openTiles++;
				if(!map[i,j].BLOCKS_MOVEMENT)
					totalTiles++;
			}
		}
		return (float)openTiles/(float)totalTiles;
	}

	/*
		Counts the tiles adjacent to (x,y) that are solid. Considers edge tiles
		to be next to solid tiles depending on the TreatMapEdgesAsSolid param.
	*/
	bool HasBlockingNeighbour(Tile[,] map, int x, int y){
		for(int i=-1; i<2; i++){
			for(int j=-1; j<2; j++){
				//Don't count yourself
				if(i == 0 && j == 0)
					continue;

				int dx = x+i; int dy = y+j;
				if(dx < 0 || dx >= map.GetLength(0) || dy < 0 || dy >= map.GetLength(1)){
					continue;
				}
				else{
					if(map[dx,dy].BLOCKS_MOVEMENT)
						return true;
				}
			}
		}
		return false;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
