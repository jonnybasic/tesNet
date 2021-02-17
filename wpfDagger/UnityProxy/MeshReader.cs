// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2020 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    Scott Taylor (jonnybasic@gmail.com)
// 
// Notes: Converted to WPF3D
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshopWpf
{
    /// <summary>
    /// Imports Daggerfall models into Wpf3D as GeometryModel3D objects.
    /// </summary>
    public class MeshReader// : MonoBehaviour
    {
        #region Fields

        DaggerfallWpf3D dfUnity = null;
        Arch3dFile arch3dFile = null;

        Dictionary<int, ModelData> modelDict = new Dictionary<int, ModelData>();

        #endregion

        #region Public Fields

        public bool AddMeshTangents = true;
        public bool AddMeshLightmapUVs = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets true if file reading is ready.
        /// You should always check this before loading model data or meshes.
        /// </summary>
        public bool IsReady
        {
            get { return ReadyCheck(); }
        }

        public Arch3dFile Arch3d
        {
            get { return arch3dFile; }
        }

        public Material BackMaterial
        { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads model data.
        /// </summary>
        /// <param name="id">Key of source mesh.</param>
        /// <param name="modelData">ModelData out.</param>
        /// <param name="scale">Scale of model.</param>
        /// <returns>True if successful.</returns>
        public bool GetModelData(uint id, out ModelData modelData)
        {
            // New model object
            modelData = new ModelData();

            // Ready check
            if (!IsReady)
                return false;

            // Return from cache if present
            if (modelDict.ContainsKey((int)id))
            {
                modelData = modelDict[(int)id];
                return true;
            }

            // Find mesh index
            int index = arch3dFile.GetRecordIndex(id);
            if (index == -1)
                return false;

            // Get DFMesh
            DFMesh dfMesh = arch3dFile.GetMesh(index);
            if (dfMesh.TotalVertices == 0)
                return false;

            // Load mesh data
            modelData.DFMesh = dfMesh;
            LoadVertices(ref modelData);
            LoadIndices(ref modelData);

            // Add to cache
            modelDict.Add((int)id, modelData);

            return true;
        }

        /// <summary>
        /// Gets Unity Mesh from Daggerfall model.
        /// </summary>
        /// <param name="dfUnity">DaggerfallWpf3D singleon for loading content.</param>
        /// <param name="modelID">Daggerfall model ID to load..</param>
        /// <param name="cachedMaterialsOut">Array of cached materials in order of submesh.</param>
        /// <param name="textureKeysOut">Array of original texture keys in order of submesh.</param>
        /// <param name="hasAnimationsOut">True if one or more materials have animations.</param>
        /// <param name="solveTangents">Solve tangents for this mesh.</param>
        /// <param name="lightmapUVs">Add secondary lightmap UVs to this mesh.</param>
        /// <returns>Mesh object or null.</returns>
        public Model3D GetMesh(
            uint modelID,
            out CachedMaterial[] cachedMaterialsOut,
            out int[] textureKeysOut,
            out bool hasAnimationsOut,
            bool solveTangents = false,
            bool lightmapUVs = false)
        {
            cachedMaterialsOut = null;
            textureKeysOut = null;
            hasAnimationsOut = false;

            // Ready check
            if (!IsReady)
                return null;

            // Get model data
            ModelData model;
            if (!GetModelData(modelID, out model))
            {
                DaggerfallWpf3D.LogMessage(string.Format("Unknown ModelID {0}.", modelID.ToString()), true);
                return null;
            }
#if true
            // Load materials
            cachedMaterialsOut = new CachedMaterial[model.SubMeshes.Length];
            textureKeysOut = new int[model.SubMeshes.Length];
            for (int i = 0; i < model.SubMeshes.Length; i++)
            {
                int archive = model.DFMesh.SubMeshes[i].TextureArchive;
                int record = model.DFMesh.SubMeshes[i].TextureRecord;
                textureKeysOut[i] = MaterialReader.MakeTextureKey((short)archive, (byte)record, (byte)0);

                // Add material to array
                CachedMaterial cachedMaterial;
                dfUnity.MaterialReader.GetCachedMaterial(archive, record, 0, out cachedMaterial);
                cachedMaterialsOut[i] = cachedMaterial;

                // Set animation flag
                if (cachedMaterial.singleFrameCount > 1 && !hasAnimationsOut)
                    hasAnimationsOut = true;
            }
            Model3DGroup model3DGroup = new Model3DGroup();
            // Build Sub Geometry
            for (int s = 0; s < model.SubMeshes.Length; ++s)
            {
                var sub = model.SubMeshes[s];

                // Create mesh; Primives x3 = indicies and x2 = points, normals, uv
                MeshGeometry3D model3D = new MeshGeometry3D()
                {
                    // Primitive Count * 3 = Total Triangle Indicies (3 verts per Triangle)
                    TriangleIndices = new Int32Collection(sub.PrimitiveCount * 3),
                    Positions = new Point3DCollection(
                        model.Vertices.Skip(sub.VertexStart).Take(sub.VertexCount)),
                    Normals = new Vector3DCollection(
                        model.Normals.Skip(sub.VertexStart).Take(sub.VertexCount)),
                    TextureCoordinates = new PointCollection(
                        model.UVs.Skip(sub.VertexStart).Take(sub.VertexCount))
                };
                for (int t = 0; t < sub.PrimitiveCount * 3; t++)
                {
                    // reindex at starting
                    model3D.TriangleIndices.Add((int)model.Indices[sub.StartIndex + t] - sub.VertexStart);
                }
                // add child
                model3DGroup.Children.Add(new GeometryModel3D
                {
                    BackMaterial = BackMaterial,
                    Geometry = model3D,
                    Material = cachedMaterialsOut[s].material
                });
            }

            //// Finalise mesh
            //if (solveTangents) TangentSolver(mesh);
            //if (lightmapUVs) AddLightmapUVs(mesh);

            return model3DGroup;
#else
            // Create mesh
            MeshGeometry3D mesh = new MeshGeometry3D
            {
                TriangleIndices = new Int32Collection(model.Indices),
                Positions = new Point3DCollection(model.Vertices),
                Normals = new Vector3DCollection(model.Normals),
                TextureCoordinates = new PointCollection(model.UVs)
            };
            //mesh.name = modelID.ToString();

            return new GeometryModel3D
            {
                BackMaterial = new DiffuseMaterial(Brushes.DarkCyan),
                Geometry = mesh,
                Material = new DiffuseMaterial(Brushes.Wheat)
            };
#endif
        }
#if false
        /// <summary>
        /// Gets Unity Mesh from previously combined model data.
        /// </summary>
        /// <param name="dfUnity">DaggerfallWpf3D singleon for loading content.</param>
        /// <param name="combiner">ModelCombiner to build from.</param>
        /// <param name="cachedMaterialsOut">Array of cached materials in order of submesh.</param>
        /// <param name="textureKeysOut">Array of original texture keys in order of submesh.</param>
        /// <param name="hasAnimationsOut">True if one or more materials have animations.</param>
        /// <param name="solveTangents">Solve tangents for this mesh.</param>
        /// <param name="lightmapUVs">Add secondary lightmap UVs to this mesh.</param>
        /// <returns>Mesh object or null.</returns>
        public Mesh GetCombinedMesh(
            DaggerfallWpf3D dfUnity,
            ModelCombiner combiner,
            out CachedMaterial[] cachedMaterialsOut,
            out int[] textureKeysOut,
            out bool hasAnimationsOut,
            bool solveTangents = false,
            bool lightmapUVs = false)
        {
            cachedMaterialsOut = null;
            hasAnimationsOut = false;
            textureKeysOut = null;

            // Ready check
            if (!IsReady)
                return null;

            // Get combined model
            ModelCombiner.CombinedModel combinedModel;
            if (!combiner.GetCombinedModel(out combinedModel))
                return null;

            // Load materials
            cachedMaterialsOut = new CachedMaterial[combinedModel.SubMeshes.Length];
            textureKeysOut = new int[combinedModel.SubMeshes.Length];
            for (int i = 0; i < combinedModel.SubMeshes.Length; i++)
            {
                int archive = combinedModel.SubMeshes[i].TextureArchive;
                int record = combinedModel.SubMeshes[i].TextureRecord;
                textureKeysOut[i] = MaterialReader.MakeTextureKey((short)archive, (byte)record, (byte)0);

                // Add material to array
                CachedMaterial cachedMaterial;
                dfUnity.MaterialReader.GetCachedMaterial(archive, record, 0, out cachedMaterial);
                cachedMaterialsOut[i] = cachedMaterial;

                // Set animation flag
                if (cachedMaterial.singleFrameCount > 1 && !hasAnimationsOut)
                    hasAnimationsOut = true;
            }

            // Create mesh
            Mesh mesh = new Mesh();
            mesh.name = "CombinedMesh";
            mesh.vertices = combinedModel.Vertices;
            mesh.normals = combinedModel.Normals;
            mesh.uv = combinedModel.UVs;
            mesh.subMeshCount = combinedModel.SubMeshes.Length;

            // Set submesh triangles
            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                var sub = combinedModel.SubMeshes[s];
                int[] triangles = new int[sub.PrimitiveCount * 3];
                for (int t = 0; t < sub.PrimitiveCount * 3; t++)
                {
                    triangles[t] = combinedModel.Indices[sub.StartIndex + t];
                }
                mesh.SetTriangles(triangles, s);
            }

            // Finalise mesh
            if (solveTangents) TangentSolver(mesh);
            if (lightmapUVs) AddLightmapUVs(mesh);
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Get a Unity quad mesh from texture to use as billboard.
        /// Will be scaled based on size settings in texture.
        /// </summary>
        /// <param name="rect">UV rect of material.</param>
        /// <param name="archive">Texture archive index. Only used to calculate size.</param>
        /// <param name="record">Texture record index. Only used to calculate size.</param>
        /// <param name="sizeOut">Size of billboard in world units.</param>
        /// <returns>Mesh object or null.</returns>
        public Mesh GetBillboardMesh(Rect rect, int archive, int record, out Point sizeOut)
        {
            sizeOut = Point.zero;

            // Ready check
            if (!IsReady)
                return null;

            // Attempt to get cached atlas data if atlases enabled
            CachedMaterial cm;
            Point size, scale;
            dfUnity.MaterialReader.GetCachedMaterialAtlas(archive, out cm);
            if (cm.keyGroup == MaterialReader.AtlasKeyGroup)
            {
                size = cm.recordSizes[record];
                scale = cm.recordScales[record];
            }
            else
            {
                // Get single material data
                // This is also fallback if atlas not available
                // Texture will be loaded singlely which takes longer
                dfUnity.MaterialReader.GetCachedMaterial(archive, record, 0, out cm);
                size = cm.recordSizes[0];
                scale = cm.recordScales[0];
            }

            // Apply scale
            Point finalSize;
            int xChange = (int)(size.x * (scale.x / BlocksFile.ScaleDivisor));
            int yChange = (int)(size.y * (scale.y / BlocksFile.ScaleDivisor));
            finalSize.x = (size.x + xChange);
            finalSize.y = (size.y + yChange);

            // Store sizeOut
            sizeOut = finalSize * MeshReader.GlobalScale;

            //// Nature (TEXTURE.500 and up) do not use scaling in dungeons. Revert scaling.
            //if (dungeon && archive > 499)
            //    finalSize = size;

            // Calcuate offset for correct positioning in scene
            //Vector3 offset = Vector3.zero;
            //if (!dungeon)
            //    offset.y = (finalSize.y / 2) * GlobalScale;

            // Vertices
            float hx = (finalSize.x / 2) * GlobalScale;
            float hy = (finalSize.y / 2) * GlobalScale;
            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(hx, hy, 0);// + offset;
            vertices[1] = new Vector3(-hx, hy, 0);// + offset;
            vertices[2] = new Vector3(hx, -hy, 0);// + offset;
            vertices[3] = new Vector3(-hx, -hy, 0);// + offset;

            // Indices
            int[] indices = new int[6]
            {
                0, 1, 2,
                3, 2, 1,
            };

            // Normals
            // Setting in between forward and up so billboards will
            // pick up some light from both above and in front.
            // This seems to work generally well for both directional and point lights.
            // Possibly need a better solution later.
            Vector3 normal = Vector3.Normalize(Vector3.up + Vector3.forward);
            Vector3[] normals = new Vector3[4];
            normals[0] = normal;
            normals[1] = normal;
            normals[2] = normal;
            normals[3] = normal;

            // UVs
            Point[] uvs = new Point[4];
            uvs[0] = new Point(rect.x, rect.yMax);
            uvs[1] = new Point(rect.xMax, rect.yMax);
            uvs[2] = new Point(rect.x, rect.y);
            uvs[3] = new Point(rect.xMax, rect.y);

            // Create mesh
            Mesh mesh = new Mesh();
            mesh.name = string.Format("BillboardMesh");
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.normals = normals;
            mesh.uv = uvs;

            return mesh;
        }

        /// <summary>
        /// Gets a most basic quad.
        /// </summary>
        /// <param name="size">Size of quad in Unity units (not Daggerfall units).</param>
        /// <returns>Mesh object.</returns>
        public Mesh GetSimpleBillboardMesh(Point size)
        {
            // Ready check
            if (!IsReady)
                return null;

            // Vertices
            float hx = size.x / 2;
            float hy = size.y / 2;
            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(hx, hy, 0);
            vertices[1] = new Vector3(-hx, hy, 0);
            vertices[2] = new Vector3(hx, -hy, 0);
            vertices[3] = new Vector3(-hx, -hy, 0);

            // Indices
            int[] indices = new int[6]
            {
                0, 1, 2,
                3, 2, 1,
            };

            // Normals
            // Setting in between forward and up so billboards will
            // pick up some light from both above and in front.
            // This seems to work generally well for both directional and point lights.
            // Possibly need a better solution later.
            Vector3 normal = Vector3.Normalize(Vector3.up + Vector3.forward);
            Vector3[] normals = new Vector3[4];
            normals[0] = normal;
            normals[1] = normal;
            normals[2] = normal;
            normals[3] = normal;

            // UVs
            Point[] uvs = new Point[4];
            uvs[0] = new Point(0, 1);
            uvs[1] = new Point(1, 1);
            uvs[2] = new Point(0, 0);
            uvs[3] = new Point(1, 0);

            // Create mesh
            Mesh mesh = new Mesh();
            mesh.name = string.Format("SimpleBillboardMesh");
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.normals = normals;
            mesh.uv = uvs;

            return mesh;
        }

        /// <summary>
        /// Gets a simple ground plane mesh.
        /// This is only used for RMB block layouts, not for terrain system.
        /// </summary>
        /// <param name="blockData">BlockData for tiles layout.</param>
        /// <param name="tileMap">Tilemap Color32 array for shader.</param>
        /// <returns>Mesh.</returns>
        public Mesh GetSimpleGroundPlaneMesh(
            ref DFBlock blockData,
            out Color32[] tileMap,
            bool solveTangents = false,
            bool lightmapUVs = false)
        {
            const int tileDim = 16;
            tileMap = new Color32[tileDim * tileDim];

            // Make ground slightly lower to minimise depth-fighting on ground aligned polygons
            // But not too low, or shadows can be seen under buildings
            float groundHeight = DaggerfallGroundPlane.GroundOffset * MeshReader.GlobalScale;

            // Create tilemap
            for (int y = 0; y < tileDim; y++)
            {
                for (int x = 0; x < tileDim; x++)
                {
                    // Get source tile data
                    DFBlock.RmbGroundTiles tile = blockData.RmbBlock.FldHeader.GroundData.GroundTiles[x, (tileDim - 1) - y];

                    // Calculate tile index
                    byte record = (byte)(tile.TextureRecord * 4);
                    if (tile.IsRotated && !tile.IsFlipped)
                        record += 1;
                    if (!tile.IsRotated && tile.IsFlipped)
                        record += 2;
                    if (tile.IsRotated && tile.IsFlipped)
                        record += 3;

                    // Assign tile index, setting random marker back to grass
                    int offset = (y * tileDim) + x;
                    if (tile.TextureRecord < 56)
                        tileMap[offset] = new Color32(record, 0, 0, record);
                    else
                        tileMap[offset] = new Color32(8, 0, 0, 8);      // Index 8 is grass
                }
            }

            // Create a basic quad
            float tileSize = DaggerfallGroundPlane.TileSize * GlobalScale;
            float quadSize = tileSize * tileDim;

            // Vertices
            Vector3[] verts = new Vector3[4];
            verts[0] = new Vector3(0, groundHeight, 0);
            verts[1] = new Vector3(0, groundHeight, quadSize);
            verts[2] = new Vector3(quadSize, groundHeight, quadSize);
            verts[3] = new Vector3(quadSize, groundHeight, 0);

            // Normals
            Vector3[] norms = new Vector3[4];
            norms[0] = Vector3.up;
            norms[1] = Vector3.up;
            norms[2] = Vector3.up;
            norms[3] = Vector3.up;

            // UVs
            Point[] uvs = new Point[4];
            uvs[0] = new Point(0, 0);
            uvs[1] = new Point(0, 1);
            uvs[2] = new Point(1, 1);
            uvs[3] = new Point(1, 0);

            // Indices
            int[] indices = new int[6];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;

            // Create mesh
            Mesh mesh = new Mesh();
            mesh.name = "SimpleGroundPlaneMesh";
            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.uv = uvs;
            mesh.triangles = indices;

            // Finalise mesh
            if (solveTangents) TangentSolver(mesh);
            if (lightmapUVs) AddLightmapUVs(mesh);
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Gets scaled billboard size based on scaling in texture file.
        /// </summary>
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        /// <param name="archive">Texture archive index.</param>
        /// <param name="record">Texture record index.</param>
        /// <returns>Final scaled size.</returns>
        public Point GetScaledBillboardSize(int archive, int record)
        {
            // Get cached material data
            CachedMaterial cm;
            if (!dfUnity.MaterialReader.GetCachedMaterial(archive, record, 0, out cm))
                return Point.zero;

            // Get size and scale
            Point size = cm.recordSizes[0];
            Point scale = cm.recordScales[0];

            // Apply scale
            Point finalSize;
            int xChange = (int)(size.x * (scale.x / BlocksFile.ScaleDivisor));
            int yChange = (int)(size.y * (scale.y / BlocksFile.ScaleDivisor));
            finalSize.x = (size.x + xChange);
            finalSize.y = (size.y + yChange);

            return finalSize;
        }
#endif

        #endregion

        #region Support

        /// <summary>
        /// Clears model cache dictionary, forcing models to reload.
        /// </summary>
        public void ClearCache()
        {
            modelDict.Clear();
            if (arch3dFile != null)
                arch3dFile.DiscardAllRecords();
        }

#endregion

#region Private Methods

        private bool ReadyCheck()
        {
            // Ensure we have a DaggerfallWpf3D reference
            if (dfUnity == null)
            {
                dfUnity = DaggerfallWpf3D.Instance;
            }

            // Do nothing if DaggerfallWpf3D not ready
            if (!dfUnity.IsReady)
            {
                DaggerfallWpf3D.LogMessage("MeshReader: DaggerfallWpf3D component is not ready. Have you set your Arena2 path?");
                return false;
            }

            // Ensure reader is created
            if (arch3dFile == null)
            {
                arch3dFile = new Arch3dFile(Path.Combine(dfUnity.Arena2Path, Arch3dFile.Filename), FileUsage.UseMemory, true);
            }

            return true;
        }

        private void LoadVertices(ref ModelData model)
        {
            const int BuildingDoors = 74;
            const int DungeonEnterDoors = 56;
            const int DungeonRuinEnterDoors = 331;
            const int ScourgExterior = 156;
            const int DungeonExitDoors = 95;
            //const int dungeonFloorRecord = 2;

            // Allocate arrays
            model.Vertices = new Point3D[model.DFMesh.TotalVertices];
            model.Normals = new Vector3D[model.DFMesh.TotalVertices];
            model.UVs = new Point[model.DFMesh.TotalVertices];
            model.Texcoords = new Point[model.DFMesh.TotalVertices];

            // Static door and dungeon floor lists
            List<ModelDoor> modelDoors = new List<ModelDoor>();
            //List<DFMesh.DFPlane> dungeonFloors = new List<DFMesh.DFPlane>();

            // Loop through all submeshes
            int vertexCount = 0;
            foreach (DFMesh.DFSubMesh dfSubMesh in model.DFMesh.SubMeshes)
            {
                // Get cached material data
                CachedMaterial cm;
                dfUnity.MaterialReader.GetCachedMaterial(dfSubMesh.TextureArchive, dfSubMesh.TextureRecord, 0, out cm);
                Vector sz = cm.recordSizes[0];// * 16;
                //Point sz = new Point(512, 512);

                // Get texture archive for this submesh as base climate
                int submeshTextureArchive = dfSubMesh.TextureArchive;
                int baseTextureArchive = (submeshTextureArchive - (submeshTextureArchive / 100) * 100);

                // Get base climate archive for door check
                // All base door textures are > 100 with some exceptions
                int doorArchive = submeshTextureArchive;
                if (doorArchive > 100 && doorArchive != DungeonRuinEnterDoors && doorArchive != ScourgExterior)
                    doorArchive = baseTextureArchive;

                // Check if this is a door archive
                bool doorFound = false;
                DoorTypes doorType = DoorTypes.None;
                switch (doorArchive)
                {
                    case BuildingDoors:
                        doorFound = true;
                        doorType = DoorTypes.Building;
                        break;
                    case DungeonEnterDoors:
                        doorFound = true;
                        doorType = DoorTypes.DungeonEntrance;
                        break;
                    case DungeonRuinEnterDoors:
                        if (dfSubMesh.TextureRecord > 0)    // Dungeon ruins index 0 is just a stone texture
                        {
                            doorFound = true;
                            doorType = DoorTypes.DungeonEntrance;
                        }
                        break;
                    case DungeonExitDoors:
                        doorFound = true;
                        doorType = DoorTypes.DungeonExit;
                        break; 
                }

                //// Check if this is a dungeon floor
                //bool dungeonFloorFound = false;
                //if (baseTextureArchive >= 19 && baseTextureArchive <= 24 && dfSubMesh.TextureRecord == dungeonFloorRecord)
                //    dungeonFloorFound = true;

                // Loop through all planes in this submesh
                int doorCount = 0;
                foreach (DFMesh.DFPlane dfPlane in dfSubMesh.Planes)
                {
                    // If this is a door then each plane is a single door
                    if (doorFound)
                    {
                        // Set door verts
                        DFMesh.DFPoint p0 = dfPlane.Points[0];
                        DFMesh.DFPoint p1 = dfPlane.Points[1];
                        DFMesh.DFPoint p2 = dfPlane.Points[2];
                        ModelDoor modelDoor = new ModelDoor()
                        {
                            Index = doorCount++,
                            Type = doorType,
                            Vert0 = new Point3D(p0.X, -p0.Y, p0.Z),
                            Vert1 = new Point3D(p1.X, -p1.Y, p1.Z),
                            Vert2 = new Point3D(p2.X, -p2.Y, p2.Z),
                        };

                        // Set door normal
                        Vector3D u = modelDoor.Vert0 - modelDoor.Vert2;
                        Vector3D v = modelDoor.Vert0 - modelDoor.Vert1;
                        modelDoor.Normal = Vector3D.CrossProduct(u, v);
                        modelDoor.Normal.Normalize();

                        // Add door to list
                        modelDoors.Add(modelDoor);
                    }

                    //// If this a floor then store the polygon
                    //if (dungeonFloorFound)
                    //    dungeonFloors.Add(dfPlane);

                    // Copy each point in this plane to vertex buffer
                    //Point uvDiff = new Point(0, 0);
                    foreach (DFMesh.DFPoint dfPoint in dfPlane.Points)
                    {
                        // Store vertex data
                        model.Vertices[vertexCount] = new Point3D(dfPoint.X, -dfPoint.Y, dfPoint.Z);
                        model.Normals[vertexCount] = new Vector3D(dfPoint.NX, -dfPoint.NY, dfPoint.NZ);
                        model.Normals[vertexCount].Normalize();
                        // raw UV
                        model.UVs[vertexCount] = new Point(dfPoint.U, dfPoint.V);
                        // normalize based on texture size
                        model.Texcoords[vertexCount] = new Point(dfPoint.U / sz.X, dfPoint.V / sz.Y);

                        // Inrement count
                        vertexCount++;
                    }
                }
            }

            // Assign found doors
            model.Doors = modelDoors.ToArray();
            //model.DungeonFloors = dungeonFloors.ToArray();
        }

        private void LoadIndices(ref ModelData model)
        {
            // Allocate model data submesh buffer
            model.SubMeshes = new ModelData.SubMeshData[model.DFMesh.SubMeshes.Length];

            // Allocate index buffer
            model.Indices = new uint[model.DFMesh.TotalTriangles * 3];
            model.Materials = new uint[model.DFMesh.TotalTriangles];

            // Iterate through all submeshes
            short indexCount = 0, materialCount = 0;
            short subMeshCount = 0, vertexCount = 0;
            foreach (DFMesh.DFSubMesh dfSubMesh in model.DFMesh.SubMeshes)
            {
                // Set start index and primitive count for this submesh
                model.SubMeshes[subMeshCount].StartIndex = indexCount;
                model.SubMeshes[subMeshCount].VertexStart = vertexCount;
                model.SubMeshes[subMeshCount].PrimitiveCount = dfSubMesh.TotalTriangles;
                model.SubMeshes[subMeshCount].TextureArchive = dfSubMesh.TextureArchive;
                model.SubMeshes[subMeshCount].TextureRecord = dfSubMesh.TextureRecord;

                // Iterate through all planes in this submesh
                foreach (DFMesh.DFPlane dfPlane in dfSubMesh.Planes)
                {
                    // Every DFPlane is a triangle fan radiating from point 0
                    int sharedPoint = vertexCount++;

                    // Index remaining points. There are (plane.Points.Length - 2) triangles in every plane
                    for (int tri = 0; tri < dfPlane.Points.Length - 2; tri++)
                    {
                        // Store 3 points of current triangle
                        model.Indices[indexCount++] = (uint)sharedPoint;
                        model.Indices[indexCount++] = (uint)vertexCount + 1;
                        model.Indices[indexCount++] = (uint)vertexCount;

                        // Store this material for this face
                        model.Materials[materialCount++] = (uint)subMeshCount;

                        // Increment vertexCount to next point in fan
                        vertexCount++;
                    }

                    // Increment vertexCount to start of next fan in vertex buffer
                    vertexCount++;
                }

                model.SubMeshes[subMeshCount].VertexCount = vertexCount - model.SubMeshes[subMeshCount].VertexStart;
                // Increment submesh count
                subMeshCount++;
            }
        }
#if false
        /// <summary>
        /// Solve tangents for the given mesh.
        /// </summary>
        /// <param name="mesh">Mesh to solve tangents for.</param>
        public static void TangentSolver(Mesh mesh)
        {
            int vertexCount = mesh.vertexCount;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] texcoords = mesh.uv;
            int[] triangles = mesh.triangles;
            int triangleCount = triangles.Length / 3;
            Vector4[] tangents = new Vector4[vertexCount];
            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];
            int tri = 0;
            for (int i = 0; i < triangleCount; i++)
            {
                int i1 = triangles[tri];
                int i2 = triangles[tri + 1];
                int i3 = triangles[tri + 2];

                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];

                Vector2 w1 = texcoords[i1];
                Vector2 w2 = texcoords[i2];
                Vector2 w3 = texcoords[i3];

                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;

                float r = 1.0f / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;

                tri += 3;
            }

            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 n = normals[i];
                Vector3 t = tan1[i];

                // Gram-Schmidt orthogonalize
                Vector3.OrthoNormalize(ref n, ref t);

                tangents[i].x = t.x;
                tangents[i].y = t.y;
                tangents[i].z = t.z;

                // Calculate handedness
                tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
            }
            mesh.tangents = tangents;
        }
#endif
        /// <summary>
        /// Adds secondary UV set to mesh.
        /// This is required for lightmapping as Daggerfall uses many tiled textures.
        /// </summary>
        /// <param name="mesh">Mesh to add secondary UV set.</param>
        private void AddLightmapUVs(MeshGeometry3D mesh)
        {
            if (mesh == null)
                return;
            if (mesh.Positions.Count == 0)
                return;

#if UNITY_EDITOR
            Unwrapping.GenerateSecondaryUVSet(mesh);
#endif
        }

#endregion
    }
}