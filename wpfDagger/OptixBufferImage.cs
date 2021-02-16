using DaggerfallWorkshopWpf;
using OptixCore.Library;
using OptixCore.Library.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
//using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace wpfDagger
{
    // shortcut for Vector3
    using float2 = System.Numerics.Vector2;
    using float3 = System.Numerics.Vector3;
    using float4 = System.Numerics.Vector4;
    //using Model3DGroup = System.Windows.Media.Media3D.Model3DGroup;
    //using GeometryModel3D = System.Windows.Media.Media3D.GeometryModel3D;
    //using MeshGeometry3D = System.Windows.Media.Media3D.MeshGeometry3D;
    using Point3D = System.Windows.Media.Media3D.Point3D;
    using Vector3D = System.Windows.Media.Media3D.Vector3D;

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct BasicLight
    {
        public float3 Position;
        public float3 Color;
        public int castShadows;
        public int pad;
    }

    internal static class Vector3Extensions
    {
        public static float3 ToFloat3(this Point3D p)
        {
            return new float3((float)p.X, (float)p.Y, (float)p.Z);
        }

        public static float3 ToFloat3(this Vector3D v)
        {
            return new float3((float)v.X, (float)v.Y, (float)v.Z);
        }

        public static float3 ToFloat3(this System.Windows.Media.Color c)
        {
            return new float3(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f);
        }

        public static float2 ToFloat2(this Point p)
        {
            return new float2((float)p.X, (float)p.Y);
        }

        public static float Aspect(this Int32Rect rect)
        {
            return (float)rect.Width / (float)rect.Height;
        }
    }

    public class OptixBufferImage : Image
    {
        public static readonly DependencyProperty OptixContextProperty = DependencyProperty.Register(
            "OptixContext",
            typeof(OptixContextModel),
            typeof(OptixBufferImage),
            new PropertyMetadata(null, new PropertyChangedCallback(OnOptixContextChanged)));

        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
            "BackgroundColor",
            typeof(System.Windows.Media.Color),
            typeof(OptixBufferImage),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ModelColorProperty = DependencyProperty.Register(
            "ModelColor",
            typeof(System.Windows.Media.Color),
            typeof(OptixBufferImage),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CheckerColorAProperty = DependencyProperty.Register(
            "CheckerColorA",
            typeof(System.Windows.Media.Color),
            typeof(OptixBufferImage),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CheckerColorBProperty = DependencyProperty.Register(
            "CheckerColorB",
            typeof(System.Windows.Media.Color),
            typeof(OptixBufferImage),
            new PropertyMetadata(null));

        private static readonly DependencyPropertyKey FrameCountPropertyKey = DependencyProperty.RegisterReadOnly(
            "FrameCount",
            typeof(uint),
            typeof(OptixBufferImage),
            new PropertyMetadata((uint)0));

        public static readonly DependencyProperty FrameCountProperty = FrameCountPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey LaunchCountPropertyKey = DependencyProperty.RegisterReadOnly(
            "LaunchCount",
            typeof(uint),
            typeof(OptixBufferImage),
            new PropertyMetadata((uint)0));

        public static readonly DependencyProperty LaunchCountProperty = LaunchCountPropertyKey.DependencyProperty;

        private static void OnOptixContextChanged(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            OptixBufferImage caller = sender as OptixBufferImage;
            if (e.NewValue is OptixContextModel newOptixContext
                && newOptixContext.IsLoaded)
            {
                // store the context for thread access
                caller.contextModel = newOptixContext;
                caller.context = newOptixContext.Context;
                try
                {
                    // new buffer sizes
                    ulong bufferWidth = Math.Max(2, (ulong)caller.Width);
                    ulong bufferHeight = Math.Max(2, (ulong)caller.Height);
                    caller.CreateBuffers(bufferWidth, bufferHeight);
                    // ported examples from the SDK
                    caller.LoadNVIDIASamples();
                    // create scene lighting
                    caller.CreateLights();
                    // create scene geometry
                    caller.CreateGeometry();
                    // create the top object
                    if (caller.recordGeometry.Count == 0)
                    {
                        caller.SetTopObject(newOptixContext.CreateGroup());
                    }
                    // check validation
                    if (!caller.contextValidated)
                    {
                        // validate context
                        caller.contextValidated = newOptixContext.Validate();
                    }
                }
                catch (NvRunTimeException eCompile)
                {
                    newOptixContext.LogMessages.Add(String.Format("Optix Compiler Error: {0}", eCompile.Message));
                }
                catch (Exception eContext)
                {
                    newOptixContext.LogMessages.Add(String.Format("Optix Error: {0}", eContext.Message));
                }
            }
        }

        public OptixContextModel OptixContext
        {
            get { return GetValue(OptixContextProperty) as OptixContextModel; }
            set { SetValue(OptixContextProperty, value); }
        }

        public System.Windows.Media.Color BackgroundColor
        {
            get => (System.Windows.Media.Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        public System.Windows.Media.Color ModelColor
        {
            get => (System.Windows.Media.Color)GetValue(ModelColorProperty);
            set => SetValue(ModelColorProperty, value);
        }

        public System.Windows.Media.Color CheckerColorA
        {
            get => (System.Windows.Media.Color)GetValue(CheckerColorAProperty);
            set => SetValue(CheckerColorAProperty, value);
        }

        public System.Windows.Media.Color CheckerColorB
        {
            get => (System.Windows.Media.Color)GetValue(CheckerColorBProperty);
            set => SetValue(CheckerColorBProperty, value);
        }

        public uint FrameCount
        {
            get { return (uint)GetValue(FrameCountProperty); }
        }

        public uint LaunchCount
        {
            get { return (uint)GetValue(LaunchCountProperty); }
        }

        public UInt32 EntryPointIndex { get; set; }

        public String BufferName { get; protected set; }

        public String BufferNameAccum { get; protected set; }

        public Format BufferFormat
        {
            get => bufferDescription.Format;
            protected set => bufferDescription.Format = value;
        }

        public Format BufferFormatAccum
        {
            get => bufferDescriptionAccum.Format;
            protected set => bufferDescriptionAccum.Format = value;
        }

        public BufferType BufferType
        {
            get => bufferDescription.Type;
            protected set => bufferDescription.Type = value;
        }

        public BufferType BufferTypeAccum
        {
            get => bufferDescriptionAccum.Type;
            protected set => bufferDescriptionAccum.Type = value;
        }

        private enum ProgramKey
        {
            NV_RayGenAccumCamera = 0,
            NV_MissConstantBG,
            NV_Exception,
            NV_BoundsSphere,
            NV_IntersectSphere,
            NV_BoundsParallelogram,
            NV_IntersectParallelogram,
            NV_AnyHitChecker,
            NV_ClosestHitChecker,
            NV_TriangleAttributes,
            NV_AnyHitBarycentric,
            NV_ClosestHitBarycentric,
            NV_AnyHitNormal,
            NV_ClosestHitNormal,
            NV_AnyHitPhong,
            NV_ClosestHitPhong,
            NV_ClosestHitPhongTextured,

            /// <summary>
            /// Must be last
            /// </summary>
            Count
        }

        private enum MaterialKey
        {
            NV_Checker,
            NV_Barycentric,
            NV_Normal,
            NV_PhongMetal,
            NV_PhongTextured,

            /// <summary>
            /// Must be last
            /// </summary>
            Count
        }

        readonly OptixProgram[] optixPrograms = new OptixProgram[(int)ProgramKey.Count];
        readonly Material[] optixMaterials = new Material[(int)MaterialKey.Count];

        private readonly DispatcherTimer backgroundTimer;
        private readonly DispatcherTimer invalidateTimer;

        float3 cameraU;
        float3 cameraV;
        float3 cameraW;
        float3 cameraEye;
        float3 cameraLookAt;
        float3 cameraUp;
        float cameraFOV;

        private Context context;
        private OptixContextModel contextModel;
        private bool contextValidated;
        private uint frameCount;
        private uint launchCount;
        private BufferDesc bufferDescription;
        private BufferDesc bufferDescriptionAccum;
        private OptixBuffer optixBuffer;
        private OptixBuffer optixBufferAccum;
        private Int32Rect imageRect;
        private IntPtr backBufferPtr;
        private WriteableBitmap writeableBitmap;

        struct RecordGeometry
        {
            public Group topGroup;
            public Acceleration topAccel;
            public GeometryGroup triangleGeometryGroup;
            public Acceleration triangleGeometryAccel;
            public GeometryGroup geometryGroup;
            public Acceleration geometryAccel;
        }

        readonly Dictionary<uint, RecordGeometry> recordGeometry;

        public OptixBufferImage() : base()
        {
            DataContextChanged += OptixBufferImage_DataContextChanged;
            frameCount = 0;
            launchCount = 0;
            contextValidated = false;
            context = null;
            optixBuffer = null;
            optixBufferAccum = null;
            writeableBitmap = null;
            backBufferPtr = IntPtr.Zero;
            BufferName = "output_buffer";
            bufferDescription = new BufferDesc
            {
                Format = Format.UByte4,
                Type = BufferType.Output
            };
            BufferNameAccum = "accum_buffer";
            bufferDescriptionAccum = new BufferDesc
            {
                Format = Format.Float4,
                Type = BufferType.InputOutputLocal
            };
            imageRect = new Int32Rect(0, 0, 0, 0);
            cameraFOV = 30.0f;
            cameraU = new float3(0, 0, 0);
            cameraV = new float3(0, 0, 0);
            cameraW = new float3(0, 0, 0);
            cameraEye = new float3(2, 0, 0);
            cameraLookAt = new float3(0, 0, 0);
            cameraUp = new float3(0, 1, 0);
            recordGeometry = new Dictionary<uint, RecordGeometry>();

            backgroundTimer = new DispatcherTimer(DispatcherPriority.Background);
            invalidateTimer = new DispatcherTimer(DispatcherPriority.Render);
            backgroundTimer.Tick += BackgroundTimer_Tick;
            backgroundTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0);
            invalidateTimer.Tick += InvalidateTimer_Tick;
            invalidateTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / 30.0);
        }

        private void InvalidateTimer_Tick(object sender, EventArgs e)
        {
            // invaldate the image rectangle
            writeableBitmap.Lock();
            writeableBitmap.AddDirtyRect(imageRect);
            writeableBitmap.Unlock();

            // update counts
            SetValue(FrameCountPropertyKey, frameCount++);
            SetValue(LaunchCountPropertyKey, launchCount);
        }

        private void BackgroundTimer_Tick(object sender, EventArgs e)
        {
            optixPrograms[(int)ProgramKey.NV_RayGenAccumCamera]["frame"].Set(launchCount);
            context.Launch(
                EntryPointIndex,
                bufferDescription.Width,
                bufferDescription.Height);
            launchCount++;
            if (backBufferPtr != IntPtr.Zero)
            {
                optixBuffer?.CopyTo(backBufferPtr);
            }
        }

        internal struct TriangleMesh
        {
            public float3[] vertices;
            public float3[] normals;
            public float2[] texcoords;
            public uint[] indices;

            public TriangleMesh(int size)
            {
                vertices = new float3[size];
                normals = new float3[size];
                texcoords = new float2[size];
                indices = new uint[size];
            }
        }

        internal static TriangleMesh CreateTetrahedron(float H, float3 trans)
        {
            const int SIZE = 12;
            TriangleMesh result = new TriangleMesh(SIZE);
            // Side length
            float a = (3.0f * H) / (float)Math.Sqrt(6.0);
            // Offset for base vertices from apex
            float d = a * (float)Math.Sqrt(3.0) / 6.0f;

            // There are only four vertex positions, but we will duplicate vertices
            // instead of sharing them among faces.
            float3 v0 = trans + new float3(0.0f, 0, H - d);
            float3 v1 = trans + new float3(a / 2.0f, 0, -d);
            float3 v2 = trans + new float3(-a / 2.0f, 0, -d);
            float3 v3 = trans + new float3(0.0f, H, 0.0f);

            // Bottom face
            result.vertices[0] = v0;
            result.vertices[1] = v1;
            result.vertices[2] = v2;

            // Duplicate the face normals across the vertices.
            float3 n = float3.Normalize(float3.Cross(v2 - v0, v1 - v0));
            result.normals[0] = n;
            result.normals[1] = n;
            result.normals[2] = n;

            result.texcoords[0] = new float2(0.5f, 1.0f);
            result.texcoords[1] = new float2(1.0f, 0.0f);
            result.texcoords[2] = new float2(0.0f, 0.0f);

            // Left face
            result.vertices[3] = v3;
            result.vertices[4] = v2;
            result.vertices[5] = v0;

            n = float3.Normalize(float3.Cross(v2 - v3, v0 - v3));
            result.normals[3] = n;
            result.normals[4] = n;
            result.normals[5] = n;

            result.texcoords[3] = new float2(0.5f, 1.0f);
            result.texcoords[4] = new float2(0.0f, 0.0f);
            result.texcoords[5] = new float2(1.0f, 0.0f);

            // Right face
            result.vertices[6] = v3;
            result.vertices[7] = v0;
            result.vertices[8] = v1;

            n = float3.Normalize(float3.Cross(v0 - v3, v1 - v3));
            result.normals[6] = n;
            result.normals[7] = n;
            result.normals[8] = n;

            result.texcoords[6] = new float2(0.5f, 1.0f);
            result.texcoords[7] = new float2(0.0f, 0.0f);
            result.texcoords[8] = new float2(1.0f, 0.0f);

            // Back face
            result.vertices[9] = v3;
            result.vertices[10] = v1;
            result.vertices[11] = v2;

            n = float3.Normalize(float3.Cross(v1 - v3, v2 - v3));
            result.normals[9] = n;
            result.normals[10] = n;
            result.normals[11] = n;

            result.texcoords[9] = new float2(0.5f, 1.0f);
            result.texcoords[10] = new float2(0.0f, 0.0f);
            result.texcoords[11] = new float2(1.0f, 0.0f);

            for (uint i = 0; i < SIZE; ++i)
                result.indices[i] = i;

            return result;
        }

        private void OptixBufferImage_DataContextChanged(
                object sender,
                DependencyPropertyChangedEventArgs e)
        {
            //if (e.OldValue is RecordModel oldRecord)
            //{
            //    SetRecordGeometry(ref oldRecord);
            //}
            if (e.NewValue is RecordModel newRecord)
            {
                SetRecordGeometry(ref newRecord);
            }
        }

        private void SetTopObject(Group topObject)
        {
            context["top_object"].Set(topObject);
            context["top_shadower"].Set(topObject);
        }

        private void CreateBuffers(ulong bufferWidth, ulong bufferHeight)
        {
            // create main buffer
            bufferDescription.Depth = 0;
            bufferDescription.Width = bufferWidth;
            bufferDescription.Height = bufferHeight;
            optixBuffer = contextModel.CreateBuffer(bufferDescription);
            context[BufferName].Set(optixBuffer);
            // create accum buffer
            bufferDescriptionAccum.Depth = 0;
            bufferDescriptionAccum.Width = bufferWidth;
            bufferDescriptionAccum.Height = bufferHeight;
            optixBufferAccum = contextModel.CreateBuffer(bufferDescriptionAccum);
            context[BufferNameAccum].Set(optixBufferAccum);
        }

        private void LoadNVIDIASamples()
        {
            contextModel.StackSize = 2800;
            contextModel.MaxTraceDepth = 12;

            // Note: high max depth for reflection and refraction through glass
            context["max_depth"].Set(10);
            context["scene_epsilon"].Set(1e-4f);
            context["ambient_light_color"].Set(0.4f, 0.4f, 0.4f);

            // Set the NVIDIA camera as the active entry point
            EntryPointIndex = (uint)DaggerEntryPoint.NVIDIA_PinholeCamera;

            contextModel.LogMessages.Add("Loading nvidia shading model resources");
            string nvDir = Path.Combine(Directory.GetCurrentDirectory(), "cuda", "nvidia");
            string nvHelpersSource = contextModel.ReadFileToEnd(Path.Combine(nvDir, "helpers.h"));
            string nvCommonSource = contextModel.ReadFileToEnd(Path.Combine(nvDir, "common.h"));
            string nvPhongSource = contextModel.ReadFileToEnd(Path.Combine(nvDir, "phong.h"));
            string nvRandomSource = contextModel.ReadFileToEnd(Path.Combine(nvDir, "random.h"));
            contextModel.LogMessages.Add("Compiling nvidia accum camera ray generation program");
            string nvPinholeCameraPTX = contextModel.CompilePtxFromString(
                contextModel.ReadFileToEnd(Path.Combine(nvDir, "accum_camera.cu")),
                "nv_accum_camera",
                null,
                new string[] { nvCommonSource, nvHelpersSource, nvRandomSource },
                new string[] { "common.h", "helpers.h", "random.h" });
            OptixProgram nvPinholeCamera = contextModel.CreateFromPTXString(ref nvPinholeCameraPTX, "pinhole_camera");
            cameraEye = new float3(-2.0f, 4.0f, 10.0f);
            cameraLookAt = new float3(0.0f, 1.0f, 0.0f);
            cameraUp = new float3(0.0f, 1.0f, 0.0f);
            calculateCameraVariables(
                cameraEye, cameraLookAt, cameraU,
                cameraFOV, imageRect.Aspect(),
                ref cameraU, ref cameraV, ref cameraW);
            nvPinholeCamera["U"].SetFloat3(ref cameraU);
            nvPinholeCamera["V"].SetFloat3(ref cameraV);
            nvPinholeCamera["W"].SetFloat3(ref cameraW);
            nvPinholeCamera["eye"].SetFloat3(ref cameraEye);
            OptixProgram nvException = contextModel.CreateFromPTXString(ref nvPinholeCameraPTX, "exception");
            nvException["bad_color"].Set(1.0f, 0.0f, 1.0f);
            contextModel.LogMessages.Add("Compiling nvidia constant bg miss program");
            string nvConstantBGPTX = contextModel.CompilePtxFromString(
                contextModel.ReadFileToEnd(Path.Combine(nvDir, "constantbg.cu")), "nv_constantbg");
            OptixProgram nvConstantBGMiss = contextModel.CreateFromPTXString(ref nvConstantBGPTX, "miss");
            float3 bgColor = BackgroundColor.ToFloat3();
            nvConstantBGMiss["bg_color"].SetFloat3(bgColor);
            //nvConstantBGMiss["bg_color"].Set(0.34f, 0.55f, 0.85f);
            optixPrograms[(int)ProgramKey.NV_RayGenAccumCamera] = nvPinholeCamera;
            optixPrograms[(int)ProgramKey.NV_MissConstantBG] = nvConstantBGMiss;
            optixPrograms[(int)ProgramKey.NV_Exception] = nvException;
            context.SetRayGenerationProgram((uint)DaggerEntryPoint.NVIDIA_PinholeCamera, nvPinholeCamera);
            context.SetExceptionProgram((uint)DaggerEntryPoint.NVIDIA_PinholeCamera, nvException);
            // set miss for each ray type
            context.SetRayMissProgram((uint)DaggerRayType.NVIDIA_Radiance, nvConstantBGMiss);
            context.SetRayMissProgram((uint)DaggerRayType.NVIDIA_Shadow, nvConstantBGMiss);

            // Sphere
            contextModel.LogMessages.Add("Compiling nvidia sphere geometry programs");
            string nvSpherePTX = contextModel.CompilePtxFromString(
                contextModel.ReadFileToEnd(Path.Combine(nvDir, "sphere.cu")), "nv_sphere");
            OptixProgram nvSphereBounds = contextModel.CreateFromPTXString(ref nvSpherePTX, "bounds");
            OptixProgram nvSphereIntersect = contextModel.CreateFromPTXString(ref nvSpherePTX, "robust_intersect");
            optixPrograms[(int)ProgramKey.NV_BoundsSphere] = nvSphereBounds;
            optixPrograms[(int)ProgramKey.NV_IntersectSphere] = nvSphereIntersect;

            // Parallelogram
            contextModel.LogMessages.Add("Compiling nvidia parallelogram geometry programs");
            string nvParallelogramPTX = contextModel.CompilePtxFromString(
                contextModel.ReadFileToEnd(Path.Combine(nvDir, "parallelogram.cu")), "nv_parallelogram");
            OptixProgram nvParallelogramBounds = contextModel.CreateFromPTXString(ref nvParallelogramPTX, "bounds");
            OptixProgram nvParallelogramIntersect = contextModel.CreateFromPTXString(ref nvParallelogramPTX, "intersect");
            optixPrograms[(int)ProgramKey.NV_BoundsParallelogram] = nvParallelogramBounds;
            optixPrograms[(int)ProgramKey.NV_IntersectParallelogram] = nvParallelogramIntersect;

            // Checker material
            contextModel.LogMessages.Add("Compiling nvidia checker material program");
            string nvCheckerPTX = contextModel.CompilePtxFromString(
                contextModel.ReadFileToEnd(Path.Combine(nvDir, "checker.cu")),
                "nv_checker",
                null,
                new string[] { nvCommonSource, nvHelpersSource, nvPhongSource },
                new string[] { "common.h", "helpers.h", "phong.h" });
            OptixProgram nvCheckerAnyHit = contextModel.CreateFromPTXString(ref nvCheckerPTX, "any_hit_shadow");
            OptixProgram nvCheckerClosestHit = contextModel.CreateFromPTXString(ref nvCheckerPTX, "closest_hit_radiance");
            Material nvChecker = contextModel.CreateMaterial(nvCheckerClosestHit, nvCheckerAnyHit);
            float3 ka1 = CheckerColorA.ToFloat3();
            float3 ka2 = CheckerColorB.ToFloat3();
            nvChecker["Kd1"].SetFloat3(ka1);
            nvChecker["Ka1"].SetFloat3(ka1);
            nvChecker["Ks1"].Set(0.0f, 0.0f, 0.0f);
            nvChecker["Kd2"].SetFloat3(ka2);
            nvChecker["Ka2"].SetFloat3(ka2);
            nvChecker["Ks2"].Set(0.0f, 0.0f, 0.0f);
            nvChecker["inv_checker_size"].Set(16.0f, 16.0f, 16.0f);
            nvChecker["phong_exp1"].Set(0.0f);
            nvChecker["phong_exp2"].Set(0.0f);
            nvChecker["Kr1"].Set(0.0f, 0.0f, 0.0f);
            nvChecker["Kr2"].Set(0.0f, 0.0f, 0.0f);
            optixPrograms[(int)ProgramKey.NV_AnyHitChecker] = nvCheckerAnyHit;
            optixPrograms[(int)ProgramKey.NV_ClosestHitChecker] = nvCheckerClosestHit;
            optixMaterials[(int)MaterialKey.NV_Checker] = nvChecker;

            // Barycentric material
            contextModel.LogMessages.Add("Compiling nvidia barycentric material program");
            string nvTrianglesPTX = contextModel.CompilePtxFromString(
                contextModel.ReadFileToEnd(Path.Combine(nvDir, "geometry_triangles.cu")),
                "nv_triangles");
            OptixProgram nvTriangleAttributes = contextModel.CreateFromPTXString(ref nvTrianglesPTX, "triangle_attributes");
            OptixProgram nvBaryAnyHit = contextModel.CreateFromPTXString(ref nvTrianglesPTX, "any_hit_shadow");
            OptixProgram nvBaryClosestHit = contextModel.CreateFromPTXString(ref nvTrianglesPTX, "closest_hit_radiance");
            optixPrograms[(int)ProgramKey.NV_TriangleAttributes] = nvTriangleAttributes;
            optixPrograms[(int)ProgramKey.NV_AnyHitBarycentric] = nvBaryAnyHit;
            optixPrograms[(int)ProgramKey.NV_ClosestHitBarycentric] = nvBaryClosestHit;
            optixMaterials[(int)MaterialKey.NV_Barycentric] = contextModel.CreateMaterial(nvBaryClosestHit, nvBaryAnyHit);

            // normal shader material
            contextModel.LogMessages.Add("Compiling nvidia normal material program");
            string nvNormalPTX = contextModel.CompilePtxFromString(
                contextModel.ReadFileToEnd(Path.Combine(nvDir, "normal_shader.cu")),
                "nv_normal_shader");
            OptixProgram nvNormalAnyHit = contextModel.CreateFromPTXString(ref nvNormalPTX, "any_hit_shadow");
            OptixProgram nvNormalClosestHit = contextModel.CreateFromPTXString(ref nvNormalPTX, "closest_hit_radiance");
            optixPrograms[(int)ProgramKey.NV_AnyHitNormal] = nvNormalAnyHit;
            optixPrograms[(int)ProgramKey.NV_ClosestHitNormal] = nvNormalClosestHit;
            optixMaterials[(int)MaterialKey.NV_Normal] = contextModel.CreateMaterial(nvNormalClosestHit, nvNormalAnyHit);

            // phong material
            contextModel.LogMessages.Add("Compiling nvidia phong material program");
            string nvPhongPTX = contextModel.CompilePtxFromString(
                contextModel.ReadFileToEnd(Path.Combine(nvDir, "phong.cu")),
                "nv_phong",
                null,
                new string[] { nvCommonSource, nvHelpersSource, nvPhongSource },
                new string[] { "common.h", "helpers.h", "phong.h" });
            OptixProgram nvPhongAnyHit = contextModel.CreateFromPTXString(ref nvPhongPTX, "any_hit_shadow");
            OptixProgram nvPhongClosestHit = contextModel.CreateFromPTXString(ref nvPhongPTX, "closest_hit_radiance");
            OptixProgram nvPhongTexturedClosestHit = contextModel.CreateFromPTXString(ref nvPhongPTX, "closest_hit_radiance_textured");
            Material nvPhongMetal = contextModel.CreateMaterial(nvPhongClosestHit, nvPhongAnyHit);
            nvPhongMetal["Ka"].Set(0.2f, 0.5f, 0.5f);
            nvPhongMetal["Kd"].Set(0.2f, 0.7f, 0.8f);
            nvPhongMetal["Ks"].Set(0.9f, 0.9f, 0.9f);
            nvPhongMetal["phong_exp"].Set(64.0f);
            nvPhongMetal["Kr"].Set(0.5f, 0.5f, 0.5f);
            optixPrograms[(int)ProgramKey.NV_AnyHitPhong] = nvPhongAnyHit;
            optixPrograms[(int)ProgramKey.NV_ClosestHitPhong] = nvPhongClosestHit;
            optixPrograms[(int)ProgramKey.NV_ClosestHitPhongTextured] = nvPhongTexturedClosestHit;
            optixMaterials[(int)MaterialKey.NV_PhongMetal] = nvPhongMetal;
            optixMaterials[(int)MaterialKey.NV_PhongTextured] = contextModel.CreateMaterial(nvPhongTexturedClosestHit, nvPhongAnyHit);
        }

        private void CreateLights()
        {
            BasicLight[] lights = {
                new BasicLight {
                    Position = new float3( -1024.0f, 2048.0f, 32.0f ),
                    Color = new float3( 1.0f, 1.0f, 1.0f ),
                    castShadows = 1
                }
            };
            BufferDesc lightDesc = new BufferDesc
            {
                Width = (ulong)lights.LongLength,
                ElemSize = (ulong)Marshal.SizeOf<BasicLight>(),
                Format = Format.User,
                Type = BufferType.Input
            };
            OptixBuffer lightBuffer = contextModel.CreateBuffer(lightDesc);
            lightBuffer.SetData(lights);

            context["lights"].Set(lightBuffer);
        }

        private void CreateGeometry()
        {
            //// Unit Sphere
            //contextModel.LogMessages.Add("Compiling unit sphere geometry programs");
            //string unitSpherePTX = contextModel.CompilePtxFromSource("unit_sphere.cu", "unit_sphere");
            //OptixProgram unitSphereBounds = contextModel.CreateFromPTXString(ref unitSpherePTX, "bounds");
            //OptixProgram unitSphereIntersect = contextModel.CreateFromPTXString(ref unitSpherePTX, "intersect");
            //Geometry unitSphereGeometry = contextModel.CreateGeometry(unitSphereBounds, unitSphereIntersect);
            //GeometryInstance unitSphereInstance = contextModel.CreateGeometryInstance(unitSphereGeometry);
            //unitSphereInstance.Materials[0] = optixMaterials[(int)MaterialKey.NV_PhongMetal];
            //// add to top group
            //topObjectGroup.AddChild(unitSphereInstance);

            if (DataContext is RecordModel record)
            {
                SetRecordGeometry(ref record);
            }
            else
            {
                contextModel.LogMessages.Add("Creating nvidia metal sphere geometry");
                Geometry sphereGeometry = contextModel.CreateGeometry(
                    optixPrograms[(int)ProgramKey.NV_BoundsSphere],
                    optixPrograms[(int)ProgramKey.NV_IntersectSphere]);
                sphereGeometry["sphere"].Set(-1.5f, 1.0f, 0.0f, 1.0f);
                GeometryInstance sphereInstance = contextModel.CreateGeometryInstance(sphereGeometry);
                sphereInstance.Materials[0] = optixMaterials[(int)MaterialKey.NV_PhongMetal];
                // add to local group
                GeometryGroup sphereGroup = contextModel.CreateGeometryGroup();
                sphereGroup.AddChild(sphereInstance);

                contextModel.LogMessages.Add("Create parallelogram geometry");
                float3 anchor = new float3(-64.0f, -1e-3f, -64.0f);
                float3 v1 = new float3(128.0f, 0.0f, 0.0f);
                float3 v2 = new float3(0.0f, 0.0f, 128.0f);
                float3 normal = float3.Normalize(float3.Cross(v1, v2));
                float d = float3.Dot(normal, anchor);
                v1 *= 1.0f / float3.Dot(v1, v1);
                v2 *= 1.0f / float3.Dot(v2, v2);
                float4 plane = new float4(normal, d);
                Geometry parallelogramGeometry = contextModel.CreateGeometry(
                    optixPrograms[(int)ProgramKey.NV_BoundsParallelogram],
                    optixPrograms[(int)ProgramKey.NV_IntersectParallelogram]);
                parallelogramGeometry["plane"].SetFloat4(plane);
                parallelogramGeometry["v1"].SetFloat3(v1);
                parallelogramGeometry["v2"].SetFloat3(v2);
                parallelogramGeometry["anchor"].SetFloat3(anchor);
                GeometryInstance parallelogramInstance = contextModel.CreateGeometryInstance(parallelogramGeometry);
                parallelogramInstance.Materials[0] = optixMaterials[(int)MaterialKey.NV_Checker];
                // add to local group
                GeometryGroup parallelogramGroup = contextModel.CreateGeometryGroup();
                parallelogramGroup.AddChild(parallelogramInstance);

                Group topGroup = contextModel.CreateGroup();
                topGroup.ChildCount = 3;
                // add to top group
                topGroup[0] = sphereGroup;
                // add to top group
                topGroup[1] = parallelogramGroup;
                // add to top group
                topGroup[2] = CreateNVIDIAGeometry();
                // set top group
                SetTopObject(topGroup);
            }
        }

        private void updateCamera(float aspect)
        {
            // update camera variables
            calculateCameraVariables(
                cameraEye,
                cameraLookAt,
                cameraUp,
                cameraFOV,
                aspect,
                ref cameraU,
                ref cameraV,
                ref cameraW);
            // do we already have the program
            if (optixPrograms[(int)ProgramKey.NV_RayGenAccumCamera] is OptixProgram camera)
            {
                camera["U"].SetFloat3(ref cameraU);
                camera["V"].SetFloat3(ref cameraV);
                camera["W"].SetFloat3(ref cameraW);
                camera["eye"].SetFloat3(ref cameraEye);
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            const float DELTA = 135.0f;
            const float INV_DELTA = -1.0f / DELTA;

            cameraFOV = e.Delta > 0
                ? cameraFOV * (DELTA / e.Delta)
                : cameraFOV * (e.Delta * INV_DELTA);
            cameraFOV = Math.Max(0.01f, Math.Min(115.0f, cameraFOV));
            updateCamera(imageRect.Aspect());
            launchCount = 0;
            base.OnMouseWheel(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == FrameworkElement.IsVisibleProperty)
            {
                bool newValue = (bool)e.NewValue;
                if (contextValidated)
                {
                    backgroundTimer.IsEnabled = newValue;
                    invalidateTimer.IsEnabled = newValue;
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (contextValidated && (sizeInfo.HeightChanged || sizeInfo.WidthChanged))
            {
                if (invalidateTimer.IsEnabled) invalidateTimer.Stop();
                if (backgroundTimer.IsEnabled) backgroundTimer.Stop();

                // resize gpu buffers
                ResizeBuffers(
                    (ulong)sizeInfo.NewSize.Width,
                    (ulong)sizeInfo.NewSize.Height);
                // update camera details
                updateCamera(imageRect.Aspect());
                // update the launch count
                launchCount = 0;
                // resize the bitmap
                ResizeBitmap();
                // Start the CUDA thread
                backgroundTimer.Start();
                // Start the Image invalidate thread
                invalidateTimer.Start();
            }
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void ResizeBitmap()
        {
            // resize the writable bitmap
            writeableBitmap = new WriteableBitmap(
                imageRect.Width,
                imageRect.Height,
                96, 96,
                System.Windows.Media.PixelFormats.Bgra32,
                null);
            // update the backbuffer pointer
            backBufferPtr = writeableBitmap.BackBuffer;
            // refresh image source bindings
            SetValue(SourceProperty, writeableBitmap);
        }

        private void ResizeBuffers(ulong nextBufferWidth, ulong nextBufferHeight)
        {
            // store new size
            bufferDescription.Width = nextBufferWidth;
            bufferDescription.Height = nextBufferHeight;
            bufferDescriptionAccum.Width = nextBufferWidth;
            bufferDescriptionAccum.Height = nextBufferHeight;
            // new image rect size
            imageRect.Width = (int)bufferDescription.Width;
            imageRect.Height = (int)bufferDescription.Height;
            // resize the buffer
            optixBuffer.Resize(
                bufferDescription.Width,
                bufferDescription.Height,
                bufferDescription.Depth);
            optixBufferAccum.Resize(
                bufferDescriptionAccum.Width,
                bufferDescriptionAccum.Height,
                bufferDescriptionAccum.Depth);
        }

        private GeometryGroup CreateNVIDIAGeometry()
        {
            // Define a regular tetrahedron of height 2, translated 1.5 units from the origin.
            TriangleMesh mesh = CreateTetrahedron(2.0f, new float3(1.5f, 0.0f, 0.0f));

            GeometryTriangles triangles = CreateGeometryTriangles(ref mesh);
            // Bind a Material to the GeometryTriangles.  Materials can be shared
            // between GeometryTriangles objects and other Geometry types, as long as
            // all of the attributes needed by the attached hit programs are produced in
            // the attribute program.
            GeometryInstance trianglesInstance = contextModel.CreateGeometryInstance(triangles);
            trianglesInstance.Materials[0] = optixMaterials[(int)MaterialKey.NV_PhongMetal];
            // add to local group
            GeometryGroup trianglesGroup = contextModel.CreateGeometryGroup(AccelBuilder.Trbvh);
            trianglesGroup.AddChild(trianglesInstance);
            // return group
            return trianglesGroup;
        }

        private void SetRecordGeometry(ref RecordModel record)
        {
            MeshReader reader = FindResource("DaggerMeshReader") as MeshReader;
            if (null != contextModel
                && reader.GetModelData(record.Id, out ModelData model))
            {
                bool wasInvalidateEnabled = invalidateTimer.IsEnabled;
                bool wasBackgroundEnabled = backgroundTimer.IsEnabled;
                if (wasInvalidateEnabled) invalidateTimer.Stop();
                if (wasBackgroundEnabled) backgroundTimer.Stop();

                if (!recordGeometry.ContainsKey(record.Id))
                {
                    RecordGeometry newGeometry = new RecordGeometry();
                    // new top object
                    newGeometry.topGroup = contextModel.CreateGroup(
                        AccelBuilder.Trbvh,
                        out newGeometry.topAccel);
                    // new group for triangle geometry
                    newGeometry.triangleGeometryGroup = contextModel.CreateGeometryGroup(
                        AccelBuilder.Trbvh,
                        out newGeometry.triangleGeometryAccel);
                    // new group for other geometry
                    newGeometry.geometryGroup = contextModel.CreateGeometryGroup(
                        AccelBuilder.NoAccel,
                        out newGeometry.geometryAccel);
                    // add to top
                    newGeometry.topGroup.ChildCount = 2;
                    newGeometry.topGroup[0] = newGeometry.geometryGroup;
                    newGeometry.topGroup[1] = newGeometry.triangleGeometryGroup;

                    TriangleMesh mesh = new TriangleMesh
                    {
                        indices = model.Indices.ToArray(),
                        normals = model.Normals.Select((n) => n.ToFloat3()).ToArray(),
                        texcoords = model.Texcoords.Select((uv) => uv.ToFloat2()).ToArray(),
                        vertices = model.Vertices.Select((v) => v.ToFloat3()).ToArray()
                    };
                    GeometryTriangles triangles = CreateGeometryTriangles(ref mesh);
                    GeometryInstance trianglesInstance = contextModel.CreateGeometryInstance(triangles);

                    //Material nvPhong = contextModel.CreateMaterial(
                    //    optixPrograms[(int)ProgramKey.NV_ClosestHitPhong],
                    //    optixPrograms[(int)ProgramKey.NV_AnyHitPhong]);
                    //float3 modelColor = ModelColor.ToFloat3();
                    //nvPhong["Ka"].SetFloat3(modelColor);
                    //nvPhong["Kd"].SetFloat3(modelColor);
                    //nvPhong["Ks"].Set(0.0f, 0.0f, 0.0f);
                    //nvPhong["phong_exp"].Set(0.0f);
                    //nvPhong["Kr"].Set(0.12f, 0.12f, 0.12f);
                    //trianglesInstance.Materials[0] = nvPhong;
                    trianglesInstance.Materials[0] = optixMaterials[(int)MaterialKey.NV_Checker];
                    trianglesInstance["inv_checker_size"].Set(4.0f, 4.0f, 4.0f);

                    //Material nvTexCoord = contextModel.CreateMaterial(
                    //    optixPrograms[(int)ProgramKey.NV_ClosestHitTexCoord],
                    //    optixPrograms[(int)ProgramKey.NV_AnyHitBarycentric]);
                    //trianglesInstance.Materials[0] = nvTexCoord;

                    newGeometry.triangleGeometryGroup.AddChild(trianglesInstance);

                    float minX = mesh.vertices.Min((v) => v.X);
                    float maxX = mesh.vertices.Max((v) => v.X);
                    float minY = mesh.vertices.Min((v) => v.Y);
                    float maxY = mesh.vertices.Max((v) => v.Y);
                    float minZ = mesh.vertices.Min((v) => v.Z);
                    float maxZ = mesh.vertices.Max((v) => v.Z);

                    const float cornerSize = 16.0f;
                    // define a regular tetrahedron, translated min,min,min
                    TriangleMesh cornerMesh = CreateTetrahedron(cornerSize, new float3(minX, minY, minZ));
                    GeometryTriangles corner000 = CreateGeometryTriangles(ref cornerMesh);
                    GeometryInstance corner000Instance = contextModel.CreateGeometryInstance(corner000);
                    corner000Instance.Materials[0] = optixMaterials[(int)MaterialKey.NV_PhongMetal];
                    // define a regular tetrahedron, translated min,min,max
                    cornerMesh = CreateTetrahedron(cornerSize, new float3(minX, minY, maxZ));
                    GeometryTriangles corner001 = CreateGeometryTriangles(ref cornerMesh);
                    GeometryInstance corner001Instance = contextModel.CreateGeometryInstance(corner001);
                    corner001Instance.Materials[0] = optixMaterials[(int)MaterialKey.NV_PhongMetal];
                    // define a regular tetrahedron, translated min,max,max
                    cornerMesh = CreateTetrahedron(cornerSize, new float3(minX, maxY, maxZ));
                    GeometryTriangles corner011 = CreateGeometryTriangles(ref cornerMesh);
                    GeometryInstance corner011Instance = contextModel.CreateGeometryInstance(corner011);
                    corner011Instance.Materials[0] = optixMaterials[(int)MaterialKey.NV_PhongMetal];
                    // define a regular tetrahedron, translated max,max,max
                    cornerMesh = CreateTetrahedron(cornerSize, new float3(maxX, maxY, maxZ));
                    GeometryTriangles corner111 = CreateGeometryTriangles(ref cornerMesh);
                    GeometryInstance corner111Instance = contextModel.CreateGeometryInstance(corner111);
                    corner111Instance.Materials[0] = optixMaterials[(int)MaterialKey.NV_PhongMetal];
                    // define a regular tetrahedron, translated min,max,min
                    cornerMesh = CreateTetrahedron(cornerSize, new float3(minX, maxY, minZ));
                    GeometryTriangles corner010 = CreateGeometryTriangles(ref cornerMesh);
                    GeometryInstance corner010Instance = contextModel.CreateGeometryInstance(corner010);
                    corner010Instance.Materials[0] = optixMaterials[(int)MaterialKey.NV_PhongMetal];
                    // define a regular tetrahedron, translated max,max,min
                    cornerMesh = CreateTetrahedron(cornerSize, new float3(maxX, maxY, minZ));
                    GeometryTriangles corner110 = CreateGeometryTriangles(ref cornerMesh);
                    GeometryInstance corner110Instance = contextModel.CreateGeometryInstance(corner110);
                    corner110Instance.Materials[0] = optixMaterials[(int)MaterialKey.NV_PhongMetal];
                    // define a regular tetrahedron, translated max,min,min
                    cornerMesh = CreateTetrahedron(cornerSize, new float3(maxX, minY, minZ));
                    GeometryTriangles corner100 = CreateGeometryTriangles(ref cornerMesh);
                    GeometryInstance corner100Instance = contextModel.CreateGeometryInstance(corner100);
                    corner100Instance.Materials[0] = optixMaterials[(int)MaterialKey.NV_PhongMetal];
                    // define a regular tetrahedron, translated max,min,max
                    cornerMesh = CreateTetrahedron(cornerSize, new float3(maxX, minY, maxZ));
                    GeometryTriangles corner101 = CreateGeometryTriangles(ref cornerMesh);
                    GeometryInstance corner101Instance = contextModel.CreateGeometryInstance(corner101);
                    corner101Instance.Materials[0] = optixMaterials[(int)MaterialKey.NV_PhongMetal];

                    newGeometry.triangleGeometryGroup.AddChildren(new GeometryInstance[]
                    {
                        corner000Instance,
                        corner001Instance,
                        corner011Instance,
                        corner111Instance,
                        corner010Instance,
                        corner110Instance,
                        corner100Instance,
                        corner101Instance
                    });

                    contextModel.LogMessages.Add("Create parallelogram geometry");
                    float3 anchor = new float3(-model.DFMesh.Radius, minY, -model.DFMesh.Radius);
                    float3 v1 = new float3(model.DFMesh.Radius * 2.0f, 0.0f, 0.0f);
                    float3 v2 = new float3(0.0f, 0.0f, model.DFMesh.Radius * 2.0f);
                    float3 normal = float3.Normalize(float3.Cross(v1, v2));
                    float d = float3.Dot(normal, anchor);
                    v1 *= 1.0f / float3.Dot(v1, v1);
                    v2 *= 1.0f / float3.Dot(v2, v2);
                    float4 plane = new float4(normal, d);
                    Geometry parallelogramGeometry = contextModel.CreateGeometry(
                        optixPrograms[(int)ProgramKey.NV_BoundsParallelogram],
                        optixPrograms[(int)ProgramKey.NV_IntersectParallelogram]);
                    parallelogramGeometry["plane"].SetFloat4(plane);
                    parallelogramGeometry["v1"].SetFloat3(v1);
                    parallelogramGeometry["v2"].SetFloat3(v2);
                    parallelogramGeometry["anchor"].SetFloat3(anchor);
                    GeometryInstance parallelogramInstance = contextModel.CreateGeometryInstance(parallelogramGeometry);
                    parallelogramInstance.Materials[0] = optixMaterials[(int)MaterialKey.NV_Checker];
                    parallelogramInstance["inv_checker_size"].Set(32.0f, 32.0f, 32.0f);

                    // add a ground plane
                    newGeometry.geometryGroup.AddChild(parallelogramInstance);

                    // update camera
                    cameraEye = new float3(model.DFMesh.Radius * -0.9f, maxY + (model.DFMesh.Radius * 0.125f), model.DFMesh.Radius * -1.25f);
                    cameraLookAt = new float3(0.0f, minY + (model.DFMesh.Radius * 0.125f), 0.0f);

                    // store new record geometry
                    recordGeometry[record.Id] = newGeometry;
                    recordGeometry[record.Id].topAccel.MarkAsDirty();
                    recordGeometry[record.Id].geometryAccel.MarkAsDirty();
                    recordGeometry[record.Id].triangleGeometryAccel.MarkAsDirty();
                    // change top object children
                    SetTopObject(recordGeometry[record.Id].topGroup);
                    // requires validation
                    if (!contextValidated)
                    {
                        // validate new geometry
                        contextValidated = contextModel.Validate();
                    }
                    else
                    {
                        // rebuild tree
                        context.BuildAccelTree();
                    }
                }
                else
                {
                    float minY = model.Vertices.Min((v) => (float)v.Y);
                    float maxY = model.Vertices.Max((v) => (float)v.Y);
                    // update camera
                    cameraEye = new float3(model.DFMesh.Radius * -0.9f, maxY + (model.DFMesh.Radius * 0.125f), model.DFMesh.Radius * -1.25f);
                    cameraLookAt = new float3(0.0f, minY + (model.DFMesh.Radius * 0.125f), 0.0f);

                    // change top object children
                    recordGeometry[record.Id].topAccel.MarkAsDirty();
                    recordGeometry[record.Id].geometryAccel.MarkAsDirty();
                    recordGeometry[record.Id].triangleGeometryAccel.MarkAsDirty();
                    SetTopObject(recordGeometry[record.Id].topGroup);
                }

                // update camera details
                cameraUp = record.CameraUpDirection.ToFloat3();
                cameraFOV = 80.0f;
                updateCamera(imageRect.Aspect());
                // restart accum
                launchCount = 0;
                // restart timers
                if (wasInvalidateEnabled) invalidateTimer.Start();
                if (wasBackgroundEnabled) backgroundTimer.Start();
            }
        }

        private GeometryTriangles CreateGeometryTriangles(ref TriangleMesh mesh)
        {
            uint vertices = (uint)mesh.vertices.Length;
            uint primitives = (uint)(mesh.indices.Length / 3);
            // Create Buffers for the triangle vertices, normals, texture coordinates, and indices.
            OptixBuffer vertex_buffer = contextModel.CreateBuffer(
                new BufferDesc
                {
                    Format = Format.Float3,
                    Type = BufferType.Input,
                    Width = vertices
                });
            OptixBuffer normal_buffer = contextModel.CreateBuffer(
                new BufferDesc
                {
                    Format = Format.Float3,
                    Type = BufferType.Input,
                    Width = vertices
                });
            OptixBuffer texcoord_buffer = contextModel.CreateBuffer(
                new BufferDesc
                {
                    Format = Format.Float2,
                    Type = BufferType.Input,
                    Width = vertices
                });
            OptixBuffer index_buffer = contextModel.CreateBuffer(
                new BufferDesc
                {
                    Format = Format.UInt3,
                    Type = BufferType.Input,
                    Width = primitives
                });
            // Copy the mesh geometry into the device Buffers.
            vertex_buffer.SetData(mesh.vertices);
            normal_buffer.SetData(mesh.normals);
            texcoord_buffer.SetData(mesh.texcoords);
            index_buffer.SetData(mesh.indices);
            // Create a GeometryTriangles object.
            // Set an attribute program for the GeometryTriangles, which will compute
            // things like normals and texture coordinates based on the barycentric
            // coordindates of the intersection.
            GeometryTriangles triangles = contextModel.CreateGeometryTriangles(
                optixPrograms[(int)ProgramKey.NV_TriangleAttributes],
                primitives);
            triangles.SetTriangleIndices(ref index_buffer);
            triangles.SetVertices(vertices, ref vertex_buffer);
            triangles.SetBuildFlags(RTgeometrybuildflags.RT_GEOMETRY_BUILD_FLAG_NONE);
            triangles["index_buffer"].Set(index_buffer);
            triangles["vertex_buffer"].Set(vertex_buffer);
            triangles["normal_buffer"].Set(normal_buffer);
            triangles["texcoord_buffer"].Set(texcoord_buffer);
            return triangles;
        }

        private void calculateCameraVariables(
            float3 eye, float3 lookat, float3 up,
            float fov, float aspect_ratio,
            ref float3 U, ref float3 V, ref float3 W,
            bool fov_is_vertical = false)
        {
            float ulen, vlen, wlen;
            W = lookat - eye; // Do not normalize W -- it implies focal length

            wlen = W.Length();
            U = float3.Normalize(float3.Cross(W, up));
            V = float3.Normalize(float3.Cross(U, W));

            if (fov_is_vertical)
            {
                vlen = wlen * (float)Math.Tan(0.5 * fov * Math.PI / 180.0);
                V *= vlen;
                ulen = vlen * aspect_ratio;
                U *= ulen;
            }
            else
            {
                ulen = wlen * (float)Math.Tan(0.5 * fov * Math.PI / 180.0);
                U *= ulen;
                vlen = ulen / aspect_ratio;
                V *= vlen;
            }
        }
    }
}
