using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Text;

public static class ExceptHandler {
	public delegate string FnSendCallback(object arg);

	private static bool isInitialized = false;
	private static string lastExceptionName;
	private static string appid; 
	private const string EXCEPTION_TAG = "u3d-c#";
	private static string version = "1.1.6";
	private static FnSendCallback callbackFn = null;
	private static object callbackArg = null;
	
	public static void Init(string appID)
	{
		if (isInitialized) {
			System.Console.Write ("ExceptHandler is already initialized. ");
			return;
		}
		
		setAppID(appID);
		
		initExceptionHandler ();

		#if UNITY_5
		Application.logMessageReceived += _OnLogCallbackHandler;
		#else
		Application.RegisterLogCallback(_OnLogCallbackHandler);
		#endif

		System.AppDomain.CurrentDomain.UnhandledException += _OnUnresolvedExceptionHandler;
				
		lastExceptionName = "";
		
		isInitialized = true;
	}
	
	
	static private void _OnLogCallbackHandler(string name, string stack, LogType type)
	{
        if (!isInitialized || (LogType.Assert != type && LogType.Exception != type && LogType.Error != type))
		{
			return;
		}
		
		if (lastExceptionName == "" || lastExceptionName.CompareTo (name) != 0) {
			
			lastExceptionName = name;
			reportException(name, stack);
		}
	}
	
	private static void _OnUnresolvedExceptionHandler(object sender, System.UnhandledExceptionEventArgs args)
	{
		if (!isInitialized || args == null || args.ExceptionObject == null)
		{
			return;
		}
		
		if (args.ExceptionObject.GetType() != typeof(System.Exception))
		{
			return;
		}
		
		
		doLogError((System.Exception)args.ExceptionObject);
	}
	
	private static void doLogError(System.Exception e)
	{
		
		StackTrace stackTrace = new StackTrace(e, true);
		string[] classes = new string[stackTrace.FrameCount];
		string[] methods = new string[stackTrace.FrameCount];
		string[] files = new string[stackTrace.FrameCount];
		int[] lineNumbers = new int[stackTrace.FrameCount];
		
		string name = e.GetType().Name;
		string stack = "";
		
		for (int i = 0; i < stackTrace.FrameCount; i++)
		{
			StackFrame frame = stackTrace.GetFrame(i);
			classes[i] = frame.GetMethod().DeclaringType.Name;
			methods[i] = frame.GetMethod().Name;
			files[i] = frame.GetFileName();
			lineNumbers[i] = frame.GetFileLineNumber();
			
			stack += classes[i] + ".";
			stack += methods[i] + "() (at ";
			stack += files[i] + ":";
			stack += lineNumbers[i] + ")\n";
		}
		
		if (lastExceptionName == "" || lastExceptionName.CompareTo (name) != 0) {
			
			lastExceptionName = name;
			reportException(name,stack);
		}
	}
	
	
	private static void reportException(string name, string stack)
	{
		#if UNITY_ANDROID
		try {
			
			AndroidJavaClass clsCrashHandler = new AndroidJavaClass ("com.netease.nis.bugrpt.CrashHandler");
			if(clsCrashHandler != null) {
				string logText = name;
				logText += "\n";
				logText += stack;

				//当用户定义了回调时候，取出其中的字符串
				if(callbackFn != null){
					string userLog = callbackFn(callbackArg);
					if(userLog != null && userLog.Length > 0){
						clsCrashHandler.CallStatic("setUserTrackLog", userLog);
					}
				}
				
				clsCrashHandler.CallStatic<bool>("sendReportsBridge", logText, EXCEPTION_TAG);
			}
		}
		catch (System.Exception e)
		{
			System.Console.Write("reportException failed, an unexpected error: " + e.ToString());
		}
		#endif
		
		#if UNITY_IPHONE || UNITY_IOS
		try{
			if(name != null && name.Length > 1){
				string[] arr = name.Split(new char[] { ':' });
				if(arr != null && arr.Length > 1){
					if(callbackFn != null){
						string userLog = callbackFn(callbackArg);
						if(userLog != null && userLog.Length > 0){
							___setUserLog(userLog);
						}
					}

					if(Application.platform == RuntimePlatform.IPhonePlayer){;
						___reportException(arr[0],arr[1],stack);
					}
				}
			}
		}
		catch(System.Exception e){
			System.Console.Write("reportException failed, an unexpected error: " + e.ToString());
		}

		#endif
	}
	
	//Android提供启动ndk崩溃收集的功能
	public static void enableNdkCrashCollect()
	{
		#if UNITY_ANDROID
		try {
			
			using (var actClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
				AndroidJavaObject curActivityContext = actClass.GetStatic<AndroidJavaObject>("currentActivity");
				if(curActivityContext != null) {
					AndroidJavaClass clsCrashHandler = new AndroidJavaClass ("com.netease.nis.bugrpt.CrashHandler");
					if(clsCrashHandler != null) {
						clsCrashHandler.CallStatic("agentEnableNdkCrashCollect", curActivityContext);
					}
				}
			}
		}
		catch (System.Exception e)
		{
			System.Console.Write("enableNdkCrashCollect failed, an unexpected error: " + e.ToString());
		}
		#endif
    }
	
	private static void initExceptionHandler()
	{
		#if UNITY_ANDROID
		try {
			
			using (var actClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
				AndroidJavaObject curActivityContext = actClass.GetStatic<AndroidJavaObject>("currentActivity");
				if(curActivityContext != null) {
					AndroidJavaClass clsCrashHandler = new AndroidJavaClass ("com.netease.nis.bugrpt.CrashHandler");
					if(clsCrashHandler != null) {
						clsCrashHandler.CallStatic("agentInit", curActivityContext, appid, version, EXCEPTION_TAG);
					}
				}
			}
		}
		catch (System.Exception e)
		{
			System.Console.Write("initAndroidExceptionHandler failed, an unexpected error: " + e.ToString());
		}
		#endif

		#if UNITY_IPHONE || UNITY_IOS
		if(Application.platform == RuntimePlatform.IPhonePlayer){
			___setVersion(version);
			___init(appid);
		}
		#endif
    }

	private static void setAppID(string appID)
	{
		appid = appID;
	}

	//设置用户定义的回调函数，当发送时，调用回调函数获取log字符串，一并上传
	public static void setSendCrashCallback(FnSendCallback fn,object args)
	{
		callbackFn = fn;
		callbackArg = args;
	}

	//以下为FnSendCallback的示例
	public static string _sendCallback(object arg)
	{
		string strTrackInfo = "track log";
		
		//...用户自定义代码
		
		return strTrackInfo;
	}

	public static void setTestAddr(bool useTestAddr)
	{
		#if UNITY_IPHONE || UNITY_IOS
		if(Application.platform == RuntimePlatform.IPhonePlayer){
			___setUseTestAddr(useTestAddr);
		}
		#endif
	}

	[DllImport("__Internal")]
	private static extern void ___init (string appid);
	[DllImport("__Internal")]
	private static extern void ___setUseTestAddr (bool useTestAddr);
	[DllImport("__Internal")]
	private static extern void ___setVersion (string version);
	[DllImport("__Internal")]
	private static extern void ___reportException (string name, string reason, string stack);
	[DllImport("__Internal")]
	private static extern void ___setUserLog (string userLog);
}