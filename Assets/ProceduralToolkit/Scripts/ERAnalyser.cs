using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;

public class ERAnalyser : MonoBehaviour {

	public GameObject image;

	public Color solidColor = Color.white;
	public Color wallColor = Color.black;

	public GameObject generator;

	public Text progressLabel;

	public string firstMetricName = "CalculateDensity";
	public string secondMetricName = "CalculateOpenness";
	public void SetMetricName1(string name){firstMetricName = name;}
	public void SetMetricName2(string name){secondMetricName = name;}

	public int numberOfAttempts = 200;

	int[,] collectExpressiveRangeData(int totalAttempts){
		int[,] data = new int[100,100];
		LevelAnalyser la = DAN.Instance.analyser;

		System.Type t1 = la.GetType();
		MethodInfo metric1 = t1.GetMethod(firstMetricName);
		System.Type t2 = la.GetType();
		MethodInfo metric2 = t2.GetMethod(secondMetricName);

		for(int att=0; att<totalAttempts; att++){
			Tile[,] map = DAN.Instance.GenerateMap();
			
			// int m1 = (int)Mathf.Round(la.CalculateDensity(map)*100);
			// int m2 = (int)Mathf.Round(la.CalculateOpenness(map)*100);

			int m1 = (int)Mathf.Round((float)metric1.Invoke(la, new object[]{map})*100);
			int m2 = (int)Mathf.Round((float)metric2.Invoke(la, new object[]{map})*100);
			
			data[m1,m2]++;
		}

		return data;
	}

	public void StartRERA(){
		StartCoroutine(DrawRandomERGraph());
	}

	public void StartERA(){
		StartCoroutine(DrawExpressiveRangeGraph());
	}

	IEnumerator DrawRandomERGraph(){
		//Hide any levels
		generator.GetComponent<CellularAutomataGenerator>().HideMapSprite();

		int[,] data = new int[100,100];
		LevelAnalyser la = DAN.Instance.analyser;
		// CellularAutomataGenerator gen = generator.GetComponent<CellularAutomataGenerator>();

		System.Type t1 = la.GetType();
		MethodInfo metric1 = t1.GetMethod(firstMetricName);
		System.Type t2 = la.GetType();
		MethodInfo metric2 = t2.GetMethod(secondMetricName);

		AutoTuner at = DAN.Instance.tuner;
		at.Push();

		for(int att=0; att<numberOfAttempts; att++){
			at.Randomise();
			Tile[,] map = DAN.Instance.GenerateMap();
			int m1 = (int)Mathf.Round((float)metric1.Invoke(la, new object[]{map})*99);
			int m2 = (int)Mathf.Round((float)metric2.Invoke(la, new object[]{map})*99);
			// Debug.Log(m1+", "+m2);
			data[m1,m2]++;
			progressLabel.text = "Evaluating expressive range...\n"+(100*(float)att/(float)numberOfAttempts).ToString("F0")+" percent complete";
			yield return 0;
		}

		at.Pop();

		int sf = 5;

 		Texture2D newTex = new Texture2D (1000, 1000, TextureFormat.ARGB32, false);

		 //Render the map
		for(int i=0; i<data.GetLength(0); i++){
			for(int j=0; j<data.GetLength(1); j++){
				float amt = (float)data[i,j]/(float)numberOfAttempts * 50;
				PaintPoint(newTex, i, j, 10, new Color(amt, amt, amt, 1.0f));
			}
		}

		progressLabel.text = "";

		 //Replace texture
		 newTex.Apply();

		 ShowERA();
		 image.GetComponent<Image>().sprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0.5f, 0.5f));
		
	}

	IEnumerator DrawExpressiveRangeGraph(){
		//Hide any levels
		generator.GetComponent<CellularAutomataGenerator>().HideMapSprite();

		int[,] data = new int[100,100];
		LevelAnalyser la = DAN.Instance.analyser;
		// CellularAutomataGenerator gen = generator.GetComponent<CellularAutomataGenerator>();

		System.Type t1 = la.GetType();
		MethodInfo metric1 = t1.GetMethod(firstMetricName);
		System.Type t2 = la.GetType();
		MethodInfo metric2 = t2.GetMethod(secondMetricName);

		for(int att=0; att<numberOfAttempts; att++){
			Tile[,] map = DAN.Instance.GenerateMap();
			int m1 = (int)Mathf.Round((float)metric1.Invoke(la, new object[]{map})*99);
			int m2 = (int)Mathf.Round((float)metric2.Invoke(la, new object[]{map})*99);
			// Debug.Log(m1+", "+m2);
			data[m1,m2]++;
			progressLabel.text = "Evaluating expressive range...\n"+(100*(float)att/(float)numberOfAttempts).ToString("F0")+" percent complete";
			yield return 0;
		}

		int sf = 5;

 		Texture2D newTex = new Texture2D (1000, 1000, TextureFormat.ARGB32, false);

		 //Render the map
		for(int i=0; i<data.GetLength(0); i++){
			for(int j=0; j<data.GetLength(1); j++){
				float amt = (float)data[i,j]/(float)numberOfAttempts * 50;
				PaintPoint(newTex, i, j, 10, new Color(amt, amt, amt, 1.0f));
			}
		}

		progressLabel.text = "";

		 //Replace texture
		 newTex.Apply();

		 ShowERA();
		 image.GetComponent<Image>().sprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0.5f, 0.5f));
	}

	public GameObject xAxisLabel;
	public GameObject yAxisLabel;

	public void ShowERA(){
		image.GetComponent<Image>().color = Color.white;
		xAxisLabel.active = true; xAxisLabel.GetComponent<Text>().text = firstMetricName;
		yAxisLabel.active = true; yAxisLabel.GetComponent<Text>().text = secondMetricName;
	}

	public void HideERA(){
		image.GetComponent<Image>().color = Color.clear;
		xAxisLabel.active = false;
		yAxisLabel.active = false;
	}

	void PaintPoint(Texture2D tex, int _x, int _y, int scaleFactor, Color c){
		int x = _x*scaleFactor; int y = _y*scaleFactor;
		for(int i=x; i<x+scaleFactor; i++){
			for(int j=y; j<y+scaleFactor; j++){
				tex.SetPixel(i, j, c);		
			}
		}
	}

	// Use this for initialization
	void Start () {
		// StartCoroutine(DrawExpressiveRangeGraph());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
