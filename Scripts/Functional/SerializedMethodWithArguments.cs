using System;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto.Exp
{

    [Serializable]
    public class SerializedMethodWithArguments
    {
#if UNITY_EDITOR
        [QuickAction] static void Test() => EditorWindow.GetWindow<TestWindow>();

        public class TestWindow : EditorWindow
        {
            public SerializedMethodWithArguments method;
            public SerializedMethodWithArguments method2;

            Editor editor;


            void OnGUI()
            {
                Editor.CreateCachedEditor(this, null, ref editor);
                editor.OnInspectorGUI();
            }

            public static void TestOne(Vector3 vector, MonoBehaviour mb) { }
            public static void TestTwo(SerializedMethodWithArguments mwa, int i) { }
        }

#endif

        public string methodName;
        public string declaringType;
        public SerializedArgument[] arguments;

        public MethodInfo GetMethod() => Type.GetType(declaringType).GetMethod(methodName);
    }

}
