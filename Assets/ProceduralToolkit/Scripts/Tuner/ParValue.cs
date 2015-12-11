using UnityEngine;
using System.Collections;
using System.Reflection;

/*
	Represents the value of a parameter in the generator
*/
public class ParValue {

	public FieldInfo field;
	public object val;

	public ParValue(FieldInfo f, object val){
		this.field = f;
		this.val = val;
	}

}
