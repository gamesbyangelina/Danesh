using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public class GeneratorAnalysisUI : MonoBehaviour {

	UnityEngine.UI.InputField TargetDensity;
	UnityEngine.UI.InputField TargetOpenness; 
	UnityEngine.UI.Button AutoTuneButton;

	AutoTuner tuner;
	LevelAnalyser analyser;

	GameObject MetricReportPanel;
	GameObject TargetMetricPanel;
	GameObject TunableParameterPanel;

	public GameObject MetricReportUIPrefab;
	public GameObject TargetMetricPanelUIPrefab;
	public GameObject TunableParameterUIPrefab;
	public GameObject TunableBoolUIPrefab;

	List<Text> MetricReportTexts;
	List<TargetSetting.MetricDelegate> MetricDelegateList;
	List<InputField> MetricInputFieldList;
	List<TunableParameter> TunableParameterInputList;

	MonoBehaviour generator;

	public void SetupTuningUI(MonoBehaviour generator){
		this.generator = generator;

		TunableParameterInputList = new List<TunableParameter>();

		//Find all the fields with tunable attributes and add them to the UI
		foreach(FieldInfo field in generator.GetType().GetFields()){
			foreach(Attribute attr in field.GetCustomAttributes(false)){
				if(attr is TunableAttribute){
					TunableAttribute t = (TunableAttribute) attr;
					
					TunableParameter tpi;
					if(t.MinValue is bool){
						GameObject tpp = Instantiate(TunableBoolUIPrefab);
						tpp.transform.parent = TunableParameterPanel.transform;
						tpi = tpp.GetComponent<TunableParameterToggleField>();
					}
					else{
						GameObject tpp = Instantiate(TunableParameterUIPrefab);
						tpp.transform.parent = TunableParameterPanel.transform;
						tpi = tpp.GetComponent<TunableParameterInput>();
					}
					
					tpi.Setup(this);
					//Set the field's name up
					tpi.targetParameter = field;
					tpi.tuneMin = t.MinValue;
					tpi.tuneMax = t.MaxValue;
					tpi.SetValue(field.GetValue(generator));
					if(t.Name == "")
						tpi.label.text = field.Name;
					else
						tpi.label.text = t.Name;
					//Add the TPIs to the corresponding list
					TunableParameterInputList.Add(tpi);

					//Also add them to the ERAnalyser as parameter setting objects
					ParSetting p = new ParSetting(field, generator, t.MinValue, t.MaxValue);
					tuner.AddParameter(p);
				}
			}
		}

		MetricReportTexts = new List<Text>();
		MetricDelegateList = new List<TargetSetting.MetricDelegate>();
		MetricInputFieldList = new List<InputField>();

		List<string> metricNames = new List<string>();

		foreach(MonoBehaviour b in generator.GetComponents<MonoBehaviour>()){
			foreach(MethodInfo method in b.GetType().GetMethods()){
				foreach(Attribute attr in method.GetCustomAttributes(false)){
					if(attr is Metric){
						GameObject metricPanel = Instantiate(TargetMetricPanelUIPrefab);
						metricPanel.transform.parent = TargetMetricPanel.transform;
						metricPanel.transform.Find("TargetLabel").GetComponent<Text>().text = "Calculate "+((Metric) attr).Name;

						//This line creates a MetricDelegate from the MethodInfo object. 
						//MetricDelegates take a Tile[,] and return a float.
						//Note that we pass in the MonoBehaviour because it needs an object to invoke the delegate on.
						MetricDelegateList.Add((TargetSetting.MetricDelegate)Delegate.CreateDelegate(typeof(TargetSetting.MetricDelegate), b, method));
						MetricInputFieldList.Add(metricPanel.transform.Find("TargetInput").GetComponent<InputField>());

						//We also create a report in the top left
						GameObject metricReport = Instantiate(MetricReportUIPrefab);
						metricReport.transform.parent = MetricReportPanel.transform;
						metricReport.name = "MetricReport"+((Metric) attr).Name;
						metricReport.transform.Find("MetricName").GetComponent<Text>().text = ""+((Metric) attr).Name;
						MetricReportTexts.Add(metricReport.transform.Find("MetricValue").GetComponent<Text>());

						metricNames.Add(method.Name);
					}
				}
			}
		}

		if(MetricInputFieldList.Count == 0 || DAN.Instance.AddDefaultMetrics){
			LevelAnalyser la = DAN.Instance.analyser;
			foreach(MethodInfo method in la.GetType().GetMethods()){
				foreach(Attribute attr in method.GetCustomAttributes(false)){
					if(attr is Metric){
						GameObject metricPanel = Instantiate(TargetMetricPanelUIPrefab);
						metricPanel.transform.parent = TargetMetricPanel.transform;
						metricPanel.transform.Find("TargetLabel").GetComponent<Text>().text = ((Metric) attr).Name;

						//This line creates a MetricDelegate from the MethodInfo object. 
						//MetricDelegates take a Tile[,] and return a float.
						//Note that we pass in the MonoBehaviour because it needs an object to invoke the delegate on.
						MetricDelegateList.Add((TargetSetting.MetricDelegate)Delegate.CreateDelegate(typeof(TargetSetting.MetricDelegate), la, method));
						MetricInputFieldList.Add(metricPanel.transform.Find("TargetInput").GetComponent<InputField>());

						//We also create a report in the top left
						GameObject metricReport = Instantiate(MetricReportUIPrefab);
						metricReport.transform.parent = MetricReportPanel.transform;
						metricReport.name = "MetricReport"+((Metric) attr).Name;
						metricReport.transform.Find("MetricName").GetComponent<Text>().text = ""+((Metric) attr).Name;
						MetricReportTexts.Add(metricReport.transform.Find("MetricValue").GetComponent<Text>());

						metricNames.Add(method.Name);
					}
				}
			}
		}

		//Update the ERA Buttons with the first two metrics
		GameObject.Find("MetricName1").GetComponent<InputField>().text = metricNames[0];
		GameObject.Find("MetricName2").GetComponent<InputField>().text = metricNames[1];

	}

	public void ClickGenerateMap(){
		Tile[,] map = DAN.Instance.GenerateMap();
		//Render the map so we can see it
		DAN.Instance.RenderMapWithSprite(map);
		//Update the metrics
		for(int i=0; i<MetricReportTexts.Count; i++){
			float v = MetricDelegateList[i](map);
			MetricReportTexts[i].text = ""+v.ToString("0.000");
		}
	}

	public int ATPopulationSize = 20;
	public int ATNumberGenerations = 20;
	public int ATRunsPerInstance = 70;

	public void ClickAutoTune(){
		AutoTuneButton.GetComponentsInChildren<UnityEngine.UI.Text>()[0].text = "Tuning...";
		AutoTuneButton.enabled = false;

		//Set up the target settings
		tuner.ClearTargetSettings();
		for(int i=0; i<MetricDelegateList.Count; i++){
			tuner.AddTargetSetting(new TargetSetting(MetricDelegateList[i], float.Parse(MetricInputFieldList[i].text)));	
		}
		
		//Set up the active parameters
		tuner.ClearParameters();
		foreach(TunableParameterInput tpi in TunableParameterInputList){
			if(tpi.toggle.isOn){
				Debug.Log("Added tuning parameter: "+tpi.targetParameter.Name+". Min: "+tpi.tuneMin+", Max: "+tpi.tuneMax);
				tuner.AddParameter(new ParSetting(tpi.targetParameter, generator, tpi.tuneMin, tpi.tuneMax));	
			}
		}

		tuner.OnTuningComplete += AutoTuneComplete;

		tuner.TuneParameters(ATPopulationSize, ATNumberGenerations, ATRunsPerInstance);
	}

	public void BoolParameterChanged(bool b){ParameterChanged();}
	public void StringParameterChanged(string s){ParameterChanged();}

	public void ParameterChanged(){
		foreach(TunableParameter tpi in TunableParameterInputList){
			if(tpi.targetParameter.FieldType.Equals(typeof(float))){
				float parsedValue = float.Parse((string)tpi.GetValue());
				tpi.targetParameter.SetValue(generator, parsedValue);
			}
			else if(tpi.targetParameter.FieldType.Equals(typeof(int))){
				int parsedValue = int.Parse((string)tpi.GetValue());
				tpi.targetParameter.SetValue(generator, parsedValue);
			}
			else if(tpi.targetParameter.FieldType.Equals(typeof(bool))){
				bool parsedValue = (bool)(tpi.GetValue());
				tpi.targetParameter.SetValue(generator, parsedValue);
			}
		}
		
	}

	public void AutoTuneComplete(){
		AutoTuneButton.enabled = true;
		AutoTuneButton.GetComponentsInChildren<UnityEngine.UI.Text>()[0].text = "Auto-Tune";

		foreach(TunableParameterInput tpi in TunableParameterInputList){
			tpi.inputField.text = ""+tpi.targetParameter.GetValue(generator);
			// if(tpi.toggle.isOn){
				// Debug.Log("Added tuning parameter: "+tpi.targetParameter.Name+". Min: "+tpi.tuneMin+", Max: "+tpi.tuneMax);
				// tuner.AddParameter(new ParSetting(tpi.targetParameter, generator, tpi.tuneMin, tpi.tuneMax));	
			// }
		}
	}

	// Use this for initialization
	void Start () {
		AutoTuneButton = GameObject.Find("AutoTuneButton").GetComponent<UnityEngine.UI.Button>();
		// TargetOpenness = GameObject.Find("TargetOpenness").GetComponent<UnityEngine.UI.InputField>();
		// TargetDensity = GameObject.Find("TargetDensity").GetComponent<UnityEngine.UI.InputField>();

		tuner = DAN.Instance.tuner;
		analyser = DAN.Instance.analyser;

		MetricReportPanel = GameObject.Find("MetricReportPanel");
		TargetMetricPanel = GameObject.Find("MetricListPanel");
		TunableParameterPanel = GameObject.Find("ParameterListPanel");

		SetupTuningUI(DAN.Instance.GetGeneratorMB());
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
