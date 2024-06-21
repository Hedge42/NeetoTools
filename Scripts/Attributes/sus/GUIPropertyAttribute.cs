#if UNITY_EDITOR

using UnityEngine;
using System;
using UnityEditor;
using Matchwork;

namespace N_
{
    /// <summary>
    /// Experimental...<br/>
    /// Check if <see cref="GUIPropertyAttribute"/> exists above the field<br/>
    /// </summary>
    public interface IPropertyGUI
    {
        public void OnGUI(Rect position, SerializedProperty property, GUIContent label);
    }

    /// <summary>
    /// Pass an <see cref="IPropertyGUI"/> type into the constructor
    /// </summary>
    public class GUIPropertyAttribute : PropertyAttribute
    {

        public Type type;
        public IPropertyGUI gui;
        public GUIPropertyAttribute(Type guiType) => ParseGUIType(type = guiType, out gui);

        /// <summary> Tries to dynamically create a GUI drawer instance of the provided type
        /// <br/>Would be better with generics but attributes don't support generics in this C# version</summary>
        /// <param name="type">A type which implements <see cref="IPropertyGUI"/></param>
        /// <param name="gui"><see cref="IPropertyGUI"/>instance</param>
        /// <returns>Was the parse successful?</returns>
        public static bool ParseGUIType(Type type, out IPropertyGUI gui)
        {
            gui = default;
            if (typeof(IPropertyGUI).IsAssignableFrom(type))
            {
                try
                {
                    gui = Activator.CreateInstance(type) as IPropertyGUI;
                    return true;
                }
                catch (Exception x)
                {
                    Debug.LogError($"Whoops! {x}");
                }
            }
            return false;
        }
    }


}
#endif