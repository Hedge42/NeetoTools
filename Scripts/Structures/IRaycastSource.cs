using System;
using System.Collections.Generic;
using UnityEngine;

namespace Neeto
{
    public interface IRaycastSource
    {
        public RaycastInfo Raycast();
    }
}