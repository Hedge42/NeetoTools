using UnityEngine;
using System;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.Diagnostics;

namespace Neeto
{
    public static class DebugHelper
    {
        public static void Try(System.Action action, int messageType = 0, UnityEngine.Object link = null)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                if (messageType == 1)
                {
                    if (link) Debug.LogWarning(e.StackTrace, link);
                    else Debug.LogWarning(e.StackTrace);
                }
                else if (messageType == 2)
                {
                    if (link) Debug.LogError(e.StackTrace, link);
                    else Debug.LogError(e.StackTrace);
                }
            }
        }
        public static void Bench(Action a, string mesg = "Action")
        {
            mesg ??= "Action";
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            a.Invoke();
            watch.Stop();

            Debug.Log($"{mesg} took {watch.ElapsedMilliseconds} MS");
        }
        public static string GetExceptionInfo(this Exception e)
        {
            return e.Message.WithHTML(Color.red)
                + "\n\nfrom: " + e.Source
                + "in: " + e.StackTrace;
        }
        public static Exception Log(this Exception e, string text = null, Object context = null)
        {
            var info = e.GetExceptionInfo();
            text = text == null ? e.GetExceptionInfo() : text + "\n" + e.GetExceptionInfo();
            Debug.Log(text, context);
            return e;
        }
        public static Exception Log(this Exception e, Object context)
        {
            var text = e.GetExceptionInfo();
            Debug.Log(text, context);
            return e;
        }
        public static void LogWarning(this Exception e, string text, Object context = null)
        {
            text += e.GetExceptionInfo();
            Debug.LogWarning(text, context);
        }
        public static void LogError(this Exception e, string text, Object context = null)
        {
            text += e.GetExceptionInfo();
            Debug.LogError(text, context);
        }
        public static void Log(this bool b, string text, Object context = null)
        {
            if (b)
            {
                if (context)
                    Debug.Log(text, context);
                else
                    Debug.Log(text);
            }
        }
        public static void Log(string text)
        {
            Debug.Log(text);
        }

        public static object Trace(this object obj)
        {
            var stackTrace = new StackTrace(true); // true to capture file information
            var frames = stackTrace.GetFrames();

            if (frames == null) return string.Empty;

            var callStack = "";
            foreach (var frame in frames)
            {
                var method = frame.GetMethod();
                var fileName = frame.GetFileName();
                var lineNumber = frame.GetFileLineNumber();
                callStack += $"{method.DeclaringType.FullName}.{method.Name} in {fileName}:line {lineNumber}\n";
            }

            Debug.Log(callStack);

            obj.Trace();
            return obj;
        }
    }
}