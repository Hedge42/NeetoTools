using UnityEngine;
using System;

namespace Neeto
{
    //[Flags]
    public enum VectorFlags : int
    {
        GlobalPosition = 0,
        LocalPosition = 1,
        GlobalDirection = 1 << 1,
        LocalDirection = 3,
        // wtf
        Local = 1 << 0,
        Direction = 1 << 1,
    }
}