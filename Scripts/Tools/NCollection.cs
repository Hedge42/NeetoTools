using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public static class NCollection
{
    // ...
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
}
