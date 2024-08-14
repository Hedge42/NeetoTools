using UnityEngine;
using System;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.Events;

namespace Neeto
{
    public static partial class NObject
    {
        public static bool Validate(this UnityEventBase unityEvent, Object mono = null)
        {
            for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
            {
                Object target = unityEvent.GetPersistentTarget(i);
                string methodName = unityEvent.GetPersistentMethodName(i);

                if (target == null || string.IsNullOrEmpty(methodName))
                {
                    if (mono)
                        Debug.LogError($"Invalid UnityEvent ", mono);
                    else
                        Debug.LogError($"Invalid UnityEvent ");
                    return false;
                }
            }
            return true;
        }

        public static IEnumerable<T> Children<T>(this Component component) where T : Component
        {
            return component.transform.GetChildren()
                .Select(t => t.GetComponent<T>())
                .Where(t => t);
        }
        public static IEnumerable<T> Children<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.transform.GetChildren()
                .Select(t => t.GetComponent<T>())
                .Where(t => t);
        }
        public static T GetComponentInSelfOrChildren<T>(this Component c)
        {
            var result = c.GetComponent<T>();
            if (result == null)
                result = c.GetComponentInChildren<T>();

            return result;
        }
        public static T GetComponentInSelfOrChildren<T>(this GameObject g)
        {
            var result = g.GetComponent<T>();
            if (result == null)
                result = g.GetComponentInChildren<T>();

            return result;
        }
        public static T[] GetComponentsInSelfAndChildren<T>(this GameObject g)
        {
            var list = new List<T>();
            //list.AddRange(g.GetComponents<T>());
            list.AddRange(g.GetComponentsInChildren<T>(true));
            return list.ToArray();
        }
        public static T GetComponentInSelfOrParent<T>(this Component c)
        {
            var result = c.GetComponent<T>();
            if (result == null)
                result = c.GetComponentInParent<T>();

            return result;
        }
        public static T GetComponentInSelfOrParent<T>(this GameObject g)
        {
            var result = g.GetComponent<T>();
            if (result == null)
                result = g.GetComponentInParent<T>();

            return result;
        }
        public static T GetOrAddComponent<T>(this Component c)
        {
            if (c.GetComponent<T>() == null)
                c.gameObject.AddComponent(typeof(T));
            return c.GetComponent<T>();
        }
        public static T GetOrAddComponent<T>(this GameObject g) where T : Component
        {
            var c = g.GetComponent<T>();
            return c ? c : g.AddComponent<T>();
        }
        public static Component GetOrAddComponent(this GameObject g, Type type)
        {
            var c = g.GetComponent(type);
            return c ? c : g.AddComponent(type);
        }
        public static async void OnDeactivate(this GameObject gameObject, Action callback)
        {
            while (gameObject && gameObject.activeSelf)
            {
                await UniTask.Yield();
            }
            callback();
        }
        public static T FindOrCreate<T>() where T : Component
        {
            var result = GameObject.FindObjectOfType<T>(true);

            return result ??= new GameObject($"{typeof(T).Name} Instance").AddComponent<T>();
        }
        public static void Destroy(this GameObject obj)
        {
            if (Application.isPlaying)
                GameObject.Destroy(obj);
            else
                GameObject.DestroyImmediate(obj);
        }
        public static void DestroyChildren<T>(this Transform transform, bool ignoreDisabled = true) where T : Component
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (!child.GetComponent<T>())
                    continue;
                else if (!child.gameObject.activeSelf && ignoreDisabled)
                    continue;

                child.gameObject.Destroy();
                i--;
            }
        }
        public static void Destroy(this Component component)
        {
            if (Application.isPlaying)
                GameObject.Destroy(component);
            else
                GameObject.DestroyImmediate(component);
        }
        public static void OnDestroy(this GameObject gameObject, UnityAction action)
        {
            var c = gameObject.GetOrAddComponent<OnDestroyEvent>();
            c.onDestroy.AddListener(action);
        }
        public static void SetEnabled(bool value, params Behaviour[] components)
        {
            foreach (var _ in components)
                _.enabled = value;
        }
        public static void EnableBehavior(this Behaviour b, bool enabled) => b.enabled = enabled;
        public static void Enable(this Behaviour b, bool enabled) => b.enabled = enabled;
        public static void Enable(this Behaviour b) => b.enabled = true;
        public static void ToggleEnabled(this Behaviour b) => b.enabled = !b.enabled;
        public static void Enable(params Behaviour[] components)
        {
            foreach (var _ in components)
                _.enabled = true;
        }
        public static void Disable(params Behaviour[] components)
        {
            foreach (var _ in components)
                _.enabled = false;
        }
        public static void Disable(this MonoBehaviour mono)
        {
            mono.enabled = false;
        }
        public static Ray Ray(this Transform transform)
        {
            return new Ray(transform.position, transform.forward);
        }
        public static string[] PhysicsLayers()
        {
            return Enumerable.Range(0, 31).Select(index => LayerMask.LayerToName(index)).Where(l => !string.IsNullOrEmpty(l)).ToArray();
        }
        public static Transform[] GetChildren(this Transform transform)
        {
            var array = new Transform[transform.childCount];
            for (int i = 0; i < array.Length; i++)
                array[i] = transform.GetChild(i);
            return array;
        }
        public static void ValidateChildCount(this Transform parentTransform, int count)
        {
            int currentCount = parentTransform.childCount;

            // Destroy extra game objects if the count is too high
            while (currentCount > count)
            {
                parentTransform.GetChild(currentCount - 1).gameObject.Destroy();
                currentCount--;
            }

            // Clone the first child if the count is too low
            while (currentCount < count)
            {
                GameObject firstChild = parentTransform.GetChild(0).gameObject;
                GameObject.Instantiate(firstChild, parentTransform);
                currentCount++;
            }
        }
        public static IEnumerator MoveBetweenPoints(this Transform transform, float speed, Transform[] waypoints, bool relative = false)
        {
            var relativeStart = transform.position - waypoints[0].position;
            float totalDistance = 0f;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                totalDistance += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
            }

            var duration = totalDistance / speed;

            Vector3 currentTarget = waypoints[0].position;
            int targetIndex = 1;
            float distanceCovered = 0f;

            while (targetIndex < waypoints.Length)
            {
                //Vector3 startPosition = transform.position;
                var startPosition = waypoints[targetIndex - 1].position;
                Vector3 endPosition = waypoints[targetIndex].position;
                float segmentDistance = Vector3.Distance(startPosition, endPosition);
                float segmentTime = segmentDistance / totalDistance;

                while (Vector3.Distance(transform.position, endPosition) > 0.01f)
                {
                    var currentDistance = Vector3.Distance(transform.position - relativeStart, waypoints[targetIndex - 1].position);
                    var segmentProgress = currentDistance / segmentDistance;
                    distanceCovered += speed * Time.deltaTime;
                    float t = distanceCovered / totalDistance;
                    float segmentFactor = t / segmentTime;
                    transform.position = Vector3.Lerp(startPosition, endPosition, segmentProgress);
                    if (relative)
                        transform.position += relativeStart;
                    yield return null;
                }

                transform.position = endPosition;
                if (relative)
                    transform.position += relativeStart;
                targetIndex++;
            }
        }
        public static Transform MinDistance<T>(Vector3 position, List<Transform> others)
        {
            return others.OrderBy(b => (b.position - position).sqrMagnitude).FirstOrDefault();
        }
        public static void ResetLocalOrientation(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        public static Vector3 Left(this Transform transform)
        {
            return transform.rotation * Vector3.left;
        }
        public static Vector3 Right(this Transform transform)
        {
            return transform.rotation * Vector3.right;
        }
        public static Vector3 RightPosition(this Transform transform, float distance = 1f)
        {
            return transform.position + transform.right * distance;
        }
        public static Vector3 LeftPosition(this Transform transform, float distance = 1f)
        {
            return transform.position - transform.right * distance;
        }
        public static Vector3 ForwardPosition(this Transform transform, float distance = 1f)
        {
            return transform.position + transform.forward * distance;
        }
        public static Vector3 BackPosition(this Transform transform, float distance = 1f)
        {
            return transform.position - transform.forward * distance;
        }
        public static Vector3 GetLocalVelocity(this Rigidbody rb)
        {
            Quaternion rotation = Quaternion.Euler(rb.rotation.eulerAngles);
            Quaternion inverseRotation = Quaternion.Inverse(rotation);
            Vector3 localVelocity = inverseRotation * rb.velocity;
            return localVelocity;
        }
        public static void AddPosition(this Rigidbody rb, Vector3 delta)
        {
            rb.MovePosition(rb.position + delta);
        }
        public static void AddLocalPosition(this Rigidbody rb, Vector3 delta)
        {
            delta = rb.rotation * delta;
            rb.MovePosition(rb.position + delta);
        }
        public static void RotateToward(this Rigidbody rb, Vector3 position, float angularSpeed)
        {
            // Calculate the direction from the Rigidbody's position to the target position
            Vector3 direction = (position - rb.position).normalized;

            // Create a look rotation in the direction of the target, but only rotate on the Y-axis
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            // Calculate the step size for rotation based on angular speed and deltaTime
            float step = angularSpeed * Time.fixedDeltaTime;

            // Slerp (spherical linear interpolate) from the current rotation to the target rotation
            Quaternion rotated = Quaternion.Slerp(rb.rotation, lookRotation, step);

            // Apply the rotation to the Rigidbody
            rb.MoveRotation(rotated);
        }
        public static void GetCapsuleCenters(this CapsuleCollider cap, out Vector3 top, out Vector3 bottom, float footClearance = .1f)
        {
            var center = cap.transform.TransformPoint(cap.center);
            var distanceFromCenter = (cap.height / 2f) - cap.radius;
            top = center + Vector3.up * distanceFromCenter;
            bottom = center + Vector3.down * (distanceFromCenter - footClearance);
        }
        public static bool Intersects(this Collider hitboxCollider, Collider otherCollider)
        {
            Vector3 direction;
            float distance;

            bool colliding = Physics.ComputePenetration(
                hitboxCollider, hitboxCollider.transform.position, hitboxCollider.transform.rotation,
                otherCollider, otherCollider.transform.position, otherCollider.transform.rotation,
                out direction, out distance
            );

            return colliding;
        }
        public static void DestroyClones(this Transform transform)
        {
            var list = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name.ToLower().Contains("(Clone)"))
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        GameObject.DestroyImmediate(transform.gameObject);
                    else
#endif
                        Destroy(transform.gameObject);
                }
            }
        }
        public static bool IsNullOrEmpty<T>(this T[] array) => array == null || array.Length == 0;

        public static void SetValue<T1, T2>(this Dictionary<T1, T2> d, T1 key, T2 value)
        {
            if (d.ContainsKey(key))
                d[key] = value;
            else
                d.Add(key, value);
        }
        public static T JsonClone<T>(this T original)
        {
            string serializedData = JsonUtility.ToJson(original);
            return JsonUtility.FromJson<T>(serializedData);
        }
        public static object JsonClone(this UnityEngine.Object original)
        {
            string serializedData = JsonUtility.ToJson(original);
            return JsonUtility.FromJson(serializedData, original.GetType());
        }

        public static Texture2D AsTexturePixel(this Color color)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }
        public static Texture2D TexturePixel(Color color, ref Texture2D tex)
        {
            return tex ??= AsTexturePixel(color);
        }
        public static Texture2D AsTexturePixel(this Color color, ref Texture2D tex)
        {
            return tex ??= AsTexturePixel(color);
        }
        public static Texture2D Multiply(this Texture2D source, Color color)
        {
            /*
             clone the texture and modify the pixels 
             */
            var tex = new Texture2D(source.width, source.height, source.format, true, true);
            Graphics.CopyTexture(source, tex);
            tex.Apply();

            for (int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                {
                    tex.SetPixel(x, y, Multiply(color, source.GetPixel(x, y)));
                }
            }
            tex.Apply();

            return tex;
        }
        static Color Multiply(Color a, Color b)
        {
            return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
        }
        public static Texture2D Outline(Vector2Int size, int border, Color outlineColor, Color fillColor)
        {
            var tex = new Texture2D(size.x, size.y);

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var isBorder = Mathf.Abs(size.x - x) < border || Mathf.Abs(size.y - y) < border;
                    if (isBorder)
                        tex.SetPixel(x, y, outlineColor);
                    else
                        tex.SetPixel(x, y, fillColor);
                }
            }

            tex.Apply();
            return tex;
        }
    }
}