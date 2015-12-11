using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class DAN : MonoBehaviour {

	public static DAN Instance {get; private set;}

	public GameObject generator;

	public bool AddDefaultMetrics;

	MonoBehaviour targetBehaviour;
	MethodInfo generateMapMethod;

	[HideInInspector]
	public AutoTuner tuner;
	[HideInInspector]
	public LevelAnalyser analyser;

	void Awake(){
		Instance = this;
		tuner = GetComponent<AutoTuner>();
		analyser = GetComponent<LevelAnalyser>();	
		mapSprite = GameObject.Find("MapSprite");

		foreach(MonoBehaviour b in generator.GetComponents<MonoBehaviour>()){
			foreach(MethodInfo method in b.GetType().GetMethods()){
				foreach(Attribute attr in method.GetCustomAttributes(false)){
					if(attr is MapGenerator){
						Debug.Log(generator.name + "." + method.Name);
						generateMapMethod = method;
						targetBehaviour = b;
					}
				}
			}
		}
	}

	public void GenerateAndRenderMap(){
		RenderMapWithSprite(GenerateMap());
	}

	public Tile[,] GenerateMap(){
		if(generateMapMethod != null)
			return (Tile[,]) generateMapMethod.Invoke(targetBehaviour, new object[]{});	
		else
			return new Tile[0,0];
	}

	public MonoBehaviour GetGeneratorMB(){
		return targetBehaviour;
	}

	GameObject mapSprite;
	public Color solidColor = Color.white;
	public Color wallColor = Color.black;

	public void RenderMapWithSprite(Tile[,] map){
		int sf = 6; int Width = map.GetLength(0); int Height = map.GetLength(1);
		mapSprite.transform.position = new Vector3(20, 20, 0);
		GameObject.Find("ExpressiveRangeGraph").GetComponent<ERAnalyser>().HideERA();

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

	void PaintPoint(Texture2D tex, int _x, int _y, int scaleFactor, Color c){
		int x = _x*scaleFactor; int y = _y*scaleFactor;
		for(int i=x; i<x+scaleFactor; i++){
			for(int j=y; j<y+scaleFactor; j++){
				tex.SetPixel(i, j, c);		
			}
		}
	}

	public void ShowMapSprite(){mapSprite.GetComponent<SpriteRenderer>().color = Color.white;}
	public void HideMapSprite(){mapSprite.GetComponent<SpriteRenderer>().color = Color.clear;}

}
