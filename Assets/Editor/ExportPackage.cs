using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

public class ExportPackage
{
    class ExportSetting
    {
        [Newtonsoft.Json.JsonProperty("dir")]
        public string Name { get; set; }

        [Newtonsoft.Json.JsonProperty("include")]
        public List<string> Files { get; set; }

        [Newtonsoft.Json.JsonProperty("exclude")]
        public List<string> Exclude { get; set; }

        [Newtonsoft.Json.JsonProperty("subs")]
        public List<ExportSetting> SubItems { get; set; }

        public List<string> GetAllFiles(string parent)
        {
            var path = System.IO.Path.Combine(parent, Name);
            List<string> includeFiles = new List<string>();
            if (Files != null)
            {
                foreach (var item in Files)
                {
                    var fullPath = System.IO.Path.Combine(path, item);
                    includeFiles.Add(fullPath);
                }
            }
            else
            {
                var files = System.IO.Directory.GetFiles(path);
                includeFiles.AddRange(files);
                if (files != null && Exclude != null)
                {
                    foreach (var item in files)
                    {
                        var fileName = System.IO.Path.GetFileName(item);
                        foreach (var ex in Exclude)
                        {
                            if (string.CompareOrdinal(fileName, ex) == 0)
                            {
                                includeFiles.Remove(item);
                            }
                        }
                    }
                }
            }
            if (SubItems != null && SubItems.Any())
            {
                foreach (var sub in SubItems)
                {
                    includeFiles.AddRange(sub.GetAllFiles(path));
                }
            }
            return includeFiles;
        }
    }

    public static void Export()
    {

        var text = System.IO.File.ReadAllText("Assets/Resources/exportconf.json");

        var conf = Newtonsoft.Json.JsonConvert.DeserializeObject<ExportSetting[]>(text);

        UnityEngine.Debug.Log("export packages...");

        List<string> exportFiles = new List<string>();

        foreach (var item in conf)
        {
            exportFiles.AddRange(item.GetAllFiles("Assets"));
        }

        AssetDatabase.ExportPackage(exportFiles.ToArray(), "NIM_Uinty_SDK.unitypackage", ExportPackageOptions.IncludeDependencies);
        AssetDatabase.ExportPackage(new string[] { "Assets" }, "NIM_Unity_All.unitypackage", ExportPackageOptions.Recurse);
    }

    static List<string> GetFilesRecurse(string path)
    {
        List<string> result = new List<string>();
        if (System.IO.Directory.Exists(path))
        {
            result.Add(path);
            var subDirs = System.IO.Directory.GetDirectories(path);
            foreach (var dir in subDirs)
            {
                result.AddRange(GetFilesRecurse(dir));
            }
            var subFiles = System.IO.Directory.GetFiles(path);
            result.AddRange(subFiles);
        }
        return result;
    }
}
