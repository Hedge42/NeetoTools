using System;
using UnityEngine;

[Flags]
public enum Axis
{
    None = 0,
    x = 1,
    y = 2,
    z = 4,
}

public static class AxisExtensions
{
    public static Quaternion Constrain(this Quaternion q, Axis filter)
    {
        return Quaternion.Euler(Constrain(q.eulerAngles, filter));
    }
    public static Vector3 Constrain(this Vector3 v, Axis filter)
    {
        return new Vector3(
            filter.HasFlag(Axis.x) ? v.x : 0,
            filter.HasFlag(Axis.y) ? v.y : 0,
            filter.HasFlag(Axis.z) ? v.z : 0);
    }
    public static Quaternion Constrain(this Axis filter, Quaternion q)
    {
        return q.Constrain(filter);
    }
    public static Vector3 Constrain(this Axis filter, Vector3 v)
    {
        return v.Constrain(filter);
    }
}