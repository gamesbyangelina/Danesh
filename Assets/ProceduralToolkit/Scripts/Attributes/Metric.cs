using UnityEngine;
using System.Collections;
using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
public class Metric : PropertyAttribute {

	string _name;

	public Metric(string Name){
		_name = Name;
	}

	public string Name{
		get {return this._name;}
	}

}