using System.Text;

namespace Neeto
{
    public static class TagLibrary
    {
        [QuickAction]
        public static void Generate()
        {
            var sb = new StringBuilder();
            foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                sb.AppendLine($"\tpublic const string {ScriptGenerator.Codify(tag)} = \"{tag}\";");
            }
            var script = ScriptGenerator.FindScript(typeof(TagLibrary));
            ScriptGenerator.OverwriteRegion(script, "GENERATED", sb.ToString());
        }

        #region GENERATED
        public const string Untagged = "Untagged";
        public const string Respawn = "Respawn";
        public const string Finish = "Finish";
        public const string EditorOnly = "EditorOnly";
        public const string MainCamera = "MainCamera";
        public const string Player = "Player";
        public const string GameController = "GameController";
        public const string Rig = "Rig";
        public const string Weapon = "Weapon";
        public const string Fire = "Fire";
        public const string Gameplay = "Gameplay";
        public const string Cutscene = "Cutscene";
        #endregion
    }
}