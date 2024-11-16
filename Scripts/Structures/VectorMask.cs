using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neeto
{
    [Flags]
    public enum VectorMask
    {
        X,
        Y,
        Z,
    }
    public static class VectorExtensions
    {
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

}