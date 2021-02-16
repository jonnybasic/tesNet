using System;
using System.Runtime.InteropServices;
using System.Text;

namespace OptixCore.Library.Native
{
    public enum CudaResult : uint
    {
        Success = 0,
        ErrorInvalidValue = 1,
        ErrorMemoryAllocation = 2,
        ErrorInitializationError = 3,

    }

    public enum CudaMemCpyKind : uint
    {
        cudaMemcpyHostToHost = 0,
        cudaMemcpyHostToDevice = 1,
        cudaMemcpyDeviceToHost = 2,
        cudaMemcpyDeviceToDevice = 3,
        cudaMemcpyDefault = 4,
    }

    internal unsafe class CudaRunTimeApi
    {
        internal const string RunTimeAPIDll = "cudart64_101.dll";
        [DllImport(RunTimeAPIDll, EntryPoint = "cudaMalloc")]
        public static extern CudaResult cudaMalloc(ref IntPtr dptr, uint bytesize);

        [DllImport(RunTimeAPIDll, EntryPoint = "cudaFree")]
        public static extern CudaResult cudaFree(IntPtr devPtr);

        [DllImport(RunTimeAPIDll, EntryPoint = "cudaMemcpy")]
        public static extern CudaResult cudaMemcpy(void* dst, IntPtr src, uint bytesize, CudaMemCpyKind kind);

        [DllImport(RunTimeAPIDll, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cudaGetErrorString")]
        public static extern IntPtr cudaGetErrorString(CudaResult c);

        public static void CudaCall(CudaResult result)
        {
            if (result != CudaResult.Success)
            {
                var str = Marshal.PtrToStringAnsi(cudaGetErrorString(result));
                throw new ApplicationException(str);
            }
        }
    }
}

namespace OptixCore.Library.Native
{
    using nvrtcProgram = IntPtr;

    public enum nvrtcResult : uint
    {
        NVRTC_SUCCESS = 0,
        NVRTC_ERROR_OUT_OF_MEMORY = 1,
        NVRTC_ERROR_PROGRAM_CREATION_FAILURE = 2,
        NVRTC_ERROR_INVALID_INPUT = 3,
        NVRTC_ERROR_INVALID_PROGRAM = 4,
        NVRTC_ERROR_INVALID_OPTION = 5,
        NVRTC_ERROR_COMPILATION = 6,
        NVRTC_ERROR_BUILTIN_OPERATION_FAILURE = 7,
        NVRTC_ERROR_NO_NAME_EXPRESSIONS_AFTER_COMPILATION = 8,
        NVRTC_ERROR_NO_LOWERED_NAMES_BEFORE_COMPILATION = 9,
        NVRTC_ERROR_NAME_EXPRESSION_NOT_VALID = 10,
        NVRTC_ERROR_INTERNAL_ERROR = 11
    }

    public class NvRunTimeException : ApplicationException
    {
        public nvrtcResult Result
        { get; private set; }

        public NvRunTimeException(nvrtcResult result) : base()
        { }
        
        public NvRunTimeException(nvrtcResult result, string message) : base(message)
        { }
    }

    internal unsafe class NvRunTimeAPI
    {
        internal const string RunTimeAPIDll = "nvrtc64_101_0.dll";

        [DllImport(RunTimeAPIDll, CharSet = CharSet.Ansi, EntryPoint = "nvrtcGetErrorString")]
        public static extern IntPtr nvrtcGetErrorString(nvrtcResult result);

        [DllImport(RunTimeAPIDll, EntryPoint = "nvrtcVersion")]
        public static extern nvrtcResult nvrtcVersion(out int major, out int minor);

#if true
        [DllImport(RunTimeAPIDll, CharSet = CharSet.Ansi, EntryPoint = "nvrtcCreateProgram")]
        public static extern nvrtcResult nvrtcCreateProgram(
            ref nvrtcProgram prog,
            string src,
            string name,
            int numHeader,
            [In] string[] header,
            [In] string[] includeNames);

        [DllImport(RunTimeAPIDll, CharSet = CharSet.Ansi, EntryPoint = "nvrtcCompileProgram")]
        public static extern nvrtcResult nvrtcCompileProgram(
            nvrtcProgram prog,
            int numOptions,
            [In] string[] options);
#else
        [DllImport(RunTimeAPIDll, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvrtcCreateProgram")]
        public static extern nvrtcResult nvrtcCreateProgram(
            ref nvrtcProgram prog,
            [MarshalAs(UnmanagedType.LPStr)]string src,
            [MarshalAs(UnmanagedType.LPStr)]string name,
            int numHeaders,
            IntPtr headers,
            IntPtr includeNames);
            //[MarshalAs(UnmanagedType.LPArray)]string[] headers,
            //[MarshalAs(UnmanagedType.LPArray)]string[] includeNames);

        [DllImport(RunTimeAPIDll, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvrtcCompileProgram")]
        public static extern nvrtcResult nvrtcCompileProgram(
            nvrtcProgram prog,
            int numOptions,
            [MarshalAs(UnmanagedType.LPArray)]string[] options);
#endif

        [DllImport(RunTimeAPIDll, EntryPoint = "nvrtcGetProgramLogSize")]
        public static extern nvrtcResult nvrtcGetProgramLogSize(nvrtcProgram prog, out int logSizeRet);

        [DllImport(RunTimeAPIDll, CharSet = CharSet.Ansi, EntryPoint = "nvrtcGetProgramLog")]
        public static extern nvrtcResult nvrtcGetProgramLog(nvrtcProgram prog, StringBuilder log);

        [DllImport(RunTimeAPIDll, EntryPoint = "nvrtcGetPTXSize")]
        public static extern nvrtcResult nvrtcGetPTXSize(nvrtcProgram prog, out int ptxSizeRet);

        [DllImport(RunTimeAPIDll, CharSet = CharSet.Ansi, EntryPoint = "nvrtcGetPTX")]
        public static extern nvrtcResult nvrtcGetPTX(nvrtcProgram prog, StringBuilder logx);

        [DllImport(RunTimeAPIDll, EntryPoint = "nvrtcDestroyProgram")]
        public static extern nvrtcResult nvrtcDestroyProgram(ref nvrtcProgram prog);

        public static string GetResultString(nvrtcResult result)
        {
            return Marshal.PtrToStringAnsi(nvrtcGetErrorString(result));
        }

        public static void CheckResult(nvrtcResult result)
        {
            if (result != nvrtcResult.NVRTC_SUCCESS)
            {
                var str = Marshal.PtrToStringAnsi(nvrtcGetErrorString(result));
                throw new NvRunTimeException(result, str);
            }
        }
    }
}