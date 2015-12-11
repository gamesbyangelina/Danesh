using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Reflection;

public class TunableParameterToggleField : TunableParameter {

	[HideInInspector]
	public Toggle valueToggle;
	[HideInInspector]
	Text toggleLabel;
	// [HideInInspector]
	// public Text label;
	// [HideInInspector]
	// public Toggle toggle;
	// [HideInInspector]
	// public FieldInfo targetParameter;

	public Color onColor;
	public Color offColor;

	public void SetValueFromUI(bool val){
		SetValue(val);
	}

	public override void SetValue(object val){
		bool ison = (bool) val;

		valueToggle.isOn = ison;
		if(ison){
			valueToggle.transform.Find("Background").GetComponent<Image>().color = onColor;
			toggleLabel.color = onColor;
			toggleLabel.text = "True";
		}
		else{
			valueToggle.transform.Find("Background").GetComponent<Image>().color = offColor;
			toggleLabel.color = offColor;
			toggleLabel.text = "False";
		}
	}

	public override object GetValue(){
		return valueToggle.isOn;
	}

	public override void Setup(GeneratorAnalysisUI g){
		valueToggle = transform.Find("ValueToggle").GetComponent<Toggle>();
		label = transform.Find("Label").GetComponent<Text>();
		toggleLabel = valueToggle.transform.Find("Label").GetComponent<Text>();
		valueToggle.onValueChanged.AddListener(g.BoolParameterChanged);
		valueToggle.onValueChanged.AddListener(SetValueFromUI);
	}

	// Use this for initialization
	void Start () {
		valueToggle = transform.Find("ValueToggle").GetComponent<Toggle>();
		label = transform.Find("Label").GetComponent<Text>();
		toggle = transform.Find("Toggle").GetComponent<Toggle>();
		tuneMax = true;
		tuneMin = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
