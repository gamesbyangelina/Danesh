using UnityEngine;
using System.Collections;
using System.Reflection;
using UnityEngine.UI;

public class TunableParameterInput : TunableParameter {

	[HideInInspector]
	public InputField inputField;
	// [HideInInspector]
	// public Text label;
	// [HideInInspector]
	// public Toggle toggle;
	// [HideInInspector]
	// public FieldInfo targetParameter;
	// [HideInInspector]
	// public object tuneMax;
	// [HideInInspector]
	// public object tuneMin;

	public override void SetValue(object val){
		string s = (string) ""+val;
		inputField.text = s;
	}

	public override object GetValue(){
		return inputField.text;
	}

	public override void Setup(GeneratorAnalysisUI g){
		inputField = transform.Find("InputField").GetComponent<InputField>();
		label = transform.Find("Label").GetComponent<Text>();
		inputField.onEndEdit.AddListener(g.StringParameterChanged);
	}

	// Use this for initialization
	void Start () {
		inputField = transform.Find("InputField").GetComponent<InputField>();
		label = transform.Find("Label").GetComponent<Text>();
		toggle = transform.Find("Toggle").GetComponent<Toggle>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
