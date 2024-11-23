using UnityEngine;
using Neeto;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ProceduralTerrain : MonoBehaviour
{
    public int resolution = 256;
    public float size = 100f;
    public float maxVerticalAngle = 60f;

    [GetComponent]
    public MeshFilter meshFilter;

    [Button]
    void Reset()
    {
        resolution = 256;
        size = 100f;
        maxVerticalAngle = 20f;
    }

    [Button]
    void GenerateTerrainPlane()
    {
        GeneratePlaneMesh(size, resolution);
    }

    private Mesh GeneratePlaneMesh(float size, int resolution)
    {
        Mesh mesh = new Mesh();

        int verticesPerSide = resolution + 1;
        int totalVertices = verticesPerSide * verticesPerSide;
        Vector3[] vertices = new Vector3[totalVertices];
        Vector2[] uvs = new Vector2[totalVertices];
        int[] triangles = new int[resolution * resolution * 6];

        float stepSize = size / resolution;

        for (int y = 0, i = 0; y < verticesPerSide; y++)
        {
            for (int x = 0; x < verticesPerSide; x++, i++)
            {
                vertices[i] = new Vector3(x * stepSize, 0, y * stepSize);
                uvs[i] = new Vector2((float)x / resolution, (float)y / resolution);
            }
        }

        for (int y = 0, i = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++, i += 6)
            {
                int vertexIndex = y * verticesPerSide + x;
                triangles[i] = vertexIndex;
                triangles[i + 1] = vertexIndex + verticesPerSide;
                triangles[i + 2] = vertexIndex + verticesPerSide + 1;
                triangles[i + 3] = vertexIndex;
                triangles[i + 4] = vertexIndex + verticesPerSide + 1;
                triangles[i + 5] = vertexIndex + 1;
            }
        }

        vertices = OffsetVerticesY(vertices, maxVerticalAngle, stepSize);

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        this.meshFilter.mesh = mesh;

        return mesh;
    }

    private Vector3[] OffsetVerticesY(Vector3[] vertices, float maxAngle, float stepSize)
    {
        float maxHeight = Mathf.Tan(Mathf.Deg2Rad * maxAngle) * stepSize;
        System.Random random = new System.Random();

        for (int i = 0; i < vertices.Length; i++)
        {
            float randomOffset = (float)random.NextDouble() * maxHeight;
            vertices[i] = new Vector3(vertices[i].x, randomOffset, vertices[i].z);
        }

        return vertices;
    }

}
