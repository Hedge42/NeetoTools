using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neeto
{
    /// <summary>
    /// Extensions for Vectors aaaaaaa
    /// </summary>
    public static class VectorLogic
    {
        public static Vector3 WithX(this Vector3 v, float x)
        {
            v.x = x;
            return v;
        }
        public static Vector3 WithY(this Vector3 v, float y)
        {
            v.y = y;
            return v;
        }
        public static Vector3 WithZ(this Vector3 v, float z)
        {
            v.z = z;
            return v;
        }
        public static Vector3 WithXY(this Vector3 v, float x, float y)
        {
            v.x = x;
            v.y = y;
            return v;
        }
        public static Vector3 WithXZ(this Vector3 v, float x, float z)
        {
            v.x = x;
            v.z = z;
            return v;
        }
        public static Vector3 WithYZ(this Vector3 v, float y, float z)
        {
            v.y = y;
            v.z = z;
            return v;
        }
        public static Vector3 With(this Vector3 value, float? x = null, float? y = null, float? z = null, float? scale = null)
        {
            value.x = x ?? value.x;
            value.y = y ?? value.y;
            value.z = z ?? value.z;

            if (scale != null)
                value = (float)scale * value.normalized;

            return value;
        }

        public static Vector3 Mask(this Vector3 v, VectorMask filter, float failValue = 0f)
        {
            if (!filter.HasFlag(VectorMask.X))
            {
                v.x = failValue;
            }
            if (!filter.HasFlag(VectorMask.Y))
            {
                v.x = failValue;
            }
            if (!filter.HasFlag(VectorMask.Z))
            {
                v.x = failValue;
            }

            return v;
        }
        public static Vector3 MaskOut(this Vector3 v, VectorMask filter, float failValue = 0f)
        {
            if (filter.HasFlag(VectorMask.X))
            {
                v.x = failValue;
            }
            if (filter.HasFlag(VectorMask.Y))
            {
                v.x = failValue;
            }
            if (filter.HasFlag(VectorMask.Z))
            {
                v.x = failValue;
            }

            return v;
        }
    }

    [Flags]
    public enum VectorMask
    {
        X,
        Y,
        Z,
    }
}