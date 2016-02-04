using UnityEngine;
using System.Collections;
using System.Reflection;

public class GeneratorParameter {

    public string name;
    public bool activated = true;

    public object minValue;
    public object maxValue;
    public object currentValue;
    public object originalValue;

    public FieldInfo field;
    public string type;

	public GeneratorParameter(string name, object currentValue, object minValue, object maxValue, FieldInfo field){
        this.name = name;

        this.currentValue = currentValue;
        this.originalValue = currentValue;
        this.minValue = minValue;
        this.maxValue = maxValue;

        this.field = field;
        if(currentValue is bool)
            type = "bool";
        if(currentValue is int)
            type = "int";
        if(currentValue is float)
            type = "float";
    }
}
