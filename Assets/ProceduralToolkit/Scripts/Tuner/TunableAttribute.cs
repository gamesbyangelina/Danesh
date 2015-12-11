using UnityEngine;
using System.Collections;
using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple=true)]
public class TunableAttribute : PropertyAttribute {

	object _minValue;
	object _maxValue;
	string _name;

	public TunableAttribute(object MinValue, object MaxValue, string Name=""){
		_minValue = MinValue;
		_maxValue = MaxValue;
		_name = Name;
	}

	public object MinValue{
		get {return this._minValue;}
	}

	public object MaxValue{
		get {return this._maxValue;}
	}

	public string Name{
		get {return this._name;}
	}

}