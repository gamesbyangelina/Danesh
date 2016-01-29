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

	public void TuneRandomly(int attempts){
		StartCoroutine(TuneRandomlyCR(attempts));
	}

	IEnumerator TuneRandomlyCR(int attempts){
		float bestScore = 0;
		ParValue[] bestArray = new ParValue[settings.Count];

		float time = Time.realtimeSinceStartup;

		for(int i=0; i<attempts; i++){
			ParValue[] ex = new ParValue[settings.Count];
			for(int s=0; s<settings.Count; s++){
				ex[s] = new ParValue(settings[s].par, GridValue(settings[s], Random.Range(0f,1f)));
			}
			//Evaluate the array
			float score = Evaluate(ex, 50);

			GameObject.Find("AutoTuneProgress").GetComponent<UnityEngine.UI.Text>().text = (100*((float)i/((float)attempts)))+" percent complete";
			yield return 0;

			if(score > bestScore){
				bestScore = score;
				bestArray = ex;
			}
		}

		GameObject.Find("AutoTuneProgress").GetComponent<UnityEngine.UI.Text>().text = "";

		//Apply the parameters
		ParValue[] pvs = bestArray;
		for(int i=0; i<pvs.Length; i++){
			settings[i].par.SetValue(settings[i].owner, pvs[i].val);
		}
		DAN.Instance.GenerateMap();

		OnTuningComplete();

		Debug.Log("Time taken: "+(Time.realtimeSinceStartup - time));

		Debug.Log("Survived!");
		Debug.Log(bestScore);

	}

	public void TuneParametersHillClimb(float timelimit){
		StopCoroutine("TuneHillClimbCR");
		StartCoroutine(TuneHillClimbCR(timelimit));
	}

	float floatChange = 0.05f;
	int intChange = 1;

	IEnumerator TuneHillClimbCR(float timelimit){
		float bestScore = 0f;
		ParValue[] bestArray = new ParValue[settings.Count];

		//Pick a random spot
		float time = Time.realtimeSinceStartup;

		ParValue[] ex = new ParValue[settings.Count];
		for(int s=0; s<settings.Count; s++){
			ex[s] = new ParValue(settings[s].par, GridValue(settings[s], Random.Range(0f,1f)));
		}
		bestScore = Evaluate(ex, 50);
		bestArray = ex;

		while(Time.realtimeSinceStartup - time < timelimit){// && bestScore < 0.93f){
			float currentTime = Time.realtimeSinceStartup - time;
			GameObject.Find("AutoTuneProgress").GetComponent<UnityEngine.UI.Text>().text = (100*(currentTime/timelimit))+" percent complete";

			//Find all of the neighbouring parameter sets
			List<ParValue[]> nbs = CalculateNeighbours(ex);
			float bestThisRound = bestScore;
			ParValue[] bestArrayThisRound = new ParValue[settings.Count];

			foreach(ParValue[] nb in nbs){
				float score = Evaluate(nb, 50);
				if(score > bestThisRound){
					bestThisRound = score;
					bestArrayThisRound = nb;
				}
			}
			yield return 0;

			if(bestThisRound > bestScore){
				bestScore = bestThisRound;
				bestArray = bestArrayThisRound;
				ex = bestArrayThisRound;
			}
			else{
				//Random restart
				ex = new ParValue[settings.Count];
				for(int s=0; s<settings.Count; s++){
					ex[s] = new ParValue(settings[s].par, GridValue(settings[s], Random.Range(0f,1f)));
				}
			}
			yield return 0;
		}

		GameObject.Find("AutoTuneProgress").GetComponent<UnityEngine.UI.Text>().text = "";

		//Apply the parameters
		ParValue[] pvs = bestArray;
		for(int i=0; i<pvs.Length; i++){
			settings[i].par.SetValue(settings[i].owner, pvs[i].val);
		}
		DAN.Instance.GenerateMap();

		OnTuningComplete();

		// Debug.Log("Time taken: "+(Time.realtimeSinceStartup - time));

		// Debug.Log("Survived!");
		// Debug.Log(bestScore);
	}

	List<ParValue[]> CalculateNeighbours(ParValue[] current){
		List<ParValue[]> res = new List<ParValue[]>();
		for(int i=0; i<current.Length; i++){
			Debug.Log(current[i]);
			object temp = settings[i].par.GetValue(settings[i].owner);
			if(temp is int){
				if((int)current[i].val+intChange < (int)settings[i].maxValue){
					ParValue[] pu = new ParValue[current.Length];
					for(int j=0; j<current.Length; j++){
						if(j != i)
							pu[j] = new ParValue(current[j].field, current[j].val);
						else{
							pu[j] = new ParValue(current[j].field, ((int)current[j].val)+intChange);
						}
					}
					res.Add(pu);
				}
				if((int)current[i].val-intChange > (int)settings[i].minValue){
					ParValue[] pu = new ParValue[current.Length];
					for(int j=0; j<current.Length; j++){
						if(j != i)
							pu[j] = new ParValue(current[j].field, current[j].val);
						else{
							pu[j] = new ParValue(current[j].field, ((int)current[j].val)-intChange);
						}
					}
					res.Add(pu);
				}
			}
			if(temp is float){
				if((float)current[i].val+floatChange < (float)settings[i].maxValue){
					ParValue[] pu = new ParValue[current.Length];
					for(int j=0; j<current.Length; j++){
						if(j != i)
							pu[j] = new ParValue(current[j].field, current[j].val);
						else{
							pu[j] = new ParValue(current[j].field, ((float)current[j].val)+floatChange);
						}
					}
					res.Add(pu);
				}
				if((float)current[i].val-intChange > (float)settings[i].minValue){
					ParValue[] pu = new ParValue[current.Length];
					for(int j=0; j<current.Length; j++){
						if(j != i)
							pu[j] = new ParValue(current[j].field, current[j].val);
						else{
							pu[j] = new ParValue(current[j].field, ((float)current[j].val)-floatChange);
						}
					}
					res.Add(pu);
				}
			}
			if(temp is bool){
				ParValue[] pu = new ParValue[current.Length];
				for(int j=0; j<current.Length; j++){
					if(j != i)
						pu[j] = new ParValue(current[j].field, current[j].val);
					else{
						pu[j] = new ParValue(current[j].field, !((bool)current[j].val));
					}
				}
				res.Add(pu);
			}
		}
		return res;
	}

	public void TuneParametersGridSearch(float units){
		StartCoroutine(TuneGridCR(units));
	}

	IEnumerator TuneGridCR(float units){
		List<List<ParValue>> gridSetups = new List<List<ParValue>>();

		float time = Time.realtimeSinceStartup;

		float base_unit = 1f/units;
		List<float> nextVals = new List<float>();
		for(int i=0; i<settings.Count; i++){nextVals.Add(base_unit);}

		float bestScore = 0;
		ParValue[] bestArray = new ParValue[settings.Count];

		for(int i=0; i<Mathf.Pow(settings.Count, units); i++){
		// while(true){
			//Make a new array
			ParValue[] ex = new ParValue[settings.Count];
			for(int s=0; s<settings.Count; s++){
				ex[s] = new ParValue(settings[s].par, GridValue(settings[s], nextVals[s]));
			}
			//Evaluate the array
			// string line = "";
			// for(int s=0; s<ex.Length; s++){
			// 	line += ex[s].val+",";
			// }
			// line +="\n";
			// for(int s=0; s<ex.Length; s++){
			// 	line += nextVals[s]+",";
			// }
			// Debug.Log(line);
			float score = Evaluate(ex, 50);

			GameObject.Find("AutoTuneProgress").GetComponent<UnityEngine.UI.Text>().text = (100*(i/Mathf.Pow(settings.Count, units)))+" percent complete";
			yield return 0;

			if(score > bestScore){
				bestScore = score;
				bestArray = ex;
			}
			//Increment the counter
			for(int c=0; c<settings.Count; c++){
				if(nextVals[c] >= 1){
					nextVals[c] = 0;
				}
				else{
					nextVals[c] += base_unit;
					break;
				}
			}
			// if(nextVals.All(x => x == 0)){
			// 	break;
			// }
		}

		// Debug.Log(settings.Count*units);
		// Debug.Log("Time taken: "+(Time.realtimeSinceStartup - time));
		// Debug.Log("Survived!");
		// Debug.Log(bestScore);

		GameObject.Find("AutoTuneProgress").GetComponent<UnityEngine.UI.Text>().text = "";

		//Apply the parameters
		ParValue[] pvs = bestArray;
		for(int i=0; i<pvs.Length; i++){
			settings[i].par.SetValue(settings[i].owner, pvs[i].val);
		}
		DAN.Instance.GenerateMap();

		OnTuningComplete();

		// for(int i=0; i<units; i++){
		// 	for(int j=0; j<settings.Count; j++){
		// 		ParValue[] geno = new ParValue[settings.Count];
		// 		ParSetting p = settings[j];
		// 		geno[j] = new ParValue(p.par, RandomValue(p));
		// 		nextVals[i] += base_unit;
		// 	}
		// 	nextVals[i] += base_unit;
		// }
	}

	public void TuneParameters(int popsize, int generations, int numberOfRunsPerInstance){
		StartCoroutine(TuneCR(popsize, generations, numberOfRunsPerInstance));
	}

	IEnumerator TuneCR(int popsize, int generations, int numberOfRunsPerInstance){
		population = new List<ParValue[]>();

		float time = Time.realtimeSinceStartup;
		Debug.Log(Time.realtimeSinceStartup+" - start");

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

			Dictionary<ParValue[], float> fitnessDic = new Dictionary<ParValue[], float>();

			float max = 0f;
			float min = 1f;
			float avg = 0f; float num = 0;

			foreach(ParValue[] pv in population){
				fitnessDic[pv] = Evaluate(pv, numberOfRunsPerInstance);
				if(fitnessDic[pv] > max)
					max = fitnessDic[pv];
				if(fitnessDic[pv] < min)
					min = fitnessDic[pv];

				avg += fitnessDic[pv];
				num += 1;
			}

			//Evaluate the population according to the given targets
			population.Sort(delegate(ParValue[] p1, ParValue[] p2) { return -(fitnessDic[p1].CompareTo(fitnessDic[p2])); });
			// population.Sort(delegate(ParValue[] p1, ParValue[] p2) { return -(Evaluate(p1, numberOfRunsPerInstance).CompareTo(Evaluate(p2, numberOfRunsPerInstance))); });

			// Debug.Log("Lowest fitness: "+min);
			// Debug.Log("Average fitness: "+(avg/num));
			// Debug.Log("Highest fitness: "+max);

			yield return 0;
			//Take the top members and build a new population through recombination
			List<ParValue[]> newpop = new List<ParValue[]>();
			while(newpop.Count < population.Count){
				ParValue[] p1 = population[0]; population.RemoveAt(0);
				ParValue[] p2 = population[0]; population.RemoveAt(0);
				newpop.Add(p1);
				newpop.Add(p2);
				newpop.Add(Crossover(p1, p2));
				newpop.Add(Crossover(p1, p2));

				ParValue[] geno = new ParValue[settings.Count];
				for(int j=0; j<settings.Count; j++){
					ParSetting p = settings[j];
					geno[j] = new ParValue(p.par, RandomValue(p));
				}
				newpop.Add(geno);

				geno = new ParValue[settings.Count];
				for(int j=0; j<settings.Count; j++){
					ParSetting p = settings[j];
					geno[j] = new ParValue(p.par, RandomValue(p));
				}
				newpop.Add(geno);
				// newpop.Add(Crossover(p1, p2));
				// newpop.Add(Crossover(p1, p2));
			}

			// Debug.Log("New population generated.");
			GameObject.Find("AutoTuneProgress").GetComponent<UnityEngine.UI.Text>().text = (100*g/generations)+" percent complete";
			// Debug.Log(GameObject.Find("AutoTuneProgress").GetComponent<UnityEngine.UI.Text>().text);

			yield return 0;
			//New pop, same as the old pop
			population = newpop;
		}

		//Evaluate the population according to the given targets, one last time
		population.Sort(delegate(ParValue[] p1, ParValue[] p2) { return -(Evaluate(p1, numberOfRunsPerInstance).CompareTo(Evaluate(p2, numberOfRunsPerInstance))); });

		GameObject.Find("AutoTuneProgress").GetComponent<UnityEngine.UI.Text>().text = "";

		// Debug.Log(Time.realtimeSinceStartup+" - finish");
		// Debug.Log("Time taken: "+(Time.realtimeSinceStartup - time));
		// Debug.Log("Highest fitness: "+Evaluate(population[0], numberOfRunsPerInstance));

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

	public object GridValue(ParSetting p, float proportion){
		object temp = p.par.GetValue(p.owner);
		if(temp is int){
			return (int)((int)((float)((int)p.maxValue - (int)p.minValue)) * proportion) + (int)p.minValue;
		}
		else if(temp is float){
			return (float)(((float)p.maxValue - (float)p.minValue) * proportion) + (float)p.minValue;
		}
		else if(temp is bool){
			if(proportion <= 0.5f)
				return false;
			else
				return true;
		}
		return null;
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
