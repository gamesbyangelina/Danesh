using UnityEngine;
using System.Collections;
using System.Reflection;

public class GeneratorMetric {

	public MethodInfo method;
    public object target;

    public string name;
    public float currentValue = 0f;
    public bool targeted = true;

    public GeneratorMetric(MethodInfo method, string name){
        this.method = method;
        this.name = name;
        this.target = null;
    }

    public GeneratorMetric(MethodInfo method, object target, string name){
        this.name = name;
        this.method = method;
        this.target = target;
    }
}
