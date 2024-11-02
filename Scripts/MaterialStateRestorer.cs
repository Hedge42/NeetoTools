using UnityEngine;

[DefaultExecutionOrder(696969)]
public class MaterialStateRestorer : MonoBehaviour
{
    [Note("Creates a copy of the material in Awake, then restores all values in OnDestroy\n" +
        "Useful for worry-free material modifications at runtime")]
    public Material material;
    private Material copy;

    void Awake()
    {
        copy = new Material(material);
    }
    void OnDestroy()
    {
        material.CopyPropertiesFromMaterial(copy);
        Destroy(copy);
    }
}
