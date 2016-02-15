using UnityEngine;
using System.Collections;

public class VisUtils : MonoBehaviour {

    public static void PaintPoint(Texture2D tex, int _x, int _y, int pointSize, Color c){
        int x = _x*pointSize; int y = _y*pointSize;
        for(int i=x; i<x+pointSize; i++){
            for(int j=y; j<y+pointSize; j++){
                tex.SetPixel(i, j, c);
            }
        }
    }

}
