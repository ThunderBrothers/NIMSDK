using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NIM
{
    /// <summary>
    /// native dll 信息
    /// </summary>
    public class NativeConfig
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        public const string NIMNativeDLL = "nim";
        public const string NIMAudioNativeDLL = "nim_audio";
        public const string ChatRoomNativeDll = "nim_chatroom";
#elif UNITY_IOS
        public const string NIMNativeDLL = "__Internal";
        public const string NIMAudioNativeDLL = "__Internal";
        public const string NIMHttpNativeDLL = "__Internal";
        public const string ChatRoomNativeDll = "__Internal";
#elif UNITY_ANDROID
        public const string NIMNativeDLL = "nim";
        public const string NIMAudioNativeDLL = "nim_audio";
        public const string ChatRoomNativeDll = "nim_chatroom";
//#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
//		public const string NIMNativeDLL = "nim_sdk";
//		public const string NIMAudioNativeDLL = "nim_audio_sdk";
//		public const string ChatRoomNativeDll = "nim_chatroom_sdk";
#endif

    }
}
