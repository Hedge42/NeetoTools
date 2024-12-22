using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using System.Security.Policy;
using Neeto;

#if UNITY_EDITOR
using UnityEditor;
#endif


[ExecuteInEditMode]
public class UniqueID : MonoBehaviour, ISerializationCallbackReceiver
{
    #region editor
#if UNITY_EDITOR
    [CustomEditor(typeof(UniqueID))]
    class UniqueIDhEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var _target = target as UniqueID;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField(_target.id);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Generate new ID"))
            {
                _target.id = UniqueID.Generate();
            }
            else if (GUILayout.Button("Empty ID"))
            {
                _target.id = "";
            }
        }
    }
#endif
    #endregion

    // registration is initialized at game startup, no complicated logic
    void ISerializationCallbackReceiver.OnAfterDeserialize() => map.TryAdd(id, gameObject);
    void ISerializationCallbackReceiver.OnBeforeSerialize() { }

    public static readonly Dictionary<string, GameObject> map = new();
    public static string Generate() => Guid.NewGuid().ToString();

    [SerializeField, HideInInspector]
    private string id;
    public string ID => id;

}
