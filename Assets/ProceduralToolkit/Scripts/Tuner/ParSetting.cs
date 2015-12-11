using UnityEngine;
using System.Collections;
using System.Reflection;

public class ParSetting  {

	public FieldInfo par;
	public object owner;

	public object minValue;
	public object maxValue;

	public ParSetting(FieldInfo f, object owner, object min, object max){
		this.par = f;
		this.owner = owner;
		this.minValue = min;
		this.maxValue = max;
	}

}
