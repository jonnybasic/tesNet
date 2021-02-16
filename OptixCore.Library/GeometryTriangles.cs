using System;
using OptixCore.Library.Native;

namespace OptixCore.Library
{
    public class GeometryTriangles : VariableContainerNode
    {
        public GeometryTriangles(Context context) : base(context)
        {
            CheckError(Api.rtGeometryTrianglesCreate(context.InternalPtr, ref InternalPtr));
        }

        internal GeometryTriangles(Context context, IntPtr geom) : base(context)
        {
            InternalPtr = geom;
        }

        protected override Func<RTresult> ValidateAction => () => Api.rtGeometryTrianglesValidate(InternalPtr);
        protected override Func<RTresult> DestroyAction => () => Api.rtGeometryTrianglesDestroy(InternalPtr);

        protected override Func<int> GetVariableCount => () =>
        {
            CheckError(Api.rtGeometryTrianglesGetVariableCount(InternalPtr, out var count));
            return (int)count;
        };

        protected override Func<int, IntPtr> GetVariable => index =>
        {
            CheckError(Api.rtGeometryTrianglesGetVariable(InternalPtr, (uint)index, out var ptr));
            return ptr;
        };

        protected override Func<string, IntPtr> QueryVariable => name =>
        {
            CheckError(Api.rtGeometryTrianglesQueryVariable(InternalPtr, name, out var ptr));
            return ptr;
        };

        protected override Func<string, IntPtr> DeclareVariable => name =>
        {
            CheckError(Api.rtGeometryTrianglesDeclareVariable(InternalPtr, name, out var ptr));
            return ptr;
        };

        protected override Func<IntPtr, RTresult> RemoveVariable => ptr => Api.rtGeometryTrianglesRemoveVariable(InternalPtr, ptr);

        /// <summary>
        /// Set the number of primitives for the geometry
        /// </summary>
        public uint PrimitiveCount
        {
            get
            {
                CheckError(Api.rtGeometryTrianglesGetPrimitiveCount(InternalPtr, out var count));
                return count;
            }
            set => CheckError(Api.rtGeometryTrianglesSetPrimitiveCount(InternalPtr, value));
        }



        /// <summary>
        /// Set the program
        /// </summary>
        public OptixProgram AttributeProgram
        {
            get
            {
                CheckError(Api.rtGeometryTrianglesGetAttributeProgram(InternalPtr, out var program));
                return new OptixProgram(mContext, program);
            }
            set => CheckError(Api.rtGeometryTrianglesSetAttributeProgram(InternalPtr, value.InternalPtr));
        }

        public void SetTriangleIndices(ref OptixBuffer buffer, RTformat format = RTformat.RT_FORMAT_UNSIGNED_INT3)
        {
            SetTriangleIndices(ref buffer, 0, (uint)buffer.ElemSize, format);
        }

        public void SetTriangleIndices(ref OptixBuffer buffer, uint byteOffset, uint byteStride, RTformat format)
        {
            CheckError(Api.rtGeometryTrianglesSetTriangleIndices(InternalPtr, buffer.InternalPtr, byteOffset, byteStride, format));
        }

        public void SetVertices(uint count, ref OptixBuffer buffer, RTformat format = RTformat.RT_FORMAT_FLOAT3)
        {
            SetVertices(count, ref buffer, 0, (uint)buffer.ElemSize, format);
        }

        public void SetVertices(uint count, ref OptixBuffer buffer, uint byteOffset, uint byteStride, RTformat format)
        {
            CheckError(Api.rtGeometryTrianglesSetVertices(InternalPtr, count, buffer.InternalPtr, byteOffset, byteStride, format));
        }

        public void SetBuildFlags(RTgeometrybuildflags flags)
        {
            CheckError(Api.rtGeometryTrianglesSetBuildFlags(InternalPtr, flags));
        }
    }
}