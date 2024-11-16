using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    [Serializable]
    public abstract class AnimatorValue : ISerializationCallbackReceiver
    {
        public AnimatorParamterName Name;
        protected int id { get; private set; }

        public void OnAfterDeserialize() => id = Animator.StringToHash(Name);
        public void OnBeforeSerialize() { }
    }

    /// <summary>
    /// Optionally create a dropdown that allows selecting parameters by name instead of typing them out
    /// </summary>
    [Serializable]
    public struct AnimatorParamterName
    {
        #region EDITOR
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(AnimatorParamterName))]
        public class Drawer : PropertyDrawer
        {
            public static (string name, string path)[] values;
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                using (NGUI.Property(position, property, label))
                {
                    position = EditorGUI.PrefixLabel(position, label);
                    var nameProperty = property.FindPropertyRelative(nameof(AnimatorParamterName.Name));
                    var inputProperty = property.FindPropertyRelative(nameof(AnimatorParamterName.normalInput));


                    var buttonRect = position.With(xMin: position.xMax - NGUI.ButtonWidth);
                    var fieldRect = position.With(xMax: buttonRect.xMin);

                    if (GUI.Button(buttonRect, EditorGUIUtility.IconContent("d_Help@2x"), EditorStyles.iconButton))
                    {
                        inputProperty.boolValue = !inputProperty.boolValue;
                    }

                    if (inputProperty.boolValue)
                    {
                        nameProperty.stringValue = EditorGUI.TextField(fieldRect, nameProperty.stringValue);
                    }
                    else if (EditorGUI.DropdownButton(fieldRect, new GUIContent(nameProperty.stringValue), FocusType.Passive))
                    {
                        if (values == null)
                        {
                            values = AssetDatabase.FindAssets($"t: {nameof(RuntimeAnimatorController)}")
                                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                                .Select(path => AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(path))
                                .SelectMany(controller => controller.parameters.Select(param => (param.name, controller.name + "/" + param.name)))
                                .ToArray();
                        }
                        Debug.Log($"Showing...({values.Length})");
                        NDropdown.Show(result =>
                        {
                            nameProperty.stringValue = result;
                            nameProperty.ApplyAndMarkDirty();
                        }, true, true, nameProperty.stringValue, values);
                    }
                }
            }
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return NGUI.FullLineHeight;
            }
        }
#endif
        #endregion

        public static implicit operator string(AnimatorParamterName _) => _.Name;
        public bool normalInput;
        public string Name;
    }
    [Serializable]
    public class AnimatorFloat : AnimatorValue
    {
        public float GetValue(Animator animator)
        {
            return animator.GetFloat(id);
        }
        public void SetValue(Animator animator, float value)
        {
            animator.SetFloat(id, value);
        }
    }
    [Serializable]
    public class AnimatorBool : AnimatorValue
    {
        public bool GetValue(Animator animator)
        {
            return animator.GetBool(id);
        }
        public void SetValue(Animator animator, bool value)
        {
            animator.SetBool(id, value);
        }
    }

    public interface ISourcedAnimatorValue
    {
        void SetValue(Animator animator);
    }

    [Serializable]
    public struct SourcedAnimatorFloat : ISourcedAnimatorValue
    {
        [NoFoldout]
        public AnimatorFloat target;
        [NoFoldout]
        public FloatSource source;

        public void SetValue(Animator animator)
        {
            target.SetValue(animator, source);
        }
    }
    [Serializable]
    public struct SourcedAnimatorBool : ISourcedAnimatorValue
    {
        [NoFoldout] public AnimatorBool target;
        [NoFoldout] public BoolSource source;

        public void SetValue(Animator animator)
        {
            target.SetValue(animator, source);
        }
    }
}
