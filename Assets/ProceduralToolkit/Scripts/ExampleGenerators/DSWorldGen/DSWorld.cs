using UnityEngine;
using System.Collections;

public class DSWorld {

    public float[,] elevation;
    public float[,] rain;
    public float[,] temperature;

    public DSWorld (int size){
        elevation = new float[size,size];
        rain = new float[size,size];
        temperature = new float[size,size];
    }

}
