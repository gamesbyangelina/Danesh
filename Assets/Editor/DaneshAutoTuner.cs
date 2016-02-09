using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class DaneshAutoTuner {

    List<GeneratorParameter> c_params;
    List<GeneratorMetric> c_metrics;
    List<float> c_targets;

    //Hill Climbing Parameters
    float floatChange = 0.05f;
    int intChange = 1;

    int iterations = 50;

	public void Tune(DaneshWindow dan, List<GeneratorParameter> ps, List<GeneratorMetric> ms, List<float> ts){
        this.c_params = ps;
        this.c_metrics = ms;
        this.c_targets = ts;

        float bestScore = 0f;
        object[] bestArray = new object[ps.Count];

        //Pick a random spot
        float time = Time.realtimeSinceStartup;

        object[] ex = new object[ps.Count];
        for(int s=0; s<ps.Count; s++){
            ex[s] = ps[s].GetRandomValue();
        }

        bestScore = Evaluate(ex, 50, dan);
        bestArray = ex;

        EditorUtility.DisplayProgressBar("Automatic Tuning In Progress", "Working...", 0f);

        for(int iter=0; iter<iterations; iter++){
            if(bestScore > 0.98f)
                break;

            //Find all of the neighbouring parameter sets
            List<object[]> nbs = CalculateNeighbours(ex);
            float bestThisRound = bestScore;
            object[] bestArrayThisRound = new object[c_params.Count];

            foreach(object[] nb in nbs){
                float score = Evaluate(nb, 50, dan);
                if(score > bestThisRound){
                    bestThisRound = score;
                    bestArrayThisRound = nb;
                }
            }

            if(bestThisRound > bestScore){
                bestScore = bestThisRound;
                bestArray = bestArrayThisRound;
                ex = bestArrayThisRound;
            }
            else{
                //Random restart
                ex = new object[c_params.Count];
                for(int s=0; s<c_params.Count; s++){
                    ex[s] = ps[s].GetRandomValue();
                }
            }

            EditorUtility.DisplayProgressBar("Auto-tuning", "Searching parameter space... "+(100*(float)iter/(float)iterations).ToString("F0")+" percent complete", (float)iter/(float)iterations);
        }

        EditorUtility.ClearProgressBar();

        //Apply the parameters
        object[] pvs = bestArray;
        for(int i=0; i<pvs.Length; i++){
            c_params[i].SetValue(pvs[i]);
        }
    }

    public float Evaluate(object[] arr, float samples, DaneshWindow dan){
        for(int i=0; i<c_params.Count; i++){
            c_params[i].field.SetValue(c_params[i].owner, arr[i]);
        }

        float totalScore = 0f;
        for(int att=0; att<samples; att++){
            Tile[,] map = dan.GenerateContent();
            float score = 0f;
            //Obtain a metric reading for every targeted metric
            for(int i=0; i<c_metrics.Count; i++){
                float val = (float)dan.GetMetric(c_metrics[i], new object[]{map});
                score += 1-Mathf.Abs(val - c_targets[i]);

            }
            //Get a score by averaging them
            totalScore += score / c_metrics.Count;
        }
        return totalScore/(float)samples;
    }

    List<object[]> CalculateNeighbours(object[] current){
        List<object[]> res = new List<object[]>();
        for(int i=0; i<current.Length; i++){
            Debug.Log(current[i]);
            object temp = c_params[i].GetValue();
            if(temp is int){
                if((int)current[i]+intChange < (int)c_params[i].maxValue){
                    object[] pu = new object[current.Length];
                    for(int j=0; j<current.Length; j++){
                        if(j != i)
                            pu[j] = current[j];
                        else{
                            pu[j] = (object) (((int)current[j])+intChange);
                        }
                    }
                    res.Add(pu);
                }
                if((int)current[i]-intChange > (int)c_params[i].minValue){
                    object[] pu = new object[current.Length];
                    for(int j=0; j<current.Length; j++){
                        if(j != i)
                            pu[j] = current[j];
                        else{
                            pu[j] = (object) (((int)current[j])-intChange);
                        }
                    }
                    res.Add(pu);
                }
            }
            if(temp is float){
                if((float)current[i]+floatChange < (float)c_params[i].maxValue){
                    object[] pu = new object[current.Length];
                    for(int j=0; j<current.Length; j++){
                        if(j != i)
                            pu[j] = current[j];
                        else{
                            pu[j] = (object) (((float)current[j])+floatChange);
                        }
                    }
                    res.Add(pu);
                }
                if((float)current[i]-intChange > (float)c_params[i].minValue){
                    object[] pu = new object[current.Length];
                    for(int j=0; j<current.Length; j++){
                        if(j != i)
                            pu[j] = current[j];
                        else{
                            pu[j] = (object) (((float)current[j])-floatChange);
                        }
                    }
                    res.Add(pu);
                }
            }
            // if(temp is bool){
            //     ParValue[] pu = new ParValue[current.Length];
            //     for(int j=0; j<current.Length; j++){
            //         if(j != i)
            //             pu[j] = new ParValue(current[j].field, current[j].val);
            //         else{
            //             pu[j] = new ParValue(current[j].field, !((bool)current[j].val));
            //         }
            //     }
            //     res.Add(pu);
            // }
        }
        return res;
    }

}
