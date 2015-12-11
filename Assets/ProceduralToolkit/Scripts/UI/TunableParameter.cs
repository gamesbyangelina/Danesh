using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Collections;

public class TunableParameter : MonoBehaviour {

	[HideInInspector]
	public Text label;
	[HideInInspector]
	public Toggle toggle;
	[HideInInspector]
	public FieldInfo targetParameter;
	[HideInInspector]
	public object tuneMax;
	[HideInInspector]
	public object tuneMin;

	public virtual void Setup(GeneratorAnalysisUI g){}
	public virtual void SetValue(object value){}
	public virtual object GetValue(){return null;}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
