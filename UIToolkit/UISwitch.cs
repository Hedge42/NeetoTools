using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Neeto
{
    [DefaultExecutionOrder(6969)]
    public class UISwitch : MonoBehaviour
    {
        [Indexer(nameof(pages), nameof(SetIndex))]
        public int index;

        [Note("call Switch(ui) and all other objects will be disabled")]
        [ReorderableList(ListStyle.Lined)]
        public UIController[] pages;


        void OnEnable()
        {
            SetIndex(index);
        }
        void OnDisable()
        {
            DisableAll();
        }

        public void DisableAll()
        {
            foreach (var ui in pages)
            {
                ui.Hide();
            }
        }
        public void SetIndex(int index)
        {
            Show(pages[this.index = index]);
        }
        public void Show(UIController ui)
        {
            foreach (var doc in pages)
            {
                if (doc != ui)
                {
                    doc.Hide();
                }
            }
            ui.Show();
        }
        public void Hide(UIController ui)
        {
            ui.Hide();
        }
    }


    public class IndexerAttribute : PropertyAttribute
    {
        public string methodName;
        public string arrayName;
        public IndexerAttribute(string arrayName, string methodName)
        {
            this.arrayName = arrayName;
            this.methodName = methodName;
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(IndexerAttribute))]
        public class IndexerDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                using (NGUI.Property(position, property))
                {
                    var attribute = this.attribute as IndexerAttribute;
                    var parent = property.Parent();
                    var array = parent != null ? parent.FindPropertyRelative(attribute.arrayName) : property.serializedObject.FindProperty(attribute.arrayName);

                    if (array.arraySize == 0)
                        return;

                    var index = EditorGUI.IntSlider(position, label, property.intValue, 0, array.arraySize - 1);

                    if (index != property.intValue)
                    {
                        property.intValue = index;

                        var target = property.FindMethodTarget(attribute.methodName, out var method);
                        method.Invoke(target, new object[] { index });
                    }
                }
            }
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                var attribute = this.attribute as IndexerAttribute;

                var parent = property.Parent();
                var array = parent != null ? parent.FindPropertyRelative(attribute.arrayName) : property.serializedObject.FindProperty(attribute.arrayName);

                return array.arraySize > 0 ? NGUI.FullLineHeight : 0f;
            }
        }
#endif
    }
}
