﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
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
        public FilterMode filterMode;
        public TextureWrapMode wrapMode;
        public float mipMapBias;
        public int anisoLevel;

        public void SetPixels32(Color32[] src, int offset = 0, int v = 0)
        {
            throw new NotImplementedException();
        }

        public void SetPixels(Color[] colors, int offset = 0)
        {
            throw new NotImplementedException();
        }

        public Color32[] GetPixels32()
        {
            throw new NotImplementedException();
        }

        public Color[] GetPixels()
        {
            throw new NotImplementedException();
        }

        public void Apply(bool mipMaps = false, bool b = false)
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
        public int width;
        public int height;

        public Texture2D()
        { }

        public Texture2D(int width, int height, TextureFormat format = TextureFormat.ARGB32, bool mipMaps = false)
        { }

        public Rect[] PackTextures(Texture2D[] texture2D, int atlasPadding, int atlasMaxSize, bool mipMaps = false)
        {
            throw new NotImplementedException();
        }

        //public static implicit operator Texture2D(Texture t)
        //    => t as Texture2D;
    }

    public class Texture2DArray : Texture, IEnumerable<Texture2D>
    {
        internal readonly List<Texture2D> textures = new List<Texture2D>();

        public Texture2DArray(int width, int height, int numSlices, TextureFormat format = TextureFormat.ARGB32, bool mipMaps = false)
        {
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
        public Texture3D(int width, int height, int depth, TextureFormat format = TextureFormat.ARGB32, bool mipMaps = false)
        {
        }
    }
}
