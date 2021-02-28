using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    using MaterialProxy = System.Windows.Media.Media3D.Material;
    using MaterialDiffuse = System.Windows.Media.Media3D.DiffuseMaterial;
    using MaterialEmissive = System.Windows.Media.Media3D.EmissiveMaterial;

    public class Material : Object
    {
        internal readonly Dictionary<int, object> uniforms = new Dictionary<int, object>();
        internal readonly System.Collections.Specialized.StringDictionary overrideTags = new System.Collections.Specialized.StringDictionary();
        internal readonly Dictionary<string, bool> enabledKeywords = new Dictionary<string, bool>();

        public int renderQueue;
        public Shader shader;
        public Texture2D mainTexture;
        //public Color color;

        public Material(Shader shader)
        {
            this.shader = shader;
            this.name = String.Format("{0}_Material", shader.name);
        }

        public void SetTexture(int id, Texture2D texture)
        {
            SetField(id, texture);
        }

        public void SetTexture(string property, Texture2D texture)
        {
            int id = Shader.PropertyToID(property);
            SetTexture(id, texture);
        }

        public void SetTexture(int id, Texture2DArray textures)
        {
            throw new NotImplementedException();
        }

        public Texture GetTexture(int id)
        {
            return GetField<Texture>(id);
        }

        protected void SetField<T>(int id, T value)
        {
            uniforms[id] = value;
        }

        protected void SetField<T>(string property, T value)
        {
            int id = Shader.PropertyToID(property);
            SetField(id, value);
        }

        protected T GetField<T>(int id)
        {
            return (T)uniforms[id];
        }

        protected T GetField<T>(string property)
        {
            int id = Shader.PropertyToID(property);
            return GetField<T>(id);
        }

        public void SetFloat(string property, float value)
        {
            SetField(property, value);
        }

        public void SetFloat(int id, float value)
        {
            SetField(id, value);
        }

        public void SetInt(string property, int value)
        {
            SetField(property, value);
        }

        public void SetInt(int id, int value)
        {
            SetField(id, value);
        }

        public void SetColor(string property, Color color)
        {
            SetField(property, color);
        }

        public void SetColor(int id, Color color)
        {
            SetField(id, color);
        }

        public Color GetColor(string property)
        {
            return GetField<Color>(property);
        }

        public Color GetColor(int id)
        {
            return GetField<Color>(id);
        }

        public void SetOverrideTag(string tag, string value)
        {
            overrideTags[tag] = value;
        }

        public bool IsKeywordEnabled(string keyword)
        {
            return enabledKeywords[keyword];
        }

        public void EnableKeyword(string keyword)
        {
            enabledKeywords[keyword] = true;
        }

        public void DisableKeyword(string keyword)
        {
            enabledKeywords[keyword] = false;
        }

        public static implicit operator MaterialProxy(Material m)
        {
#if true
            //var emissiveTextureId = Shader.PropertyToID("_EmissionMap");
            //var emissiveColorId = Shader.PropertyToID("_EmissionColor");
            // just use abedo for now
            System.Windows.Media.ImageSource source = m.mainTexture;
            int width = (int)Math.Round(source.Width);
            int height = (int)Math.Round(source.Height);
            if (source is System.Windows.Media.Imaging.BitmapSource bitmap)
            {
                width = bitmap.PixelWidth;
                height = bitmap.PixelHeight;
            }
            var brush = new System.Windows.Media.ImageBrush(source)
            {
                AlignmentX = System.Windows.Media.AlignmentX.Left,
                AlignmentY = System.Windows.Media.AlignmentY.Top,
                Stretch = System.Windows.Media.Stretch.Uniform,
                TileMode = System.Windows.Media.TileMode.Tile,
                ViewportUnits = System.Windows.Media.BrushMappingMode.Absolute,
                Viewport = new System.Windows.Rect(0, 0, width, height)
                //Viewport = new System.Windows.Rect(0, 0, 1, 1),
                //ViewboxUnits = System.Windows.Media.BrushMappingMode.Absolute,
                //Viewbox = new System.Windows.Rect(0, 0, width, height)
            };
            if (brush.CanFreeze)
            {
                brush.Freeze();
            }
            return new MaterialDiffuse(brush);
#else
            return new MaterialDiffuse(System.Windows.Media.Brushes.Wheat);
#endif
        }
    }
}
