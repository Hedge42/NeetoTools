using System.Text;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Neeto
{
    [Serializable]
    public struct LayerHelper
    {
        public static string[] GetPhysicsLayerNames()
        {
            var list = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                var name = LayerMask.LayerToName(i);
                if (name.IsEmpty())
                    continue;

                list.Add(name);
            }
            return list.ToArray();
        }
    }
}