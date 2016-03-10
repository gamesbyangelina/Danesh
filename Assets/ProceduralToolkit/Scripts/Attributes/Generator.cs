using UnityEngine;
using System.Collections;
using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
public class Generator : PropertyAttribute {

    string _type;

	public Generator(string type="generic"){
        _type = type;
	}

    public string Type{
        get {return this._type;}
    }

}
