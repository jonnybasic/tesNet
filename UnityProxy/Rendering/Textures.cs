using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    using TextureProxy = System.Windows.Media.Imaging.BitmapSource;
    using PixelFormat = System.Windows.Media.PixelFormat;
    using PixelFormats = System.Windows.Media.PixelFormats;

    public enum TextureMap
    {
        Albedo,
        Normal,
        Emission,
        MetallicGloss,
    }

    public enum TextureImport
    {
        None,
        AllLocations,
        LooseFiles,
    }

    public enum TextureFormat
    {
        ARGB32,
        RGBA32,
        ARGB4444,
        RGBA4444
    }

    public enum RenderTextureFormat
    {
        ARGB32
    }

    public enum TextureWrapMode
    {
        Clamp
    }

    public enum FilterMode
    {
        Point,
    }

    public class Texture : GameObject
    {
        internal PixelFormat pixelFormat;

        public TextureFormat format;
        public FilterMode filterMode;
        public TextureWrapMode wrapMode;
        public float mipMapBias;
        public int anisoLevel;

        public virtual void SetPixels32(Color32[] src, int offset = 0, int v = 0)
        {
            throw new NotImplementedException();
        }

        public virtual void SetPixels(Color[] colors, int offset = 0)
        {
            throw new NotImplementedException();
        }

        public virtual Color32[] GetPixels32()
        {
            throw new NotImplementedException();
        }

        public virtual Color[] GetPixels()
        {
            throw new NotImplementedException();
        }

        public virtual void Apply(bool mipMaps = false, bool freeze = false)
        {
            throw new NotImplementedException();
        }
    }

    public class RenderTexture : Texture
    {
        public static RenderTexture active;
        public int width;
        public int height;
        public FilterMode filterMode;
        private RenderTextureFormat format;

        public RenderTexture(int width, int height, int mip, RenderTextureFormat format)
        {
            this.width = width;
            this.height = height;
            this.format = format;
        }

        public void Create()
        {
            throw new NotImplementedException();
        }

        public bool IsCreated()
        {
            throw new NotImplementedException();
        }
    }

    public class Texture2D : Texture
    {
        internal TextureProxy proxy;
        internal static double DpiX = 96;
        internal static double DpiY = 96;

        public readonly int width;
        public readonly int height;

        public byte[] bytesBGRA
        { get; private set; }

        //public Texture2D()
        //{ }

        public Texture2D(int width, int height, TextureFormat format = TextureFormat.ARGB32, bool mipMaps = false)
        {
            this.width = width;
            this.height = height;
            this.format = format;
        }

        public override void SetPixels32(Color32[] src, int offset = 0, int v = 0)
        {
            PixelFormat pixelFormat = PixelFormats.Bgra32;
            bytesBGRA = new byte[src.Length * 4];
            int byteOffset = 0;
            foreach (Color32 c in src)
            {
                bytesBGRA[byteOffset++] = c.b;
                bytesBGRA[byteOffset++] = c.g;
                bytesBGRA[byteOffset++] = c.r;
                bytesBGRA[byteOffset++] = c.a;
            }
            // width of image as a multiple of 8
            int stride = (width * pixelFormat.BitsPerPixel + 7) / 8;
            proxy = TextureProxy.Create(width, height, DpiX, DpiY, pixelFormat, null, bytesBGRA, stride);
        }

        public override void Apply(bool mipMaps = false, bool freeze = false)
        {
            System.Diagnostics.Debug.Print("Apply({0}, {1}) - NotImplemented", mipMaps, freeze);
        }

        public Rect[] PackTextures(Texture2D[] texture2D, int atlasPadding, int atlasMaxSize, bool mipMaps = false)
        {
            throw new NotImplementedException();
        }

        public static implicit operator TextureProxy(Texture2D t)
            => t.proxy;
    }

    public class Texture2DArray : Texture, IEnumerable<Texture2D>
    {
        public int width;
        public int height;

        internal readonly List<Texture2D> textures = new List<Texture2D>();

        public Texture2DArray(int width, int height, int numSlices, TextureFormat format = TextureFormat.ARGB32, bool mipMaps = false)
        {
            this.width = width;
            this.height = height;
            this.format = format;
        }

        public IEnumerator<Texture2D> GetEnumerator()
        {
            return textures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return textures.GetEnumerator();
        }
    }

    public class Texture3D : Texture
    {
        public int width;
        public int height;
        public int depth;

        public Texture3D(int width, int height, int depth, TextureFormat format = TextureFormat.ARGB32, bool mipMaps = false)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.format = format;
        }
    }
}
