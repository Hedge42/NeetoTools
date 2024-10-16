using System.Text;
using System;

namespace Neeto
{
    [Serializable]
    public struct TagGenerator : IScriptGenerator
    {
        public void Write(ScriptGenerator gen)
        {
            var sb = new StringBuilder();
            foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                sb.AppendLine($"\tpublic const string {tag.Replace(' ', '_')} = \"{tag}\";");
            }
            gen.WriteContent(sb.ToString());
        }
    }
}