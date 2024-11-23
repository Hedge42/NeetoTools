using UnityEngine;
using Neeto;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class OpenCylinderGenerator : MonoBehaviour
{
    public bool onAwake;

    public int sides = 6;
    public float radius = 1.0f;
    public float height = 2.0f;

    [GetComponent]
    public MeshFilter meshFilter;
    [GetComponent]
    public MeshRenderer meshRenderer;


    [Button]
    void Generate()
    {
        meshFilter.mesh = GenerateOpenCylinderMesh(sides, radius, height);
    }

    private void Awake()
    {
        if (onAwake)
            Generate();
    }

    private Mesh GenerateOpenCylinderMesh(int sides, float radius, float height)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[2 * (sides + 1)];
        int[] triangles = new int[6 * sides];
        Vector2[] uvs = new Vector2[vertices.Length];

        float angleIncrement = 2 * Mathf.PI / sides;

        for (int i = 0; i <= sides; i++)
        {
            float angle = angleIncrement * i;
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);

            vertices[i] = new Vector3(x, 0, z);
            vertices[i + sides + 1] = new Vector3(x, height, z);
            uvs[i] = new Vector2((float)i / sides, 0);
            uvs[i + sides + 1] = new Vector2((float)i / sides, 1);

            if (i < sides)
            {
                int offset = 6 * i;
                triangles[offset] = i;
                triangles[offset + 1] = i + sides + 1;
                triangles[offset + 2] = i + 1;
                triangles[offset + 3] = i + 1;
                triangles[offset + 4] = i + sides + 1;
                triangles[offset + 5] = i + sides + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }
}
