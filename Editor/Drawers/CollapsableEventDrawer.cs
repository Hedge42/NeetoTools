using UnityEngine;
using UnityEngine.Events;
using Toolbox.Editor;
using Rhinox.Lightspeed;

namespace Neeto
{

#if UNITY_EDITOR
    using UnityEditorInternal;
    using UnityEditor;
    [CustomPropertyDrawer(typeof(UnityEventBase), true)]
    public class CollapsableEventDrawer : PropertyDrawer
    {
        UnityEventDrawer _drawer;
        UnityEventDrawer drawer => _drawer ??= new UnityEventDrawer();

        //bool foldout;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property))
            {
                if (property.IsExpanded(position))
                {
                    // draw default event
                    drawer.OnGUI(position, property, label);
                }
                else // add type arguments to label and show listener count
                {
                    var text = label.text + $" (";
                    var generics = fieldInfo.FieldType.GetGenericArguments();
                    for (int i = 0; i < generics.Length; i++)
                    {
                        text += generics[i].Name;
                        if (i < generics.Length - 1)
                        {
                            text += ", ";
                        }
                    }

                    // show listeners count
                    text += $") [{(property.GetProperValue(fieldInfo) as UnityEventBase).GetPersistentListeners().Count}]";
                    position = position.Move(xMin: 2f);
                    EditorGUI.DrawRect(position, Color.black * .25f);
                    EditorGUI.LabelField(position.Move(xMin: 5f), text);
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                return drawer.GetPropertyHeight(property, label);
            }
            else
            {
                return NGUI.LineHeight;
            }
        }
    }
#endif
}