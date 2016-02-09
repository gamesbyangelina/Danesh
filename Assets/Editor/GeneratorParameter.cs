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

    public object owner;
    public FieldInfo field;
    public string type;

	public GeneratorParameter(string name, object currentValue, object minValue, object maxValue, FieldInfo field, object owner){
        this.name = name;

        this.currentValue = currentValue;
        this.originalValue = currentValue;
        this.minValue = minValue;
        this.maxValue = maxValue;

        this.field = field;
        this.owner = owner;
        if(currentValue is bool)
            type = "bool";
        if(currentValue is int)
            type = "int";
        if(currentValue is float)
            type = "float";
    }

    public void ParseAndSetValue(string s){
        // if(currentValue is bool)
            //???
        if(currentValue is int){
            int o;
            if(int.TryParse(s, out o))
                SetValue(o);
        }
        if(currentValue is float){
            float o;
            if(float.TryParse(s, out o))
                SetValue(o);
        }
    }

    public void SetValue(object o){
        field.SetValue(owner, o);
        currentValue = o;
    }

    public object GetValue(){
        return field.GetValue(owner);
    }

    public object GetRandomValue(){
        object temp = field.GetValue(owner);
        if(temp is int){
            return Random.Range((int)minValue, ((int)maxValue)+1);
        }
        else if(temp is float){
            return Random.Range((float)minValue, (float)maxValue);
        }
        else if(temp is bool){
            return Random.Range(0, 2) == 0;
        }

        return null;
    }

    public void RandomiseValue(){
        object temp = field.GetValue(owner);
        if(temp is int){
            field.SetValue(owner, Random.Range((int)minValue, ((int)maxValue)+1));
        }
        else if(temp is float){
            field.SetValue(owner, Random.Range((float)minValue, (float)maxValue));
        }
        else if(temp is bool){
            field.SetValue(owner, Random.Range(0, 2) == 0);
        }
    }

}
