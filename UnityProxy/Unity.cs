using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unity
{
    public class UnityObject
    { }
}

namespace Unity.Collections
{
    public class UnityCollection
    { }
}

namespace Unity.Jobs
{
    public interface IJob
    { }


    public interface IJobParallelFor
    { }

    public class JobHandle
    { }

    public static class JobExtensions
    {
        public static JobHandle Schedule(this IJob job, JobHandle dependency)
        {
            throw new NotImplementedException();
        }

        public static JobHandle Schedule(this IJobParallelFor job, int size, int batch, JobHandle dependency)
        {
            throw new NotImplementedException();
        }
    }
}

namespace UnityEngine
{
    using Color32Proxy = System.Drawing.Color;
    using ColorProxy = System.Windows.Media.Color;
    using ColorsProxy = System.Windows.Media.Colors;
    using Vector3Proxy = System.Windows.Media.Media3D.Vector3D;
    using Vector4Proxy = System.Windows.Media.Media3D.Point4D;
    using Vector2Proxy = System.Windows.Vector;
    using QuaternionProxy = System.Windows.Media.Media3D.Quaternion;
    using Matrix4x4Proxy = System.Windows.Media.Media3D.Matrix3D;

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

        public static implicit operator Color32(Color32Proxy c)
            => new Color32(c.R, c.G, c.B, c.A);

        public static implicit operator Color32(ColorProxy c)
            => new Color32(c.R, c.G, c.B, c.A);

        public static implicit operator Color32(Color c)
            => new Color32(c.color.R, c.color.G, c.color.B, c.color.A);

        public static implicit operator Color32Proxy(Color32 c)
            => Color32Proxy.FromArgb(c.a, c.r, c.g, c.b);

        public static implicit operator Color(Color32 c)
            => ColorProxy.FromArgb(c.a, c.r, c.g, c.b);

        public static Color32 Lerp(Color neutralColor, Color32 color32, float scale)
        {
            throw new NotImplementedException();
        }
    }

    public struct Color
    {
        internal ColorProxy color;

        public float a
        {
            get => color.ScA;
            set => color.ScA = value;
        }
        public float r
        {
            get => color.ScR;
            set => color.ScR = value;
        }
        public float g
        {
            get => color.ScG;
            set => color.ScG = value;
        }
        public float b
        {
            get => color.ScB;
            set => color.ScB = value;
        }

        private Color(ColorProxy c)
        {
            color = c;
        }

        public Color(float r, float g, float b, float a = 1)
        {
            color = ColorProxy.FromScRgb(a, r, g, b);
        }

        public static implicit operator Color(ColorProxy c)
            => new Color(c);

        public static implicit operator ColorProxy(Color c)
            => c.color;

        public static Color operator *(Color c, float s)
            => new Color(c * s);

        public static readonly Color black = ColorsProxy.Black;
        public static readonly Color white = ColorsProxy.White;
        public static readonly Color red = ColorsProxy.Red;
        public static readonly Color green = ColorsProxy.Green;
        public static readonly Color blue = ColorsProxy.Blue;
        public static readonly Color grey = ColorsProxy.Gray;
        public static readonly Color clear = ColorsProxy.Transparent;

        public static bool operator ==(Color c1, Color c2)
            => c1.color == c2.color;

        public static bool operator !=(Color c1, Color c2)
            => c1.color != c2.color;

        public static Color32 Lerp(Color white, Color grey, float value)
        {
            throw new NotImplementedException();
        }

        public static void RGBToHSV(Color32 color32, out float h, out float s, out float v)
        {
            throw new NotImplementedException();
        }
    }

    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y) : this()
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2Int other)
            {
                return x == other.x && y == other.y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Vector2Int v1, Vector2Int v2)
            => (v1.x == v2.x && v1.y == v2.y);

        public static bool operator !=(Vector2Int v1, Vector2Int v2)
            => (v1.x != v2.x || v1.y != v2.y);
    }

    public struct Vector2
    {
        internal Vector2Proxy point;

        public float magnitude
        { get; private set; }

        public float x
        {
            get => (float)point.X;
            set => point.X = value;
        }

        public float y
        {
            get => (float)point.Y;
            set => point.Y = value;
        }

        internal Vector2(Vector2Proxy p)
        {
            point = p;
            magnitude = (float)point.Length;
        }

        public Vector2(float x, float y)
        {
            point = new Vector2Proxy(x, y);
            magnitude = (float)point.Length;
        }

        public static float Distance(Vector2 a, Vector2 b)
        {
            var v = b.point - a.point;
            return (float)v.Length;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2 other)
            {
                return point == other.point;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return point.GetHashCode();
        }

        public static implicit operator Vector2(Vector2Proxy p)
            => new Vector2(p);

        public static implicit operator Vector2Proxy(Vector2 v)
            => v.point;

        public static implicit operator Vector2(Vector3 v)
            => new Vector2(v.x, v.y);

        public static Vector2 operator *(Vector2 v, float s)
            => new Vector2(v * s);

        public static Vector2 operator +(Vector2 v1, Vector2 v2)
            => new Vector2(v1.point + v2.point);

        public static Vector2 operator -(Vector2 v1, Vector2 v2)
            => new Vector2(v1.point - v2.point);

        public static bool operator ==(Vector2 v1, Vector2 v2)
            => (v1.point == v2.point);

        public static bool operator !=(Vector2 v1, Vector2 v2)
            => (v1.point != v2.point);

        public static readonly Vector2 zero = new Vector2Proxy(0.0, 0.0);
        public static readonly Vector2 one = new Vector2Proxy(1.0, 1.0);
    }

    public struct Vector3
    {
        internal Vector3Proxy vector;

        public float x
        {
            get => (float)vector.X;
            set => vector.X = value;
        }

        public float y
        {
            get => (float)vector.Y;
            set => vector.Y = value;
        }

        public float z
        {
            get => (float)vector.Z;
            set => vector.Z = value;
        }

        internal Vector3(Vector3Proxy v)
        {
            vector = v;
        }

        public Vector3(float x, float y, float z)
        {
            vector = new Vector3Proxy(x, y, z);
        }

        public Vector3 normalized { get => throw new NotImplementedException(); }

        public float magnitude { get => throw new NotImplementedException(); }

        public static float Distance(Vector3 a, Vector3 b)
        {
            var v = b.vector - a.vector;
            return (float)v.Length;
        }

        public static float Dot(Vector3 a, Vector3 b)
        {
            return (float)Vector3Proxy.DotProduct(a, b);
        }

        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return Vector3Proxy.CrossProduct(a.vector, b.vector);
        }

        public static void OrthoNormalize(ref Vector3 n, ref Vector3 t)
        {
            throw new NotImplementedException();
        }

        public static Vector3 Normalize(Vector3 v)
        {
            Vector3 result = new Vector3(v);
            result.vector.Normalize();
            return result;
        }

        public void Normalize()
        {
            vector.Normalize();
        }

        public static Vector3 Lerp(Vector3 smoothFollowerPrevWorldPos, Vector3 position, float v)
        {
            throw new NotImplementedException();
        }

        public void Scale(Vector3 amount)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3 other)
            {
                return vector == other.vector;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return vector.GetHashCode();
        }

        public static implicit operator Vector3(Vector3Proxy v)
            => new Vector3(v);

        public static implicit operator Vector3Proxy(Vector3 v)
            => v.vector;

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
            => new Vector3(v1 + v2);

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
            => new Vector3(v1 - v2);

        public static Vector3 operator *(Vector3 v, float d)
            => new Vector3(v * d);

        public static Vector3 operator /(Vector3 v, float d)
            => new Vector3(v / d);

        public static bool operator ==(Vector3 v1, Vector3 v2)
            => (v1.vector == v2.vector);

        public static bool operator !=(Vector3 v1, Vector3 v2)
            => (v1.vector != v2.vector);

        public static implicit operator Vector3(Vector2 v)
        {
            throw new NotImplementedException();
        }

        public static readonly Vector3 one = new Vector3Proxy(1.0, 1.0, 1.0);
        public static readonly Vector3 zero = new Vector3Proxy(0.0, 0.0, 0.0);

        public static readonly Vector3 up = new Vector3Proxy(0.0, 1.0, 0.0);
        public static readonly Vector3 down = new Vector3Proxy(0.0, -1.0, 0.0);
        public static readonly Vector3 forward = new Vector3Proxy(1.0, 0.0, 0.0);
        public static readonly Vector3 back = new Vector3Proxy(-1.0, 0.0, 0.0);
        public static readonly Vector3 right = new Vector3Proxy(0.0, 0.0, 1.0);
        public static readonly Vector3 left = new Vector3Proxy(0.0, 0.0, -1.0);
    }

    public struct Bounds
    {
        public Vector3 min;
        public Vector3 max;

        public Vector3 extents
        {
            get => throw new NotImplementedException();
        }
        public Vector3 center
        {
            get => throw new NotImplementedException();
        }

        public bool Contains(Vector3 point)
        {
            throw new NotImplementedException();
        }
    }

    public struct Vector4
    {
        internal Vector4Proxy vector;

        public float x
        {
            get => (float)vector.X;
            set => vector.X = value;
        }

        public float y
        {
            get => (float)vector.Y;
            set => vector.Y = value;
        }

        public float z
        {
            get => (float)vector.Z;
            set => vector.Z = value;
        }

        public float w
        {
            get => (float)vector.W;
            set => vector.W = value;
        }

        internal Vector4(Vector4Proxy v)
        {
            vector = v;
        }

        public Vector4(float x, float y, float z, float w)
        {
            vector = new Vector4Proxy(x, y, z, w);
        }
    }

    public class Quaternion
    {
        internal QuaternionProxy quaternion;
        public static readonly object identity;

        public float x, y, z, w;

        public Quaternion()
        { }

        internal Quaternion(QuaternionProxy q)
        {
            quaternion = q;
        }

        public static implicit operator Quaternion(QuaternionProxy q)
            => new Quaternion(q);

        public static implicit operator QuaternionProxy(Quaternion q)
            => q.quaternion;

        public static Quaternion LookRotation(Vector3 vector3)
        {
            throw new NotImplementedException();
        }

        public static Quaternion Euler(float xrot, float angle, float v)
        {
            throw new NotImplementedException();
        }
    }

    public class Matrix4x4
    {
        internal readonly Matrix4x4Proxy matrix;
        public static readonly Matrix4x4 identity;

        public Matrix4x4(Matrix4x4Proxy q)
        {
            matrix = q;
        }

        public Vector3 GetColumn(int col)
        {
            throw new NotImplementedException();
        }

        public Vector3 MultiplyPoint3x4(Vector3 vector3)
        {
            throw new NotImplementedException();
        }

        public Vector3 MultiplyVector(Vector3 vector3)
        {
            throw new NotImplementedException();
        }

        public float this[int r, int c]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public Quaternion rotation;
        public Vector3 lossyScale;

        public static implicit operator Matrix4x4(Matrix4x4Proxy m)
            => new Matrix4x4(m);

        public static implicit operator Matrix4x4Proxy(Matrix4x4 m)
            => m.matrix;
    }

    public struct Rect
    {
        public Vector2 position;
        public Vector2 size;

        public Rect(float x = 0, float y = 0, float width = 0, float height = 0)
        {
            position = new Vector2(x, y);
            size = new Vector2(width, height);
        }

        public Rect(Vector2 position, Vector2 size)
        {
            this.position = position;
            this.size = size;
        }

        public float x
        {
            get => position.x;
            set => position.x = value;
        }

        public float y
        {
            get => position.y;
            set => position.y = value;
        }

        public float width
        {
            get => size.x;
            set => size.x = value;
        }

        public float height
        {
            get => size.y;
            set => size.y = value;
        }

        public float xMin
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public float xMax
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public float yMin
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public float yMax
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }

    public static class Mathf
    {
        public static readonly int Rad2Deg;

        public static float Abs(float a)
        {
            return Math.Abs(a);
        }

        public static float Pow(float a, float e)
        {
            return (float)Math.Pow(a, e);
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        public static float Clamp01(float a)
        {
            return Math.Max(Math.Min(1.0f, a), 0.0f);
        }

        public static float Clamp(float a, float min, float max)
        {
            return Math.Max(Math.Min(max, a), min);
        }

        public static int Clamp(int a, int min, int max)
        {
            return Math.Max(Math.Min(max, a), min);
        }

        public static float PerlinNoise(float v1, float v2)
        {
            throw new NotImplementedException();
        }

        public static int Atan2(float v, float delta)
        {
            throw new NotImplementedException();
        }

        public static bool Approximately(float delta, int v)
        {
            throw new NotImplementedException();
        }

        public static float Exp(float v)
        {
            throw new NotImplementedException();
        }

        public static float Sqrt(float p)
        {
            throw new NotImplementedException();
        }

        public static float Sign(float p)
        {
            throw new NotImplementedException();
        }

        public static float Max(float a, float b)
        {
            throw new NotImplementedException();
        }

        public static float Min(float a, float b)
        {
            throw new NotImplementedException();
        }
    }
}
