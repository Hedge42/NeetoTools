using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class SingletonObject : ScriptableObject { }
public abstract class SingletonObject<T> : SingletonObject where T : SingletonObject<T>
{
    public static T instance { get; protected set; }
    protected virtual void Awake() => instance ??= (T)this;
}

#if UNITY_EDITOR
static class SingletonObjectInitializer
{
    [InitializeOnLoadMethod]
    static void Initialize()
    {
        EditorApplication.delayCall += () =>
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom(typeof(SingletonObject)))
            {
                // Ensure the type is a valid, non-abstract ScriptableObject type
                if (type.IsAbstract)
                    continue;

                // Use AssetDatabase to find an object of the type
                var guid = AssetDatabase.FindAssets($"t:{type.Name}");
                if (guid.Length == 0) // No asset of this type exists, create one
                {
                    var instance = ScriptableObject.CreateInstance(type);

                    // Create a new asset in a default path, or specify a directory
                    var path = $"Assets/{type.Name}.asset";
                    AssetDatabase.CreateAsset(instance, path);
                    AssetDatabase.SaveAssets();

                    var asset = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
                    Debug.Log($"Created {nameof(SingletonObject)} '{asset.name}' at '{path}'", asset);
                }
            }
        };
    }
}
#endif