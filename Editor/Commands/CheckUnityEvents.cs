//using ExtEvents;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
public static class CheckUnityEvents
{
    [MenuItem(MPath.Debug + "Check empty UnityEvents in scene")]
    static void CheckScene()
    {
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            foreach (MonoBehaviour mono in obj.GetComponents<MonoBehaviour>())
            {
                if (mono == null) continue;

                FieldInfo[] fieldInfos = mono.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (FieldInfo fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType.IsSubclassOf(typeof(UnityEventBase)))
                    {
                        UnityEventBase unityEvent = fieldInfo.GetValue(mono) as UnityEventBase;

                        Validate(unityEvent, mono);
                    }
                    //else if (fieldInfo.FieldType.IsSubclassOf(typeof(ExtEvents.BaseExtEvent)))
                    //{
                    //    BaseExtEvent unityEvent = fieldInfo.GetValue(mono) as BaseExtEvent;

                    //    foreach (var listener in unityEvent.PersistentListeners)
                    //    {
                    //        if (listener == null || string.IsNullOrEmpty(listener.MethodName))
                    //        {
                    //            Debug.LogError($"Invalid ExtEvent ({obj.name}/{mono.GetType().Name}/{fieldInfo.Name})", mono);
                    //        }
                    //    }
                    //}
                }
            }
        }
    }

    public static bool Validate(UnityEventBase unityEvent, Object mono = null)
    {
        for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
        {
            UnityEngine.Object target = unityEvent.GetPersistentTarget(i);
            string methodName = unityEvent.GetPersistentMethodName(i);

            if (target == null || string.IsNullOrEmpty(methodName))
            {
                if (mono)
                    Debug.LogError($"Invalid UnityEvent ", mono);
                else
                    Debug.LogError($"Invalid UnityEvent ");
                return false;
            }
        }
        return true;
    }

    [MenuItem(MPath.Debug + "Check empty UnityEvents in project")]
    public static void CheckProject()
    {
        string[] guids = AssetDatabase.FindAssets("t:Object");  // t:Object will get you all assets, not just GameObjects

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath); // Load all sub-assets for things like prefabs

            foreach (Object asset in subAssets)
            {
                // Skips non-MonoBehaviour assets
                if (asset is not MonoBehaviour)
                    continue;

                var mono = asset as MonoBehaviour;
                var obj = mono.gameObject;

                // Reflective checks on each MonoBehaviour
                var fieldInfos = mono.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType.IsSubclassOf(typeof(UnityEventBase)))
                    {
                        var unityEvent = fieldInfo.GetValue(mono) as UnityEventBase;

                        if (unityEvent == null) continue;

                        for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
                        {
                            UnityEngine.Object target = unityEvent.GetPersistentTarget(i);
                            string methodName = unityEvent.GetPersistentMethodName(i);

                            if (target == null || string.IsNullOrEmpty(methodName))
                            {
                                Debug.LogError($"Invalid UnityEvent({obj.name}/{mono.GetType().Name}/{fieldInfo.Name})", mono);
                            }
                        }
                    }
                }
            }
        }

        Debug.Log("Completed UnityEvent check in project assets.");
    }
}
#endif