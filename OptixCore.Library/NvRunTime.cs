using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OptixCore.Library.Native;

namespace OptixCore.Library
{
    public class NvProgram : IDisposable
    {
        public static readonly string[] DefaultIncludes = new string[]
        {
            "D:/NVIDIA Corporation/OptiX SDK 6.5.0/include",
            "D:/NVIDIA Corporation/OptiX SDK 6.5.0/include/optixu",
            "D:/NVIDIA Corporation/NVIDIA GPU Computing Toolkit/CUDA/v10.1/include"
        };

        public static readonly string[] DefaultOptions = new string[]
        {
            "-arch", "compute_61", "-use_fast_math", "-lineinfo", "-default-device", "-rdc", "true", "-D__x86_64"
        };

        protected internal IntPtr InternalPtr;
        private GCHandle gch;

        public NvProgram(String cuSource, String name, IEnumerable<String> headers = null, IEnumerable<String> includeNames = null)
        {
            gch = GCHandle.Alloc(InternalPtr, GCHandleType.Pinned);
            if (headers != null && includeNames != null)
            {
                int count = headers.Count();
                NvRunTimeAPI.CheckResult(NvRunTimeAPI.nvrtcCreateProgram(
                    ref InternalPtr, cuSource, name, count, headers.ToArray(), includeNames.ToArray()));
            }
            else
            {
                NvRunTimeAPI.CheckResult(NvRunTimeAPI.nvrtcCreateProgram(
                    ref InternalPtr, cuSource, name, 0, null, null));//, IntPtr.Zero, IntPtr.Zero));
            }
        }

        public void Compile(IEnumerable<string> options)
        {
            nvrtcResult compileResult = NvRunTimeAPI.nvrtcCompileProgram(InternalPtr, options.Count(), options.ToArray());
            if (compileResult != nvrtcResult.NVRTC_SUCCESS)
            {
                string message = NvRunTimeAPI.GetResultString(compileResult);
                message += Environment.NewLine;
                message += "Compiler Log:" + Environment.NewLine;
                message += GetCompileLog();
                // get the log
                throw new NvRunTimeException(compileResult, message);
            }
        }

        public String GetCompileLog()
        {
            NvRunTimeAPI.CheckResult(NvRunTimeAPI.nvrtcGetProgramLogSize(InternalPtr, out int ptrSize));
            StringBuilder builder = new StringBuilder(ptrSize);
            NvRunTimeAPI.CheckResult(NvRunTimeAPI.nvrtcGetProgramLog(InternalPtr, builder));
            return builder.ToString();
        }

        public String GetPTX()
        {
            NvRunTimeAPI.CheckResult(NvRunTimeAPI.nvrtcGetPTXSize(InternalPtr, out int ptrSize));
            StringBuilder builder = new StringBuilder(ptrSize);
            NvRunTimeAPI.CheckResult(NvRunTimeAPI.nvrtcGetPTX(InternalPtr, builder));
            return builder.ToString();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Destroy();
            }
        }

        private void Destroy()
        {
            if (InternalPtr != IntPtr.Zero)
            {
                nvrtcResult result = NvRunTimeAPI.nvrtcDestroyProgram(ref InternalPtr);
                InternalPtr = IntPtr.Zero;
                gch.Free();
                NvRunTimeAPI.CheckResult(result);
            }
        }
    }

    public class NvRunTime
    {
        public static void GetVersion(out int major, out int minor)
        {
            NvRunTimeAPI.CheckResult(NvRunTimeAPI.nvrtcVersion(out major, out minor));
        }

        /*
    // Create program
    nvrtcProgram prog = 0;
    NVRTC_CHECK_ERROR( nvrtcCreateProgram( &prog, cu_source, name, 0, NULL, NULL ) );

    // Gather NVRTC options
    std::vector<const char *> options;

    std::string base_dir = std::string( sutil::samplesDir() );

    // Set sample dir as the primary include path
    std::string sample_dir;
    if( sample_name )
    {
        sample_dir = std::string( "-I" ) + base_dir + "/" + sample_name;
        options.push_back( sample_dir.c_str() );
    }

    // Collect include dirs
    std::vector<std::string> include_dirs;
    const char *abs_dirs[] = { SAMPLES_ABSOLUTE_INCLUDE_DIRS };
    const char *rel_dirs[] = { SAMPLES_RELATIVE_INCLUDE_DIRS };

    const size_t n_abs_dirs = sizeof( abs_dirs ) / sizeof( abs_dirs[0] );
    for( size_t i = 0; i < n_abs_dirs; i++ )
        include_dirs.push_back(std::string( "-I" ) + abs_dirs[i]);
    const size_t n_rel_dirs = sizeof( rel_dirs ) / sizeof( rel_dirs[0] );
    for( size_t i = 0; i < n_rel_dirs; i++ )
        include_dirs.push_back(std::string( "-I" ) + base_dir + rel_dirs[i]);
    for( std::vector<std::string>::const_iterator it = include_dirs.begin(); it != include_dirs.end(); ++it )
        options.push_back( it->c_str() );

    // Collect NVRTC options
    const char *compiler_options[] = { CUDA_NVRTC_OPTIONS };
    const size_t n_compiler_options = sizeof( compiler_options ) / sizeof( compiler_options[0] );
    for( size_t i = 0; i < n_compiler_options - 1; i++ )
        options.push_back( compiler_options[i] );

    // JIT compile CU to PTX
    const nvrtcResult compileRes = nvrtcCompileProgram( prog, (int) options.size(), options.data() );

    // Retrieve log output
    size_t log_size = 0;
    NVRTC_CHECK_ERROR( nvrtcGetProgramLogSize( prog, &log_size ) );
    g_nvrtcLog.resize( log_size );
    if( log_size > 1 )
    {
        NVRTC_CHECK_ERROR( nvrtcGetProgramLog( prog, &g_nvrtcLog[0] ) );
        if( log_string )
            *log_string = g_nvrtcLog.c_str();
    }
    if( compileRes != NVRTC_SUCCESS )
        throw Exception( "NVRTC Compilation failed.\n" + g_nvrtcLog );

    // Retrieve PTX code
    size_t ptx_size = 0;
    NVRTC_CHECK_ERROR( nvrtcGetPTXSize( prog, &ptx_size ) );
    ptx.resize( ptx_size );
    NVRTC_CHECK_ERROR( nvrtcGetPTX( prog, &ptx[0] ) );

    // Cleanup
    NVRTC_CHECK_ERROR( nvrtcDestroyProgram( &prog ) );
         */
    }
}
