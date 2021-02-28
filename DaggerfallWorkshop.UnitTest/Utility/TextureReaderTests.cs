using Microsoft.VisualStudio.TestTools.UnitTesting;
using DaggerfallWorkshop.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerfallWorkshop.Utility.Tests
{
    [TestClass()]
    public class TextureReaderTests
    {
        [TestMethod()]
        public void CreateTextureSettingsTest()
        {
            GetTextureSettings settings = TextureReader.CreateTextureSettings();
            Assert.AreEqual(settings.archive, 0);
            Assert.AreEqual(settings.archive, 0);
            Assert.AreEqual(settings.record, 0);
            Assert.AreEqual(settings.frame, 0);
            Assert.AreEqual(settings.alphaIndex, 0);
            Assert.AreEqual(settings.borderSize, 0);
            Assert.AreEqual(settings.dilate, false);
            Assert.AreEqual(settings.atlasMaxSize, 2048);
        }

        [TestMethod()]
        [Ignore]
        public void CreateFromAPIImageTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void CreateFromSolidColorTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void TextureReaderTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void GetTexture2DTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void GetTexture2DTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void SetSpectralTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void GetTexture2DAtlasTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void GetTerrainTilesetTextureTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void GetTerrainAlbedoTextureArrayTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void GetTerrainNormalMapTextureArrayTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void GetTerrainMetallicGlossMapTextureArrayTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void IsEmissiveArchiveTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void IsEmissiveTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Ignore]
        public void IsChildNPCTextureTest()
        {
            Assert.Fail();
        }
    }
}