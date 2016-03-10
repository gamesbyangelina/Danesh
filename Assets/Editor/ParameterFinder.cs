using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class ParameterFinder {

    int numSamples = 5;

    public List<GeneratorParameter> FindParameters(DaneshWindow d, MonoBehaviour b){
        // Debug.Log("Searching for parameters... "+d.metricList.Count+" metrics found.");
        List<GeneratorParameter> res = new List<GeneratorParameter>();

        //Collect a control sample set
        List<float> metricAverages = new List<float>();
        List<float[]> stdev_samples = new List<float[]>();
        List<float> stdev = new List<float>();
        for(int i=0; i<d.metricList.Count; i++){
            metricAverages.Add(0);
            stdev_samples.Add(new float[numSamples]);
        }

        for(int s=0; s<numSamples; s++){
            object output = d.GenerateContent();
            for(int i=0; i<d.metricList.Count; i++){
                GeneratorMetric m = d.metricList[i];
                float sc = (float) m.method.Invoke(null, new object[]{output});
                metricAverages[i] += sc;
                stdev_samples[i][s] = sc;
            }
        }

        //Calculate averages and standard deviation
        for(int i=0; i<d.metricList.Count; i++){
            metricAverages[i] = metricAverages[i]/numSamples;
            // Debug.Log("Average for metric "+i+": "+metricAverages[i]);
            float sqdif = 0;
            foreach(float s in stdev_samples[i]){
                sqdif += Mathf.Pow(s - metricAverages[i], 2f);
            }
            sqdif = sqdif / numSamples;
            stdev.Add(Mathf.Sqrt(sqdif));
            // Debug.Log("Standard deviation for metric "+i+": "+stdev[i]);
        }

        foreach(FieldInfo field in d.generator.GetType().GetFields()){
            bool useParam = true;
            foreach(Attribute _attr in field.GetCustomAttributes(false)){
                if(_attr is TunableAttribute){
                    //We already know this parameter is good so don't worry.
                    useParam = false;
                }
            }
            if(!useParam)
                continue;

            //Remember the original value
            object original_value = field.GetValue(b);
            //Change the value to something
            List<object> sampleValues = GetSampleValues(field, b);
            // Debug.Log(sampleValues.Count+" sample values found");
            //For each sample value, change the field to that value, run samples, get average
            foreach(object o in sampleValues){
                field.SetValue(b, o);

                List<float> sampleData = new List<float>();
                for(int i=0; i<d.metricList.Count; i++){
                    sampleData.Add(0);
                }
                bool sampleFailed = false;
                for(int _=0; _<numSamples; _++){
                    try{
                        object output = d.GenerateContent();
                        for(int i=0; i<d.metricList.Count; i++){
                            GeneratorMetric m = d.metricList[i];
                            sampleData[i] += (float) m.method.Invoke(null, new object[]{output});
                        }
                    }
                    catch(Exception e){
                        //This failed, check the next value
                        sampleFailed = true;
                        break;
                    }
                }
                if(sampleFailed)
                    continue;
                //Did this change any of the metrics more than one standard deviation from the original sampling?
                bool nextParam = false;
                for(int i=0; i<d.metricList.Count; i++){
                    sampleData[i] = sampleData[i]/numSamples;
                    if(Mathf.Abs(sampleData[i] - metricAverages[i]) > stdev[i]){
                        Debug.Log("Parameter "+field.Name+" may have an impact on metrics");
                        nextParam = true;
                        GeneratorParameter p = new GeneratorParameter(field.Name, original_value, original_value, original_value, field, b);
                        p.locked = true;
                        res.Add(p);
                        break;
                    }
                }
                if(nextParam){
                    field.SetValue(b, original_value);
                    break;
                }
            }

            field.SetValue(b, original_value);
        }

        return res;
    }

    public List<object> GetSampleValues(FieldInfo f, object owner){
        List<object> res = new List<object>();

        object v = f.GetValue(owner);

        if(v is bool){
            //Not sure what the interplay is between null equality, so
            if((bool)f.GetValue(owner) != false)
                res.Add((object) false);
            if(!(bool)f.GetValue(owner) != true)
                res.Add((object) true);
            return res;
        }

        if(v is int){
            res.Add((object) -1);
            res.Add((object) 0);
            res.Add((object) 1);
            res.Add((object) UnityEngine.Random.Range(0, 100));
            return res;
        }

        if(v is float){
            res.Add((object) -1f);
            res.Add((object) 0f);
            res.Add((object) 1f);
            res.Add((object) UnityEngine.Random.Range(0f, 1f));
            res.Add((object) 100f);
            return res;
        }

        return res;

    }

}
