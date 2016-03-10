using UnityEngine;
using System.Collections;

public class VisUtils : MonoBehaviour {

    public static void PaintTexture(Texture2D text, int _x, int _y, int sf, Texture2D t, int w, int h){
        Color[] pixels = t.GetPixels();
        Color[] orig_pixels = text.GetPixels(_x*sf, _y*sf, w, h);

        for(int i=0; i<pixels.Length; i++){
            pixels[i] = Color.Lerp(orig_pixels[i], pixels[i], pixels[i].a);
        }

        text.SetPixels(_x*sf, _y*sf, w, h, pixels);
    }

    public static void PaintPoint(Texture2D tex, int _x, int _y, int pointSize, Color c){
        int x = _x*pointSize; int y = _y*pointSize;
        for(int i=x; i<x+pointSize; i++){
            for(int j=y; j<y+pointSize; j++){
                tex.SetPixel(i, j, c);
            }
        }
    }

}
