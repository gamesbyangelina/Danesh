using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public class DaneshWindow : EditorWindow{

    GUIStyle leftAlignText;
    GUIStyle rightAlignText;
    GUIStyle centerAlignText;
    GUIStyle errorTextStyle;

    void SetupStyles(){
        leftAlignText = new GUIStyle();
        leftAlignText.alignment = TextAnchor.MiddleLeft;

        rightAlignText = new GUIStyle();
        rightAlignText.alignment = TextAnchor.MiddleRight;

        centerAlignText = new GUIStyle();
        centerAlignText.fontStyle = FontStyle.Bold;
        centerAlignText.alignment = TextAnchor.MiddleCenter;

        errorTextStyle = new GUIStyle();
        errorTextStyle.alignment = TextAnchor.MiddleCenter;
        errorTextStyle.fontStyle = FontStyle.Bold;
        errorTextStyle.normal.textColor = Color.red;
    }

    public Texture2D GeneratorOutput;

    void SetupOutputCanvas(){
        GeneratorOutput = new Texture2D (500, 500, TextureFormat.ARGB32, false);
    }

    MonoBehaviour generator;
    MethodInfo generateMapMethod;

    bool noGeneratorFound = false;

    void OnGUI() {

        SetupStyles();

        GUILayout.Space(15);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Generator:", GUILayout.Width(75));
        generator = (MonoBehaviour) EditorGUILayout.ObjectField("", generator, typeof(MonoBehaviour), true);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        //Header Section
        GUILayout.BeginHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Width(300));
        GUILayout.FlexibleSpace();
        DrawOnGUISprite(scientistSprite);
        if(GUILayout.Button("Load Metrics", GUILayout.Width(100))){
            SetupMetrics();
        }
        DrawOnGUISprite(scientistSprite);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        DrawOnGUISprite(botSprite);
        if(GUILayout.Button("Setup Generator", GUILayout.Width(100))){
            SetupGenerator();
        }
        else if(generator == null){
            GUILayout.Label("Please select a generator to instrument.");
        }
        DrawOnGUISprite(botSprite);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Width(300));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        /*
            MAIN EDITOR SPACE
        */

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(10));
        GUILayout.Space(10);
        GUILayout.EndVertical();

        //Left Column: Metrics + Auto-Tuning
        GUILayout.BeginVertical(GUILayout.Width(300));

        GUILayout.BeginVertical();
        GUILayout.Label ("Metrics", centerAlignText);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(20);

        if(metricList != null){
            for(int i=0; i<metricList.Count; i++){
                GeneratorMetric m = metricList[i];

                GUILayout.BeginHorizontal();
                GUILayout.Label(m.name, leftAlignText);
                GUILayout.FlexibleSpace();
                GUILayout.Label(Math.Round((Decimal)(float)m.currentValue, 3, MidpointRounding.AwayFromZero)+"", rightAlignText);
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
        }
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical();
        GUILayout.Label ("Auto-Tuning", centerAlignText);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(20);

        for(int i=0; i<metricList.Count; i++){
            GeneratorMetric m = metricList[i];

            GUILayout.BeginHorizontal();
            m.targeted = GUILayout.Toggle(m.targeted, "", GUILayout.Width(20));
            GUILayout.Space(10);
            GUILayout.Label(m.name, leftAlignText);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Value:", leftAlignText);
            GUILayout.Space(10);
            GUILayout.TextField("0", 25, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("Start Auto-Tuning", GUILayout.Width(150))){

        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.EndVertical();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.Label("Generator Output", centerAlignText);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(generator != null && !noGeneratorFound){
            GUILayout.Label(GeneratorOutput);
        }
        else if(noGeneratorFound){
            GUILayout.Label("Danesh couldn't find a generation method. Did you use the [MapGenerator] attribute?", errorTextStyle);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(generator != null && !noGeneratorFound && GUILayout.Button("Generate Content", GUILayout.Width(150))){
            GenerateMap();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(300));
        GUILayout.Label ("Generator Parameters", centerAlignText);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(20);

        if(parameterList != null){
            for(int i=0; i<parameterList.Count; i++){
                GeneratorParameter p = parameterList[i];

                GUILayout.BeginHorizontal();

                GUILayout.Label(p.name, rightAlignText);
                GUILayout.Space(10);
                p.activated = GUILayout.Toggle(p.activated, "", GUILayout.Width(20));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if(p.type == "float"){
                    object newVal = (object) GUILayout.HorizontalSlider((float) p.currentValue, (float)p.minValue, (float)p.maxValue);
                    if(newVal != p.currentValue){
                        //Change in the generator
                        p.field.SetValue(generator, newVal);
                    }
                    p.currentValue = newVal;
                    GUILayout.TextField(Math.Round((Decimal)(float)p.currentValue, 3, MidpointRounding.AwayFromZero)+"", 20, GUILayout.Width(50));
                }
                if(p.type == "int"){
                    object newVal = (object)(int) GUILayout.HorizontalSlider(Convert.ToInt32(p.currentValue), Convert.ToInt32(p.minValue), Convert.ToInt32(p.maxValue));
                    if(newVal != p.currentValue){
                        //Change in the generator
                        p.field.SetValue(generator, newVal);
                    }
                    p.currentValue = newVal;
                    GUILayout.TextField(Convert.ToInt32(p.currentValue)+"", 20, GUILayout.Width(50));
                }
                if(p.type == "bool")
                    Debug.Log("Todo");
                    //p.currentValue = (object) GUILayout.HorizontalSlider((bool) p.currentValue, 0.0F, 10.0F);
                // GUILayout.TextField(Math.Round((Decimal)(float)p.currentValue, 3, MidpointRounding.AwayFromZero)+"", 20, GUILayout.Width(50));
                GUILayout.EndHorizontal();
                GUILayout.Space(15);
            }
        }

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(!reallyReset && GUILayout.Button("Reset Generator", GUILayout.Width(100))){
            reallyReset = true;
        }
        else if(reallyReset){
            GUILayout.Label("Really Reset?");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Yes", GUILayout.Width(50))){
                foreach(GeneratorParameter p in parameterList){
                    p.field.SetValue(generator, p.originalValue);
                    p.currentValue = p.originalValue;
                }
                reallyReset = false;
            }
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("No", GUILayout.Width(50))){
                reallyReset = false;
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        //
        GUILayout.BeginVertical();
        GUILayout.Label ("Auto-Tuning", centerAlignText);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(20);

        for(int i=0; i<metricList.Count; i++){
            GeneratorMetric m = metricList[i];

            GUILayout.BeginHorizontal();
            m.targeted = GUILayout.Toggle(m.targeted, "", GUILayout.Width(20));
            GUILayout.Space(10);
            GUILayout.Label(m.name, leftAlignText);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Value:", leftAlignText);
            GUILayout.Space(10);
            GUILayout.TextField("0", 25, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("Start Auto-Tuning", GUILayout.Width(150))){

        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.EndVertical();

        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(10));
        GUILayout.Space(10);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    bool reallyReset = false;

    public void GenerateMap(){
        if(generator != null){
            if(generateMapMethod != null){
                Tile[,] output = (Tile[,]) generateMapMethod.Invoke(generator, new object[]{});
                UpdateTextureWithMap(output);
                foreach(GeneratorMetric m in metricList){
                    m.currentValue = (float) m.method.Invoke(null, new object[]{output});
                }
            }
        }
    }

    Color solidColor = Color.black;
    Color wallColor = Color.white;

    public void UpdateTextureWithMap(Tile[,] map){
        int sf = 10; int Width = map.GetLength(0); int Height = map.GetLength(1);
        SetupOutputCanvas();

        for(int i=0; i<Width; i++){
            for(int j=0; j<Height; j++){
                if(map[i,j].BLOCKS_MOVEMENT){
                    PaintPoint(GeneratorOutput, i, j, sf, solidColor);
                }
                else{
                    PaintPoint(GeneratorOutput, i, j, sf, wallColor);
                }
            }
        }

         //Replace texture
         GeneratorOutput.Apply();
         Repaint();
    }

    void PaintPoint(Texture2D tex, int _x, int _y, int scaleFactor, Color c){
        int x = _x*scaleFactor; int y = _y*scaleFactor;
        for(int i=x; i<x+scaleFactor; i++){
            for(int j=y; j<y+scaleFactor; j++){
                tex.SetPixel(i, j, c);
            }
        }
    }

    [MenuItem ("Window/DANESH")]
    public static void  ShowWindow () {
        EditorWindow.GetWindow(typeof(DaneshWindow));
    }

    private Sprite botSprite = null;
    private Sprite scientistSprite = null;

    void OnEnable()
    {
        botSprite = Resources.Load<Sprite>("bot");
        scientistSprite = Resources.Load<Sprite>("scientist");
        SetupMetrics();
    }

    List<GeneratorMetric> metricList;

    void SetupMetrics(){
        metricList = new List<GeneratorMetric>();

        foreach(MonoBehaviour b in Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour))){
            foreach(MethodInfo method in b.GetType().GetMethods()){
                foreach(Attribute attr in method.GetCustomAttributes(false)){
                    if(attr is Metric){
                        // Debug.Log(((Metric)attr).Name);
                        if(method.IsStatic){
                            metricList.Add(new GeneratorMetric(method, ((Metric)attr).Name));
                        }
                        else{
                            //Not sure what to do here...
                        }
                    }
                }
            }
        }
    }

    List<GeneratorParameter> parameterList;

    void SetupGenerator(){
        noGeneratorFound = false;

        parameterList = new List<GeneratorParameter>();

        foreach(FieldInfo field in generator.GetType().GetFields()){
            foreach(Attribute _attr in field.GetCustomAttributes(false)){
                if(_attr is TunableAttribute){
                    TunableAttribute attr = (TunableAttribute) _attr;
                    parameterList.Add(new GeneratorParameter(attr.Name, field.GetValue(generator), attr.MinValue, attr.MaxValue, field));
                }
            }
        }

        foreach(MethodInfo method in generator.GetType().GetMethods()){
            foreach(Attribute attr in method.GetCustomAttributes(false)){
                if(attr is MapGenerator){
                    // Debug.Log(generator.name + "." + method.Name);
                    generateMapMethod = method;
                }
            }
        }

        if(generateMapMethod == null){
            noGeneratorFound = true;
        }
        else{
            GenerateMap();
        }
    }

    void DrawOnGUISprite(Sprite aSprite)
     {
         Rect c = aSprite.rect;
         float spriteW = c.width;
         float spriteH = c.height;
         Rect rect = GUILayoutUtility.GetRect(spriteW, spriteH);
         rect.width = Mathf.Min(rect.width,rect.height);
         rect.height = rect.width;
         if (Event.current.type == EventType.Repaint)
         {
             var tex = aSprite.texture;
             c.xMin /= tex.width;
             c.xMax /= tex.width;
             c.yMin /= tex.height;
             c.yMax /= tex.height;

             GUI.DrawTextureWithTexCoords(rect, tex, c);
         }
     }
}
