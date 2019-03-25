using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

/// <summary>
/// Unity command line build
/// </summary>
public class BuildCommand
{
    private static List<string> _levels = new List<string>();

    static Dictionary<string,string> ParseCommandArgs()
    {
        var args = System.Environment.GetCommandLineArgs();
        Dictionary<string, string> argsDict = new Dictionary<string, string>();
        for (int i = 0; i < args.Length;)
        {
            var tmp = args[i];
            int step = 1;
            int offset = 1;
            if (tmp.StartsWith("--"))
            {
                offset = 2;
            }
            else if (tmp.StartsWith("-"))
            {
                offset = 1;
            }
            else
            {
                i++;
                continue;
            }

            var arg = tmp.Substring(offset);
            argsDict[arg] = string.Empty;
            if (i + 1 < args.Length)
            {
                if (!args[i + 1].StartsWith("-"))
                {
                    argsDict[arg] = args[i + 1];
                    step++;
                }
            }
                
            i += step;
        }
        foreach(var item in argsDict)
        {
            Debug.Log("----> command line arg " + item.Key + " : " + item.Value);
        }
        return argsDict;
    }

    public static void BuildPlayer()
    {
        Dictionary<string, string> argsDict = ParseCommandArgs();
        foreach(var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                _levels.Add(scene.path);
        }
        
        BuildTarget target = BuildTarget.StandaloneWindows;
        BuildOptions options = BuildOptions.None;
        var timestamp = DateTime.Now.ToString("MMddHHmm");
        string filename = string.Format("nim_unity_win32_{0}.exe", timestamp);
        string output = null;
        if (argsDict.ContainsKey("p"))
        {
            var platrorm = argsDict["p"];
            if (platrorm == "windows" && argsDict.ContainsKey("b") && argsDict["b"] == "64")
            {
                target = BuildTarget.StandaloneWindows64;
                filename = string.Format("nim_unity_win64_{0}.exe", timestamp);
            }
                
            if (string.Compare(platrorm,"android",true) == 0)
            {
                target = BuildTarget.Android;
                filename = string.Format("nim_unity_android_{0}.apk", timestamp);
            }
               
            if (string.Compare(platrorm,"ios",true) == 0)
            {
                target = BuildTarget.iOS;
                filename = string.Format("nim_unity_iOS_{0}", timestamp);
            }
        }
        if (argsDict.ContainsKey("o"))
        {
            output = argsDict["o"];
        }
        if(argsDict.ContainsKey("m"))
        {
            var mode = argsDict["m"];
            if (string.Compare(mode, "debug", true) == 0)
                options |= (BuildOptions.Development | BuildOptions.AllowDebugging);
        }
        if(argsDict.ContainsKey("run"))
        {
            options |= BuildOptions.AutoRunPlayer;
        }
        if(!string.IsNullOrEmpty(output))
        {
            output = System.IO.Path.Combine(System.Environment.CurrentDirectory, output);
        }
        else
        {
            output = System.IO.Path.Combine(System.Environment.CurrentDirectory, "unity_demo_output/" + filename);
        }
        var targetdir = System.IO.Path.GetDirectoryName(output);
        if(!System.IO.Directory.Exists(targetdir))
        {
            System.IO.Directory.CreateDirectory(targetdir);
        }
        Debug.Log("target filepath:" + output);
        EditorUserBuildSettings.SwitchActiveBuildTarget(target);
        string res = BuildPipeline.BuildPlayer(_levels.ToArray(), output, target, options).ToString();
        Debug.Log("Build reuslt:" + res);
    }
}

[InitializeOnLoad]
public class PreloadKeystoreSetting
{
#if UNITY_ANDROID
    static PreloadKeystoreSetting()
    {
        PlayerSettings.Android.keystoreName = "Assets/Resources/nim_u3d.jks";
        PlayerSettings.Android.keyaliasName = "u3d";
        PlayerSettings.Android.keystorePass = "111111";
        PlayerSettings.Android.keyaliasPass = "111111";
}
#endif

}
