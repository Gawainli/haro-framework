using System;
using System.Diagnostics;
using System.Text;

namespace ProxFramework
{
    public enum LogLevel
    {
        Info,
        Success,
        Assert,
        Warning,
        Error,
        Exception,
    }

    [System.Flags]
    public enum OutputType
    {
        None = 0,
        Editor = 0x1,
        Gui = 0x2,
        File = 0x4
    }

    public static class PLogger
    {
        public static LogLevel filterLevel = LogLevel.Info;
        public static OutputType outputType = OutputType.Editor;

        public delegate void OnLogFunc(LogLevel type, string msg);

        public static event OnLogFunc OnLog;

        private static StringBuilder stringBuilder = new StringBuilder(1024);

        public static void Info(string msg)
        {
            Log(LogLevel.Info, msg);
        }

        public static void Assert(bool condition, string msg = "")
        {
            if (!condition)
            {
                Log(LogLevel.Assert, string.IsNullOrEmpty(msg) ? "Assert Failed" : msg);
            }
        }

        public static void Warning(string msg)
        {
            Log(LogLevel.Warning, msg);
        }

        public static void Error(string msg)
        {
            Log(LogLevel.Error, msg);
        }

        public static void Exception(string msg, Exception e)
        {
            Log(LogLevel.Exception, $"{msg}{e.Message}");
            UnityEngine.Debug.Log(e.StackTrace);
        }

        private static void Log(LogLevel type, string logString)
        {
            if (outputType == OutputType.None || type < filterLevel)
            {
                return;
            }

            StringBuilder infoBuilder = GetFormatString(type, logString);

            if (type >= LogLevel.Warning)
            {
                AppendStackTrace(infoBuilder);
            }

            string logStr = infoBuilder.ToString();

            switch (type)
            {
                case LogLevel.Info:
                case LogLevel.Success:
                    UnityEngine.Debug.Log(logStr);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(logStr);
                    break;
                case LogLevel.Assert:
                    UnityEngine.Debug.LogAssertion(logStr);
                    break;
                case LogLevel.Error:
                case LogLevel.Exception:
                    UnityEngine.Debug.LogError(logStr);
                    break;
            }

            OnLog?.Invoke(type, logStr);
        }

        private static StringBuilder GetFormatString(LogLevel logLevel, string logString)
        {
            stringBuilder.Clear();
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff");
            logString = logString.Replace("<", "[").Replace(">", "]").Replace("\0", "");

            switch (logLevel)
            {
                case LogLevel.Success:
                    stringBuilder.AppendFormat(
                        "><color=gray><b>[INFO] ► </b></color>[{0}] - <color=#00FF18>{1}</color>", timestamp,
                        logString);
                    break;
                case LogLevel.Info:
                    stringBuilder.AppendFormat("<color=gray><b>[INFO] ► </b></color>[{0}] - <color=gray>{1}</color>",
                        timestamp, logString);
                    break;
                case LogLevel.Assert:
                    stringBuilder.AppendFormat(
                        "<color=#FF00BD><b>[ASSERT] ► </b></color>[{0}] - <color=green>{1}</color>", timestamp,
                        logString);
                    break;
                case LogLevel.Warning:
                    stringBuilder.AppendFormat(
                        "<color=#FF9400><b>[WARNING] ► </b></color>[{0}] - <color=yellow>{1}</color>", timestamp,
                        logString);
                    break;
                case LogLevel.Error:
                    stringBuilder.AppendFormat("<color=red><b>[ERROR] ► </b></color>[{0}] - <color=red>{1}</color>",
                        timestamp, logString);
                    break;
                case LogLevel.Exception:
                    stringBuilder.AppendFormat("<color=red><b>[EXCEPTION] ► </b></color>[{0}] - <color=red>{1}</color>",
                        timestamp, logString);
                    break;
            }

            return stringBuilder;
        }

        private static void AppendStackTrace(StringBuilder infoBuilder)
        {
            infoBuilder.AppendLine();
            StackFrame[] stackFrames = new StackTrace().GetFrames();
            foreach (var frame in stackFrames)
            {
                string declaringTypeName = frame.GetMethod().DeclaringType.FullName;
                string methodName = frame.GetMethod().Name;
                infoBuilder.AppendLine($"{declaringTypeName}::{methodName}");
            }
        }
    }
}