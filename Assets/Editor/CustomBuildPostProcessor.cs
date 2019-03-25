using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

#if UNITY_XCODE_API_BUILD
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions
#else
using UnityEditor.iOS.Xcode.Custom;
using UnityEditor.iOS.Xcode.Custom.Extensions;
#endif
using System.IO;

namespace AssemblyCSharpEditor
{
	public class CustomBuildPostProcessor
	{


		[PostProcessBuildAttribute(1)]
		public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject) {

			if (buildTarget != BuildTarget.iOS)
				return;

#if UNITY_EDITOR_OSX

			Debug.Log("##########" +  pathToBuiltProject + "##########" );

			string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
			PBXProject proj = new PBXProject();
			proj.ReadFromFile(projPath);
			string target = proj.TargetGuidByName("Unity-iPhone");

			Debug.Log ("####### target : " + target + " ############");

			string guidNVS = proj.FindFileGuidByRealPath("Frameworks/Plugins/iOS/NVS.framework", PBXSourceTree.Source);

			Debug.Log ("####### NVS Guid : " + guidNVS + "###########");

			if (!string.IsNullOrEmpty (guidNVS)) {

				//把NVS.framework放到最后去
				proj.RemoveFile(guidNVS);
				proj.RemoveFileFromBuild (target, guidNVS);
				proj.RemoveFrameworkFromProject(target,guidNVS);

				//Debug.Log ("####### add NVS : " +  "###########");
				guidNVS =  proj.AddFile("Frameworks/Plugins/iOS/NVS.framework","Frameworks/Plugins/iOS/NVS.framework");
				if (string.IsNullOrEmpty(guidNVS))
					Debug.LogError("######### addFile Error ######");
				
				proj.AddFileToBuild (target, guidNVS);
				PBXProjectExtensions.AddFileToEmbedFrameworks(proj,target,guidNVS);
				proj.SetBuildProperty(target, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");

			}


            string stripPath = Path.Combine (pathToBuiltProject, "strip_archs.sh");
            File.Copy(Path.Combine(pathToBuiltProject,"../strip_archs.sh"),stripPath,true);

            string guidStripArchs = proj.AddFile("strip_archs.sh","strip_archs.sh");
            proj.AppendShellScriptBuildPhase (target, "strip_archs", "/bin/sh", "\"$PROJECT_DIR/strip_archs.sh\"");

            Debug.Log ("####### guidStripArchs Guid : " + guidStripArchs + "###########");
			//增加sqlite3库
			proj.AddFrameworkToProject(target, "libsqlite3.tbd",false);
#if GC_VOICE
			proj.AddFrameworkToProject(target, "libstdc++.6.0.9.tbd",false);
			proj.AddFrameworkToProject(target, "libresolv.tbd",false);
#endif

			//设置属性
			proj.SetBuildProperty(target,"OTHER_LDFLAGS","-weak_framework CoreMotion -weak-lSystem -ObjC");
			proj.SetBuildProperty(target,"ENABLE_BITCODE","NO");

			File.WriteAllText (projPath, proj.WriteToString ());

			Debug.Log("@@@@@@@@@@" +  pathToBuiltProject + "@@@@@@@@@@" );
#endif
		}
	}
}

