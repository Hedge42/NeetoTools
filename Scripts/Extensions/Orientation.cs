using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Matchwork
{
    /// <summary>
    /// Combine position and rotation when both are frequently needed <br/>
    /// useful for replacing Transforms with value-types
    /// </summary>
    [Serializable]
    public partial struct Orientation
    {
        #region Editor
        /* wtf
         */
        //[CustomPropertyDrawer(typeof(Orientation))]
#if UNITY_EDITOR
        public class OrientationDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.PropertyField(position, property, label);


                var e = Event.current;
                switch (e.type)
                {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        if (!position.Contains(e.mousePosition)) break;

                        var transform = (Transform)DragAndDrop.objectReferences
                            .Where(_ => _ is GameObject)
                            .First(_ => (_ as GameObject).transform);

                        if (!transform)
                            return;

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (e.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            //e.Use();

                            var positionProperty = property.FindPropertyRelative(MReflect.BackingField(nameof(Orientation.position)));
                            var rotationProperty = property.FindPropertyRelative(MReflect.BackingField(nameof(Orientation.rotation)));

                            positionProperty.vector3Value = transform.position;
                            rotationProperty.quaternionValue = transform.rotation;

                            property.ApplyAndMarkDirty();
                        }
                        break;
                }
            }
        }
#endif
        #endregion

        #region Properties
        [field: SerializeField] public Vector3 position { get; set; }
        [field: SerializeField] public Vector3 eulerAngles { get; set; }

        public static Orientation Zero => (Vector3.zero, Quaternion.identity);
        public Quaternion rotation
        {
            get => Quaternion.Euler(eulerAngles);
            set => eulerAngles = value.eulerAngles;
        }
        public Vector3 forward
        {
            get => rotation * Vector3.forward;
            set => rotation = Quaternion.LookRotation(value);
        }
        public Vector3 right => rotation * Vector3.right;
        public Ray ray => new Ray(position, forward);
        #endregion

        #region Constructors
        public Orientation(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.eulerAngles = rotation.eulerAngles;
        }
        public Orientation(Vector3 position, Vector3 eulerAngles)
        {
            this.position = position;
            this.eulerAngles = eulerAngles;
        }
        public Orientation(Transform transform)
        {
            position = transform.position;
            eulerAngles = transform.eulerAngles;
        }
        #endregion

        #region Functions
        public void Translate(Vector3 translation)
        {
            position += translation;
        }
        public void Rotate(Quaternion rotation)
        {
            this.rotation *= rotation;
        }
        public void ApplyTo(Transform transform)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
        public override string ToString()
        {
            return "Position: " + position.ToString() + ", Rotation: " + rotation.eulerAngles.ToString();
        }
        public override bool Equals(object obj)
        {
            return obj is Orientation orientation &&
                   position.Equals(orientation.position) &&
                   eulerAngles.Equals(orientation.eulerAngles) &&
                   rotation.Equals(orientation.rotation) &&
                   forward.Equals(orientation.forward) &&
                   EqualityComparer<Ray>.Default.Equals(ray, orientation.ray);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(position, eulerAngles, rotation, forward, ray);
        }
        public static Orientation Local(Transform transform)
        {
            return new Orientation(transform.localPosition, transform.localRotation);
        }
        public static Orientation World(Transform transform)
        {
            return new Orientation(transform.position, transform.rotation);
        }
        public static float GetOrbitDistanceFromPitch(Orientation orientation, Vector2 pitchMinMax, Vector2 distanceMinMax, AnimationCurve curve = null)
        {
            // Convert [0, 360] to [-180, 180]
            float pitch = orientation.eulerAngles.x;
            if (pitch > 180) pitch -= 360;

            var t = (pitch - pitchMinMax.x) / pitchMinMax.Range();
            if (curve != null) t = curve.Evaluate(t);
            return Mathf.Lerp(distanceMinMax.x, distanceMinMax.y, t);
        }
        public Orientation WithOrbitDelta(Vector3 anchorPosition, Vector2 inputDelta)
        {
            // Compute the rotations
            Quaternion rotationAroundY = Quaternion.AngleAxis(inputDelta.y, Vector3.up);
            Quaternion rotationAroundX = Quaternion.AngleAxis(inputDelta.x, right);

            // Compute the new position
            Vector3 direction = position - anchorPosition;
            direction = rotationAroundY * direction;
            direction = rotationAroundX * direction;
            Vector3 newPosition = anchorPosition + direction;

            // Compute the new rotation
            Quaternion newRotation = rotationAroundY * rotation;
            newRotation = rotationAroundX * newRotation;

            return new Orientation(newPosition, newRotation);
        }
        public static Orientation FromDifference(Vector3 a, Vector3 b) => (a, b - a);
        public static Orientation Lerp(Orientation a, Orientation b, float t)
        {
            Vector3 position = Vector3.Lerp(a.position, b.position, t);
            Quaternion rotation = Quaternion.Slerp(a.rotation, b.rotation, t);
            return new Orientation(position, rotation);
        }
        public static Orientation SmoothTowards(Orientation a, Orientation b, float smoothing)
        {
            smoothing = Mathf.Clamp01(smoothing);
            return Orientation.Lerp(a, b, 1 - smoothing);
        }
        public bool RoughlyEquals(Orientation other, float positionThreshold = .01f, float rotationThreshold = .01f)
        {
            float positionDistance = Vector3.Distance(this.position, other.position);
            float rotationDistance = Vector3.Distance(this.eulerAngles, other.eulerAngles);

            return positionDistance <= positionThreshold && rotationDistance <= rotationThreshold;
        }
        #endregion

        #region Operators
        public static implicit operator Orientation((Vector3 pos, Vector3 euler) t) => new Orientation(t.pos, t.euler);
        public static implicit operator Orientation((Vector3 pos, Quaternion rot) t) => new Orientation(t.pos, t.rot);
        public static implicit operator Orientation(Transform t) => t.GetWorldOrientation();
        public static Vector3 operator +(Orientation o) => o;
        public static Orientation operator -(Orientation o) => (-1 * o.position, Quaternion.Inverse(o));
        public static implicit operator Vector3(Orientation o) => o.position;
        public static implicit operator Quaternion(Orientation o) => o.rotation;
        public static implicit operator Ray(Orientation o) => o.ray;

        /// <summary>return updated position</summary>
        public static Orientation operator +(Orientation w, Orientation delta)
        {
            return new Orientation(w.position + delta.position, w.rotation * delta.rotation);
        }
        /// <summary>return updated position</summary>
        public static Transform operator +(Transform t, Orientation dw)
        {
            var w = (Orientation)t + dw;
            t.SetWorldOrientation(w);
            return t;
        }
        /// <summary>return updated position</summary>
        public static Transform operator &(Transform t, Orientation w)
        {
            t.SetWorldOrientation(w);
            return t;
        }
        /// <summary>return updated position</summary>
        public static Transform operator -(Transform t, Orientation dw)
        {
            var w = (Orientation)t - dw;
            t.SetWorldOrientation(w);
            return t;
        }
        /// <summary>return updated position</summary>
        public static Orientation operator +(Orientation dw, Transform t) => (Orientation)t + dw;
        /// <summary>return updated position</summary>
        public static Quaternion operator *(Orientation w, Quaternion b)
        {
            w.rotation *= b;
            return w;
        }
        /// <summary>return delta position</summary>
        public static Orientation operator -(Orientation left, Orientation right)
        {
            return new Orientation(
                left.position - right.position,
                left.rotation * Quaternion.Inverse(right.rotation));
        }
        /// <summary>"to"</summary>
        public static Orientation operator ^(Transform t, Orientation o) => (t.TransformPoint(o.position), t.rotation * o.rotation);
        public static Orientation operator %(Orientation w, Transform t)
        {
            Orientation prev = t;
            t.SetWorldOrientation(w);
            return w - prev;
        }
        public static bool operator ==(Orientation left, Orientation right)
        {
            return left.position == right.position && left.rotation == right.rotation;
        }
        public static bool operator !=(Orientation left, Orientation right)
        {
            return left.position != right.position || left.rotation != right.rotation;
        }
        #endregion
    }
    public static class OrientationExtensions
    {
        public static Orientation GetWorldOrientation(this Transform transform)
        {
            return new Orientation(transform.position, transform.rotation);
        }
        public static Orientation GetLocalOrientation(this Transform transform)
        {
            return new Orientation(transform.localPosition, transform.localRotation);
        }

        public static Orientation GetWorldDeltaTo(this Transform transform, Transform other)
        {
            return new Orientation(other.position - transform.position, other.rotation * Quaternion.Inverse(transform.rotation));
        }
        public static Orientation GetLocalDeltaTo(this Transform transform, Transform other)
        {
            return new Orientation(other.localPosition - transform.localPosition, other.localRotation * Quaternion.Inverse(transform.localRotation));
        }


        public static void SetWorldOrientation(this Transform transform, Orientation orientation)
        {
            transform.position = orientation.position;
            transform.rotation = orientation.rotation;
        }
        public static void SetLocalOrientation(this Transform transform, Orientation orientation)
        {
            transform.localPosition = orientation.position;
            transform.localRotation = orientation.rotation;
        }

        public static Orientation GetWorldOrientation(this Rigidbody rigidbody)
        {
            return new Orientation(rigidbody.position, rigidbody.rotation);
        }
        public static Orientation GetLocalOrientation(this Rigidbody rigidbody)
        {
            // necessary to keep with rigidbody position?
            // TODO
            rigidbody.transform.position = rigidbody.position;
            rigidbody.transform.rotation = rigidbody.rotation;

            var localPosition = rigidbody.transform.InverseTransformPoint(rigidbody.position);
            var localRotation = rigidbody.transform.localRotation;

            return new Orientation(localPosition, localRotation);
        }


        public static Orientation LocalToWorld(Orientation local, Transform t)
        {
            var position = t.InverseTransformPoint(local);
            var rotation = t.rotation * local.rotation;
            return (position, rotation);
        }
        public static Orientation WorldToLocal(Orientation world, Transform t)
        {
            var position = t.TransformPoint(world);
            var rotation = t.rotation * Quaternion.Inverse(world.rotation);
            return (position, rotation);
        }

        public static void SetWorldOrientation(this Rigidbody rigidbody, Orientation orientation)
        {
            rigidbody.MovePosition(orientation.position);
            rigidbody.MoveRotation(orientation.rotation);
        }

        public static Orientation WeightedAverage(this (float weight, Orientation orientation)[] items)
        {
            var totalWeight = items.Sum(item => item.weight);
            var averagePos = items.Aggregate(Vector3.zero, (acc, item) => acc + item.orientation.position * item.weight, total => total / totalWeight);
            var averageRot = Quaternion.Lerp(items[0].orientation.rotation, items[1].orientation.rotation, items[1].weight / totalWeight);
            for (int i = 2; i < items.Length; i++)
            {
                averageRot = Quaternion.Lerp(averageRot, items[i].orientation.rotation, items[i].weight / totalWeight);
            }
            return new Orientation(averagePos, averageRot);
        }
        public static bool TryGetWeightedAverage(this (float weight, Orientation orientation)[] items, out Orientation result)
        {
            var totalWeight = items.Sum(item => item.weight);
            result = Orientation.Zero;

            if (totalWeight == 0f)
                return false;

            Quaternion averageRot = Quaternion.identity;
            Vector3 averagePos = Vector3.zero;
            var count = items.Count();

            averagePos = items.Aggregate(Vector3.zero, (acc, item) => acc + item.orientation.position * item.weight, total => total / totalWeight);

            // TODO one-liner this
            if (count >= 2)
            {
                averageRot = Quaternion.Lerp(items[0].orientation.rotation, items[1].orientation.rotation, items[1].weight / totalWeight);
                for (int i = 2; i < items.Length; i++)
                {
                    averageRot = Quaternion.Lerp(averageRot, items[i].orientation.rotation, items[i].weight / totalWeight);
                }
            }
            else if (count == 1)
            {
                averageRot = items[0].orientation.rotation;
            }

            result = new Orientation(averagePos, averageRot);
            return true;
        }
    }
}