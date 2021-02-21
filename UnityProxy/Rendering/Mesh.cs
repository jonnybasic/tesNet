using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public enum PrimitiveType
    {
        Cylinder
    }

    public class Mesh : GameObject
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector4[] tangents;
        public Vector2[] uv;
        public Int32[] triangles;

        public int vertexCount
        { get => vertices.Length; }

        public int subMeshCount;

        public void SetTriangles(int[] triangles, int subMeshIndex)
        {
            throw new NotImplementedException();
        }

        public void RecalculateBounds()
        {
            throw new NotImplementedException();
        }
    }

    public class TreeInstance : GameObject
    {
        public float heightScale;
        public float widthScale;
        public Color32 color;
        public Color lightmapColor;
        public Vector3 position;
        public float rotation;
        public int prototypeIndex;
    }

    public class TreePrototype : GameObject
    {
        public GameObject prefab;
        public int bendFactor;
    }

    public class TerrainData : GameObject
    {
        public int treeInstanceCount;
        public TreeInstance[] treeInstances;
        public TreePrototype[] treePrototypes;

        public void RefreshPrototypes()
        {
            throw new NotImplementedException();
        }
    }

    public class Terrain : GameObject
    {
        public TerrainData terrainData;
        public int treeDistance;
        public int treeBillboardDistance;
        public int treeMaximumFullLODCount;
        public int treeCrossFadeLength;

        public void AddTreeInstance(TreeInstance treeInstance)
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public static GameObject CreateTerrainGameObject(object p)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class MeshFilter : GameObject
    {
        public abstract Mesh sharedMesh
        { get; set; }
    }

    public class MeshRenderer : GameObject
    {
        public bool receiveShadows;
        public Bounds bounds;
        public Material[] sharedMaterials;
        public Material sharedMaterial;
        public ShadowCastingMode shadowCastingMode;
        public Material[] materials;
    }
}
