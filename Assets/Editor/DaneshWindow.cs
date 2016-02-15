using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;

public class DaneshWindow : EditorWindow{

    GUIStyle leftAlignText;
    GUIStyle rightAlignText;
    GUIStyle centerAlignText;

    GUIStyle centerAlignSubtext;

    GUIStyle leftAlignTextGhost;
    GUIStyle rightAlignTextGhost;

    GUIStyle errorTextStyle;
    GUIStyle labelStyle;
    GUIStyle tooltipStyle;
    GUIStyle buttonStyle;

    void SetupStyles(){
        leftAlignText = new GUIStyle();
        leftAlignText.alignment = TextAnchor.MiddleLeft;
        leftAlignText.fontSize = 14;

        rightAlignText = new GUIStyle();
        rightAlignText.alignment = TextAnchor.MiddleRight;
        rightAlignText.fontSize = 14;

        centerAlignText = new GUIStyle();
        centerAlignText.fontStyle = FontStyle.Bold;
        centerAlignText.alignment = TextAnchor.MiddleCenter;
        centerAlignText.fontSize = 16;

        centerAlignSubtext = new GUIStyle();
        centerAlignSubtext.alignment = TextAnchor.MiddleCenter;
        centerAlignSubtext.fontSize = 14;

        rightAlignTextGhost = new GUIStyle();
        rightAlignTextGhost.alignment = TextAnchor.MiddleRight;
        rightAlignTextGhost.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        rightAlignTextGhost.fontSize = 14;

        leftAlignTextGhost = new GUIStyle();
        leftAlignTextGhost.alignment = TextAnchor.MiddleLeft;
        leftAlignTextGhost.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        leftAlignTextGhost.fontSize = 14;

        errorTextStyle = new GUIStyle();
        errorTextStyle.alignment = TextAnchor.MiddleCenter;
        errorTextStyle.fontStyle = FontStyle.Bold;
        errorTextStyle.normal.textColor = Color.red;
        errorTextStyle.fontSize = 14;

        labelStyle = new GUIStyle();
        labelStyle.wordWrap = true;
        labelStyle.fontSize = 14;

        tooltipStyle = new GUIStyle();
        tooltipStyle.wordWrap = true;

        buttonStyle = new GUIStyle("button");
        buttonStyle.fontSize = 14;

    }

    public Texture2D GeneratorOutput;
    Texture2D textureToBeDisplayed;

    void SetupOutputCanvas(){
        GeneratorOutput = new Texture2D (500, 500, TextureFormat.ARGB32, false);
    }

    MonoBehaviour generator;
    MethodInfo generateMapMethod;
    MethodInfo contentVisualiser;

    ERAnalyser eranalyser;

    bool noGeneratorFound = false;

    public int x_axis_era = 0;
    public int y_axis_era = 0;
    int numberOfERARuns = 250;
    int numberOfRERARuns = 2000;
    string[] axis_options;

    public List<GeneratorMetric> metricList;

    bool beginSave;
    string saveName;
    bool beginLoad;
    List<string> possibleLoadFiles;
    int selectedFile;

    void OnGUI() {

        if(!wantsMouseMove){
            wantsMouseMove = true;
        }
        if (Event.current.type == EventType.MouseMove)
            Repaint ();

        // Event e = Event.current;
        // GUILayout.Label("Mouse pos: " + e.mousePosition);

        if(eranalyser == null){
            eranalyser = new ERAnalyser();
        }

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
        if(GUILayout.Button("Load Metrics", buttonStyle, GUILayout.Width(150))){
            SetupMetrics();
        }
        DrawOnGUISprite(scientistSprite);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        DrawOnGUISprite(botSprite);
        if(GUILayout.Button("Setup Generator", buttonStyle, GUILayout.Width(150))){
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



        GUILayout.Label("Targeted Parameters", centerAlignSubtext);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box("", GUILayout.Width(125), GUILayout.Height(1));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

         if(parameterList != null){
            for(int i=0; i<parameterList.Count; i++){
                GeneratorParameter p = parameterList[i];

                GUILayout.BeginHorizontal();

                p.activated = GUILayout.Toggle(p.activated, "", GUILayout.Width(20));
                GUILayout.Space(10);
                if(p.activated)
                    GUILayout.Label(p.name, leftAlignText, GUILayout.Width(200));
                else
                    GUILayout.Label(p.name, leftAlignTextGhost, GUILayout.Width(200));
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
        }

        GUILayout.Space(20);

        GUILayout.Label("Targeted Metrics", centerAlignSubtext);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box("", GUILayout.Width(125), GUILayout.Height(1));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if(metricList != null && metricTargets != null && metricInputs != null){
            for(int i=0; i<metricList.Count; i++){
                GeneratorMetric m = metricList[i];

                GUILayout.BeginHorizontal();
                m.targeted = GUILayout.Toggle(m.targeted, "", GUILayout.Width(20));
                GUILayout.Space(10);
                if(m.targeted)
                    GUILayout.Label(m.name, leftAlignText);
                else
                    GUILayout.Label(m.name, leftAlignTextGhost);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if(m.targeted)
                    GUILayout.Label("Target Value:", rightAlignText);
                else
                    GUILayout.Label("Target Value:", rightAlignTextGhost);
                GUILayout.Space(10);

                if(metricInputs != null)
                    metricInputs[i] = GUILayout.TextField(metricInputs[i], 25, GUILayout.Width(50));

                float res = 0f;
                if(float.TryParse(metricInputs[i], out res)){
                    metricTargets[i] = res;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
        }

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("Start Auto-Tuning",buttonStyle, GUILayout.Width(150))){
            DaneshAutoTuner at = new DaneshAutoTuner();
            //Construct the list of parameters
            List<GeneratorParameter> tuningParams = new List<GeneratorParameter>();
            foreach(GeneratorParameter p in parameterList){
                if(p.activated)
                    tuningParams.Add(p);
            }
            List<GeneratorMetric> tuningMetrics = new List<GeneratorMetric>();
            List<float> tuningTargets = new List<float>();
            for(int i=0; i<metricList.Count; i++){
                GeneratorMetric m = metricList[i];
                if(m.targeted){
                    tuningTargets.Add(metricTargets[i]);
                    tuningMetrics.Add(m);
                }
            }

            at.Tune(this, tuningParams, tuningMetrics, tuningTargets);
        }
        if(!showATTooltip && GUILayout.Button("?", GUILayout.Width(25))){
            showATTooltip = true;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if(showATTooltip){
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Label("Tick the parameters you want to modify, and set the metric values you would like the generated content to have. When you click Auto-Tune, Danesh will try to find parameter values that make the generator produce content close to the target metrics.", tooltipStyle, GUILayout.Width(300));

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Close", GUILayout.Width(100))){
                showATTooltip = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(10);
        }
        else{
            GUILayout.Space(20);
        }

        GUILayout.EndVertical();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        if(MainTab == Tabs.OUTPUT){
            GUILayout.Label("Generator Output", centerAlignText);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(hasDisplay){
                GUILayout.Label(textureToBeDisplayed);
            }
            else if(noGeneratorFound){
                GUILayout.Label("Danesh couldn't find a generation method. Did you use the [MapGenerator] attribute?", errorTextStyle);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(generator != null && !noGeneratorFound && GUILayout.Button("Generate Content", buttonStyle, GUILayout.Width(150))){
                GenerateMap();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else if(MainTab == Tabs.ERA){
            GUILayout.Label("Expressive Range Histogram", centerAlignText);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(hasDisplay){
                GUILayout.Label(textureToBeDisplayed);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(30);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Metric 1 (X-Axis):");
            int prior_x = x_axis_era;
            x_axis_era = EditorGUILayout.Popup(x_axis_era, axis_options);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Metric 2 (Y-Axis):");
            int prior_y = y_axis_era;
            y_axis_era = EditorGUILayout.Popup(y_axis_era, axis_options);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(30);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Back To Generator", buttonStyle)){
               SwitchToOutputMode();
               GenerateMap();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if(prior_x != x_axis_era || prior_y != y_axis_era){
                //Change
                DisplayTexture(eranalyser.GenerateGraphForAxes(x_axis_era, y_axis_era));
            }

        }

        /*
        GUILayout.Label("Metric 1 (X-Axis):");
        x_axis_era = EditorGUILayout.Popup(x_axis_era, axis_options);
        GUILayout.Label("Metric 2 (Y-Axis):");
        y_axis_era = EditorGUILayout.Popup(y_axis_era, axis_options);
        */

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
                // GUILayout.Space(10);
                // p.activated = GUILayout.Toggle(p.activated, "", GUILayout.Width(20));

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
        if(parameterList != null && !reallyReset && GUILayout.Button("Reset Generator", buttonStyle, GUILayout.Width(150))){
            reallyReset = true;
        }
        else if(reallyReset){
            GUILayout.Label("Really Reset?");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Yes", buttonStyle, GUILayout.Width(50))){
                foreach(GeneratorParameter p in parameterList){
                    p.field.SetValue(generator, p.originalValue);
                    p.currentValue = p.originalValue;
                }
                reallyReset = false;
            }
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("No", buttonStyle, GUILayout.Width(50))){
                reallyReset = false;
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(parameterList != null && !beginSave && GUILayout.Button("Save Configuration", buttonStyle, GUILayout.Width(150))){
            beginSave = true;
            beginLoad = false;
            saveName = GetRandomConfigName();
        }
        else if(beginSave){
            GUILayout.Label("Config. Name:");
            saveName = GUILayout.TextField(saveName, 20, GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Save", buttonStyle, GUILayout.Width(100))){
                //Save the configuration
                if (File.Exists(saveName+".param"))
                {
                    Debug.Log("File '"+saveName+"'' already exists.");
                    return;
                }
                var sr = File.CreateText(saveName+".param");
                foreach(GeneratorParameter p in parameterList){
                    sr.WriteLine(p.field.Name+":"+p.type+":"+p.currentValue);
                }
                sr.Close();

                beginSave = false;
            }
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Cancel", buttonStyle, GUILayout.Width(100))){
                beginSave = false;
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(parameterList != null && !beginLoad && GUILayout.Button("Load Configuration", buttonStyle, GUILayout.Width(150))){
            beginSave = false;
            beginLoad = true;
            possibleLoadFiles = new List<string>();

            DirectoryInfo root = new DirectoryInfo(".");
            FileInfo[] infos = root.GetFiles();
            // Debug.Log(infos.Length+" files found");
            foreach(FileInfo f in infos){
                // Debug.Log(f.Name);
                if(f.Name.EndsWith(".param")){
                    possibleLoadFiles.Add(f.Name);
                }
            }

            selectedFile = 0;
        }
        else if(beginLoad){
            if(possibleLoadFiles.Count == 0){
                GUILayout.Label("Found no files to load!");
                if(GUILayout.Button("Cancel", buttonStyle, GUILayout.Width(100))){
                    beginLoad = false;
                }
            }
            else{
                GUILayout.BeginVertical();
                GUILayout.Label("Select a file", centerAlignSubtext);
                selectedFile = EditorGUILayout.Popup("", selectedFile, possibleLoadFiles.ToArray(), GUILayout.Width(250));
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if(GUILayout.Button("Load", buttonStyle, GUILayout.Width(100))){
                    //Load the configuration
                    string line;
                    StreamReader theReader = new StreamReader(possibleLoadFiles[selectedFile]);
                    using (theReader){
                        do{
                            line = theReader.ReadLine();
                            if (line != null){
                                //Parse the line
                                string[] parts = line.Split(':');
                                foreach(GeneratorParameter p in parameterList){
                                    if(p.field.Name == parts[0]){
                                        p.ParseAndSetValue(parts[2]);
                                    }
                                }
                            }
                        }
                        while (line != null);
                    }
                    theReader.Close();

                    beginLoad = false;
                }
                GUILayout.FlexibleSpace();
                if(GUILayout.Button("Cancel", buttonStyle, GUILayout.Width(100))){
                    beginLoad = false;
                }
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        //
        GUILayout.BeginVertical();
        GUILayout.Label ("Expressive Range", centerAlignText);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(20);

        GUILayout.Space(20);

        numberOfERARuns = EditorGUILayout.IntField("Sample Size (ERA):", numberOfERARuns);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("Perform ER Analysis", buttonStyle, GUILayout.Width(150))){
            eranalyser.StartERA(this, numberOfERARuns);
            DisplayTexture(eranalyser.GenerateGraphForAxes(x_axis_era, y_axis_era));
            SwitchToERAMode();
        }
        if(!showERATooltip && GUILayout.Button("?", GUILayout.Width(25))){
            showERATooltip = true;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if(showERATooltip){
            GUILayout.BeginVertical();
            GUILayout.Label("An ER Analysis lets you analyse the kind of content the generator is currently making. Danesh uses the generator to make "+numberOfERARuns+" pieces of content (set by 'Sample Size', above) and then plots the metric scores of each piece of content on a histogram.", tooltipStyle, GUILayout.Width(300));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Close", GUILayout.Width(100))){
                showERATooltip = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        GUILayout.Space(20);

        numberOfRERARuns = EditorGUILayout.IntField("Sample Size (RERA):", numberOfRERARuns);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("Random ER Analysis", buttonStyle, GUILayout.Width(150))){
            eranalyser.StartRERA(this, numberOfRERARuns);
            DisplayTexture(eranalyser.GenerateGraphForAxes(x_axis_era, y_axis_era));
            SwitchToERAMode();
        }
        if(!showRERATooltip && GUILayout.Button("?", GUILayout.Width(25))){
            showRERATooltip = true;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

         if(showRERATooltip){
            GUILayout.BeginVertical();
            GUILayout.Label("A Random ER Analysis is a special kind of ERA that lets you see what your generator can do with different parameters. It randomises the parameters of the generator each time it generates a piece of content, creating a histogram of all the different kinds of output possible.", tooltipStyle, GUILayout.Width(300));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Close", GUILayout.Width(100))){
                showERATooltip = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        GUILayout.Space(20);
        GUILayout.EndVertical();

        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(10));
        GUILayout.Space(10);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    enum Tabs{ERA, OUTPUT};
    Tabs MainTab = Tabs.OUTPUT;

    public void SwitchToERAMode(){
        MainTab = Tabs.ERA;
    }

    public void SwitchToOutputMode(){
        MainTab = Tabs.OUTPUT;
    }

    bool hasDisplay = false;

    public void DisplayTexture(Texture2D tex){
        hasDisplay = true;
        textureToBeDisplayed = tex;
    }

    bool reallyReset = false;
    bool showERATooltip = false;
    bool showRERATooltip = false;
    bool showATTooltip = false;

    public object GenerateContent(){
        if(generateMapMethod != null){
            return generateMapMethod.Invoke(generator, new object[]{});
        }
        else{
            return null;
        }
    }

    public float GetMetric(int index, object[] content){
        GeneratorMetric m = metricList[index];
        return (float) m.method.Invoke(null, content);
    }

    public float GetMetric(GeneratorMetric m, object[] content){
        return (float) m.method.Invoke(null, content);
    }

    public void GenerateMap(){
        if(generator != null){
            if(generateMapMethod != null){
                object output = GenerateContent();
                UpdateDaneshWithContent(output);
                foreach(GeneratorMetric m in metricList){
                    m.currentValue = (float) m.method.Invoke(null, new object[]{output});
                }
            }
        }
    }

    Color solidColor = Color.black;
    Color wallColor = Color.white;

    public void UpdateDaneshWithContent(object content){
        if(contentVisualiser != null){
            SetupOutputCanvas();
            GeneratorOutput = (Texture2D) contentVisualiser.Invoke(generator, new object[]{content, GeneratorOutput});
            GeneratorOutput.Apply();
            DisplayTexture(GeneratorOutput);
            Repaint();
        }
    }

    // public void UpdateTextureWithMap(Tile[,] map){
    //     int sf = 10; int Width = map.GetLength(0); int Height = map.GetLength(1);
    //     SetupOutputCanvas();

    //     for(int i=0; i<Width; i++){
    //         for(int j=0; j<Height; j++){
    //             if(map[i,j].BLOCKS_MOVEMENT){
    //                 PaintPoint(GeneratorOutput, i, j, sf, solidColor);
    //             }
    //             else{
    //                 PaintPoint(GeneratorOutput, i, j, sf, wallColor);
    //             }
    //         }
    //     }

    //      //Replace texture
    //      GeneratorOutput.Apply();
    //      DisplayTexture(GeneratorOutput);
    //      Repaint();
    // }

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
        this.wantsMouseMove = true;
        SetupMetrics();
    }

    void SetupMetrics(){
        metricList = new List<GeneratorMetric>();
        List<string> list_axis_options = new List<string>();

        metricTargets = new List<float>();
        metricInputs = new List<string>();


        foreach(MonoBehaviour b in Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour))){
            foreach(MethodInfo method in b.GetType().GetMethods()){
                foreach(Attribute attr in method.GetCustomAttributes(false)){
                    if(attr is Metric){
                        // Debug.Log(((Metric)attr).Name);
                        if(method.IsStatic){
                            metricList.Add(new GeneratorMetric(method, ((Metric)attr).Name));
                            list_axis_options.Add(((Metric)attr).Name);
                            metricTargets.Add(0f);
                            metricInputs.Add("0");
                        }
                        else{
                            //Not sure what to do here...
                        }
                    }
                }
            }
        }

        axis_options = list_axis_options.ToArray();
    }

    public List<GeneratorParameter> parameterList;
    public List<float> metricTargets;
    List<string> metricInputs;

    void SetupGenerator(){
        noGeneratorFound = false;
        SwitchToOutputMode();

        parameterList = new List<GeneratorParameter>();

        foreach(FieldInfo field in generator.GetType().GetFields()){
            foreach(Attribute _attr in field.GetCustomAttributes(false)){
                if(_attr is TunableAttribute){
                    TunableAttribute attr = (TunableAttribute) _attr;
                    parameterList.Add(new GeneratorParameter(attr.Name, field.GetValue(generator), attr.MinValue, attr.MaxValue, field, generator));
                }
            }
        }

        foreach(MethodInfo method in generator.GetType().GetMethods()){
            foreach(Attribute attr in method.GetCustomAttributes(false)){
                if(attr is Generator){
                    // Debug.Log(generator.name + "." + method.Name);
                    generateMapMethod = method;
                }
                if(attr is Visualiser){
                    contentVisualiser = method;
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

     public string GetRandomConfigName(){
        string raw = @"nappy
        fanatical
        invincible
        private
        slim
        wide
        fast
        magnificent
        steady
        maddening
        hurt
        unequaled
        erratic
        squealing
        living
        rough
        messy
        ragged
        imaginary
        optimal
        guttural
        well-groomed
        alert
        grubby
        obscene
        gusty
        certain
        second-hand
        incredible
        marvelous
        closed
        ancient
        quirky
        legal
        ill-fated
        fallacious
        well-made
        filthy
        outgoing
        neighborly
        puzzled
        simplistic
        reminiscent
        strong
        noiseless
        obsequious
        melodic
        endurable
        far
        seemly
        afraid
        flashy
        tremendous
        scared
        detailed
        cheerful
        careful
        unable
        dangerous
        straight
        godly
        medical
        quick
        precious
        rustic
        famous
        dispensable
        nonstop
        ready
        amused
        dusty
        fascinated
        slippery
        null
        halting
        utopian
        wrong
        scrawny
        alike
        real
        unsuitable
        wanting
        amuck
        safe
        voracious
        periodic
        absent
        zany
        stale
        oafish
        glistening
        jittery
        faithful
        venomous
        acrid
        accurate
        late
        flaky
        meek
        healthy
        somber
        callous
        hissing
        dark
        enormous
        obtainable
        innocent
        juicy
        motionless
        assorted
        piquant
        irate
        youthful
        wholesale
        protective
        damaging
        wide-eyed
        lively
        enchanted
        skillful
        wet
        solid
        curvy
        mushy
        thick
        workable
        nosy
        yellow
        horrible
        parallel
        elegant
        crowded
        tall
        level
        billowy
        dead
        thoughtful
        tough
        versed
        annoyed
        muddled
        super
        dreary
        black
        statuesque
        giddy
        exotic
        redundant
        woozy
        glossy
        stiff
        abrasive
        panoramic
        dazzling
        abnormal
        kindly
        heartbreaking
        adorable
        ritzy
        abortive
        animated
        hesitant
        abstracted
        lacking
        torpid
        sad
        stingy
        chunky
        fresh
        energetic
        breezy
        insidious
        willing
        illegal
        alive
        supreme
        funny
        vacuous
        fluttering
        obnoxious
        fumbling
        extra-large
        magenta
        military
        hard-to-find
        receptive
        common
        decorous
        unsightly
        determined
        bashful
        thoughtless
        quack
        difficult
        better
        raspy
        frantic
        amusing
        wiggly";
        string[] adjs = raw.Split('\n');
        string raw_anims = @"builder,conceiver,generator,initiator,inventor,constructor,designer,deviser,organizer,planner,actualizer,author,father,mother,formulator,institutor,discoverer,producer,establisher,smith,shaper,spawner,founder,starter,effector,fabricator";
        string[] anims = raw_anims.Split(',');
        return Cap(adjs[UnityEngine.Random.Range(0, adjs.Length)].Trim()) + Cap(anims[UnityEngine.Random.Range(0, anims.Length)]);
     }

     public string Cap(string s){
        return char.ToUpper(s[0]) + s.Substring(1);
     }

}
