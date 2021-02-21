using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public class Material : Object
    {
        public int renderQueue;
        public Shader shader;
        public Texture2D mainTexture;
        public Color color;

        public Material(Shader shader)
        {
            this.shader = shader;
        }

        public void SetTexture(int id, Texture2D texture)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        protected void SetField<T>(string property, T value)
        {
            int id = Shader.PropertyToID(property);
            SetField(id, value);
        }

        protected void SetField<T>(int id, T value)
        {
            throw new NotImplementedException();
        }

        public void SetFloat(string property, float value)
        {
            SetField(property, value);
        }

        public void SetFloat(int id, float value)
        {
            SetField(id, value);
        }

        public void SetInt(int id, int value)
        {
            SetField(id, value);
        }

        public void SetColor(int id, Color color)
        {
            SetField(id, color);
        }

        public Color GetColor(string v)
        {
            throw new NotImplementedException();
        }

        public void SetOverrideTag(string tag, string value)
        {
            throw new NotImplementedException();
        }

        public bool IsKeywordEnabled(string keyword)
        {
            throw new NotImplementedException();
        }

        public void EnableKeyword(string keyword)
        {
            throw new NotImplementedException();
        }

        public void DisableKeyword(string keyword)
        {
            throw new NotImplementedException();
        }

        public Color GetColor(int emissionColor)
        {
            throw new NotImplementedException();
        }
    }
}
