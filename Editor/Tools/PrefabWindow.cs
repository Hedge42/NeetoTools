using UnityEngine;
using System;
using System.Collections;
using Object = UnityEngine.Object;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
#if UNITY_EDITOR
    public class PrefabWindow : EditorWindow
    {
        public List<GameObject> selections;

        public GameObject prefab;

        //bool showSelected = true;

        Editor editor;

        // Called when this window is open or on scripts loaded
        private void OnEnable()
        {
            selections ??= new List<GameObject>();
        }

        // Opens this editor window
        [MenuItem(MenuPath.Open + nameof(PrefabWindow), priority = MenuOrder.Top)]
        public static PrefabWindow Open()
        {
            var window = GetWindow<PrefabWindow>(false, "Prefab Editor", true);
            window.Show();
            return window;
        }

        interface ITransformOperation
        {
            public void Execute(Transform transform);
        }
        [Serializable]
        abstract class TransformOperation : ITransformOperation
        {
            public abstract void Execute(Transform transform);
        }

        [Serializable]
        class RotationRandomizer : TransformOperation
        {
            public Vector3 axis = Vector3.up;
            public Space space = Space.Self;
            public override void Execute(Transform transform)
            {
                transform.Rotate(axis, UnityEngine.Random.Range(0f, 360f), space);
            }
        }
        [Serializable]
        class ScaleRandomizer : TransformOperation
        {
            public float minScale = .8f;
            public float maxScale = .1f;
            public Space space = Space.Self;
            public override void Execute(Transform transform)
            {
                transform.localScale *= UnityEngine.Random.Range(minScale, maxScale);
            }
        }

        [Serializable]
        abstract class PrefabOverride
        {
            public abstract void Execute(GameObject prev, GameObject next);
        }

        [Serializable]
        class InheritTransform : PrefabOverride
        {
            public bool inheritRotation = true;
            public bool inheritScale = true;
            public bool inheritPosition = true;

            public override void Execute(GameObject prev, GameObject next)
            {
                if (inheritRotation)
                    next.transform.rotation = prev.transform.rotation;

                if (inheritScale)
                    next.transform.localScale = prev.transform.localScale;

                if (inheritPosition)
                    next.transform.position = prev.transform.position;
            }
        }
        [Serializable]
        class MultiplyPosition : TransformOperation
        {
            public float scale = .1f;
            public Space space = Space.World;

            public override void Execute(Transform transform)
            {
                if (space == Space.World)
                    transform.position *= scale;
                else
                    transform.localPosition *= scale;
            }
        }
        [Serializable]
        class ResetScale : TransformOperation
        {
            public Space space = Space.World;
            public override void Execute(Transform transform)
            {
                if (space == Space.World)
                {
                    transform.localScale = transform.localScale.Divide(transform.lossyScale);
                }
                else
                    transform.localScale = Vector3.one;
            }
        }
        [Serializable]
        class OffsetPosition : TransformOperation
        {
            public Space space = Space.Self;
            public Vector3 offset;
            public override void Execute(Transform transform)
            {
                transform.Translate(offset, space);
            }
        }

        [SerializeField, SerializeReference, Polymorphic(
            include: new[] { typeof(ITransformOperation), typeof(PrefabOverride)
        })]
        object[] options = new object[]
        {
            new InheritTransform(),
            //new OffsetPosition(),
            //new ScaleRandomizer(),
            //new RotationRandomizer(),
        };

        Vector2 scrollPos;

        protected void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true));

            Editor.CreateCachedEditor(this, null, ref editor);

            //editor.DrawDefaultInspector();
            editor.OnInspectorGUI();

            if (selections.Count > 0 && prefab)
            {
                Apply();
            }

            EditorGUILayout.EndScrollView();
        }

        [Button]
        void LoadSelections()
        {
            selections = Selection.gameObjects.ToList();
        }

        [Button]
        void Apply()
        {
            Undo.RecordObjects(selections.ToArray(), "Replace prefabs");

            // apply transforms
            var instances = new List<GameObject>();
            foreach (var selection in selections)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, selection.transform.parent);
                instances.Add(instance);

                Undo.RegisterCreatedObjectUndo(instance, "Replace prefabs");

                // inherit position always
                //instance.transform.localPosition = selection.transform.localPosition;

                foreach (var op in options)
                {
                    if (op is ITransformOperation transformOp)
                        transformOp.Execute(instance.transform);
                    else if (op is PrefabOverride po)
                        po.Execute(selection, instance);
                }

                EditorUtility.SetDirty(instance.transform);
            }


            var destroy = selections.ToArray();
            for (int i = 0; i < destroy.Length; i++)
            {
                Undo.DestroyObjectImmediate(destroy[i]);
            }

            Selection.objects = instances.ToArray();
            LoadSelections();
        }

        [Button]
        void ApplyOptions()
        {
            Undo.RecordObjects(selections.Select(s => s.transform).ToArray(), "Apply Options");
            foreach (var selection in selections)
            {
                foreach (var op in options)
                {
                    if (op is ITransformOperation transformOp)
                        transformOp.Execute(selection.transform);
                }
                EditorUtility.SetDirty(selection.transform);
            }
        }
    }
#endif
}