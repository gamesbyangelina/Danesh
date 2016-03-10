using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelAnalyser : MonoBehaviour {

	// [Metric("Randomness")]
	public static float RandomMetric(object map){
		return Random.Range(0f, 1f);
	}

	[Metric("Connectedness")]
	public static float CalculateConnectedness(object _map){

		if(_map == null || !(_map is Tile[,])){
			Debug.Log("Failed connectedness - null map");
			return 0f;
		}

		Tile[,] map = (Tile[,]) _map;

		int totalOpenTiles = 0;

		bool[,] marked = new bool[map.GetLength(0), map.GetLength(1)];
		List<int[]> nbs = new List<int[]>();
		nbs.Add(new int[]{-1, 0});
		nbs.Add(new int[]{1, 0});
		nbs.Add(new int[]{0, -1});
		nbs.Add(new int[]{0, 1});

		float largestOpenArea = 0;

		for(int i=0; i<map.GetLength(0); i++){
			for(int j=0; j<map.GetLength(1); j++){
				if(!map[i,j].BLOCKS_MOVEMENT){
					if(!marked[i,j]){
						float thisAreaSize = 0;
						List<int[]> open = new List<int[]>();
						List<int[]> visited = new List<int[]>();
						open.Add(new int[]{i,j});
						int count = 0;

						while(open.Count > 0){
							thisAreaSize++;
							totalOpenTiles++;
							int[] p = open[0];
							marked[p[0], p[1]] = true;

							visited.Add(p);
							open.Remove(open[0]);

							foreach(int[] nb in nbs){
								int dx = p[0] + nb[0];
								int dy = p[1] + nb[1];
								if(dx >= 0 && dy >= 0 && dx < map.GetLength(0) && dy < map.GetLength(1)){
									bool seen = false;

									foreach(int[] v in open){
										if(v[0] == dx && v[1] == dy){
											seen = true;
											break;
										}
									}
									if(!seen && !marked[dx,dy] && !map[dx,dy].BLOCKS_MOVEMENT){
										open.Add(new int[]{dx, dy});
									}
								}
							}
						}
						if(thisAreaSize > largestOpenArea)
							largestOpenArea = thisAreaSize;
					}
				}
			}
		}

		return ((float) largestOpenArea)/((float)totalOpenTiles);
	}

	/*
		Density is the proportion of tiles which are solid.
	*/
	[Metric("Density")]
	public static float CalculateDensity(object _map){
		if(_map == null || !(_map is Tile[,])){
			Debug.Log("Failed density - null map");
			return 0f;
		}

		Tile[,] map = (Tile[,]) _map;

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
	public static float CalculateOpenness(object _map){
		if(_map == null || !(_map is Tile[,])){
			Debug.Log("Failed openness - null map");
			return 0f;
		}

		Tile[,] map = (Tile[,]) _map;

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

	[Metric("Wall Distribution")]
    public static float ProportionalWallClustering(object _map){
    	if(_map == null || !(_map is Tile[,])){
			Debug.Log("Failed connectedness - null map");
			return 0f;
		}

		Tile[,] map = (Tile[,]) _map;

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
		return (float)(totalTiles-openTiles)/(float)totalTiles;
    }

    static bool HasOpenNeighbour(Tile[,] map, int x, int y){
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
					if(!map[dx,dy].BLOCKS_MOVEMENT)
						return true;
				}
			}
		}
		return false;
    }

	/*
		Counts the tiles adjacent to (x,y) that are solid. Considers edge tiles
		to be next to solid tiles depending on the TreatMapEdgesAsSolid param.
	*/
	static bool HasBlockingNeighbour(Tile[,] map, int x, int y){
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
