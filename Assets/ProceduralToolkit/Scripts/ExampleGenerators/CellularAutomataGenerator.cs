using UnityEngine;
using System.Collections;

public class CellularAutomataGenerator : MonoBehaviour {

	[Tunable(MinValue: 0.2f, MaxValue: 0.5f, Name:"Initial Spawn Chance")]
	public float ChanceTileWillSpawnAlive = 0.45f;

	[Tunable(MinValue: 0, MaxValue: 8, Name: "No. Of Iterations")]
	public int NumberOfIterations = 5;

	[Tunable(MinValue: 2, MaxValue: 5, Name: "Death Limit")]
	public int StarvationLowerLimit = 2;

	[Tunable(MinValue: 2, MaxValue: 5, Name: "Birth Limit")]
	public int BirthCount = 3;

	public int Width = 40;
	public int Height = 40;
	public Vector3 CubeSize = new Vector3(1f, 1f, 1f);
	public Vector3 FloorSize = new Vector3(1f, 1f, 1f);

	public GameObject WallPiece;
	public bool GenerateFloor = false;
	public GameObject FloorPiece;

	[Tooltip("Setting this to false will cause the edge of the map to become rounded over time.")]
	public bool TreatMapEdgesAsSolid = true;

	GameObject mapSprite;

	[Generator]
	public Tile[,] GenerateLevel(){

		Tile[,] map = new Tile[Width, Height];

		/*
			Randomly initialise the map based on the ChanceSpawnAlive parameter
		*/
		for(int i=0; i<Width; i++){
			for(int j=0; j<Height; j++){
				if(Random.Range(0f, 1f) < ChanceTileWillSpawnAlive)
					map[i,j] = new Tile(i, j, true);
				else
					map[i,j] = new Tile(i, j, false);
			}
		}

		/*
			Iterate through the map a set number of times, updating each tile
			according to the rules of the cellular automata parameters
		*/
		for(int iter=0; iter<NumberOfIterations; iter++){
			//For each iteration, create a new map that will store the new data
			Tile [,] nextMap = new Tile[Width, Height];
			for(int i=0; i<Width; i++){
				for(int j=0; j<Height; j++){
					//A tile is updated based on its neighbours.
					int NumberOfNeighbours = CountBlockingNeighbours(map, i, j);
					//'Live' cells become dead (empty) if they are too isolated.
					if(map[i,j].BLOCKS_MOVEMENT){
						if(NumberOfNeighbours <= StarvationLowerLimit)
							nextMap[i,j] = new Tile(i, j, false);
						else
							nextMap[i,j] = new Tile(i, j, true);
					}
					//Dead cells become alive (solid) if they are surrounded by enough live cells
					else{
						if(NumberOfNeighbours >= BirthCount)
							nextMap[i,j] = new Tile(i, j, true);
						else
							nextMap[i,j] = new Tile(i, j, false);
					}
				}
			}
			//Update the map to point to the next iteration, then continue;
			map = nextMap;
		}

		return map;
	}

	/*
		Counts the tiles adjacent to (x,y) that are solid. Considers edge tiles
		to be next to solid tiles depending on the TreatMapEdgesAsSolid param.
	*/
	public int CountBlockingNeighbours(Tile[,] map, int x, int y){
		int count = 0;
		for(int i=-1; i<2; i++){
			for(int j=-1; j<2; j++){
				//Don't count yourself
				if(i == 0 && j == 0)
					continue;

				int dx = x+i; int dy = y+j;
				if(dx < 0 || dx >= map.GetLength(0) || dy < 0 || dy >= map.GetLength(1)){
					if(TreatMapEdgesAsSolid)
						count++;
					continue;
				}
				else{
					if(map[dx,dy].BLOCKS_MOVEMENT)
						count++;
				}
			}
		}
		return count;
	}

	[Visualiser]
	public Texture2D RenderMap(object _m, Texture2D tex){
		Tile[,] map = (Tile[,]) _m;
        int sf = 10; int Width = map.GetLength(0); int Height = map.GetLength(1);

        for(int i=0; i<Width; i++){
            for(int j=0; j<Height; j++){
                if(map[i,j].BLOCKS_MOVEMENT){
                    VisUtils.PaintPoint(tex, i, j, sf, Color.black);
                }
                else{
                    VisUtils.PaintPoint(tex, i, j, sf, Color.white);
                }
            }
        }

         tex.Apply();
         return tex;
    }

	void Awake(){
		mapSprite = GameObject.Find("MapSprite");
	}

	// Use this for initialization
	void Start () {
		// GameObject.Find("WidthInTiles").GetComponent<UnityEngine.UI.InputField>().text = Width+"";
		// GameObject.Find("HeightInTiles").GetComponent<UnityEngine.UI.InputField>().text = Height+"";
		// GameObject.Find("BirthLimit").GetComponent<UnityEngine.UI.InputField>().text = BirthCount+"";
		// GameObject.Find("DeathLimit").GetComponent<UnityEngine.UI.InputField>().text = StarvationLowerLimit+"";
		// GameObject.Find("InitialBirthChance").GetComponent<UnityEngine.UI.InputField>().text = ChanceTileWillSpawnAlive+"";
		// GameObject.Find("Iterations").GetComponent<UnityEngine.UI.InputField>().text = NumberOfIterations+"";

		// refreshUIValues += UpdateInitialBirthChanceUI;
		// refreshUIValues += UpdateIterationsUI;
	}

	public void TuningComplete(ParValue[] settings){
		// refreshUIValues();
	}

	public delegate void UpdateUI();
	public UpdateUI refreshUIValues;

	public void UpdateInitialBirthChanceUI(){GameObject.Find("InitialBirthChance").GetComponent<UnityEngine.UI.InputField>().text = ChanceTileWillSpawnAlive+"";}
	public void UpdateIterationsUI(){GameObject.Find("Iterations").GetComponent<UnityEngine.UI.InputField>().text = NumberOfIterations+"";}

	public void UpdateWidth(string width){ Width = int.Parse(width);}
	public void UpdateHeight(string height){ Height = int.Parse(height);}
	public void UpdateIterations(string numiter){ NumberOfIterations = int.Parse(numiter);}
	public void UpdateInitialBirthChance(string chance){ ChanceTileWillSpawnAlive = float.Parse(chance);}
	public void UpdateDeathLimit(string dl){ StarvationLowerLimit = int.Parse(dl);}
	public void UpdateBirthLimit(string bl){ BirthCount = int.Parse(bl);}

	// Update is called once per frame
	void Update () {
		// if(Input.GetKeyDown(KeyCode.Space)){
		// 	GenerateSingleMap();
		// }

		// if(Input.GetKeyDown(KeyCode.R)){
		// 	AnalyseLargeSet();
		// }
	}


	public void GenerateSingleMap(){
		Tile[,] map = GenerateLevel(); //GetComponent<DrunkardWalkGenerator>().GenerateLevel();

		// RenderMapWithGameObjects(map);
		RenderMapWithSprite(map);

		// GameObject.Find("DAN").GetComponent<LevelAnalyser>()
		float densMetric = LevelAnalyser.CalculateDensity(map);
		float openMetric = LevelAnalyser.CalculateOpenness(map);
		GameObject.Find("MetricValues").GetComponent<UnityEngine.UI.Text>().text = ""+densMetric.ToString("0.000")+"\n"+openMetric.ToString("0.000");
	}

	public Color solidColor = Color.white;
	public Color wallColor = Color.black;

	void RenderMapWithSprite(Tile[,] map){
		int sf = 6;
		mapSprite.transform.position = new Vector3(20, 20, 0);
		// GameObject.Find("ExpressiveRangeGraph").GetComponent<ERAnalyser>().HideERA();

 		 Texture2D newTex = new Texture2D (Width*sf,Height*sf, TextureFormat.ARGB32, false);

		 //Render the map
		for(int i=0; i<Width; i++){
			for(int j=0; j<Height; j++){
				if(map[i,j].BLOCKS_MOVEMENT){
					PaintPoint(newTex, i, j, sf, solidColor);
				}
				else{
					PaintPoint(newTex, i, j, sf, wallColor);
				}
			}
		}

		 //Replace texture
		 newTex.Apply();

		 ShowMapSprite();
		 mapSprite.GetComponent<SpriteRenderer>().sprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0.5f, 0.5f), 10f);
	}

	public void ShowMapSprite(){mapSprite.GetComponent<SpriteRenderer>().color = Color.white;}
	public void HideMapSprite(){mapSprite.GetComponent<SpriteRenderer>().color = Color.clear;}

	void PaintPoint(Texture2D tex, int _x, int _y, int scaleFactor, Color c){
		int x = _x*scaleFactor; int y = _y*scaleFactor;
		for(int i=x; i<x+scaleFactor; i++){
			for(int j=y; j<y+scaleFactor; j++){
				tex.SetPixel(i, j, c);
			}
		}
	}

	void RenderMapWithGameObjects(Tile[,] map){
		if(GameObject.Find("Parent")){
			Destroy(GameObject.Find("Parent"));
		}
		GameObject p = new GameObject("Parent");

		//Render the map
		for(int i=0; i<Width; i++){
			for(int j=0; j<Height; j++){
				if(map[i,j].BLOCKS_MOVEMENT){
					RenderBlock(i, j, p);
				}
				else if(GenerateFloor){
					RenderFloor(i, j, p);
				}
			}
		}

		Camera.main.transform.position = new Vector3(CubeSize.x*Width/2, (Mathf.Max(CubeSize.x*Height,CubeSize.z*Width)), CubeSize.z*Height/2);
	}

	void RenderBlock(int i, int j, GameObject p){
		GameObject t = Instantiate(WallPiece);
		t.transform.position = new Vector3(i*CubeSize.x, 0, j*CubeSize.z);
		t.transform.parent = p.transform;
	}

	void RenderFloor(int i, int j, GameObject p){
		GameObject t = Instantiate(FloorPiece);
		t.transform.position = new Vector3(i*FloorSize.x, 0, j*FloorSize.z);
		t.transform.parent = p.transform;
	}

	public void AnalyseLargeSet(int amount=100){
		float oTotal = 0;
		float dTotal = 0;
		for(int i=0; i<amount; i++){
			Tile[,] map = GenerateLevel();
			dTotal += LevelAnalyser.CalculateDensity(map);
			oTotal += LevelAnalyser.CalculateOpenness(map);
		}
		float densMetric = dTotal/100f;
		float openMetric = oTotal/100f;
		GameObject.Find("CMetricValues").GetComponent<UnityEngine.UI.Text>().text = ""+densMetric.ToString("0.000")+"\n"+openMetric.ToString("0.000");
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
}
