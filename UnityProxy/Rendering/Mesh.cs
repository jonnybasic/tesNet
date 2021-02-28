using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    using MaterialProxy = System.Windows.Media.Media3D.Material;
    using GeometryProxy = System.Windows.Media.Media3D.MeshGeometry3D;
    using MeshProxy = System.Windows.Media.Media3D.Model3DGroup;
    using SubMeshproxy = System.Windows.Media.Media3D.GeometryModel3D;
    using Point3D = System.Windows.Media.Media3D.Point3D;
    using Vector3D = System.Windows.Media.Media3D.Vector3D;
    using Point = System.Windows.Point;

    public enum PrimitiveType
    {
        Cylinder
    }

    public class Mesh : GameObject
    {
        internal struct SubMesh
        {
            public int index;
            public int vertexStart;
            public int vertexCount;
            public int primitiveCount;
            public int[] triangles;
        }

        internal readonly Dictionary<int, SubMesh> subMeshes = new Dictionary<int, SubMesh>();

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
            subMeshes[subMeshIndex] = new SubMesh
            {
                index = subMeshIndex,
                primitiveCount = triangles.Length / 3,
                triangles = triangles,
                vertexCount = triangles.Distinct().Count(),
                vertexStart = triangles.Min(),
            };
        }

        public void RecalculateBounds()
        {
            //throw new NotImplementedException();
            //proxy.Bounds
        }

        public MeshProxy GetMeshProxy(Material[] materials)
        {
            MeshProxy proxy = new MeshProxy();

            foreach (SubMesh sm in subMeshes.Values)
            {
                // get material
                System.Windows.Media.ImageSource source = materials[sm.index].mainTexture;
                Vector2 size = new Vector2((float)source.Width, (float)source.Height);
                // create a child model3d
                GeometryProxy geometry = new GeometryProxy()
                {
                    TriangleIndices = new System.Windows.Media.Int32Collection(
                        sm.triangles.Select((t) => t - sm.vertexStart)),
                    Positions = new System.Windows.Media.Media3D.Point3DCollection(
                        vertices.Skip(sm.vertexStart).Take(sm.vertexCount).Select<Vector3, Point3D>((p) => p)),
                    Normals = new System.Windows.Media.Media3D.Vector3DCollection(
                        normals.Skip(sm.vertexStart).Take(sm.vertexCount).Select<Vector3, Vector3D>((n) => n)),
                    TextureCoordinates = new System.Windows.Media.PointCollection(
                        uv.Skip(sm.vertexStart).Take(sm.vertexCount).Select<Vector2, Point>((c) => (c * size))),
                };

                // add sub-mesh
                proxy.Children.Insert(
                    sm.index,
                    new SubMeshproxy
                    {
                        Geometry = geometry,
                        Material = materials[sm.index]
                    });
            }

            return proxy;
        }

        public static implicit operator MeshProxy(Mesh m)
        {
            MeshProxy proxy = new MeshProxy();

            foreach (SubMesh sm in m.subMeshes.Values)
            {
                // create a child model3d
                GeometryProxy geometry = new GeometryProxy()
                {
                    TriangleIndices = new System.Windows.Media.Int32Collection(
                        sm.triangles.Select((t) => t - sm.vertexStart)),
                    Positions = new System.Windows.Media.Media3D.Point3DCollection(
                        m.vertices.Skip(sm.vertexStart).Take(sm.vertexCount).Select<Vector3, Point3D>((p) => p)),
                    Normals = new System.Windows.Media.Media3D.Vector3DCollection(
                        m.normals.Skip(sm.vertexStart).Take(sm.vertexCount).Select<Vector3, Vector3D>((n) => n)),
                    TextureCoordinates = new System.Windows.Media.PointCollection(
                        m.uv.Skip(sm.vertexStart).Take(sm.vertexCount).Select<Vector2, Point>((c) => c)),
                };

                // add sub-mesh
                proxy.Children.Insert(
                    sm.index,
                    new SubMeshproxy
                    {
                        Geometry = geometry,
                    });
            }

            return proxy;
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
        public ShadowCastingMode shadowCastingMode;
        public Material material;
        public Material[] materials;
        public Material sharedMaterial;
        public Material[] sharedMaterials;
    }
}
