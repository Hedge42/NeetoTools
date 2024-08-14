using UnityEditor;
using UnityEngine;

public class AnimationCommands
{
    [MenuItem("Assets/Remove Root Motion Z", true)]
    private static bool ValidateRemoveRootMotionZ()
    {
        return Selection.activeObject is AnimationClip;
    }
    [MenuItem("Assets/Remove Root Motion Z")]
    private static void RemoveRootMotionZ()
    {
        var sourceClip = Selection.activeObject as AnimationClip;
        if (sourceClip == null)
        {
            Debug.LogError("Selected asset is not an AnimationClip.");
            return;
        }

        var newClip = CloneAndRemoveRootMotionZ(sourceClip);

        var path = AssetDatabase.GetAssetPath(sourceClip);
        var newPath = path.Replace(".anim", "_NoRootMotionZ.anim");

        AssetDatabase.CreateAsset(newClip, newPath);
        AssetDatabase.SaveAssets();

        Debug.Log("Created new animation clip without root motion in z direction: " + newPath);
    }

    [MenuItem(MenuPath.Assets + nameof(LogBindings), validate = false)]
    static void LogBindings()
    {
        Debug.Log(AnimationUtility.GetCurveBindings(Selection.activeObject as AnimationClip)
            .JoinString('\n', b => $"{b.path}:{b.type.Name}:{b.propertyName}"));
    }
    [MenuItem(MenuPath.Assets + nameof(LogBindings), validate = false)]
    static bool OnLogBindings()
    {
        return Selection.activeObject is AnimationClip;
    }

    private static AnimationClip CloneAndRemoveRootMotionZ(AnimationClip clip)
    {
        var newClip = new AnimationClip
        {
            name = clip.name + "_NoRootMotionZ",
            frameRate = clip.frameRate
        };

        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            var curve = AnimationUtility.GetEditorCurve(clip, binding);

            if (binding.propertyName.Contains("RootT.z"))
            {
                // Create a new curve without any z motion (flat curve)
                var newCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(clip.length, 0) });
                AnimationUtility.SetEditorCurve(newClip, binding, newCurve);
            }
            else
            {
                AnimationUtility.SetEditorCurve(newClip, binding, curve);
            }
        }

        foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
        {
            var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            AnimationUtility.SetObjectReferenceCurve(newClip, binding, keyframes);
        }

        return newClip;
    }
}
