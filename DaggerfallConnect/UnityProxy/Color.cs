using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    /// <summary>
    /// UnityEntity Color32 Proxy
    /// </summary>
    public struct Color32
    {
        public byte a;
        public byte r;
        public byte g;
        public byte b;

        public Color32(byte r = 0, byte g = 0, byte b = 0, byte a = 255)
        {
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public Color32(float r = 0, float g = 0, float b = 0, float a = 1)
        {
            this.a = (byte)(a * 255);
            this.r = (byte)(r * 255);
            this.g = (byte)(g * 255);
            this.b = (byte)(b * 255);
        }

        /// <summary>
        /// Lerp color components
        /// </summary>
        /// <param name="colorA"></param>
        /// <param name="colorB"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static Color32 Lerp(Color32 colorA, Color32 colorB, float amount)
        {
            float a = Mathf.Lerp(colorA.a / 255.0f, colorB.a / 255.0f, amount);
            float r = Mathf.Lerp(colorA.r / 255.0f, colorB.r / 255.0f, amount);
            float g = Mathf.Lerp(colorA.g / 255.0f, colorB.g / 255.0f, amount);
            float b = Mathf.Lerp(colorA.b / 255.0f, colorB.b / 255.0f, amount);
            return new Color32(r, g, b, a);
        }
        
        public static void RGBToHSV(Color32 color, out float H, out float S, out float V)
        {
            Color temp = color;
            H = temp.GetHue();
            S = temp.GetSaturation();
            V = temp.GetSaturation();
        }
        
        /// <summary>
        /// Implicit from Color32 to Color
        /// </summary>
        /// <param name="c"></param>
        public static implicit operator Color(Color32 c)
            => Color.FromArgb(c.a, c.r, c.g, c.b);

        /// <summary>
        /// Explicit from Color sto Color32
        /// </summary>
        /// <param name="c"></param>
        public static implicit operator Color32(Color c)
            => new Color32(c.R, c.G, c.B, c.A);
    }
}
