using System.Text;
using UnityEngine;
using System;

namespace Neeto
{
    [Serializable]
    public struct SortingLayerGenerator : IScriptGenerator
    {
        public void Write(ScriptGenerator gen)
        {
            var sb = new StringBuilder("public static class SortingLibrary\n{");
            foreach (var layer in SortingLayer.layers)
            {
                sb.AppendLine($"\tpublic const int {layer.name.Replace(' ', '_')} = {layer.value};");
            }
            sb.AppendLine("}");
            gen.WriteContent(sb.ToString());
        }
    }
}