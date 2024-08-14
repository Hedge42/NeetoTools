using System.Reflection;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;

namespace Neeto
{
    //[CustomEditor(typeof(ScriptableObject), true)]
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class MonoBehaviourEditorBase : Editor
    {
        public override void OnInspectorGUI()
        {
            try
            {
                if (target)
                {

                    //DrawDefaultInspector();
                    base.OnInspectorGUI();
                    DrawButtons(target);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                //Debug.Log($"haha ({e.GetType().Name})\n{e.Message}{e.StackTrace}", target);
            }
        }

        protected void IterateProperties(SerializedProperty iterator)
        {
            try
            {
                while (iterator.NextVisible(false))
                {
                    DrawProperty(iterator);
                }
            }
            catch { }
        }

        protected virtual bool DrawScriptProperty(out SerializedProperty prop)
        {
            try
            {
                prop = serializedObject.GetIterator();
                if (prop.NextVisible(true))
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(prop);
                    EditorGUI.EndDisabledGroup();
                }
                return true;
            }
            catch (Exception e)
            {
                prop = null;
                return false;
            }
        }
        protected virtual void DrawProperty(SerializedProperty prop)
        {
            EditorGUILayout.PropertyField(prop);
        }

        private void DrawButtons(object obj)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var methods = obj.GetType().GetMethods(flags);
            foreach (var m in methods)
            {
                var buttonAttribute = m.GetCustomAttribute<ButtonAttribute>();

                if (buttonAttribute != null)
                {
                    var valid = m.GetGenericArguments().Length == 0 && m.GetParameters().Length == 0;

                    if (valid)
                    {
                        var name = buttonAttribute.label ?? m.Name;
                        if (GUILayout.Button(m.Name))
                        {
                            m.Invoke(obj, null);

                            serializedObject?.Update();
                        }
                    }
                }
            }
        }
    }
}
#endif