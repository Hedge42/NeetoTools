using UnityEngine;
using System;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

using UnityEditor;
using Cysharp.Threading.Tasks;

namespace Neeto
{
    public class TransformCommands
    {
        // ...

        [MenuItem(MenuPath.GameObject + nameof(ResetParentTransform))]
        static void ResetParentTransform(MenuCommand command)
        {
            var parent = (command.context as GameObject)?.transform;

            if (!parent)
            {
                Debug.LogError($"Transform not found for command '{nameof(ResetParentTransform)}'", parent);
                return;
            }

            if (parent)
            {
                Debug.Log($"{nameof(ResetParentTransform)} ({parent.name})", parent);

                var pos = parent.localPosition;
                var rot = parent.localRotation;

                /* set local position and rotation to 0
				 */

                var flag = false;


                foreach (Transform child in parent)
                {
                    PrepareToReset(child, () => flag);
                }

                Undo.RecordObject(parent, nameof(ResetParentTransform));
                parent.localPosition -= pos;
                parent.localRotation *= Quaternion.Inverse(rot);
                flag = true;
            }
        }
        static void PrepareToReset(Transform transform, Func<bool> GetFlag)
        {
            UniTask.Void(async () =>
            {
                var o = transform.GetWorldPoint();
                while (!GetFlag())
                    await UniTask.Yield();
                Undo.RecordObject(transform, nameof(PrepareToReset));
                transform.SetWorldPoint(o);
            });
        }

        [MenuItem(MenuPath.GameObject + nameof(SetChildAsParentOrigin))]
        static void SetChildAsParentOrigin(MenuCommand command)
        {
            var originTransform = (command.context as GameObject)?.transform;
            var parent = originTransform?.parent;

            if (!parent)
            {
                Debug.LogError($"Parent transform not found for command '{nameof(SetChildAsParentOrigin)}'");
                return;
            }

            var offset = Point.Local(originTransform);
            var flag = false;

            foreach (Transform t in parent)
            {
                PrepareToReset(t, () => flag);
            }

            Undo.RecordObject(parent, nameof(SetChildAsParentOrigin));
            parent.transform.SetWorldPoint(originTransform.GetWorldPoint());
            flag = true;
        }
        [MenuItem(MenuPath.GameObject + nameof(SetChildAsParentOriginPosition))]
        static void SetChildAsParentOriginPosition(MenuCommand command)
        {
            var originTransform = (command.context as GameObject)?.transform;
            var parent = originTransform?.parent;

            if (!parent)
            {
                Debug.LogError($"Parent transform not found for command '{nameof(SetChildAsParentOriginPosition)}'");
                return;
            }

            var offset = originTransform.GetLocalPoint();
            var flag = false;

            foreach (Transform t in parent)
            {
                PrepareToReset(t, () => flag);
            }

            Undo.RecordObject(parent, nameof(SetChildAsParentOriginPosition));
            parent.transform.position = originTransform.position;
            flag = true;
        }

        [MenuItem(MenuPath.GameObject + nameof(SetChildAsParentOriginRotation))]
        static void SetChildAsParentOriginRotation(MenuCommand command)
        {
            var originTransform = (command.context as GameObject)?.transform;
            var parent = originTransform?.parent;

            if (!parent)
            {
                Debug.LogError($"Parent transform not found for command '{nameof(SetChildAsParentOriginRotation)}'");
                return;
            }

            var offset = originTransform.GetLocalPoint();
            var flag = false;

            foreach (Transform t in parent)
            {
                PrepareToReset(t, () => flag);
            }

            Undo.RecordObject(parent, nameof(SetChildAsParentOriginRotation));
            parent.transform.rotation = originTransform.rotation;
            flag = true;
        }

    }
}