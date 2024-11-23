using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
#endif


namespace Neeto
{
    public class MeshInstance : MonoBehaviour
    {
        public bool createOnAwake = true;

        public Mesh sourceMesh;

        [Disabled] public Mesh meshInstance;

        public UnityEvent<Mesh> onCreateInstance;

        protected void Awake()
        {
            if (createOnAwake)
                CreateInstance();
        }
        protected virtual void OnDestroy()
        {
            if (meshInstance != null)
            {
                DestroyImmediate(meshInstance);
            }
        }

        void ClearInstance()
        {
            if (meshInstance != null)
            {
                // don't destroy user Meshs
                if (meshInstance.name.EndsWith("(Clone)"))
                {
                    DestroyImmediate(meshInstance);
                }
                meshInstance = null;
            }
        }

        [Button]
        public Mesh CreateInstance()
        {
            ClearInstance();

            meshInstance = new Mesh
            {
                vertices = sourceMesh.vertices,
                triangles = sourceMesh.triangles,
                normals = sourceMesh.normals,
                uv = sourceMesh.uv,
                tangents = sourceMesh.tangents,
                colors = sourceMesh.colors,
                boneWeights = sourceMesh.boneWeights,
                bindposes = sourceMesh.bindposes
            };

            if (sourceMesh.subMeshCount > 1)
            {
                meshInstance.subMeshCount = sourceMesh.subMeshCount;
                for (int i = 0; i < sourceMesh.subMeshCount; i++)
                {
                    meshInstance.SetTriangles(sourceMesh.GetTriangles(i), i);
                }
            }
            meshInstance.name += "(Clone)";

            return meshInstance;
        }
    }
}
