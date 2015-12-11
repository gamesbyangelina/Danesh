using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class AutoTuner : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// GameObject gen = GameObject.Find("Generator");
		// CellularAutomataGenerator ca = gen.GetComponent<CellularAutomataGenerator>();

		LevelAnalyser la = DAN.Instance.analyser;

		// //Set up the target metrics
		targets = new List<TargetSetting>();
		// targets.Add(new TargetSetting(la.CalculateOpenness, 0.2f));
		// targets.Add(new TargetSetting(la.CalculateDensity, 0.6f));

		// //Set the parameter settings
		settings = new List<ParSetting>();
		// settings.Add(new ParSetting(typeof(CellularAutomataGenerator).GetField("ChanceTileWillSpawnAlive"), ca, 0.2f, 0.6f));
		// settings.Add(new ParSetting(typeof(CellularAutomataGenerator).GetField("NumberOfIterations"), ca, (int)1, (int)6));

		//Give AutoTuner the generation method
		// generator = ca.GenerateLevel;

		OnTuningComplete = DoNothing;
	}

	public void ClearParameters(){
		settings = new List<ParSetting>();
	}

	public void AddParameter(ParSetting p){
		settings.Add(p);
	}

	public void ClearTargetSettings(){
		//Set up the target metrics
		targets = new List<TargetSetting>();
	}
	public void AddTargetSetting(TargetSetting t){
		targets.Add(t);
	}

	List<TargetSetting> targets;
	List<ParValue[]> population;
	public List<ParSetting> settings;

	public void TuneParameters(int popsize, int generations, int numberOfRunsPerInstance){
		StartCoroutine(TuneCR(popsize, generations, numberOfRunsPerInstance));
	}

	IEnumerator TuneCR(int popsize, int generations, int numberOfRunsPerInstance){
		population = new List<ParValue[]>();

		//Initialise the population, using random values between the parameter settings
		for(int i=0; i<popsize; i++){
			ParValue[] geno = new ParValue[settings.Count];
			for(int j=0; j<settings.Count; j++){
				ParSetting p = settings[j];
				geno[j] = new ParValue(p.par, RandomValue(p));
			}
			population.Add(geno);
		}

		Debug.Log("Population generated.");
		yield return 0;

		for(int g=0; g<generations; g++){
			Debug.Log("Generation "+g);
			//Evaluate the population according to the given targets
			population.Sort(delegate(ParValue[] p1, ParValue[] p2) { return -(Evaluate(p1, numberOfRunsPerInstance).CompareTo(Evaluate(p2, numberOfRunsPerInstance))); });
			yield return 0;
			//Take the top members and build a new population through recombination
			List<ParValue[]> newpop = new List<ParValue[]>();
			while(newpop.Count < population.Count){
				ParValue[] p1 = population[0]; population.RemoveAt(0);
				ParValue[] p2 = population[0]; population.RemoveAt(0);
				newpop.Add(Crossover(p1, p2));
				newpop.Add(Crossover(p1, p2));
				newpop.Add(Crossover(p1, p2));
			}
			Debug.Log("New population generated.");
			GameObject.Find("AutoTuneProgress").GetComponent<UnityEngine.UI.Text>().text = (100*g/generations)+" percent complete";
			yield return 0;
			//New pop, same as the old pop
			population = newpop;
		}

		//Evaluate the population according to the given targets, one last time
		population.Sort(delegate(ParValue[] p1, ParValue[] p2) { return -(Evaluate(p1, numberOfRunsPerInstance).CompareTo(Evaluate(p2, numberOfRunsPerInstance))); });
			
		GameObject.Find("AutoTuneProgress").GetComponent<UnityEngine.UI.Text>().text = "";

		//Apply the parameters
		ParValue[] pvs = population[0];
		for(int i=0; i<pvs.Length; i++){
			settings[i].par.SetValue(settings[i].owner, pvs[i].val);
		}
		DAN.Instance.GenerateMap();

		OnTuningComplete();
		OnTuningComplete = DoNothing;
	}

	public delegate void TuningCompleteDelegate();
	public TuningCompleteDelegate OnTuningComplete;
	public void DoNothing(){}

	public float MutateChance = 0.05f;

	public ParValue[] Crossover(ParValue[] p1, ParValue[] p2){
		ParValue[] child = new ParValue[p1.Length];

		int point = Random.Range(0, p1.Length);
		for(int i=0; i<point; i++){
			if(Random.Range(0f,1f) < MutateChance)
				child[i] = (new ParValue(p1[i].field, RandomValue(settings[i])));
			else
				child[i] = (new ParValue(p1[i].field, p1[i].val));
		}
		for(int i=point; i<p1.Length; i++){
			if(Random.Range(0f,1f) < MutateChance)
				child[i] = (new ParValue(p1[i].field, RandomValue(settings[i])));
			else
				child[i] = (new ParValue(p2[i].field, p2[i].val));
		}

		return child;
	}

	public delegate Tile[,] MapGenerator();
	public MapGenerator generator;

	public float Evaluate(ParValue[] pvs, int runs){
		//First, apply all the field value data
		for(int i=0; i<pvs.Length; i++){
			settings[i].par.SetValue(settings[i].owner, pvs[i].val);
		}
		//Then run the generator, now it has these values set. Note that we don't check that these fields
		//accurately parameterise the generator - I don't know if this will be a problem later...
		float totalScore = 0;
		for(int i=0; i<runs; i++){
			Tile[,] map = DAN.Instance.GenerateMap();
			//For each map, the optimal fitness is that it exactly correlates with the target metric values
			float score = 0;
			for(int t=0; t<targets.Count; t++){
				float m = targets[t].evaluateForMetric(map);
				//Fitness is one minus the absolute difference between the target metric and what we got back
				score += 1 - Mathf.Abs(m-targets[t].targetValue);
			}
			//Then normalise score
			totalScore += score / targets.Count;
		}
		return totalScore / runs;
	}

	public void Randomise(){
		foreach(ParSetting p in settings){
			p.par.SetValue(p.owner, RandomValue(p));
		}
	}

	List<object> values;

	public void Push(){
		values = new List<object>();
		foreach(ParSetting p in settings){
			values.Add(p.par.GetValue(p.owner));
		}
	}

	public void Pop(){
		for(int i=0; i<values.Count; i++){
			ParSetting p = settings[i];
			p.par.SetValue(p.owner, values[i]);
		}
	}

	public object RandomValue(ParSetting p){
		object temp = p.par.GetValue(p.owner);
		if(temp is int){
			return Random.Range((int)p.minValue, ((int)p.maxValue)+1);
		}
		else if(temp is float){
			return Random.Range((float)p.minValue, (float)p.maxValue);
		}
		else if(temp is bool){
			return Random.Range(0, 2) == 0;
		}
		return null;
	}
	
	// Update is called once per frame
	void Update () {
		// if(Input.GetKeyDown(KeyCode.Q)){
			// TuneParameters(30, 30, 100);
		// }
	}
}
