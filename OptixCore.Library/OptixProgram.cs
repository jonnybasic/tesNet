using System;
using OptixCore.Library.Native;

namespace OptixCore.Library
{
    public class OptixProgram : VariableContainerNode
    {
        public OptixProgram(Context context, string fileName, string programName) : base(context)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(programName))
            {
                throw new OptixException("Program Error: Null or Empty filename or program name");
            }
            CheckError(Api.rtProgramCreateFromPTXFile(context.InternalPtr, fileName, programName,out InternalPtr));
        }

        internal OptixProgram(Context context) : base(context)
        { }

        public static OptixProgram CreateFromPTXString(Context context, string ptx, string programName)
        {
            if (string.IsNullOrWhiteSpace(ptx))
            {
                throw new ArgumentException("A valid PTX String is required", "filename");
            }
            else if (string.IsNullOrWhiteSpace(programName))
            {
                throw new ArgumentException("A valid program name is required", "programName");
            }
            OptixProgram result = new OptixProgram(context);
            result.CheckError(Api.rtProgramCreateFromPTXString(context.InternalPtr, ptx, programName, out result.InternalPtr));
            return result;
        }

        public static OptixProgram CreateFromPTXFile(Context context, string filename, string programName)
        {
            if (string.IsNullOrWhiteSpace(filename) || System.IO.File.Exists(filename))
            {
                throw new ArgumentException("A valid PTX File is required", "filename");
            }
            else if (string.IsNullOrWhiteSpace(programName))
            {
                throw new ArgumentException("A valid program name is required", "programName");
            }
            OptixProgram result = new OptixProgram(context);
            result.CheckError(Api.rtProgramCreateFromPTXFile(context.InternalPtr, filename, programName, out result.InternalPtr));
            return result;
        }

        public OptixProgram(Context context, IntPtr program) : base(context)
        {
            InternalPtr = program;
        }

        public void SetProgram(string name, OptixProgram @object)
        {
            var v = this[name].InternalPtr;
            var pr = @object.InternalPtr;
            CheckError(Api.rtVariableSetObject(v, pr));
        }

        protected override Func<RTresult> ValidateAction => () => Api.rtProgramValidate(InternalPtr);
        protected override Func<RTresult> DestroyAction => () => Api.rtProgramDestroy(InternalPtr);
        protected override Func<int, IntPtr> GetVariable => index =>
        {
            CheckError(Api.rtProgramGetVariable(InternalPtr, (uint)index, out var ptr));
            return ptr;
        };
        protected override Func<string, IntPtr> QueryVariable => name =>
        {
            CheckError(Api.rtProgramQueryVariable(InternalPtr, name, out var ptr));
            return ptr;
        };
        protected override Func<string, IntPtr> DeclareVariable => name =>
        {
            CheckError(Api.rtProgramDeclareVariable(InternalPtr, name, out var ptr));
            return ptr;
        };
        protected override Func<IntPtr, RTresult> RemoveVariable => ptr => Api.rtProgramRemoveVariable(InternalPtr, ptr);
        protected override Func<int> GetVariableCount => () =>
        {
            CheckError(Api.rtProgramGetVariableCount(InternalPtr, out var count));
            return (int)count;
        };

    }
}