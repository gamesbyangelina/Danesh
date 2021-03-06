﻿using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class ERAnalyser {

	public GameObject image;

	public Color solidColor = Color.white;
	public Color wallColor = Color.black;

	// public GameObject generator;

	DaneshWindow danesh;

	// public Text progressLabel;

	public string firstMetricName = "CalculateDensity";
	public string secondMetricName = "CalculateOpenness";
	public void SetMetricName1(string name){firstMetricName = name;}
	public void SetMetricName2(string name){secondMetricName = name;}

	public int numberOfAttempts = 200;
	public int numberOfAttemptsRandom = 2000;

	public List<List<object[,]>> eraSamples;

	List<List<float>> SampleExpressiveRangeRandomly(int totalAttempts, DaneshWindow gen){
		float progressBar = 0f;
		EditorUtility.DisplayProgressBar("Computing Randomised Expressive Range Histogram", "Working...", progressBar);
		List<List<float>> res = new List<List<float>>();

		/*
			Current:
			eraSample[metric1][metric2] = object[,] of samples representing the histogram
			Lot of data duplication here, not the most efficient way to do it
		*/
		eraSamples = new List<List<object[,]>>();
		for(int i=0; i<danesh.metricList.Count; i++){
			List<object[,]> secondmetric = new List<object[,]>();
			for(int j=0; j<danesh.metricList.Count; j++){
				object[,] sampleH = new object[100,100];
				secondmetric.Add(sampleH);
			}
			eraSamples.Add(secondmetric);
		}

		for(int att=0; att<totalAttempts; att++){

			//Randomly parameterise the generator
			foreach(GeneratorParameter p in gen.parameterList){
				p.RandomiseValue();
			}

			object map = danesh.GenerateContent();
			List<float> nums = new List<float>();
			for(int i=0; i<danesh.metricList.Count; i++){
				float score = (float)danesh.GetMetric(i, new object[]{map});
				nums.Add(score);
			}

			//Update the samples list
			for(int i=0; i<nums.Count; i++){
				int index1 = (int)Mathf.Round(nums[i]*100f);
				if(index1 < 0) index1 = 0;
				if(index1 >99) index1 = 99;
				for(int j=0; j<nums.Count; j++){
					int index2 = (int)Mathf.Round(nums[j]*100f);
					if(index2 < 0) index2 = 0;
					if(index2 >99) index2 = 99;
					eraSamples[i][j][index1,index2] = map;
				}
			}

			res.Add(nums);
			EditorUtility.DisplayProgressBar("Computing Randomised Expressive Range Histogram", "Evaluating random expressive range... "+(100*(float)att/(float)totalAttempts).ToString("F0")+" percent complete", (float)att/(float)totalAttempts);
		}
		EditorUtility.ClearProgressBar();
		return res;
	}

	List<List<float>> SampleExpressiveRange(int totalAttempts){
		float progressBar = 0f;
		EditorUtility.DisplayProgressBar("Computing Expressive Range Histogram", "Working...", progressBar);
		List<List<float>> res = new List<List<float>>();

		eraSamples = new List<List<object[,]>>();
		for(int i=0; i<danesh.metricList.Count; i++){
			List<object[,]> secondmetric = new List<object[,]>();
			for(int j=0; j<danesh.metricList.Count; j++){
				object[,] sampleH = new object[100,100];
				secondmetric.Add(sampleH);
			}
			eraSamples.Add(secondmetric);
		}

		for(int att=0; att<totalAttempts; att++){
			object map = danesh.GenerateContent();
			List<float> nums = new List<float>();
			for(int i=0; i<danesh.metricList.Count; i++){
				float score = (float)danesh.GetMetric(i, new object[]{map});
				nums.Add(score);
			}
			res.Add(nums);

			//Update the samples list
			for(int i=0; i<nums.Count; i++){
				int index1 = (int)Mathf.Round(nums[i]*100f);
				if(index1 < 0) index1 = 0;
				if(index1 >99) index1 = 99;
				for(int j=0; j<nums.Count; j++){
					int index2 = (int)Mathf.Round(nums[j]*100f);
					if(index2 < 0) index2 = 0;
					if(index2 >99) index2 = 99;
					eraSamples[i][j][index1,index2] = map;
				}
			}

			EditorUtility.DisplayProgressBar("Computing Expressive Range Histogram", "Evaluating expressive range... "+(100*(float)att/(float)totalAttempts).ToString("F0")+" percent complete", (float)att/(float)totalAttempts);
		}
		EditorUtility.ClearProgressBar();
		return res;
	}

	int[,] collectExpressiveRangeData(int totalAttempts){
		int[,] data = new int[100,100];

		for(int att=0; att<totalAttempts; att++){
			object map = danesh.GenerateContent();

			int m1 = (int)Mathf.Round((float)danesh.GetMetric(danesh.x_axis_era, new object[]{map})*100);
			int m2 = (int)Mathf.Round((float)danesh.GetMetric(danesh.y_axis_era, new object[]{map})*100);

			data[m1,m2]++;
		}

		return data;
	}

	public List<List<float>> LastERA;

	public void StartRERA(DaneshWindow gen, int num){
		danesh = gen;
		numberOfAttemptsRandom = num;
		// DrawRandomERGraph();
		LastERA = SampleExpressiveRangeRandomly(num, gen);
	}

	public void StartERA(DaneshWindow gen, int num){
		danesh = gen;
		numberOfAttempts = num;
		LastERA = SampleExpressiveRange(num);
	}

	public Texture2D GenerateGraphForAxes(int x, int y){
		if(LastERA == null){
			Debug.Log("Returned null");
	 		return new Texture2D (1000, 1000, TextureFormat.ARGB32, false);
		}
		else{
			Debug.Log("Last ERA wasn't null");
			int[,] data = new int[100,100];
			for(int att=0; att<LastERA.Count; att++){
				// at.Randomise();
				int m1 = (int)Mathf.Round(LastERA[att][x]*100f);
				int m2 = (int)Mathf.Round(LastERA[att][y]*100f);
				if(LastERA[att][x] > 0){
					// Debug.Log(m1);
					// Debug.Log(LastERA[att][x]*100f);
					// Debug.Log(Mathf.Round(LastERA[att][x]*100f));
				}
				if(m1 < 0)
					m1 = 0;
				if(m2 < 0)
					m2 = 0;
				if(m1 >= 0 && m2 >= 0 && m1 < data.GetLength(0) && m2 < data.GetLength(1)){
					data[m1,m2]++;
				}
			}
			int sf = 5;

 			Texture2D newTex = new Texture2D (500, 500, TextureFormat.ARGB32, false);

		 	//Render the map
			for(int i=0; i<data.GetLength(0); i++){
				for(int j=0; j<data.GetLength(1); j++){
					float amt = (float)data[i,j]/(float)numberOfAttempts * 50;
					PaintPoint(newTex, i, j, 5, new Color(amt, amt, amt, 1.0f));
				}
			}

			//Replace texture
			newTex.Apply();

			return newTex;
		}
	}

	public Texture2D GeneratedGraph;

	void DrawRandomERGraph(){

		//Hide any levels
		// generator.GetComponent<CellularAutomataGenerator>().HideMapSprite();

		int[,] data = new int[100,100];
		LevelAnalyser la = DAN.Instance.analyser;
		// CellularAutomataGenerator gen = generator.GetComponent<CellularAutomataGenerator>();

		// System.Type t1 = la.GetType();
		// MethodInfo metric1 = t1.GetMethod(firstMetricName);
		// System.Type t2 = la.GetType();
		// MethodInfo metric2 = t2.GetMethod(secondMetricName);

		// AutoTuner at = DAN.Instance.tuner;
		// at.Push();

		for(int att=0; att<numberOfAttemptsRandom; att++){
			// at.Randomise();
			object map = danesh.GenerateContent();
			int m1 = (int)Mathf.Round((float)danesh.GetMetric(danesh.x_axis_era, new object[]{map})*99);
			int m2 = (int)Mathf.Round((float)danesh.GetMetric(danesh.y_axis_era, new object[]{map})*99);
			Debug.Log(m1+", "+m2);
			if(m1 < 0)
				m1 = 0;
			if(m2 < 0)
				m2 = 0;
			data[m1,m2]++;
			// progressLabel.text = "Evaluating expressive range...\n"+(100*(float)att/(float)numberOfAttemptsRandom).ToString("F0")+" percent complete";
			// yield return 0;
		}

		// at.Pop();

		int sf = 5;

 		Texture2D newTex = new Texture2D (1000, 1000, TextureFormat.ARGB32, false);

		 //Render the map
		for(int i=0; i<data.GetLength(0); i++){
			for(int j=0; j<data.GetLength(1); j++){
				float amt = (float)data[i,j]/(float)numberOfAttempts * 50;
				PaintPoint(newTex, i, j, 5, new Color(amt, amt, amt, 1.0f));
			}
		}

		// progressLabel.text = "";

		 //Replace texture
		 newTex.Apply();

		 GeneratedGraph = newTex;

		 // ShowERA();
		 // image.GetComponent<Image>().sprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0.5f, 0.5f));

	}

	void DrawExpressiveRangeGraph(){
		float progressBar = 0.0f;
        EditorUtility.DisplayProgressBar("Computing Expressive Range Histogram", "Working...", progressBar);
		//Hide any levels
		// generator.GetComponent<CellularAutomataGenerator>().HideMapSprite();

		int[,] data = new int[100,100];
		// LevelAnalyser la = DAN.Instance.analyser;
		// CellularAutomataGenerator gen = generator.GetComponent<CellularAutomataGenerator>();

		// System.Type t1 = la.GetType();
		// MethodInfo metric1 = t1.GetMethod(firstMetricName);
		// System.Type t2 = la.GetType();
		// MethodInfo metric2 = t2.GetMethod(secondMetricName);

		for(int att=0; att<numberOfAttempts; att++){
			object map = danesh.GenerateContent();
			int m1 = (int)Mathf.Round((float)danesh.GetMetric(danesh.x_axis_era, new object[]{map})*99);
			int m2 = (int)Mathf.Round((float)danesh.GetMetric(danesh.y_axis_era, new object[]{map})*99);
			// Debug.Log(m1+", "+m2);
			data[m1,m2]++;
			// progressLabel.text = "Evaluating expressive range...\n"+(100*(float)att/(float)numberOfAttempts).ToString("F0")+" percent complete";

			// yield return 0;
		}

		int sf = 5;

 		Texture2D newTex = new Texture2D (500, 500, TextureFormat.ARGB32, false);

		 //Render the map
		for(int i=0; i<data.GetLength(0); i++){
			for(int j=0; j<data.GetLength(1); j++){
				float amt = (float)data[i,j]/(float)numberOfAttempts * 50;
				PaintPoint(newTex, i, j, 5, new Color(amt, amt, amt, 1.0f));
			}
		}

		// progressLabel.text = "";

		 //Replace texture
		newTex.Apply();

		GeneratedGraph = newTex;
		EditorUtility.ClearProgressBar();
		danesh.DisplayTexture(GeneratedGraph);
		danesh.SwitchToERAMode();

		 // ShowERA();
		 // image.GetComponent<Image>().sprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0.5f, 0.5f));
	}

	// public GameObject xAxisLabel;
	// public GameObject yAxisLabel;

	// public void ShowERA(){
	// 	image.GetComponent<Image>().color = Color.white;
	// 	xAxisLabel.active = true; xAxisLabel.GetComponent<Text>().text = firstMetricName;
	// 	yAxisLabel.active = true; yAxisLabel.GetComponent<Text>().text = secondMetricName;
	// }

	// public void HideERA(){
	// 	image.GetComponent<Image>().color = Color.clear;
	// 	xAxisLabel.active = false;
	// 	yAxisLabel.active = false;
	// }

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
