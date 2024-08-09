using System.Text;
using UnityEngine;
using System;

namespace Neeto
{
    [Serializable]
    public struct PhysicsLayerGenerator : IScriptGenerator
    {
        public string Script()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 32; i++)
            {
                var name = LayerMask.LayerToName(i);
                if (name.IsEmpty())
                    continue;

                sb.AppendLine($"\t\tpublic const int {name.Replace(' ', '_')} = {1 << i};");
            }
            return sb.ToString();
        }
    }
}