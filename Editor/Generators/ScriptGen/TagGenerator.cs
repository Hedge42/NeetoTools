using System.Text;
using System;

namespace Neeto
{
    [Serializable]
    public struct TagGenerator : IScriptGenerator
    {
        #region generated
        #endregion

        public string Script()
        {
            var sb = new StringBuilder();
            foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                sb.AppendLine($"\tpublic const string {tag.Replace(' ', '_')} = \"{tag}\";");
            }
            return sb.ToString();
        }
    }
}