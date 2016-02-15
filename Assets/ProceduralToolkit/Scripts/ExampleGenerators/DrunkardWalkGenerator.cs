using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrunkardWalkGenerator : MonoBehaviour {

	//http://www.roguebasin.com/index.php?title=Random_Walk_Cave_Generation

	public int Width = 50;
	public int Height = 50;

	[Tunable(MinValue: false, MaxValue: true, Name: "Use Center Bias")]
	public bool UseCenterBias = true;
	public bool UseLinearBias = true;

	public bool WalkersStartTogether = true;

	[Tunable(MinValue: 1, MaxValue: 10, Name: "Number Of Walkers")]
	public int NumberOfWalkers = 1;

	[Tunable(MinValue: 0.01f, MaxValue: 1f, Name: "Starting Area Size")]
	public float SizeOfStartingArea = 0.5f;

	[Tunable(MinValue: 1, MaxValue: 4000, Name: "Number Of Iterations")]
	public int NumberOfIterations = 1000;

	[Generator]
	public Tile[,] GenerateLevel(){
		Tile[,] res = new Tile[Width,Height];

		//Initially, the cave is entirely solid
		for(int i=0; i<Width; i++){
			for(int j=0; j<Height; j++){
				res[i,j] = new Tile(i, j, true);
			}
		}

		int numOpenTiles = 0;

		List<int[]> walkerPos = new List<int[]>();

		//Walker setup
		float xDiff = Width*(1-SizeOfStartingArea); float xPart = Width * SizeOfStartingArea;
		float yDiff = Height*(1-SizeOfStartingArea); float yPart = Height * SizeOfStartingArea;
		int dx = (int) Mathf.Floor(Random.Range(xDiff/2, xPart + xDiff/2));
		int dy = (int) Mathf.Floor(Random.Range(yDiff/2, yPart + yDiff/2));

		for(int i=0; i<NumberOfWalkers; i++){
			//Pick a random point
			if(!WalkersStartTogether){
				xDiff = Width*(1-SizeOfStartingArea); xPart = Width * SizeOfStartingArea;
				yDiff = Height*(1-SizeOfStartingArea); yPart = Height * SizeOfStartingArea;
				dx = (int) Mathf.Floor(Random.Range(xDiff/2, xPart + xDiff/2));
				dy = (int) Mathf.Floor(Random.Range(yDiff/2, yPart + yDiff/2));
			}
			walkerPos.Add(new int[]{dx, dy});
		}

		for(int i=0; i<NumberOfIterations; i++){
			for(int j=0; j<NumberOfWalkers; j++){
				dx = walkerPos[j][0];
				dy = walkerPos[j][1];

				//Turn the tile into empty space
				if(res[dx,dy].BLOCKS_MOVEMENT){
					numOpenTiles++;
					res[dx,dy].BLOCKS_MOVEMENT = false;
				}

				//Move the drunkard
				int[] rm = SelectMovement(dx, dy, UseCenterBias);

				walkerPos[j][0] += rm[0];
				walkerPos[j][1] += rm[1];
			}
			i += NumberOfWalkers-1;
		}

		return res;
	}

	public int[] SelectMovement(int dx, int dy, bool bias = false){
		int[] res = movements[Random.Range(0,movements.Count)];

		if(bias && !UseLinearBias){
			float[] chanceToSelect = new float[]{0.25f,0.25f,0.25f,0.25f};
			if(dx < Width/2){
				chanceToSelect[0] = 0.33f; chanceToSelect[1] = 0.5f;
			}
			else if(dx > Width/2){
				chanceToSelect[0] = 0.17f; chanceToSelect[1] = 0.5f;
			}
			if(dy < Height/2){
				chanceToSelect[2] = 0.5f+0.33f; chanceToSelect[3] = 1f;
			}
			else if(dy > Height/2){
				chanceToSelect[2] = 0.5f+0.17f; chanceToSelect[3] = 1f;
			}

			do{
				float rng = Random.Range(0f, 1f);
				for(int i=0; i<4; i++){
					if(rng < chanceToSelect[i]){
						res = movements[i];
						break;
					}
				}
			}
			while(!(dx+res[0] >= 0 && dx+res[0] < Width && dy+res[1] >= 0 && dy+res[1] < Height));

		}
		else if(bias && UseLinearBias){
			float[] chanceToSelect = new float[]{0.25f,0.25f,0.25f,0.25f};

			chanceToSelect[0] = 0.5f-((float)dx/(float)Width)/2f;
			chanceToSelect[1] = 0.5f;

			chanceToSelect[2] = 0.5f+(0.5f-(((float)dy/(float)Height)/2f));
			chanceToSelect[3] = 1f;

			// Debug.Log(chanceToSelect[0]+","+chanceToSelect[1]+","+chanceToSelect[2]+","+chanceToSelect[3]+",");

			do{
				float rng = Random.Range(0f, 1f);
				for(int i=0; i<4; i++){
					if(rng < chanceToSelect[i]){
						res = movements[i];
						break;
					}
				}
			}
			while(!(dx+res[0] >= 0 && dx+res[0] < Width && dy+res[1] >= 0 && dy+res[1] < Height));
		}
		else{
			res = movements[Random.Range(0,movements.Count)];

			while(!(dx+res[0] >= 0 && dx+res[0] < Width && dy+res[1] >= 0 && dy+res[1] < Height)){
				res = movements[Random.Range(0,movements.Count)];
			}
		}
		return res;
	}

	List<int[]> movements = new List<int[]>();

	void Start(){

		movements.Add(new int[]{1,0});
		movements.Add(new int[]{-1,0});
		movements.Add(new int[]{0,1});
		movements.Add(new int[]{0,-1});

	}

}
