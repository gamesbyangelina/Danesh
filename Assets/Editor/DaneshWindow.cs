using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;

public class DaneshWindow : EditorWindow{

    //Text and button styles
    GUIStyle leftAlignText;
    GUIStyle rightAlignText;
    GUIStyle centerAlignText;
    GUIStyle centerAlignSubtext;
    GUIStyle leftAlignTextGhost;
    GUIStyle rightAlignTextGhost;
    GUIStyle errorTextStyle;
    GUIStyle labelStyle;
    GUIStyle tooltipStyle;
    GUIStyle warningTTStyle;
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

        warningTTStyle = new GUIStyle();
        warningTTStyle.wordWrap = true;
        warningTTStyle.fontStyle = FontStyle.Bold;
        warningTTStyle.normal.textColor = Color.red;

        buttonStyle = new GUIStyle("button");
        buttonStyle.fontSize = 14;
    }

    //Texture variable stored to be displayed in the repaint actions.
    public Texture2D GeneratorOutput;
    Texture2D textureToBeDisplayed;

    //Variable size textures are currently possible, in future we'll restrict this and/or rescale after drawing most likely.
    void SetupOutputCanvas(){
        GeneratorOutput = new Texture2D (500, 500, TextureFormat.ARGB32, false);
    }

    Rect eraRect;

    //The MBs where the [Generator] and [Metric] methods are found
    //In future we may relax constraints on where [Metric] methods are stored
    public MonoBehaviour generator;
    public MonoBehaviour analyser;
    //Specific methods for generating a map and drawing content objects to texture
    MethodInfo generateMapMethod;
    MethodInfo contentVisualiser;

    ERAnalyser eranalyser;

    bool noGeneratorFound = false;

    //Which metrics are we showing ERAs of currently
    public int x_axis_era = 0;
    public int y_axis_era = 0;
    string[] axis_options;
    //Settings for the ERA and RERA runs
    int numberOfERARuns = 250;
    int numberOfRERARuns = 2000;

    //List of all discovered metrics
    public List<GeneratorMetric> metricList;

    //UI vars for the save/load dialogs, such as they are
    bool beginSave;
    string saveName;
    bool beginLoad;
    List<string> possibleLoadFiles;
    int selectedFile;

    void OnGUI() {
        //Still developing the mouseover interface for ERAs, I believe this is necessary to force an update when the mouse position changes.
        if(!wantsMouseMove){
            wantsMouseMove = true;
        }

        if(eranalyser == null){
            eranalyser = new ERAnalyser();
        }

        //This probably needs to be called on wake
        SetupStyles();

        GUILayout.Space(15);

        /*
            HEADER SECTION

            This includes the 'Click here to load a new generator!' and 'Load metrics!' section with Monobehaviour selector.
            In future this area may include the tabbing environment for switching between user modes.
        */
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal(GUILayout.Width(300));
        GUILayout.FlexibleSpace();
        GUILayout.Label("Metrics:",rightAlignText, GUILayout.Width(75));
        analyser = (MonoBehaviour) EditorGUILayout.ObjectField("", analyser, typeof(MonoBehaviour), true);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Width(300));
        GUILayout.FlexibleSpace();
        DrawOnGUISprite(scientistSprite);
        if(analyser == null){
            GUILayout.Label("Please select a MonoBehaviour with metrics to load");
        }
        else if(GUILayout.Button("Load Metrics", buttonStyle, GUILayout.Width(150))){
            SetupMetrics(analyser);
        }
        DrawOnGUISprite(scientistSprite);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Generator:", rightAlignText, GUILayout.Width(75));
        generator = (MonoBehaviour) EditorGUILayout.ObjectField("", generator, typeof(MonoBehaviour), true, GUILayout.Width(300));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        DrawOnGUISprite(botSprite);

        if(generator == null){
            GUILayout.Label("Please select a generator to instrument.");
        }
        else{
            if(GUILayout.Button("Setup Generator", buttonStyle, GUILayout.Width(150))){
                SetupGenerator();
            }
        }
        DrawOnGUISprite(botSprite);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal(GUILayout.Width(300));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        /*
            MAIN EDITOR SPACE

            Currently this area is split into three regions: the left-hand bar contains metrics and auto-tuning,
            the central pane contains the main visual content (either the generator output or the ERA analysis),
            and the right-hand bar contains discovered parameters + settings menu, and the ERA interface.
        */

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(10));
        GUILayout.Space(10);
        GUILayout.EndVertical();

        /*
            METRICS

            For each discovered metric, display its current value here and its name. This is updated whenever a new
            piece of content is generated.
        */
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
                if(Double.IsNaN(m.currentValue))
                    m.currentValue = 0;
                GUILayout.Label(Math.Round((Decimal)(float)m.currentValue, 3, MidpointRounding.AwayFromZero)+"", rightAlignText);
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
        }
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        /*
            AUTO-TUNING

            Auto-tuning allows Danesh to automatically discover parameter configuraitons to achieve a certain average metric value.
            The user can select which parameters to change by checking boxes, and then check metrics to target and input values for them.
        */

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
                if(p.locked)
                    continue;

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

        /*
            MAIN PANE

            We display a lot of information here. At the moment, what we show is based on the MainTab enum value. I'm going to change the way this is rendered eventually
            and break some of it out into other files or parts of this file. I get the feeling that Unity UIs are supposed to be a bit clunky but they shouldn't be this
            cluttered, I don't think.
        */

        GUILayout.BeginVertical();

        //Display the output of the generator, using the textureToBeDisplayed variable.
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
        //Display the output of the ERA/RERA here, with selectors to change the axes.
        //TODO: Draw axes on/near the image to show the metric values for X/Y
        else if(MainTab == Tabs.ERA){
            GUILayout.Label("Expressive Range Histogram", centerAlignText);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(hasDisplay){
                GUILayout.Label(textureToBeDisplayed);
                if(Event.current.type == EventType.Repaint){
                    eraRect = GUILayoutUtility.GetLastRect();
                }
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

        GUILayout.EndVertical();

        /*
            RIGHT COLUMN

            Parameters and the ERA/RERA setup interface.
        */

        GUILayout.BeginVertical(GUILayout.Width(300));
        GUILayout.Label ("Generator Parameters", centerAlignText);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(20);

        /*
            PARAMETERS

            Parameters are displayed with a slider that lets you change their value. The textbox shows their current value
            but it's slightly misleading because it can't be edited at the moment. I might change that but there's feedback
            issues in updating all the UI stuff at once. It's a nice feature but not a priority.

            If the Parameter is 'locked' this means it was discovered by Danesh but does not have all the information it needs
            yet. It prompts the user to add in information (min/max values) and then lock or discard the parameter.
        */

        if(parameterList != null){
            for(int i=0; i<parameterList.Count; i++){
                GeneratorParameter p = parameterList[i];

                GUILayout.BeginHorizontal();

                if(!p.locked)
                    GUILayout.Label(p.name, rightAlignText);
                else if(p.locked)
                    GUILayout.Label("New Parameter: "+p.name, leftAlignText);

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if(!p.locked){
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
                    if(p.type == "bool"){
                        GUILayout.FlexibleSpace();
                        bool cval = (bool) p.currentValue;
                        cval = (bool) GUILayout.Toggle(cval, "", GUILayout.Width(20));
                        p.field.SetValue(generator, (object) cval);
                        p.currentValue = (object) cval;
                    }
                }
                else{
                    if(p.type == "float"){
                        GUILayout.Label("Min: ", rightAlignText);
                        string mnv = GUILayout.TextField((Decimal)(float)(p.minValue)+"", 20, GUILayout.Width(40));
                        GUILayout.Label("Max: ", rightAlignText);
                        string mxv = GUILayout.TextField((Decimal)(float)(p.maxValue)+"", 20, GUILayout.Width(40));
                        if(GUILayout.Button("\u2713", buttonStyle, GUILayout.Width(30))){
                            p.ParseSetMinValue(mnv);
                            p.ParseSetMaxValue(mxv);
                            p.locked = false;
                        }
                        if(GUILayout.Button("\u00D7", buttonStyle, GUILayout.Width(30))){
                            parameterList.RemoveAt(i);
                        }
                        GUILayout.TextField(Math.Round((Decimal)(float)p.currentValue, 3, MidpointRounding.AwayFromZero)+"", 20, GUILayout.Width(50));
                    }
                    if(p.type == "int"){
                        GUILayout.Label("Min: ", rightAlignText);
                        string mnv = GUILayout.TextField(Convert.ToInt32(p.minValue)+"", 20, GUILayout.Width(40));
                        p.ParseSetMinValue(mnv);
                        GUILayout.Label("Max: ", rightAlignText);
                        string mxv = GUILayout.TextField(Convert.ToInt32(p.maxValue)+"", 20, GUILayout.Width(40));
                        p.ParseSetMaxValue(mxv);
                        if(GUILayout.Button("\u2713", buttonStyle, GUILayout.Width(30))){
                            p.ParseSetMinValue(mnv);
                            p.ParseSetMaxValue(mxv);
                            p.locked = false;
                        }
                        if(GUILayout.Button("\u00D7", buttonStyle, GUILayout.Width(30))){
                            parameterList.RemoveAt(i);
                        }
                        GUILayout.TextField(Convert.ToInt32(p.currentValue)+"", 20, GUILayout.Width(50));
                    }
                    if(p.type == "bool"){
                        bool cval = (bool) p.currentValue;
                        GUILayout.FlexibleSpace();

                        p.currentValue = (bool) GUILayout.Toggle(cval, "", GUILayout.Width(20));

                        if(GUILayout.Button("\u2713", buttonStyle, GUILayout.Width(30))){
                            p.locked = false;
                        }
                        if(GUILayout.Button("\u00D7", buttonStyle, GUILayout.Width(30))){
                            parameterList.RemoveAt(i);
                        }
                    }
                }
                //p.currentValue = (object) GUILayout.HorizontalSlider((bool) p.currentValue, 0.0F, 10.0F);
                // GUILayout.TextField(Math.Round((Decimal)(float)p.currentValue, 3, MidpointRounding.AwayFromZero)+"", 20, GUILayout.Width(50));
                GUILayout.EndHorizontal();
                GUILayout.Space(15);
            }
        }

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(parameterList != null && metricList != null && GUILayout.Button("Search For Parameters", buttonStyle, GUILayout.Width(200))){
            ParameterFinder pf = new ParameterFinder();
            List<GeneratorParameter> ps = pf.FindParameters(this, generator);
            foreach(GeneratorParameter p in ps){
                parameterList.Add(p);
            }
        }
        if(!showParamSearchTooltip && GUILayout.Button("!", GUILayout.Width(25))){
            showParamSearchTooltip = true;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if(showParamSearchTooltip){
            GUILayout.BeginVertical();
            GUILayout.Label("Warning: This is an experimental feature and can change the internal workings of your generator. Back up before using this!", warningTTStyle, GUILayout.Width(300));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Close", GUILayout.Width(100))){
                showParamSearchTooltip = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        /*
            RESET/SAVE/LOAD

            Avoiding side effects in the generator is a really important objective for Danesh. We can't avoid all side effects without
            knowing what the generator code does, of course, but we do our best. One piece of functionality is the reset button which restores
            parameter values to what they were on load. It does not undo other unseen changes.

            Saving and loading serialises to text files. This is basic and there are no safety catches, meaning it's possible to load a config
            for a different generator. We can secure this later by connecting the monobehaviour id to it or something.
        */

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

        /*
            EXPRESSIVE RANGE

            See the details on danesh.procjam.com for more information about what expressive range analysis consists of.
            This runs the RERA and ERA functions and updates the main context pane with the histogram results when done.
        */
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
            Texture2D tex = eranalyser.GenerateGraphForAxes(x_axis_era, y_axis_era);
            DisplayTexture(tex);
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

        /*
            This is the current code that detects mouse movement and position over the ERA and paints in a data point if it exists.
            It needs rewriting, badly.
        */

        if (MainTab == Tabs.ERA && Event.current.type == EventType.MouseMove){

            float tex_mx = Event.current.mousePosition.x - eraRect.x;
            float tex_my = Event.current.mousePosition.y - eraRect.y + 5;

            /*
                Hi, welcome to Open Source With Mike Cook, today I'll be revealing my secrets to writing the worst code in human history
            */

            int x = (int)(100*(tex_mx/eraRect.width));
            if(x < 0) x = 0;
            if(x > 99) x = 99;
            int y = 100-(int)(100*(tex_my/eraRect.height));
            if(y < 0) y = 0;
            if(y > 99) y = 99;

            Debug.Log("Cursor at "+tex_mx+", "+tex_my+". Within "+eraRect.width+", "+eraRect.height+". "+(tex_mx > 0 && tex_my > 0 && tex_mx < eraRect.width && tex_my < eraRect.height));

            if(tex_mx > 0 && tex_my > 0 && tex_mx < eraRect.width && tex_my < eraRect.height){
                Debug.Log("Cursor at "+tex_mx+", "+tex_my+". Within "+eraRect.width+", "+eraRect.height+". "+(tex_mx > 0 && tex_my > 0 && tex_mx < eraRect.width && tex_my < eraRect.height));

                // int x = 100*(int)(tex_mx/eraRect.width);
                // int y = 100*(int)(tex_my/eraRect.height);

                Debug.Log("Looking for metrics "+x+", "+y);
                Debug.Log(" - "+(eranalyser.eraSamples[x_axis_era][y_axis_era][x,y] != null));
                Debug.Log(" - "+(eranalyser.eraSamples[x_axis_era][y_axis_era][y,x] != null));

                //If the cursor target is null, be generous and find an adjacent one, because it's really hard to find the exact spot right now
                if(eranalyser.eraSamples[x_axis_era][y_axis_era][x,y] != null){
                    for(int i=-1; i<2; i++){
                        for(int j=-1; j<2; j++){
                            if((i != 0 || j != 0) && x+i >= 0 && x+i < 100 && y+j >=0 && y+j < 100 && eranalyser.eraSamples[x_axis_era][y_axis_era][x+i,y+j] != null){
                                x = x+i;
                                y = y+j;
                                break;
                            }
                        }
                    }
                }

                if(eranalyser.eraSamples[x_axis_era][y_axis_era][x,y] != null && (x != last_era_x || y != last_era_y)){
                    Debug.Log("Generating a new preview window");
                    last_era_preview = VisualiseContent(eranalyser.eraSamples[x_axis_era][y_axis_era][x,y]);
                    TextureScale.Point(last_era_preview, 100, 100);
                    last_era_preview.Apply();
                    last_era_x = x;
                    last_era_y = y;
                    Repaint ();
                }
                if(eranalyser.eraSamples[x_axis_era][y_axis_era][x,y] == null && (x != last_era_x || y != last_era_y)){
                    last_era_preview = null;
                    last_era_x = 0;
                    last_era_y = 0;
                    // GUI.DrawTexture(new Rect(tex_mx, tex_my, 100, 100), last_era_preview);
                }
                else{
                    // last_era_preview = null;
                }

            }
            else{
                //Outside of the ERA, nullify
                last_era_preview = null;
                last_era_x = 0;
                last_era_y = 0;
            }
        }
        else if(MainTab == Tabs.ERA && Event.current.type == EventType.Repaint){
            if(last_era_preview != null){
                GUI.DrawTexture(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 100, 100), last_era_preview);
                // GUILayout.Label(last_era_preview);
            }
        }
    }

    int last_era_x = 0;
    int last_era_y = 0;
    Texture2D last_era_preview;

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
    bool showParamSearchTooltip = false;
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

    public Texture2D VisualiseContent(object content){
        return (Texture2D) contentVisualiser.Invoke(generator, new object[]{content, new Texture2D (500, 500, TextureFormat.ARGB32, false)});
    }

    public void UpdateDaneshWithContent(object content){
        if(contentVisualiser != null){
            SetupOutputCanvas();
            GeneratorOutput = (Texture2D) contentVisualiser.Invoke(generator, new object[]{content, GeneratorOutput});
            // GeneratorOutput.Resize(500,500);
            TextureScale.Point(GeneratorOutput, 800, 800);
            GeneratorOutput.Apply();
            DisplayTexture(GeneratorOutput);
            Repaint();
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
        this.wantsMouseMove = true;
        SetupMetrics(analyser);
    }

    void SetupMetrics(MonoBehaviour tgt){
        metricList = new List<GeneratorMetric>();
        List<string> list_axis_options = new List<string>();

        metricTargets = new List<float>();
        metricInputs = new List<string>();

        foreach(MethodInfo method in tgt.GetType().GetMethods()){
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

     /*
        Rubbish attempt at a gfycat-style naming scheme. Needs to be moved out of this code.
     */

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
