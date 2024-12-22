using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Playables;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    public static class App
    {
        #region CALLBACKS
        public static event Action onQuit;
        public static bool isQuitting { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        static void Setup()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += _ =>
            {
                if (_ == PlayModeStateChange.ExitingPlayMode)
                {
                    OnQuit();
                }
            };
#endif
            Application.quitting += OnQuit;
        }
        static void OnQuit()
        {
            Debug.Log("quitting...");
            isQuitting = true;
            onQuit?.Invoke();
            Application.quitting -= OnQuit;
            onQuit = null;
        }
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        public static void OnAfterSerialization(Action callback)
        {
            bool flag = false;
#if UNITY_EDITOR
            UniTask.Void(async () =>
            {
                await UniTask.WaitWhile(() => EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isUpdating);
                callback();
            });
            flag = true;
#endif
            if (!flag)
                callback();
        }
        #endregion

        #region MATH / PHYSICS / TRANSFORMS

        public static Vector3 With(this Vector3 value, float? x = null, float? y = null, float? z = null, float? scale = null)
        {
            value.x = x ?? value.x;
            value.y = y ?? value.y;
            value.z = z ?? value.z;

            if (scale != null)
                value = (float)scale * value.normalized;

            return value;
        }
        public static Vector2 With(this Vector2 value, float? x = null, float? y = null)
        {
            value.x = x ?? value.x;
            value.y = y ?? value.y;

            return value;
        }
        public static Vector3 MoveToward(this Vector3 v, Vector3 target, float maxDistanceDelta)
        {
            return Vector3.MoveTowards(v, target, maxDistanceDelta);
        }
        public static Vector2 MoveToward(this Vector2 v, Vector2 target, float maxDistanceDelta)
        {
            return Vector2.MoveTowards(v, target, maxDistanceDelta);
        }
        public static Quaternion DampToward(this Quaternion q, Quaternion other, float damping = .5f)
        {
            return Quaternion.Slerp(q, other, damping);
        }
        public static Quaternion WithConstraints(this Quaternion rotation, Rigidbody rigidbody)
        {
            var bodyEuler = rigidbody.rotation.eulerAngles;
            var rotEuler = rotation.eulerAngles;

            var cons = rigidbody.constraints;
            if (cons.HasFlag(RigidbodyConstraints.FreezeRotationX))
                rotEuler.x = bodyEuler.x;
            if (cons.HasFlag(RigidbodyConstraints.FreezeRotationY))
                rotEuler.y = bodyEuler.y;
            if (cons.HasFlag(RigidbodyConstraints.FreezeRotationZ))
                rotEuler.z = bodyEuler.z;

            return Quaternion.Euler(rotEuler);
        }
        public static Vector3 MultiplyParts(this Vector3 a, Vector3 b)
        {
            a.x *= b.x;
            a.y *= b.y;
            a.z *= b.z;
            return a;
        }
        public static Vector3 DivideParts(this Vector3 a, Vector3 b)
        {
            a.x /= b.x;
            a.y /= b.y;
            a.z /= b.z;
            return a;
        }
        public static void Divide(ref Vector3 v, float amount)
        {
            v.x /= amount;
            v.y /= amount;
            v.z /= amount;
        }
        public static float Range(this Vector2 range)
        {
            return range.y - range.x;
        }
        public static float Random(this Vector2 range)
        {
            return UnityEngine.Random.Range(range.x, range.y);
        }

        public static float Lerp(this Vector2 range, float t, bool clamped = true)
        {
            return clamped
                ? Mathf.Lerp(range.x, range.y, t)
                : Mathf.LerpUnclamped(range.x, range.y, t);
        }
        public static bool RoughlyEquals(this float a, float b, float threshold = .01f)
        {
            return Mathf.Abs(a - b) <= threshold;
        }
        public static float Average(this Vector3 v)
        {
            return (v.x + v.y + v.y) / 3f;
        }
        public static float Average(this Vector2 v)
        {
            return (v.x + v.y) / 2f;
        }

        public static float Clamp01(this float f)
        {
            return Mathf.Clamp01(f);
        }
        public static float ClampE(this float f)
        {
            // because Unity has an issue with weights being 1
            return f.Clamp(Mathf.Epsilon, 1 - Mathf.Epsilon);
        }
        public static float Clamp(this float f, float min, float max)
        {
            return Mathf.Clamp(f, min, max);
        }
        public static float Clamp(this float f, Vector2 range)
        {
            return Mathf.Clamp(f, range.x, range.y);
        }
        public static int Clamp(this int f, int min, int max)
        {
            return Mathf.Clamp(f, min, max);
        }
        public static float ClampLength(float f, float max)
        {
            return Mathf.Sign(f) * Mathf.Min(Mathf.Abs(f), max);
        }
        public static Vector3 Clamp(this Vector3 source, Vector3 min, Vector3 max)
        {
            source.x = Mathf.Clamp(source.x, min.x, max.x);
            source.y = Mathf.Clamp(source.y, min.y, max.y);
            source.z = Mathf.Clamp(source.z, min.z, max.z);
            return source;
        }
        public static bool IsInRange(this int value, int min, int max)
        {
            return value <= max && value >= min;
        }
        public static float Remap(this float value, float inputMin, float inputMax, float outputMin, float outputMax, bool clamp = false)
        {
            // TODO test
            float mappedValue = (value - inputMin) / (inputMax - inputMin) * (outputMax - outputMin) + outputMin;
            if (clamp)
            {
                mappedValue = Mathf.Clamp(mappedValue, outputMin, outputMax);
            }
            return mappedValue;
        }
        public static float Round(this float value)
        {
            return Mathf.Round(value);
        }
        public static float ToRadians(this float degrees)
        {
            return degrees * Mathf.PI / 180f;
        }
        public static float ToDegrees(this float radians)
        {
            return radians * 180f / Mathf.PI;
        }
        public static float Wrap(this float value, float minValue, float maxValue)
        {
            float range = maxValue - minValue;
            return ((value - minValue) % range + range) % range + minValue;
        }
        public static float TriangleWrap(this float value, float minValue, float maxValue)
        {
            float range = maxValue - minValue;
            float halfRange = range * 0.5f;
            float offset = value - minValue;
            float wrappedValue = (offset % range + range) % range;
            float centeredValue = wrappedValue - halfRange;
            float absCenteredValue = Mathf.Abs(centeredValue);
            float signedValue = centeredValue / halfRange;
            float triangleValue = (1f - absCenteredValue / halfRange) * signedValue;
            float finalValue = (triangleValue * halfRange) + halfRange + minValue;
            return finalValue;
        }
        public static float SmoothStep(this float value, float minValue, float maxValue)
        {
            float t = Mathf.Clamp01((value - minValue) / (maxValue - minValue));
            return t * t * (3f - 2f * t);
        }
        public static void SetPosition(this Transform transform, Vector3 position, Space space)
        {
            if (space == Space.World)
            {
                transform.position = position;
            }
            else
            {
                transform.position = transform.TransformPoint(position);
            }
        }
        public static bool IsGrounded(this CapsuleCollider collider)
        {
            if (collider.attachedRigidbody.velocity.y > .1f)
            {
                return false;
            }

            var origin = collider.transform.position;
            var direction = Vector3.down;
            var radius = collider.radius / 2f;
            var distance = collider.height / 2f;
            var layerMask = LayerMask.GetMask("Level", "Terrain");
            var qti = QueryTriggerInteraction.Ignore;

            var result = Physics.SphereCast(origin, radius, direction, out var hit, distance, layerMask, qti);

            return result;

        }
        public static int ToMilliseconds(this float f)
        {
            return (int)(f * 1000);
        }

        public static float AngleTo(this Vector2 from, Vector2 to, bool isOutputDegrees = true)
        {
            Vector2 direction = to - from;
            float angle = Mathf.Atan2(direction.y, direction.x);
            if (isOutputDegrees)
                angle *= Mathf.Rad2Deg;
            return angle;
        }
        public static float Abs(this float f)
        {
            return Mathf.Abs(f);
        }
        public static float Deadzone(this float input, float deadzone = 0.1f, bool remapMin = false)
        {
            float min = remapMin ? -1f : Mathf.Clamp01(-1f + deadzone);
            float max = Mathf.Clamp01(1f - deadzone);
            float output = Mathf.Clamp(input, min, max);

            if (Mathf.Abs(output) < deadzone)
            {
                return 0f;
            }
            else if (output > 0f)
            {
                return (output - deadzone) / (max - deadzone);
            }
            else
            {
                return (output + deadzone) / (max - deadzone);
            }
        }
        public static Vector2 Rotate(this Vector2 vector, float angle, bool isInputDegrees = true)
        {
            float radians = angle;
            if (isInputDegrees)
                radians *= Mathf.Deg2Rad;

            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            float x = vector.x * cos - vector.y * sin;
            float y = vector.x * sin + vector.y * cos;

            return new Vector2(x, y);
        }
        public static Vector2 SnapToGrid(this Vector2 vector, float gridSize)
        {
            float x = Mathf.Round(vector.x / gridSize) * gridSize;
            float y = Mathf.Round(vector.y / gridSize) * gridSize;
            return new Vector2(x, y);
        }
        public static Vector2 Flatten(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }
        public static Vector2 Deadzone(this Vector2 input, float deadzone = 0.1f, bool remapMin = false)
        {
            float minMagnitude = remapMin ? 0f : Mathf.Clamp01(deadzone);
            float maxMagnitude = 1f;
            float inputMagnitude = input.magnitude;
            Vector2 output = Vector2.zero;

            if (inputMagnitude >= minMagnitude)
            {
                float normalizedMagnitude = (inputMagnitude - deadzone) / (maxMagnitude - deadzone);
                float factor = Mathf.Clamp(normalizedMagnitude, 0f, 1f) / inputMagnitude;
                output = input * factor;
            }

            return output;
        }
        public static Vector2 GetPreferredSize(this TextMeshProUGUI tmp)
        {
            return new Vector2(tmp.renderedWidth, tmp.renderedHeight);
        }
        public static Vector3 Scale(Vector3 v, Vector3 s)
        {
            v.x *= s.x;
            v.y *= s.y;
            v.z *= s.z;
            return v;
        }

        public static Vector2 DirectionTo(this Vector2 from, Vector2 to)
        {
            Vector2 direction = to - from;
            return direction.normalized;
        }
        public static Vector3 DirectionTo(this Vector3 from, Vector3 to, bool normalize = true)
        {
            var dir = to - from;
            if (normalize)
                dir = dir.normalized;
            return dir;
        }
        public static Vector3 DirectionTo(this Transform from, Vector3 to, bool normalize = true)
        {
            var dir = to - from.position;
            if (normalize)
                dir = dir.normalized;
            return dir;
        }
        public static Vector3 DirectionTo(this Transform from, Transform to, bool normalize = true)
        {
            var dir = to.position - from.position;
            if (normalize)
                dir = dir.normalized;
            return dir;
        }
        public static Vector3 DirectionTo(this Component from, Component to, bool normalize = true)
        {
            var dir = to.transform.position - from.transform.position;
            if (normalize)
                dir = dir.normalized;

            return dir;
        }
        public static Vector3 LocalDirectionTo(this Transform from, Transform to, bool normalize = true)
        {
            var dir = to.position - from.position;
            dir = from.InverseTransformPoint(dir);
            if (normalize)
                dir = dir.normalized;
            return dir;
        }
        public static Quaternion SetLookDirection(this Transform transform, Vector3 forward)
        {
            var result = Quaternion.LookRotation(forward, Vector3.up);
            transform.rotation = result;
            return result;
        }
        public static Quaternion SetLookDirection(this Rigidbody rb, Vector3 forward)
        {
            var result = Quaternion.LookRotation(forward, Vector3.up);
            rb.MoveRotation(result);
            rb.transform.rotation = result; // ??
            return result;
        }

        public static Ray GetRay(this Vector3 from, Vector3 to)
        {
            return new Ray(from, to - from);
        }
        public static Quaternion FilterAxes(this Quaternion q, Axis filter)
        {
            return Quaternion.Euler(FilterAxes(q.eulerAngles, filter));
        }
        public static Vector3 FilterAxes(this Vector3 v, Axis filter)
        {
            return new Vector3(
                filter.HasFlag(Axis.x) ? v.x : 0,
                filter.HasFlag(Axis.y) ? v.y : 0,
                filter.HasFlag(Axis.z) ? v.z : 0);
        }
        public static bool IsValid(this Quaternion quaternion)
        {
            // thanks ChatGPT!
            const float tolerance = 0.01f;  // Set your own tolerance
            float magnitude = quaternion.x * quaternion.x + quaternion.y * quaternion.y
                             + quaternion.z * quaternion.z + quaternion.w * quaternion.w;
            return Math.Abs(magnitude - 1f) <= tolerance;
        }
        public static void SnapToGround(this Rigidbody rb, LayerMask env)
        {
            var qti = QueryTriggerInteraction.Ignore;
            var pos = rb.position + Vector3.up * 2f; // start from above
            var dir = Vector3.down;
            if (Physics.Raycast(pos, dir, out var hit, 10f, env, qti))
            {
                rb.position = hit.point;
            }
        }
        public static Ray Ray(this Transform transform)
        {
            return new Ray(transform.position, transform.forward);
        }
        public static string[] PhysicsLayers()
        {
            return Enumerable.Range(0, 31).Select(index => LayerMask.LayerToName(index)).Where(l => !string.IsNullOrEmpty(l)).ToArray();
        }
        public static void CopyTo(this Transform from, Transform to)
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(to, "Copy Transform");
#endif

            to.transform.SetParent(from?.parent);
            to.localPosition = from.localPosition;
            to.localRotation = from.localRotation;
            to.localScale = from.localScale;
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
                    distanceCovered += speed * UnityEngine.Time.deltaTime;
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
        public static void ResetLocalPoint(this Transform transform)
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
        public static bool NaN(this Vector3 _)
        {
            return
                float.IsNaN(_.x) ||
                float.IsNaN(_.y) ||
                float.IsNaN(_.z);
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
            float step = angularSpeed * UnityEngine.Time.fixedDeltaTime;

            // Slerp (spherical linear interpolate) from the current rotation to the target rotation
            Quaternion rotated = Quaternion.Slerp(rb.rotation, lookRotation, step);

            // Apply the rotation to the Rigidbody
            rb.MoveRotation(rotated);
        }
        public static Quaternion InverseRotation(this Rigidbody rb)
        {
            return Quaternion.Inverse(rb.rotation);
        }
        public static Quaternion Inverse(this Quaternion q)
        {
            return Quaternion.Inverse(q);
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

        /// <summary>RGB applied first, then HSV, then finally alpha</summary>
        public static Color With(this Color color, float? r = null, float? g = null, float? b = null, float? a = null, float? h = null, float? s = null, float? v = null)
        {
            var result = new Color(r ?? color.r, g ?? color.g, b ?? color.b);
            Color.RGBToHSV(result, out var H, out var S, out var V);
            result = Color.HSVToRGB(h ?? H, s ?? S, v ?? V);
            result.a = a ?? color.a;
            return result;
        }
        /// <summary>returns #RRGGBBAA</summary>
        public static string ToHexRGBA(this Color color, int i)
        {
            int R = Mathf.RoundToInt(color.r * 255);
            int G = Mathf.RoundToInt(color.g * 255);
            int B = Mathf.RoundToInt(color.b * 255);
            int A = Mathf.RoundToInt(color.a * 255);
            return $"#{R:X1}{G:X1}{B:X1}{A:X1}";
        }
        /// <summary>returns #RRGGBB</summary>
        public static string ToHexRGB(this Color color)
        {
            int R = Mathf.RoundToInt(color.r * 255);
            int G = Mathf.RoundToInt(color.g * 255);
            int B = Mathf.RoundToInt(color.b * 255);
            return $"#{R:X1}{G:X1}{B:X1}";
        }


        public static Vector3 GetXZVelocity(this Rigidbody body) => new(body.velocity.x, 0, body.velocity.z);
        public static void SetXZVelocity(this Rigidbody body, Vector3 xz) => body.velocity = xz.With(y: body.velocity.y);
        #endregion

        #region ANIMATION
        public static float NormalizedTime(this Playable playable)
        {
            return Mathf.Clamp01((float)playable.GetTime() / (float)playable.GetDuration());
        }
        public static async UniTask Time(this Playable playable, float time, CancellationToken token, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            while (playable.IsValid() && playable.GetTime() < time)
            {
                await UniTask.Yield(timing, token, true);
            }
        }
        public static async UniTask WaitForNormalizedTime(this Playable playable, float t, CancellationToken token, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            while (playable.IsValid() && playable.GetPlayState() == PlayState.Playing && playable.NormalizedTime() < t)
            {
                await UniTask.Yield(timing, token, true);
            }
        }
        public static async UniTaskVoid InterpolateAsync(Playable playable, float start, float end, CancellationToken token)
        {
            playable.Pause();
            while (playable.IsValid())
            {
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, token);
                var nextTime = (float)playable.GetTime() + UnityEngine.Time.deltaTime * (float)playable.GetSpeed();
                playable.SetTime(Mathf.Clamp(nextTime, start, end));
            }
        }
        public static async UniTaskVoid DebugWeightsAsync(Playable mixer, CancellationToken token)
        {
            StringBuilder sb = new();
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(token);
                Debug.Log(DebugWeights(mixer, sb));
            }
        }
        public static async UniTaskVoid DebugWeightsAsync(Func<Playable> getMixer, CancellationToken token)
        {
            StringBuilder sb = new();
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(token);
                Debug.Log(DebugWeights(getMixer(), sb));
            }
        }
        public static string DebugWeights(Playable playable, StringBuilder sb = null)
        {
            sb ??= new();
            sb.Clear();
            sb.Append('[');
            var count = playable.GetInputCount();
            var sum = 0f;
            for (int i = 0; i < count; i++)
            {
                var w = playable.GetInputWeight(i);
                sum += w;
                sb.Append(w.ToString("f2"));
                if (i < count - 1)
                    sb.Append(',');
            }
            sb.Append($"]({sum:f2})");
            return sb.ToString();
        }
        public static void DisconnectAndDestroy(this Playable mixer, int input)
        {
            var count = mixer.GetInputCount();
            if (input >= count)
                return;

            var sub = mixer.GetInput(input);
            mixer.DisconnectInput(input);
            sub.DestroyRecursive();
            mixer.GetGraph().Evaluate();
        }
        public static void DestroyRecursive(this Playable playable)
        {
            if (!playable.IsValid())
                return;

            // destroy all sub-playables, starting at the leafs
            for (int i = 0; i < playable.GetInputCount(); i++)
                playable.GetInput(i).DestroyRecursive();

            // destroy the leaf playable
            playable.Destroy();
        }
        public static void TPose(this Animator animator)
        {
            animator.Rebind();
            //animator.Update(0f); // Force immediate reset to T-pose or default pose
#if UNITY_EDITOR
            EditorUtility.SetDirty(Selection.activeGameObject);
#endif
        }
        #endregion

        #region LAYERS
        public static void SetLayer(this Collider[] colliders, int layer)
        {
            foreach (var collider in colliders)
            {
                if (collider.gameObject.layer != layer)
                    collider.gameObject.layer = layer;
            }
        }
        public static bool Evaluate(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
        public static bool Evaluate(this LayerMask mask, GameObject gameObject)
        {
            return mask.Evaluate(gameObject.layer);
        }
        public static bool Evaluate(this LayerMask mask, Collider collider)
        {
            return mask.Evaluate(collider.gameObject.layer);
        }
        #endregion

        #region GAMEOBJECT_COMPONENT
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
            return g?.GetComponent<T>() ?? g?.AddComponent<T>();
        }
        public static Component GetOrAddComponent(this GameObject g, Type type)
        {
            var c = g.GetComponent(type);
            return c ? c : g.AddComponent(type);
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
            gameObject?.GetOrAddComponent<OnDestroyEvent>().onDestroy.AddListener(action);
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
        #endregion

        #region EVENTS
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
        public static async void OnDeactivate(this GameObject gameObject, Action callback)
        {
            while (gameObject && gameObject.activeSelf)
            {
                await UniTask.Yield();
            }
            callback();
        }
        #endregion

        #region COLLECTIONS
        public static bool TryPop<T>(this List<T> list, out T instance)
        {
            var result = list.Count > 0;
            instance = default;
            if (result)
            {
                instance = list[0];
                list.RemoveAt(0);

            }
            return result;
        }

        public static void SetValue<T1, T2>(this Dictionary<T1, T2> d, T1 key, T2 value)
        {
            if (d.ContainsKey(key))
                d[key] = value;
            else
                d.Add(key, value);
        }
        public static Vector3 WeightedAverage(this (float weight, Vector3 position)[] items) =>
        items.Aggregate(Vector3.zero, (acc, item) => acc + item.position * item.weight, total => total / items.Sum(item => item.weight));
        public static T RandomElement<T>(this T[] elements)
        {
            return elements[UnityEngine.Random.Range(0, elements.Length)];
        }
        public static T RandomWeighted<T>(this T[] attacks, Func<T, float> getWeight)
        {
            if (attacks == null || attacks.Length == 0)
            {
                throw new InvalidOperationException("No attacks available.");
            }

            float totalWeight = 0f;
            foreach (var attack in attacks)
            {
                totalWeight += getWeight(attack);
            }

            float randomValue = UnityEngine.Random.Range(0, totalWeight);
            float currentSum = 0f;

            foreach (var attack in attacks)
            {
                currentSum += getWeight(attack);
                if (randomValue <= currentSum)
                {
                    return attack;
                }
            }

            // Fallback in case of rounding errors, though unlikely
            return attacks.LastOrDefault();
        }
        public static void Iterate(this int count, Action<int> action)
        {
            for (int i = 0; i < count; i++)
                action.Invoke(i);
        }
        public static int IndexOf<T>(this T[] array, T item)
        {
            /*LINQ doesn't do this for arrays for some stupid reason
              Default: -1
             */
            for (int i = 0; i < array.Length; i++)
                if (array[i].Equals(item))
                    return i;

            return -1;
        }
        public static void ShuffleInPlace<T>(this T[] array)
        {
            // https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net
            int n = array.Length;
            while (n > 1)
            {
                int k = UnityEngine.Random.Range(0, n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
        public static T[] Shuffle<T>(this T[] src)
        {
            // https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net

            var dest = src.Copy();
            dest.ShuffleInPlace();
            return dest;
        }
        public static T[] Copy<T>(this T[] src)
        {
            int n = src.Length;
            var dest = new T[n];
            for (int i = 0; i < src.Length; i++)
                dest[i] = src[i];
            return dest;
        }
        public static Span<int> ShuffleIndexes(int length)
        {
            Span<int> indexes = new Span<int>();
            for (int i = 0; i < length; i++)
                indexes[i] = i;
            indexes.Shuffle();
            return indexes;
        }
        public static Span<int> SequenceSpan(int incStart, int excEnd)
        {
            var indexes = new Span<int>();
            for (int i = incStart; i < excEnd; i++)
                indexes[i] = i;
            return indexes;
        }
        public static int[] SequenceArray(int start, int end)
        {
            var n = end - start;
            var arr = new int[end - start];
            for (int i = 0; i < n; i++)
                arr[i] = i;
            return arr;
        }
        public static void Shuffle(ref this Span<int> span)
        {
            int n = span.Length;
            for (int i = 0; i < n; i++)
            {
                int j = UnityEngine.Random.Range(i, n);
                (span[i], span[j]) = (span[j], span[i]);
            }
        }
        public static void Shuffle<T>(this List<T> list)
        {
            // https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net
            int n = list.Count;
            while (n > 1)
            {
                int k = UnityEngine.Random.Range(0, n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }
        public static IEnumerable<T> Foreach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var _ in collection)
            {
                action(_);
            }
            return collection;
        }
        public static bool Next<T>(this T[] array, ref int index, out T result)
        {
            result = default;
            if (array.Length == 0)
                return false;

            index = (index + 1) % array.Length;
            result = array[index];
            return result != null;
        }
        public static bool Flip<T>(this T[] array, ref int index, out T result)
        {
            result = default;
            if (array.Length == 0)
                return false;

            index = 1 - index;
            index = Mathf.Clamp(index, 0, array.Length);
            result = array[index];
            return result != null;
        }
        #endregion

        #region IO_SERIALIZATION
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
        #endregion

        #region TEXTURE
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
        #endregion
    }
}