using Cysharp.Threading.Tasks;
using Neeto;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Scene = UnityEngine.SceneManagement.Scene;

public static class N
{
    public static void OnSceneChanged(Action action, bool persistent = false)
    {
        UnityAction<Scene, Scene> del = null;
        del = (_, _) =>
        {
            action?.Invoke();
            if (!persistent)
                SceneManager.activeSceneChanged -= del;
        };

        SceneManager.activeSceneChanged += del;
    }

   


    

    #region TASKS
    // -- Tasks
    public static async UniTask DelayAsync(float seconds, Action completed, CancellationToken token, PlayerLoopTiming timing)
    {
        var elapsed = 0f;

        while (elapsed < seconds)
        {
            await UniTask.Yield(timing, token);
            elapsed += timing.GetDeltaTime();
        }

        completed?.Invoke();
    }
    public static void DelayCall(Action delayedAction)
    {
#if UNITY_EDITOR
        EditorApplication.delayCall += () =>
        {
#endif

            delayedAction();

#if UNITY_EDITOR

        };
#endif
    }
    public static T DelayCall<T>(Func<T> func)
    {

        T value = default;
        bool flag = false;

#if UNITY_EDITOR
        EditorApplication.delayCall += () =>
        {
#endif

            value = func();

#if UNITY_EDITOR
        };
#endif

        if (!Application.isEditor)
        {
            return value = func();
        }

        return flag ? value : func();
    }
    //public static void Delayed
    public static NTask Delay(this Action action, float seconds)
    {
        return new NTask().Switch(action.DelayAsync(seconds));
    }
    public static async UniTask DelayAsync(this Action completed, float seconds, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        var elapsed = 0f;

        while (elapsed < seconds)
        {
            await UniTask.Yield(timing);
            elapsed += timing.GetDeltaTime();
        }

        completed?.Invoke();
    }
    public static async UniTask Fade(float rangeStart, float rangeEnd, float value, float duration, Action<float> setValue, CancellationToken token)
    {
        var elapsed = Mathf.InverseLerp(rangeStart, rangeEnd, value);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            setValue(Mathf.Lerp(rangeStart, rangeEnd, elapsed / duration));
            await UniTask.Yield(token);
        }
    }
    public static async UniTask Then(this UniTask task, Action action, CancellationToken token)
    {
        await UniTask.WaitUntil(() => task.Status.IsCompleted(), cancellationToken: token);
        action.Invoke();
    }
    public static async UniTask Then(this UniTask task, Action action)
    {
        await UniTask.WaitUntil(() => task.Status.IsCompleted());
        action.Invoke();
    }
    public static async UniTaskVoid LazyUpdate(PlayerLoopTiming timing, int frameSkip, CancellationToken token, Action action)
    {
        while (!token.IsCancellationRequested)
        {
            action?.Invoke();

            for (int i = 0; i < frameSkip; i++)
                await UniTask.Yield(timing, token);
        }
    }
    public static async UniTaskVoid LazyUpdate(PlayerLoopTiming timing, float delay, bool ignoreTimeScale, CancellationToken token, Action action)
    {
        int ms = delay.ToMilliseconds();
        while (!token.IsCancellationRequested)
        {
            action?.Invoke();
            await UniTask.Delay(ms, ignoreTimeScale, timing, token);
        }
    }
    public static async UniTaskVoid InputBuffer(float duration, Func<bool> canAct, Action action, CancellationToken token)
    {
        var elapsed = 0f;

        while (elapsed < duration)
        {
            if (canAct())
            {
                action();
                return;
            }

            elapsed += Time.fixedDeltaTime;
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }
    }
    public static async UniTask LerpAsync(float a, float b, float d, Action<float> action)
    {
        var e = 0f;
        var c = b - a;

        while (e < d)
        {
            await UniTask.Yield();
            e += Time.deltaTime;
            var t = Mathf.Clamp01(e / d);

            action(a + t * c);
        }
    }
    public static async UniTask LerpAsyncUnscaled(float a, float b, float d, Action<float> action)
    {
        var e = 0f;
        var c = b - a;

        while (e < d)
        {
            await UniTask.Yield();
            e += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(e / d);

            action(a + t * c);
        }
    }
    public static async UniTask LerpAsync(Vector2 a, Vector2 b, float duration, Action<Vector2> action, CancellationToken token)
    {
        var elapsed = 0f;

        while (elapsed < duration)
        {
            await UniTask.Yield(token);
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);

            action(Vector2.Lerp(a, b, t));
        }
    }
    public static UniTask EmptyTask() => UniTask.CompletedTask;
    public static async UniTask Frame() => await UniTask.Yield();

    public static async UniTask DelayOrCondition(TimeSpan duration, Func<bool> condition, CancellationToken token)
    {
        var elapsed = 0f;
        var ignoreTimeScale = false;
        var timing = PlayerLoopTiming.Update;
        Func<bool> cancel = () => token.IsCancellationRequested || elapsed >= (float)duration.TotalSeconds || condition();
        while (!cancel())
        {
            await UniTask.Yield(timing, token);
            elapsed += timing.GetDeltaTime(ignoreTimeScale);
        }
    }

    public static UniTask.Awaiter GetAwaiter(this CancellationToken token)
    {
        return token.GetAwaiter();
    }
    public static NTask ExecuteAfter(this Action action, float seconds)
    {
        async UniTask DelayAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds));
            action?.Invoke();
        }
        return new NTask().Switch(DelayAsync());
    }
    public static NTask ExecuteAfter<T>(this Action<T> action, float seconds, T data)
    {
        async UniTask DelayAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds));
            action?.Invoke(data);
        }
        return new NTask().Switch(DelayAsync());
    }
    public static async UniTask WhileActiveAsync(this GameObject g)
    {
        await UniTask.WaitWhile(() => g.activeInHierarchy);
    }

    //public AutoResizeBackground textArea;
    public static async UniTask StartQueueAsync(ConcurrentQueue<UniTask> queue, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out var task))
            {
                await task;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }
    public static async UniTask StartQueueAsync(Queue<UniTask> queue, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Yield(token);
            await UniTask.WaitUntil(() => queue.Count > 0, PlayerLoopTiming.Update, token);

            await queue.Peek().AttachExternalCancellation(token);
            var x = queue.Dequeue();
        }
    }
    public static async UniTask WithTimeout(this UniTask task, float seconds, bool ignoreTimeScale = true)
    {
        var cts = new CancellationTokenSource();

        try
        {
            await UniTask.WhenAny(
                task.AttachExternalCancellation(cts.Token),
                UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale, cancellationToken: cts.Token)
                );
        }
        finally
        {
            cts.Kill();
        }
    }

    #endregion

    // -- Math
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
    public static Quaternion WithRigidbodyConstraints(this Quaternion rotation, Rigidbody rigidbody)
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
    public static Vector3 Multiply(this Vector3 a, Vector3 b)
    {
        a.x *= b.x;
        a.y *= b.y;
        a.z *= b.z;
        return a;
    }
    public static Vector3 Divide(this Vector3 a, Vector3 b)
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
    public static Vector2 WithX(this Vector2 vector, float x)
    {
        return new Vector2(x, vector.y);
    }
    public static Vector2 WithY(this Vector2 vector, float y)
    {
        return new Vector2(vector.x, y);
    }
    public static float Range(this Vector2 range)
    {
        return range.y - range.x;
    }
    public static float Random(this Vector2 range)
    {
        return UnityEngine.Random.Range(range.x, range.y);
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
    public static bool IsInRange(this int value, int min, int max)
    {
        return value <= max && value >= min;
    }
    public static float ClampLength(float f, float max) => Mathf.Sign(f) * Mathf.Min(Mathf.Abs(f), max);
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
    public static float Average(this Vector2 v)
    {
        return (v.x + v.y) / 2f;
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
    public static Vector2 DirectionTo(this Vector2 from, Vector2 to)
    {
        Vector2 direction = to - from;
        return direction.normalized;
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
    public static Vector3 Clamp(this Vector3 source, Vector3 min, Vector3 max)
    {
        source.x = Mathf.Clamp(source.x, min.x, max.x);
        source.y = Mathf.Clamp(source.y, min.y, max.y);
        source.z = Mathf.Clamp(source.z, min.z, max.z);
        return source;
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

    // -- Colors
    

    // -- Data
    


    // -- Textures
    
}


public class UpdateLoop
{
    public Action onUpdate { get; set; }
    public CancellationToken token { get; set; }
    public PlayerLoopTiming timing { get; set; }


    public UpdateLoop(CancellationToken token, Action onUpdate = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        this.token = token;
        this.onUpdate = onUpdate;
        this.timing = timing;
    }

    public async UniTask RunAsync()
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Yield(timing, token);
            onUpdate.Invoke();
        }
    }
}