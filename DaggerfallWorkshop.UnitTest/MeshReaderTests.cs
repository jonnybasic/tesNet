using Microsoft.VisualStudio.TestTools.UnitTesting;
using DaggerfallWorkshop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerfallWorkshop.Tests
{
    [TestClass()]
    public class MeshReaderTests
    {
        internal static readonly string Arena2Path = "B:\\Bethesda.net Launcher\\games\\TES Daggerfall\\DF\\DAGGER\\ARENA2";
        //System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Daggerfall Workshop", "Unit Tests");
        internal static readonly string UserPath = System.IO.Directory.GetCurrentDirectory();

        [TestInitialize()]
        public void CreateDaggerfallUnity()
        {
            // setup a unity instance
            UnityEngine.Application.persistentDataPath = UserPath;
            //UnityEngine.Resources.ResourceManager = 
            // setup a utility instance
            DaggerfallUnity.Settings = new SettingsManager();
            DaggerfallUnity.Instance = new DaggerfallUnity();
            DaggerfallUnity.Instance.Arena2Path = Arena2Path;
            DaggerfallUnity.Instance.TextProvider = new Utility.DefaultTextProvider();
            DaggerfallUnity.Instance.MaterialReader = new MaterialReader();
        }

        [TestMethod()]
        public void GetModelDataTest()
        {
            // sample building
            uint recordId = 0;
            MeshReader instance = new MeshReader();

            Assert.IsTrue(instance.GetModelData(recordId, out ModelData data));
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.DFMesh);
            Assert.IsNotNull(data.Doors);
            Assert.IsNotNull(data.Indices);
            Assert.IsNotNull(data.Normals);
            Assert.IsNotNull(data.SubMeshes);
            Assert.IsNotNull(data.UVs);
            Assert.IsNotNull(data.Vertices);
        }

        [TestMethod()]
        public void GetMeshTest()
        {
            // sample building
            uint recordId = 0;
            MeshReader instance = new MeshReader();

            Assert.IsTrue(instance.GetModelData(recordId, out ModelData data));
            Assert.IsNotNull(data);
            CachedMaterial[] cachedMaterials;
            int[] textureKeys;
            bool hasAnimations;
            var mesh = instance.GetMesh(
                DaggerfallUnity.Instance,
                recordId,
                out cachedMaterials,
                out textureKeys,
                out hasAnimations);
            Assert.IsNotNull(mesh);
        }

        [TestMethod()]
        [Ignore]
        public void GetCombinedMeshTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void GetBillboardMeshTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void GetSimpleBillboardMeshTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void GetSimpleGroundPlaneMeshTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void GetScaledBillboardSizeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void ClearCacheTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void TangentSolverTest()
        {
            Assert.Fail();
        }
    }
}