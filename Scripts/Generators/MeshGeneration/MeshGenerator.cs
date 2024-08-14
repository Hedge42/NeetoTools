using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    public class MeshGenerator : MonoBehaviour
    {
        const string PATH = "Assets/Interwoven/Generated";

        [Disabled] public Mesh mesh;

        public string meshName = "New Mesh";

        [SerializeReference, Polymorphic]
        public IMeshGenerator generator = new PlaneMesh();

        public UnityEvent<Mesh> onGenerateMesh;

#if UNITY_EDITOR
        [Button]
        public void Generate()
        {
            if (mesh != null)
            {
                Undo.DestroyObjectImmediate(mesh);
            }

            mesh = generator.Generate();
            mesh.name = meshName;
            Undo.RegisterCreatedObjectUndo(mesh, "Generated Mesh");

            onGenerateMesh?.Invoke(mesh);
        }
        [Button]
        public void Export()
        {
            // open save file dialogue
            var path = EditorUtility.SaveFilePanel("Save generated mesh", PATH, mesh.name, "mesh");

            path = NString.SystemToAssetPath(path);

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(mesh, path);
                Undo.RegisterCreatedObjectUndo(mesh, "Export Mesh");

                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
                EditorGUIUtility.PingObject(mesh);

                //MEditor.CreateAsset(typeof(Mesh));
            }
        }

        public static void InvertNormals(Mesh mesh)
        {
            var normals = new List<Vector3>();
            for (int i = 0; i < normals.Count; i++)
            {
                normals[i] = normals[i].WithY(-normals[i].y);
                //normals[i] *= -1;
            }

            //mesh.normals = normals.ToArray();
            mesh.SetNormals(normals);
        }
#endif
    }

    public interface IMeshGenerator
    {
        public Mesh Generate();
    }

    [Serializable]
    public class OpenCone : IMeshGenerator
    {
        public float outerRadius = 1f;
        public float innerRadius = .5f;
        public float height = 1f;
        public int circleResolution = 40;
        public float offset = -180f;
        public bool wrap;

        public bool invert;

        [Range(0, 1)]
        public float arc = .5f;



        public Mesh Generate()
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var vertexRes = 0;

            float stepAngle = 360f / circleResolution;
            var fullArc = arc * 360f;
            var c = circleResolution * arc;
            var angleOffset = fullArc / -2f * Mathf.Deg2Rad;
            for (int i = 0; i <= circleResolution * arc; i++)
            {

                float angleInRad = Mathf.Deg2Rad * i * stepAngle + angleOffset;
                float cos = Mathf.Cos(angleInRad);
                float sin = Mathf.Sin(angleInRad);

                // draw slice
                vertices.Add(new Vector3(innerRadius * sin, height / 2, innerRadius * cos));
                vertices.Add(new Vector3(outerRadius * sin, 0f, outerRadius * cos));
                vertices.Add(new Vector3(innerRadius * sin, -height / 2, innerRadius * cos));

                vertexRes = vertices.Count;
            }
            var n = vertexRes;
            if (!wrap)
                n -= 3;

            for (int i = 0; i < n; i += 3)
            {
                // 1, 2, 4
                // 5, 4, 2
                // 2, 3, 5
                // 5, 3, 6
                var _0 = i;
                var _1 = i + 1;
                var _2 = i + 2;
                var _3 = (i + 3) % vertexRes;
                var _4 = (i + 4) % vertexRes;
                var _5 = (i + 5) % vertexRes;

                if (!invert)
                {
                    triangles.AddRange(new int[] { _0, _1, _3 });
                    triangles.AddRange(new int[] { _3, _1, _4 });
                    triangles.AddRange(new int[] { _1, _2, _4 });
                    triangles.AddRange(new int[] { _4, _2, _5 });
                }
                else
                {
                    triangles.AddRange(new int[] { _0, _3, _1 });
                    triangles.AddRange(new int[] { _3, _4, _1 });
                    triangles.AddRange(new int[] { _1, _4, _2 });
                    triangles.AddRange(new int[] { _4, _5, _2 });
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            //if (invert)
                //MeshGenerator.InvertNormals(mesh);

            mesh.RecalculateBounds();
            //mesh.RecalculateTangents();
            mesh.RecalculateUVDistributionMetrics();

            return mesh;
        }

    }

    [Serializable]
    public class OpenCylinder : IMeshGenerator
    {
        public int sides = 12;
        public float radius = .5f;
        public float height = 1f;

        public Mesh Generate()
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

    [Serializable]
    public class PolygonGenerator : IMeshGenerator
    {
        #region setup
        //mesh properties
        Mesh mesh;
        public Vector3[] polygonPoints;
        public int[] polygonTriangles;

        //polygon properties
        public bool isFilled;
        public int polygonSides;
        public float polygonRadius;
        public float centerRadius;



        public Mesh Generate()
        {
            mesh = new Mesh();

            if (isFilled)
            {
                DrawFilled(polygonSides, polygonRadius);
            }
            else
            {
                DrawHollow(polygonSides, polygonRadius, centerRadius);
            }

            return mesh;
        }
        #endregion

        void DrawFilled(int sides, float radius)
        {
            polygonPoints = GetCircumferencePoints(sides, radius).ToArray();
            polygonTriangles = DrawFilledTriangles(polygonPoints);
            mesh.Clear();
            mesh.vertices = polygonPoints;
            mesh.triangles = polygonTriangles;
        }

        void DrawHollow(int sides, float outerRadius, float innerRadius)
        {
            List<Vector3> pointsList = new List<Vector3>();
            List<Vector3> outerPoints = GetCircumferencePoints(sides, outerRadius);
            pointsList.AddRange(outerPoints);
            List<Vector3> innerPoints = GetCircumferencePoints(sides, innerRadius);
            pointsList.AddRange(innerPoints);

            polygonPoints = pointsList.ToArray();

            polygonTriangles = DrawHollowTriangles(polygonPoints);
            mesh.Clear();
            mesh.vertices = polygonPoints;
            mesh.triangles = polygonTriangles;
        }

        int[] DrawHollowTriangles(Vector3[] points)
        {
            int sides = points.Length / 2;
            int triangleAmount = sides * 2;

            List<int> newTriangles = new List<int>();
            for (int i = 0; i < sides; i++)
            {
                int outerIndex = i;
                int innerIndex = i + sides;

                //first triangle starting at outer edge i
                newTriangles.Add(outerIndex);
                newTriangles.Add(innerIndex);
                newTriangles.Add((i + 1) % sides);

                //second triangle starting at outer edge i
                newTriangles.Add(outerIndex);
                newTriangles.Add(sides + ((sides + i - 1) % sides));
                newTriangles.Add(outerIndex + sides);
            }
            return newTriangles.ToArray();
        }

        List<Vector3> GetCircumferencePoints(int sides, float radius)
        {
            List<Vector3> points = new List<Vector3>();
            float circumferenceProgressPerStep = (float)1 / sides;
            float TAU = 2 * Mathf.PI;
            float radianProgressPerStep = circumferenceProgressPerStep * TAU;

            for (int i = 0; i < sides; i++)
            {
                float currentRadian = radianProgressPerStep * i;
                points.Add(new Vector3(Mathf.Cos(currentRadian) * radius, Mathf.Sin(currentRadian) * radius, 0));
            }
            return points;
        }

        int[] DrawFilledTriangles(Vector3[] points)
        {
            int triangleAmount = points.Length - 2;
            List<int> newTriangles = new List<int>();
            for (int i = 0; i < triangleAmount; i++)
            {
                newTriangles.Add(0);
                newTriangles.Add(i + 2);
                newTriangles.Add(i + 1);
            }
            return newTriangles.ToArray();
        }
    }

    [Serializable]
    public class PlaneMesh : IMeshGenerator
    {
        public int subdivisions = 256;
        public bool centerOrigin;

        public Mesh Generate()
        {
            var mesh = new Mesh();
            int vertCount = (subdivisions + 1) * (subdivisions + 1);
            Vector3[] vertices = new Vector3[vertCount];
            int[] triangles = new int[subdivisions * subdivisions * 6];

            float stepSize = 1f / subdivisions;
            int triIndex = 0;

            for (int i = 0, z = 0; z <= subdivisions; z++)
            {
                for (int x = 0; x <= subdivisions; x++, i++)
                {
                    vertices[i] = new Vector3(x * stepSize, 0, z * stepSize);

                    if (centerOrigin)
                        vertices[i] -= new Vector3(.5f, 0, .5f);

                    if (x < subdivisions && z < subdivisions)
                    {
                        triangles[triIndex++] = i;
                        triangles[triIndex++] = i + subdivisions + 1;
                        triangles[triIndex++] = i + 1;
                        triangles[triIndex++] = i + 1;
                        triangles[triIndex++] = i + subdivisions + 1;
                        triangles[triIndex++] = i + subdivisions + 2;
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateUVDistributionMetrics();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            mesh.name = "Plane_" + subdivisions.ToString();

            return mesh;
        }
    }
}