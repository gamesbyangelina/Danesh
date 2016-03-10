using UnityEngine;
using System.Collections;

public class DSWorld {

    public int deepWaterLimit;
    public int waterLimit;
    public int plainsLimit;
    public int hillsLimit;
    public int mountainLimit;

    public float[,] elevation;
    public float[,] rain;
    public float[,] temperature;

    public DSWorld (int size){
        elevation = new float[size,size];
        rain = new float[size,size];
        temperature = new float[size,size];
    }

}
