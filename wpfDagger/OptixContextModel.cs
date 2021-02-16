using OptixCore.Library;
using OptixCore.Library.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using wpfDagger.Properties;

namespace wpfDagger
{
    using float3 = System.Numerics.Vector3;

    public enum DaggerEntryPoint : uint
    {
        SolidColorTest = 0,
        NVIDIA_PinholeCamera,

        /// <summary>
        /// Must be last
        /// </summary>
        Count,

    }

    public enum DaggerRayType : int
    {
        NVIDIA_Radiance = 0,
        NVIDIA_Shadow,

        /// <summary>
        /// Must be last
        /// </summary>
        Count,
    }

    /// <summary>
    /// Optix Context Model
    /// </summary>
    public class OptixContextModel : INotifyPropertyChanged
    {
        private List<OptixNode> nodes;

        public Context Context
        { get; private set; }

        public bool IsValid
        { get; private set; }

        public bool IsLoaded
        { get; private set; }

        public ObservableCollection<String> LogMessages
        { get; private set; }

        private ulong stackSize;
        public ulong StackSize
        {
            get => stackSize;
            set
            {
                if (stackSize != value)
                {
                    stackSize = value;
                    Context.SetStackSize(stackSize);
                    NotifyPropertyChanged("StackSize");
                }
            }
        }

        private uint maxTraceDepth;
        public uint MaxTraceDepth
        {
            get => maxTraceDepth;
            set
            {
                if (maxTraceDepth != value)
                {
                    maxTraceDepth = value;
                    Context.SetMaxTraceDepth(maxTraceDepth);
                    NotifyPropertyChanged("MaxTraceDepth");
                }
            }
        }

        public OptixContextModel()
        {
            IsValid = false;
            IsLoaded = false;
            nodes = new List<OptixNode>();
            maxTraceDepth = 4;
            stackSize = 1024;
            List<String> logMessages = new List<string>();
            try
            {
                logMessages.Add("Creating context");
                Context = new Context
                {
                    RayTypeCount = (uint)DaggerRayType.Count,
                    EntryPointCount = (uint)DaggerEntryPoint.Count
                };

                logMessages.Add("Loading embedded ray generation program");
                string colorSource = ReadStreamToEnd(new MemoryStream(Resources.draw_color));
                // use the embedded draw color ray generation
                string colorPTX = CompilePtxFromString(
                    colorSource,
                    "draw_solid_color",
                    null,
                    new string[] { Resources.common },
                    new string[] { "common.h" });
                // set as ray generation
                OptixProgram scRayGen = CreateFromPTXString(ref colorPTX, "draw_solid_color");
                scRayGen["draw_color"].Set(0.462f, 0.725f, 0.0f);
                Context.SetRayGenerationProgram((uint)DaggerEntryPoint.SolidColorTest, scRayGen);

                // Context was successfully loaded
                logMessages.Add("Context loaded successfully");
                IsLoaded = true;
            }
            catch (NvRunTimeException eCompile)
            {
                System.Diagnostics.Debug.Print("Optix Compiler Error: {0}", eCompile.Message);
                logMessages.Add(String.Format("Optix Compiler Error: {0}", eCompile.Message));
            }
            catch (Exception eCreateContext)
            {
                System.Diagnostics.Debug.Print("Optix Error: {0}", eCreateContext.Message);
                logMessages.Add(String.Format("Optix Error: {0}", eCreateContext.Message));
            }
            // create the observable collection of logs
            LogMessages = new ObservableCollection<string>(logMessages);
        }

        public bool Validate()
        {
            if (!IsLoaded)
            {
                return false;
            }
            try
            {
                // validate
                Context.Validate();
                IsValid = true;
            }
            catch (Exception eOptix)
            {
                System.Diagnostics.Debug.Print("Optix Error: {0}", eOptix.Message);
                LogMessages.Add(String.Format("Optix Validation Error: {0}", eOptix));
                IsValid = false;
            }
            NotifyPropertyChanged("IsValid");
            return IsValid;
        }

        public void AddNode(OptixNode node)
        {
            nodes.Add(node);
        }

        public void Destroy()
        {
            if (Context == null)
                return;

            foreach (var node in nodes)
            {
                node.Destroy();
            }
            nodes.Clear();
            Context.Dispose();
            Context = null;
            IsValid = false;
            NotifyPropertyChanged("IsValid");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        internal OptixProgram CreateFromPTXString(ref string ptx, string name)
        {
            OptixProgram result = OptixProgram.CreateFromPTXString(Context, ptx, name);
            nodes.Add(result);
            return result;
        }

        internal Material CreateMaterial(
            OptixProgram closestHit,
            OptixProgram anyHit = null,
            DaggerRayType closestHitType = DaggerRayType.NVIDIA_Radiance,
            DaggerRayType anyHitType = DaggerRayType.NVIDIA_Shadow)
        {
            Material result = new Material(Context);
            nodes.Add(result);
            result.SetSurfaceProgram((int)closestHitType, SurfaceProgram.CreateFromProgram(closestHit, RayHitType.Closest));
            if (null != anyHit)
            {
                result.SetSurfaceProgram((int)anyHitType, SurfaceProgram.CreateFromProgram(anyHit, RayHitType.Any));
            }
            return result;
        }

        internal OptixBuffer CreateBuffer(BufferDesc desc)
        {
            OptixBuffer result = new OptixBuffer(Context, desc);
            nodes.Add(result);
            return result;
        }

        internal Group CreateGroup(AccelBuilder builder = AccelBuilder.NoAccel)
        {
            return CreateGroup(builder, out Acceleration accel);
        }

        internal Group CreateGroup(AccelBuilder builder, out Acceleration accel)
        {
            accel = new Acceleration(Context, builder);
            nodes.Add(accel);
            Group result = new Group(Context)
            {
                Acceleration = accel
            };
            nodes.Add(result);
            return result;
        }

        internal GeometryGroup CreateGeometryGroup(AccelBuilder builder = AccelBuilder.NoAccel)
        {
            return CreateGeometryGroup(builder, out Acceleration accel);
        }

        internal GeometryGroup CreateGeometryGroup(AccelBuilder builder, out Acceleration accel)
        {
            accel = new Acceleration(Context, builder);
            nodes.Add(accel);
            GeometryGroup result = new GeometryGroup(Context)
            {
                Acceleration = accel
            };
            nodes.Add(result);
            return result;
        }

        internal Geometry CreateGeometry(OptixProgram bounds, OptixProgram intersect, uint primitives = 1)
        {
            Geometry result = new Geometry(Context)
            {
                BoundingBoxProgram = bounds,
                IntersectionProgram = intersect,
                PrimitiveCount = primitives,
            };
            nodes.Add(result);
            return result;
        }

        internal GeometryTriangles CreateGeometryTriangles(OptixProgram attribute, uint primitives = 1)
        {
            GeometryTriangles result = new GeometryTriangles(Context)
            {
                AttributeProgram = attribute,
                PrimitiveCount = primitives
            };
            nodes.Add(result);
            return result;
        }

        internal GeometryInstance CreateGeometryInstance(GeometryTriangles geometryTriangles, uint materials = 1)
        {
            GeometryInstance result = new GeometryInstance(Context)
            {
                GeometryTriangles = geometryTriangles,
                MaterialCount = materials
            };
            nodes.Add(result);
            return result;
        }

        internal GeometryInstance CreateGeometryInstance(Geometry geometry, uint materials = 1)
        {
            GeometryInstance result = new GeometryInstance(Context)
            {
                Geometry = geometry,
                MaterialCount = materials
            };
            nodes.Add(result);
            return result;
        }

        internal string ReadFileToEnd(String filepath)
        {
            using (StreamReader sr = File.OpenText(filepath))
            {
                return sr.ReadToEnd();
            }
        }

        internal string ReadStreamToEnd(Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }

        internal String CompilePtxFromSource(String fileName, String programName, String localDir = "cuda")
        {
            string cuDirectory = String.IsNullOrEmpty(localDir)
                ? Directory.GetCurrentDirectory()
                : Path.Combine(Directory.GetCurrentDirectory(), localDir);
            string cuSource = ReadFileToEnd(Path.Combine(cuDirectory, fileName));
            return CompilePtxFromString(cuSource, programName, new string[] { cuDirectory });
        }

        internal String CompilePtxFromString(
            String cuSource,
            String programName,
            IEnumerable<string> additionalIncludeDirectories = null,
            IEnumerable<string> headerSources = null,
            IEnumerable<string> headerNames = null)
        {
            // Use CUDA Runtime to Compile PTX files
            string ptxSource = null;
            using (NvProgram nvProgram = new NvProgram(cuSource, programName, headerSources, headerNames))
            {
                List<string> compilerOptions = new List<string>();
                // add any additional include directories
                if (additionalIncludeDirectories != null)
                {
                    foreach (string includeDir in additionalIncludeDirectories)
                    {
                        compilerOptions.Add("-I" + includeDir);
                    }
                }
                // add the default include directories
                foreach (string includeDir in NvProgram.DefaultIncludes)
                {
                    compilerOptions.Add("-I" + includeDir);
                }
                // add the compiler options
                foreach (string option in Settings.Default.CudaCompileOptions)
                {
                    compilerOptions.Add(option);
                }
                // compile the ptx
                nvProgram.Compile(compilerOptions);
                ptxSource = nvProgram.GetPTX();
            }
            return ptxSource;
        }
    }
}