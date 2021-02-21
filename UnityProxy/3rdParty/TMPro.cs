using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TMPro
{
    public struct TMP_FaceInfo
    {
        public float pointSize;
        public float baseline;
    }

    public struct TMP_Metrics
    {
        public float horizontalBearingX;
        public float horizontalBearingY;
        public float width;
        public float height;
        public float horizontalAdvance;
    }

    public struct TMP_Glyph
    {
        public Rect glyphRect;
        public TMP_Metrics metrics;
    }

    public class TMP_FontAsset : Asset
    {
        public Texture2D atlas;
        public TMP_FaceInfo faceInfo;

        public IDictionary<short, TMP_Character> characterLookupTable => throw new NotImplementedException();
    }

    public class TMP_Character : Asset
    {
        public TMP_Glyph glyph;
    }
}
