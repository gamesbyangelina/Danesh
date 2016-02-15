using UnityEngine;
using System.Collections;

public class DiamondSquareWorldGen : MonoBehaviour {

    float randomSeed;

    [Tunable(MinValue: 0, MaxValue: 255, Name: "Water Level")]
    public int waterLimit = 140;
    [Tunable(MinValue: 0, MaxValue: 255, Name: "Plains Level")]
    public int plainsLimit = 200;
    [Tunable(MinValue: 0, MaxValue: 255, Name: "Hills Level")]
    public int hillsLimit = 230;
    [Tunable(MinValue: 0, MaxValue: 255, Name: "Mountain Level")]
    public int mountainLimit = 245;

    public Color deepWaterColor;
    public Color shallowWaterColor;
    public Color beachColor;
    public Color plainsColor;
    public Color hillsColor;
    public Color snowColor;

    public int mapsize = 257;

    [Tunable(MinValue: 0f, MaxValue: 1f, Name: "Granularity/Zoom")]
    public float randChangeFactor = 0.54f;

    [Generator]
    public DSWorld GenerateDSWorld(){
        randomSeed = Random.Range(0, 1000);

        DSWorld w = new DSWorld(mapsize);

        // deepWaterColor = HexToColor("729E9A");
        // shallowWaterColor = HexToColor("B2CCDD");
        // beachColor = HexToColor("DBD2BF");
        // plainsColor = HexToColor("A4B87F");
        // hillsColor = HexToColor("756354");
        // snowColor = Color.white;

        float[,] data = new float[mapsize,mapsize];

        data[0,0] = randomSeed;
        data[0,mapsize-1] = randomSeed;
        data[mapsize-1,0] = randomSeed;
        data[mapsize-1,mapsize-1] = randomSeed;

        float h = 255;

        int sideLength = mapsize - 1;
        int iteration = 0;

        while(sideLength >= 2){
            iteration++;

            int halfSide = (int)(sideLength/2);


            int x = 0;
            while(x < mapsize - 1){
                int y = 0;
                while(y < mapsize - 1){

                    float avg = data[x,y] + data[x+sideLength,y] + data[x,y+sideLength] + data[x+sideLength,y+sideLength];
                    avg /= 4f;

                    data[x+halfSide,y+halfSide] = ((int)(avg + (Random.Range(0f,1f)*2*h) - h)+255)%255;

                    y = y + sideLength;
                }
                x = x + sideLength;
            }


            x = 0;
            while(x < mapsize - 1){
                int y = (int)((x+halfSide) % sideLength);

                while(y < mapsize - 1){

                    float avg = data[(x-halfSide+(mapsize-1))%(mapsize-1),y] + data[(x+halfSide)%(mapsize-1),y] + data[x,(y+halfSide)%(mapsize-1)] + data[x,(y-halfSide+(mapsize-1))%(mapsize-1)];
                    avg /= 4f;

                    avg = avg + (Random.Range(0f,1f)*2*h) - h;
                    data[x,y] = ((int)(avg)+255)%255;

                    if(x == 0)
                        data[mapsize-1,y] = ((int)(avg)+255)%255;
                    if(y == 0)
                        data[x,mapsize-1] = ((int)(avg)+255)%255;

                    y = y + sideLength;
                }

                x = x + halfSide;
            }

            sideLength = (int)(sideLength/2);

            if(iteration > 1)
                h = (int)(h * randChangeFactor);
        }

        //Elevation data complete
        w.elevation = data;

        return w;
    }

    //http://wiki.unity3d.com/index.php?title=HexConverter
    Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r,g,b, 255);
    }

    [Visualiser]
    public Texture2D Visualise(object _w, Texture2D target){
        DSWorld w = (DSWorld) _w;

        //scale factor
        //not really sure how to handle varying output size yet in danesh so for now we just overshoot slightly
        int sf = 2;

        target = new Texture2D (mapsize*sf, mapsize*sf, TextureFormat.ARGB32, false);

        if(sf < 0){
            //Do something to fix the problem...? Sure.
        }

        for(int i=0; i<w.elevation.GetLength(0); i++){
            for(int j=0; j<w.elevation.GetLength(1); j++){
                Color c = beachColor;
                float v = w.elevation[i,j];
                //Select a colour
                if(v < waterLimit-40){
                    c = deepWaterColor;
                }
                else if(v < waterLimit){
                    c = shallowWaterColor;
                }
                else if(v < waterLimit+5){
                    c = beachColor;
                }
                else if(v < plainsLimit){
                    // if(temp_data[i][j] < -5):
                        // col = white;
                    // elif(temp_data[i][j] < 5):
                        // if(random.randint(0, 255)/float(255) > (temp_data[i][j]+5)/float(10)):
                            // col = white
                        // else:
                            // col = grass;
                    // else:
                        c = plainsColor;
                }
                else if(v < hillsLimit && v < mountainLimit){
                    c = hillsColor;
                }
                else{
                    c = snowColor;
                }

                VisUtils.PaintPoint(target, i, j, sf, c);
                // target.SetPixel(i, j, new Color(v/255f, v/255f, v/255f, 1f));
                // target.SetPixel(i, j, c);
            }
        }
        target.Apply();

        return target;
    }

}
