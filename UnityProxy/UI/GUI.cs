using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.UI
{
    public class Text
    { }
}

namespace UnityEngine
{
    [Flags]
    public enum CameraClearFlags : short
    {
        Depth,
        Color,
        SolidColor
    }

    public enum ScaleMode
    {
        StretchToFill,
        ScaleToFit,
    }

    public enum TextAnchor
    {
        UpperLeft
    }

    public struct RectOffset
    {
        private int minX;
        private int maxX;
        private int minY;
        private int maxY;

        public RectOffset(int minX, int maxX, int minY, int maxY)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }
    }

    public enum Space
    {
        Self,
        World
    }

    public struct Resolution
    {
        public readonly int width;
        public readonly int height;
    }

    public static class Cursor
    {
        public static void SetCursor(Texture2D tex, Vector2 zero, CursorMode cursorMode)
        {
            throw new NotImplementedException();
        }
    }

    public static class Screen
    {
        public static readonly Resolution[] resolutions;
        public static bool fullScreen;
        public static Resolution currentResolution;

        public static int width { get; }

        public static int height { get; }

        public static void SetResolution(int width, int height, bool isChecked)
        {
            throw new NotImplementedException();
        }
    }

    public static class GUI
    {
        public static Color color;
        public static int depth;

        public static void Label(Rect rect, string text, GUIStyle style = null)
        {

        }

        public static void DrawTexture(Rect rect, Texture2D image)
        {
            throw new NotImplementedException();
        }

        public static void DrawTexture(Rect rect, Texture2D image, ScaleMode scaleMode, bool alphaBlend = true)
        {
            throw new NotImplementedException();
        }

        public static void DrawTextureWithTexCoords(Rect rect, Texture image, Rect texCoords, bool alphaBlend = true)
        {
            throw new NotImplementedException();
        }

    }

    public class Event
    {
        public static Event current
        { get; private set; }

        public EventType type
        { get; set; }
    }

    public enum EventType
    {
        Repaint,
        KeyDown
    }

    public class AStyle
    {
        public Color textColor;
    }

    public class GUIStyle
    {
        public readonly AStyle normal = new AStyle();
        public int fontSize;
        public TextAnchor alignment;
    }
}
