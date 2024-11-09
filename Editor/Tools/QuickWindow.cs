using UnityEditor;

namespace Neeto
{
    public class QuickWindow<T> : EditorWindow
    {
        [NoFoldout] public T value;

        Editor editor;
        protected virtual void OnGUI()
        {
            Editor.CreateCachedEditor(this, null, ref editor);
            editor.OnInspectorGUI();
        }
    }
}