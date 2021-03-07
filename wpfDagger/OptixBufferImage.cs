using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using OptixCore.Library;
using OptixCore.Library.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace wpfDagger
{
    // shortcut for Vector3
    using float2 = System.Numerics.Vector2;
    using float3 = System.Numerics.Vector3;
    using float4 = System.Numerics.Vector4;
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

            //NVA_RayGenPathTraceCamera,
            //NVA_MissSunSky,
            //NVA_Exception,
            NVA_AnyHitDiffuse,
            NVA_ClosestHitDiffuse,

            BoundsUnitSphere,
            IntersectUnitSphere,

            TriangleAttributes,
            AnyHitPhong,
            ClosestHitPhong,
            ClosestHitPhongTextured,

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

            NVA_Diffuse,

            DefaultChecker,
            //PhongTextured,

            /// <summary>
            /// Must be last
            /// </summary>
            Count
        }

        readonly OptixProgram[] optixPrograms = new OptixProgram[(int)ProgramKey.Count];
        readonly Material[] optixMaterials = new Material[(int)MaterialKey.Count];

        private readonly Dictionary<int, Material> cachedMaterials;

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

        private readonly DaggerfallUnity daggerfallUnity;
        private readonly MeshReader meshReader;

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
            cachedMaterials = new Dictionary<int, Material>();

            backgroundTimer = new DispatcherTimer(DispatcherPriority.Background);
            invalidateTimer = new DispatcherTimer(DispatcherPriority.Render);
            backgroundTimer.Tick += BackgroundTimer_Tick;
            backgroundTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0);
            invalidateTimer.Tick += InvalidateTimer_Tick;
            invalidateTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / 30.0);

            daggerfallUnity = FindResource("DaggerfallUnity") as DaggerfallUnity;
            meshReader = FindResource("MeshReader") as MeshReader;
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
            //optixPrograms[(int)ProgramKey.NVA_RayGenPathTraceCamera]["frame"].Set(launchCount);
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
            public uint[] materials;
        }

        internal static TriangleMesh CreateTetrahedron(float H, float3 trans)
        {
            const int SIZE = 12;
            TriangleMesh result = new TriangleMesh
            {
                vertices = new float3[SIZE],
                normals = new float3[SIZE],
                texcoords = new float2[SIZE],
                indices = new uint[SIZE]
            };

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
            //nvConstantBGMiss["bg_color"].Set(1.0f, 1.0f, 1.0f);
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

        private void LoadNVIDIAAdvancedSamples()
        {
            contextModel.StackSize = 600;
            contextModel.MaxTraceDepth = 12;

            // Note: high max depth for reflection and refraction through glass
            context["max_depth"].Set(10);
            context["cutoff_color"].Set(0.2f, 0.2f, 0.2f);
            context["scene_epsilon"].Set(1e-3f);

            // Set the NVIDIA camera as the active entry point
            //EntryPointIndex = (uint)DaggerEntryPoint.NVIDIA_PathTraceCamera;

            contextModel.LogMessages.Add("Loading nvidia advanced shading model resources");
            string nvDir = Path.Combine(Directory.GetCurrentDirectory(), "cuda", "advanced");
            string nvHelpersSource = contextModel.ReadFileToEnd(Path.Combine(nvDir, "helpers.h"));
            string nvCommonStructsSource = contextModel.ReadFileToEnd(Path.Combine(nvDir, "commonStructs.h"));
            string nvPRDSource = contextModel.ReadFileToEnd(Path.Combine(nvDir, "prd.h"));
            string nvRandomSource = contextModel.ReadFileToEnd(Path.Combine(nvDir, "random.h"));
            contextModel.LogMessages.Add("Compiling nvidia path trace camera ray generation program");
            string nvPinholeCameraPTX = contextModel.CompilePtxFromString(
                contextModel.ReadFileToEnd(Path.Combine(nvDir, "path_trace_camera.cu")),
                "nva_path_trace_camera",
                null,
                new string[] { nvPRDSource, nvHelpersSource, nvRandomSource },
                new string[] { "prd.h", "helpers.h", "random.h" });
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
            // TODO: Sunsky miss
            //optixPrograms[(int)ProgramKey.NVA_RayGenPathTraceCamera] = nvPinholeCamera;
            //optixPrograms[(int)ProgramKey.NVA_MissSunSky] = nvSunSkyMiss;
            //optixPrograms[(int)ProgramKey.NVA_Exception] = nvException;
            //context.SetRayGenerationProgram((uint)DaggerEntryPoint.NVIDIA_PathTraceCamera, nvPinholeCamera);
            //context.SetExceptionProgram((uint)DaggerEntryPoint.NVIDIA_PathTraceCamera, nvException);
            // set miss for each ray type
            //context.SetRayMissProgram((uint)DaggerRayType.NVIDIA_Radiance, nvSunSkyMiss);
            //context.SetRayMissProgram((uint)DaggerRayType.NVIDIA_Shadow, nvSunSkyMiss);

            // diffuse shader material
            contextModel.LogMessages.Add("Compiling nvidia advanced diffuse material program");
            string nvDiffusePTX = contextModel.CompilePtxFromString(
                contextModel.ReadFileToEnd(Path.Combine(nvDir, "diffuse.cu")),
                "nva_diffuse",
                null,
                new string[] { nvPRDSource, nvHelpersSource, nvRandomSource, nvCommonStructsSource },
                new string[] { "prd.h", "helpers.h", "random.h", "commonStructs.h" });
            OptixProgram nvDiffuseAnyHit = contextModel.CreateFromPTXString(ref nvDiffusePTX, "any_hit_shadow");
            OptixProgram nvDiffuseClosestHit = contextModel.CreateFromPTXString(ref nvDiffusePTX, "closest_hit_radiance");
            optixPrograms[(int)ProgramKey.NVA_AnyHitDiffuse] = nvDiffuseAnyHit;
            optixPrograms[(int)ProgramKey.NVA_ClosestHitDiffuse] = nvDiffuseClosestHit;
            optixMaterials[(int)MaterialKey.NVA_Diffuse] = contextModel.CreateMaterial(nvDiffuseClosestHit, nvDiffuseAnyHit);

            /*
             */
        }

        private void CreateLights()
        {
            BasicLight[] lights = {
                new BasicLight {
                    Position = new float3( -256, 1024.0f, -768.0f ),
                    Color = new float3( 1.0f, 1.0f, 1.0f ),
                    castShadows = 1
                },
                new BasicLight {
                    Position = new float3( -64, 8.0f, -64.0f ),
                    Color = new float3( 0.015f, 0.015f, 0.015f ),
                    castShadows = 0
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
            // Unit Sphere
            contextModel.LogMessages.Add("Compiling unit sphere geometry programs");
            string unitSpherePTX = contextModel.CompilePtxFromSource("unit_sphere.cu", "unit_sphere");
            optixPrograms[(int)ProgramKey.BoundsUnitSphere] = contextModel.CreateFromPTXString(ref unitSpherePTX, "bounds");
            optixPrograms[(int)ProgramKey.IntersectUnitSphere] = contextModel.CreateFromPTXString(ref unitSpherePTX, "intersect");
            //Geometry unitSphereGeometry = contextModel.CreateGeometry(unitSphereBounds, unitSphereIntersect);
            //GeometryInstance unitSphereInstance = contextModel.CreateGeometryInstance(unitSphereGeometry);
            //unitSphereInstance.Materials[0] = optixMaterials[(int)MaterialKey.NV_PhongMetal];
            //// add to top group
            //topObjectGroup.AddChild(unitSphereInstance);

            contextModel.LogMessages.Add("Compiling phong material program");
            //string nvDir = Path.Combine(Directory.GetCurrentDirectory(), "cuda", "nvidia");
            string nvHelpersSource = contextModel.ReadCUFileToEnd("helpers.h", "cuda", "nvidia");
            string nvCommonSource = contextModel.ReadCUFileToEnd("common.h", "cuda", "nvidia");
            string nvPhongSource = contextModel.ReadCUFileToEnd("dagger_phong.h", "cuda");
            string phongPTX = contextModel.CompilePtxFromString(
                contextModel.ReadCUFileToEnd("dagger_phong.cu", "cuda"),
                "dagger_phong",
                null,
                new string[] { nvCommonSource, nvHelpersSource, nvPhongSource },
                new string[] { "common.h", "helpers.h", "dagger_phong.h" });
            optixPrograms[(int)ProgramKey.AnyHitPhong] = contextModel.CreateFromPTXString(ref phongPTX, "any_hit_shadow");
            optixPrograms[(int)ProgramKey.ClosestHitPhong] = contextModel.CreateFromPTXString(ref phongPTX, "closest_hit_radiance");
            optixPrograms[(int)ProgramKey.ClosestHitPhongTextured] = contextModel.CreateFromPTXString(ref phongPTX, "closest_hit_radiance_textured");

            // load uint version of triangle attributes
            contextModel.LogMessages.Add("Compiling triangle attribute program");
            string trianglesPTX = contextModel.CompilePtxFromSource("triangle_mesh.cu", "uint_triangles");
            OptixProgram triangleAttributes = contextModel.CreateFromPTXString(ref trianglesPTX, "triangle_attributes");
            optixPrograms[(int)ProgramKey.TriangleAttributes] = triangleAttributes;

            Material checker = contextModel.CreateMaterial(
                optixPrograms[(int)ProgramKey.NV_ClosestHitChecker],
                optixPrograms[(int)ProgramKey.NV_AnyHitChecker]);
            float3 ka1 = CheckerColorA.ToFloat3();
            float3 ka2 = CheckerColorB.ToFloat3();
            checker["Kd1"].SetFloat3(ka1);
            checker["Ka1"].SetFloat3(ka1);
            checker["Ks1"].Set(0.0f, 0.0f, 0.0f);
            checker["Kd2"].SetFloat3(ka2);
            checker["Ka2"].SetFloat3(ka2);
            checker["Ks2"].Set(0.0f, 0.0f, 0.0f);
            checker["inv_checker_size"].Set(4.0f, 4.0f, 4.0f);
            checker["phong_exp1"].Set(0.0f);
            checker["phong_exp2"].Set(0.0f);
            checker["Kr1"].Set(0.1f, 0.1f, 0.1f);
            checker["Kr2"].Set(0.0f, 0.0f, 0.0f);
            optixMaterials[(int)MaterialKey.DefaultChecker] = checker;

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
            var emissiveKey = UnityEngine.Shader.PropertyToID("_EmissionColor");
#if true
            if (null != contextModel
                && meshReader.GetModelData(record.Id, out ModelData model))
            {
                bool wasInvalidateEnabled = invalidateTimer.IsEnabled;
                bool wasBackgroundEnabled = backgroundTimer.IsEnabled;
                if (wasInvalidateEnabled) invalidateTimer.Stop();
                if (wasBackgroundEnabled) backgroundTimer.Stop();

                float scaledRadius = model.DFMesh.Radius * MeshReader.GlobalScale;
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
                    // bounding box
                    float minX = model.Vertices.Min((v) => (float)v.x);
                    float maxX = model.Vertices.Max((v) => (float)v.x);
                    float minY = model.Vertices.Min((v) => (float)v.y);
                    float maxY = model.Vertices.Max((v) => (float)v.y);
                    float minZ = model.Vertices.Min((v) => (float)v.z);
                    float maxZ = model.Vertices.Max((v) => (float)v.z);

                    List<uint> materialIndices = new List<uint>(model.DFMesh.TotalTriangles);
                    for (uint smi = 0u; smi < model.SubMeshes.Length; ++smi)
                    {
                        // repeat the material index for each primitive
                        materialIndices.AddRange(Enumerable.Repeat(smi, model.SubMeshes[smi].PrimitiveCount));
                    }
                    TriangleMesh mesh = new TriangleMesh
                    {
                        indices = model.Indices.Select((i) => (uint)i).ToArray(),
                        normals = model.Normals.Select((n) => new float3(n.x, n.y, n.z)).ToArray(),
                        texcoords = model.UVs.Select((uv) => new float2(uv.x, uv.y)).ToArray(),
                        vertices = model.Vertices.Select((v) => new float3(v.x, v.y, v.z)).ToArray(),
                        materials = materialIndices.ToArray()
                    };
                    GeometryTriangles triangles = CreateGeometryTriangles(ref mesh);
                    GeometryInstance trianglesInstance = contextModel.CreateGeometryInstance(
                        triangles,
                        (uint)model.SubMeshes.Length);
                    // must match when using material index buffer
                    triangles.MaterialCount = (uint)model.SubMeshes.Length;

                    // create an instance for each child (convert to material buffer)
                    for (int s = 0; s < model.SubMeshes.Length; ++s)
                    {
                        var subMesh = model.SubMeshes[s];
                        var cached = record._cachedMaterials[s];
                        int materialKey = MaterialReader.MakeTextureKey(
                            (short)subMesh.TextureArchive,
                            (byte)subMesh.TextureRecord,
                            (byte)0);
                        if (cachedMaterials.ContainsKey(materialKey))
                        {
                            // use the cacned material
                            trianglesInstance.Materials[s] = cachedMaterials[materialKey];
                            continue;
                        }
                        else if (cached.albedoMap != null
                            && cached.albedoMap.width > 0
                            && cached.albedoMap.height > 0)
                        {
                            var albedoTexture = cached.albedoMap;
                            // new textured material
                            Material daggerMaterial = contextModel.CreateMaterial(
                                optixPrograms[(int)ProgramKey.ClosestHitPhongTextured],
                                optixPrograms[(int)ProgramKey.AnyHitPhong]);

                            // buffer for texture data
                            OptixBuffer albedoBuffer = contextModel.CreateBuffer(
                                new BufferDesc
                                {
                                    Format = Format.UByte4,
                                    Height = (uint)albedoTexture.height,
                                    Width = (uint)albedoTexture.width,
                                    Type = BufferType.Input
                                });
                            // convert pixel data to RGBA
                            byte[] albedoPixels = new byte[albedoTexture.bytesBGRA.Length];
                            for (int p = 0; p < albedoPixels.Length; p += 4)
                            {
                                albedoPixels[p + 0] = albedoTexture.bytesBGRA[p + 2];
                                albedoPixels[p + 1] = albedoTexture.bytesBGRA[p + 1];
                                albedoPixels[p + 2] = albedoTexture.bytesBGRA[p + 0];
                                albedoPixels[p + 3] = albedoTexture.bytesBGRA[p + 3];
                            }
                            albedoBuffer.SetData(albedoPixels);
                            // texture sampler
                            TextureSamplerDesc desc = TextureSamplerDesc.Default;
                            desc.Filter.Mag = FilterMode.Nearest;
                            desc.Filter.Min = FilterMode.Nearest;
                            desc.Wrap.WrapU = WrapMode.Repeat;
                            desc.Wrap.WrapV = WrapMode.Repeat;
                            TextureSampler albedoSampler = contextModel.CreateTextureSampler(desc);
                            albedoSampler.SetBuffer(albedoBuffer);
                            // buffer for emissive data
                            OptixBuffer emissiveBuffer = contextModel.CreateBuffer(
                                new BufferDesc
                                {
                                    Format = Format.UByte4,
                                    Width = 2,
                                    Height = 2,
                                    Type = BufferType.Input
                                });
                            // check for emissive
                            if (cached.emissionMap != null
                                && cached.emissionMap.width > 0
                                && cached.emissionMap.height > 0)
                            {
                                //System.Windows.Media.Color color = cached.material.GetColor(emissiveKey);
                                var emissiveTexture = cached.emissionMap;
                                // convert pixel data to RGBA
                                byte[] emissivePixels = new byte[emissiveTexture.bytesBGRA.Length];
                                for (int p = 0; p < emissivePixels.Length; p += 4)
                                {
                                    emissivePixels[p + 0] = emissiveTexture.bytesBGRA[p + 2]; //color.R; 
                                    emissivePixels[p + 1] = emissiveTexture.bytesBGRA[p + 1]; //color.G; 
                                    emissivePixels[p + 2] = emissiveTexture.bytesBGRA[p + 0]; //color.B; 
                                    emissivePixels[p + 3] = emissiveTexture.bytesBGRA[p + 3];
                                }
                                // resize buffer
                                emissiveBuffer.Resize((ulong)emissiveTexture.width, (ulong)emissiveTexture.height, 0);
                                emissiveBuffer.SetData(emissivePixels);
                            }
                            else
                            {   // empty RGBA (2x2)
                                byte[] emissivePixels = Enumerable.Repeat<byte>(0, 16).ToArray();
                                emissiveBuffer.SetData(emissivePixels);
                            }
                            // emissive sampler
                            desc.Filter.Mag = FilterMode.Nearest;
                            desc.Filter.Min = FilterMode.Nearest;
                            desc.Wrap.WrapU = WrapMode.Repeat;
                            desc.Wrap.WrapV = WrapMode.Repeat;
                            TextureSampler emissiveSampler = contextModel.CreateTextureSampler(desc);
                            emissiveSampler.SetBuffer(emissiveBuffer);
                            // assign the sampler
                            daggerMaterial["Kd"].Set(1.0f, 1.0f, 1.0f);
                            if (cached.isWindow)
                            {
                                var windowColor = cached.material.GetColor(emissiveKey);
                                daggerMaterial["Kr"].Set(windowColor.r, windowColor.g, windowColor.b);
                                //daggerMaterial["Kr"].Set(0.5f, 0.5f, 0.5f);
                                daggerMaterial["Ka"].Set(0.0f, 0.0f, 0.0f);
                                daggerMaterial["Ks"].Set(0.0f, 0.0f, 0.0f);
                                daggerMaterial["phong_exp"].Set(0.0f);
                            }
                            else
                            {
                                daggerMaterial["Kr"].Set(0.0f, 0.0f, 0.0f);
                                daggerMaterial["Ka"].Set(0.0f, 0.0f, 0.0f);
                                daggerMaterial["Ks"].Set(0.0f, 0.0f, 0.0f);
                                daggerMaterial["phong_exp"].Set(0.0f);
                            }
                            daggerMaterial["Kd_map"].Set(albedoSampler);
                            daggerMaterial["Em_map"].Set(emissiveSampler);
                            // use the new material
                            trianglesInstance.Materials[s] = daggerMaterial;
                            // cache material
                            cachedMaterials[materialKey] = daggerMaterial;
                            continue;
                        }

                        // use default material, do not cache
                        trianglesInstance.Materials[s] = optixMaterials[(int)MaterialKey.DefaultChecker];
                    }

                    newGeometry.triangleGeometryGroup.AddChild(trianglesInstance);

                    const float cornerSize = 32.0f * MeshReader.GlobalScale;
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
                    float3 anchor = new float3(-scaledRadius, minY, -scaledRadius);
                    float3 v1 = new float3(scaledRadius * 2.0f, 0.0f, 0.0f);
                    float3 v2 = new float3(0.0f, 0.0f, scaledRadius * 2.0f);
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
                    cameraEye = new float3(scaledRadius * -0.9f, maxY + (scaledRadius * 0.125f), scaledRadius * -1.25f);
                    cameraLookAt = new float3(0.0f, minY + (scaledRadius * 0.125f), 0.0f);

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
                    float minY = model.Vertices.Min((v) => (float)v.y);
                    float maxY = model.Vertices.Max((v) => (float)v.y);
                    // update camera
                    cameraEye = new float3(scaledRadius * -0.9f, maxY + (scaledRadius * 0.125f), scaledRadius * -1.25f);
                    cameraLookAt = new float3(0.0f, minY + (scaledRadius * 0.125f), 0.0f);

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
#endif
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
            OptixBuffer material_buffer = (null == mesh.materials)
                ? null
                : contextModel.CreateBuffer(
                    new BufferDesc
                    {
                        Format = Format.UInt,
                        Type = BufferType.Input,
                        Width = primitives
                    });

            // Copy the mesh geometry into the device Buffers.
            vertex_buffer.SetData(mesh.vertices);
            normal_buffer.SetData(mesh.normals);
            texcoord_buffer.SetData(mesh.texcoords);
            index_buffer.SetData(mesh.indices);
            // copy if available
            material_buffer?.SetData(mesh.materials);

            // Create a GeometryTriangles object.
            // Set an attribute program for the GeometryTriangles, which will compute
            // things like normals and texture coordinates based on the barycentric
            // coordindates of the intersection.
            GeometryTriangles triangles = contextModel.CreateGeometryTriangles(
                optixPrograms[(int)ProgramKey.TriangleAttributes],
                primitives);
            triangles.SetTriangleIndices(ref index_buffer);
            triangles.SetVertices(vertices, ref vertex_buffer);
            if (null != material_buffer)
            {
                triangles.SetMaterialIndices(ref material_buffer);
            }
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
