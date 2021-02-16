using System;
using System.Runtime.InteropServices;
using OptixCore.Library.Native;

namespace OptixCore.Library
{
    /// <summary>
    /// Defines the Matrix layout used by Optix: Row major or Column major
    /// </summary>
    public enum MatrixLayout
    {
        ColumnMajor = 0,
        RowMajor = 1
    };

    /// <summary>
    /// Type of Bounding Volume Hierarchy algorithm that will be used for <see cref="OptixCore.Library.Acceleration">Acceleration</see> construction.
    /// </summary>
    public enum AccelBuilder
    {
        /// <summary>
        ///  Specifies that no acceleration structure is explicitly built. Traversal linearly loops
        /// through the list of primitives to intersect. This can be useful e.g. for higher level groups with only
        /// few children, where managing a more complex structure introduces unnecessary overhead.
        /// </summary>
        NoAccel,

        /// <summary>
        /// A standard bounding volume hierarchy, useful for most types of graph levels and geometry.
        /// Medium build speed, good ray tracing performance.
        /// </summary>
        Bvh,

        /// <summary>
        /// A high quality BVH variant for maximum ray tracing performance. Slower build speed and
        /// slightly higher memory footprint than <see cref="AccelBuilder.Bvh">Bvh</see>
        /// </summary>
        Sbvh,

        /// <summary>
        /// High quality similar to <see cref="AccelBuilder.Sbvh">Sbvh</see> but with fast build performance. The Trbvh builder uses about
        /// 2.5 times the size of the final BVH for scratch space. A CPU-based Trbvh builder that does not
        /// have the memory constraints is available. OptiX includes an optional automatic fallback to the
        /// CPU version when out of GPU memory
        /// </summary>
        Trbvh,

        /// <summary>
        /// Very fast GPU based accelertion good for animated scenes.<br/>
        /// Traversers: <see cref="OptixCore.Library.AccelTraverser">Bvh</see> or <see cref="OptixCore.Library.AccelTraverser">BvhCompact</see>.
        /// </summary>
        [Obsolete("version 4.0")]
        Lbvh,

        /// <summary>
        /// Uses a fast construction scheme to produce a medium quality bounding volume hierarchy.<br/>
        /// Traversers: <see cref="OptixCore.Library.AccelTraverser.Bvh">Bvh</see> or <see cref="OptixCore.Library.AccelTraverser.Bvh">BvhCompact</see>.
        /// </summary>
        [Obsolete("version 4.0")]
        MedianBvh,

        /// <summary>
        /// Constructs a high quality kd-tree and is comparable to Sbvh in traversal perfromance.<br/>
        /// Has the highest memory footprint and construction time.<br/>
        /// Requires certain properties to be set, such as: <see cref="OptixCore.Library.Acceleration.VertexBufferName">VertexBufferName</see>
        /// and <see cref="OptixCore.Library.Acceleration.IndexBufferName">IndexBufferName</see>.<br/>
        /// Traversers: <see cref="OptixCore.Library.AccelTraverser">Bvh</see> or <see cref="OptixCore.Library.AccelTraverser">BvhCompact</see>.
        /// </summary>
        [Obsolete("version 4.0")]
        TriangleKdTree
    };

    /// <summary>
    /// Type of Bounding Volume Hierarchy algorithm that will be used for <see cref="OptixCore.Library.Acceleration">Acceleration</see> traversal.
    /// </summary>
    [Obsolete("Obsolete after verseion 4.0")]
    public enum AccelTraverser
    {
        /// <summary>
        /// No Acceleration structure. Linearly traverses primitives in the scene.<br/>
        /// Builders: <see cref="AccelBuilder">NoAccel</see>.
        /// </summary>
        NoAccel,

        /// <summary>
        /// Uses a classic Bounding Volume Hierarchy traversal.<br/>
        /// Builders: <see cref="OptixCore.Library.AccelBuilder">Lbvh</see>, <see cref="OptixCore.Library.AccelBuilder">Bvh</see>
        /// <see cref="OptixCore.Library.AccelBuilder">MedianBvh</see>, or <see cref="OptixCore.Library.AccelBuilder">Sbvh</see>.<br/>
        /// </summary>
        Bvh,

        /// <summary>
        /// Compresses bvh data by a factor of 4 before uploading to the GPU. Acceleration data is decompressed on the fly during traversal.<br/>
        /// Useful for large static scenes that require more than a gigabyte of memory, and to minimize page misses for virtual memory.<br/>
        /// Builders: <see cref="OptixCore.Library.AccelBuilder">Lbvh</see>, <see cref="OptixCore.Library.AccelBuilder">Bvh</see>
        /// <see cref="OptixCore.Library.AccelBuilder">MedianBvh</see>, or <see cref="OptixCore.Library.AccelBuilder">Sbvh</see>.<br/>
        /// </summary>
        BvhCompact,

        /// <summary>
        /// Uses a kd-tree traverser.<br/>
        /// Builders: <see cref="OptixCore.Library.AccelBuilder">TriangleKdTree</see>.
        /// </summary>
        KdTree
    };

    public class Acceleration : OptixNode
    {
        AccelBuilder mBuilder;
        public Acceleration(Context context, AccelBuilder mBuilder) : base(context)
        {
            CheckError(Api.rtAccelerationCreate(context.InternalPtr, ref InternalPtr));
            gch = GCHandle.Alloc(InternalPtr, GCHandleType.Pinned);

            Builder = mBuilder;

            MarkAsDirty();
        }

        internal Acceleration(Context ctx, IntPtr acc) : base(ctx)
        {
            InternalPtr = acc;
            IntPtr str = IntPtr.Zero;
            CheckError(Api.rtAccelerationGetBuilder(InternalPtr, ref str));
            String temp = Marshal.PtrToStringAnsi(str);
            if (temp == "RtcBvh")
            {
                mBuilder = AccelBuilder.Trbvh;
            }
            else
            {
                mBuilder = AccelBuilder.NoAccel;
            }
            //mBuilder = (AccelBuilder)Enum.Parse(mBuilder.GetType(), Marshal.PtrToStringAnsi(str));
        }

        public override void Validate()
        {
            CheckError(Api.rtAccelerationValidate(InternalPtr));
        }

        public override void Destroy()
        {
            if (InternalPtr != IntPtr.Zero)
                CheckError(Api.rtAccelerationDestroy(InternalPtr));

            InternalPtr = IntPtr.Zero;
            gch.Free();
        }

        public void MarkAsDirty()
        {
            CheckError(Api.rtAccelerationMarkDirty(InternalPtr));
        }

        public bool IsDirty()
        {
            CheckError(Api.rtAccelerationIsDirty(InternalPtr, out var dirty));
            return dirty == 1;
        }

        public BufferStream Data
        {
            get
            {
                uint size = 0u;
                CheckError(Api.rtAccelerationGetDataSize(InternalPtr, ref size));

                var data = IntPtr.Zero;
                CheckError(Api.rtAccelerationGetData(InternalPtr, data));

                return new BufferStream(data, size, true, false, true);
            }
            set => CheckError(Api.rtAccelerationSetData(InternalPtr, value.DataPointer, (uint)value.Length));
        }

        /// <summary>
        /// Sets the 'refit' property on the acceleration structure. Refit tells Optix that only small geometry changes have been made,
        /// and to NOT perform a full rebuild of the hierarchy. Only a valid property on Bvh built acceleration structures.
        /// </summary>
        /// <remarks>
        /// Available in: Trbvh, Bvh If set to "1", the builder will only readjust the node bounds of the
        /// bounding volume hierarchy instead of constructing it from scratch.Refit is only effective if there is
        /// an initial BVH already in place, and the underlying geometry has undergone relatively modest
        /// deformation.In this case, the builder delivers a very fast BVH update without sacrificing too much
        /// ray tracing performance.The default is "0".
        /// </remarks>
        public bool Refit
        {

            get
            {
                switch (mBuilder)
                {
                    case AccelBuilder.Trbvh:
                    case AccelBuilder.Bvh:
                        IntPtr str = IntPtr.Zero;
                        CheckError(Api.rtAccelerationGetProperty(InternalPtr, "refit", ref str));
                        return int.Parse(Marshal.PtrToStringAnsi(str)) == 1;
                    default:
                        return false;
                }
            }
            set
            {
                switch (mBuilder)
                {
                    case AccelBuilder.Trbvh:
                    case AccelBuilder.Bvh:
                        CheckError(Api.rtAccelerationSetProperty(InternalPtr, "refit", value ? "1" : "0"));
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Sets 'vertex_buffer_name' property of the acceleration structure. This notifies Sbvh and TriangkeKdTree builders to look at the
        /// vertex buffer assigned to 'vertex_buffer_name' in order to build the hierarchy.
        /// This must match the name of the variable the buffer is attached to.
        /// Property only valid for Sbvh or TriangleKdTree built acceleration structures.
        /// </summary>
        /// <remarks>
        /// Available in: Trbvh, Sbvh The name of the buffer variable holding triangle
        /// vertex data.Each vertex consists of 3 floats.The default is "vertex_buffer"
        /// </remarks>
        public string VertexBufferName
        {
            get
            {
                switch (mBuilder)
                {
                    case AccelBuilder.Trbvh:
                    case AccelBuilder.Sbvh:
                        IntPtr str = IntPtr.Zero;
                        CheckError(Api.rtAccelerationGetProperty(InternalPtr, "vertex_buffer_name", ref str));
                        return Marshal.PtrToStringAnsi(str);
                    default:
                        return "vertex_buffer";
                }
            }
            set
            {
                switch (mBuilder)
                {
                    case AccelBuilder.Trbvh:
                    case AccelBuilder.Sbvh:
                        CheckError(Api.rtAccelerationSetProperty(InternalPtr, "vertex_buffer_name", value));
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Sets 'vertex_buffer_stride' property in bytes of the acceleration structure. This defines the offset between two vertices. Default is assumed to be 0.
        /// Property only valid for Sbvh or TriangleKdTree built acceleration structures.
        /// </summary>
        /// <remarks>
        ///  Available in: Trbvh, Sbvh The offset between two vertices in the vertex
        /// buffer, given in bytes.The default value is "0", which assumes the vertices are tightly packed
        /// </remarks>
        public int VertexBufferStride
        {
            get
            {
                switch (mBuilder)
                {
                    case AccelBuilder.Trbvh:
                    case AccelBuilder.Sbvh:
                        IntPtr str = IntPtr.Zero;
                        CheckError(Api.rtAccelerationGetProperty(InternalPtr, "vertex_buffer_stride", ref str));
                        return int.Parse(Marshal.PtrToStringAnsi(str));
                    default:
                        return 0;
                }
            }
            set
            {
                switch (mBuilder)
                {
                    case AccelBuilder.Trbvh:
                    case AccelBuilder.Sbvh:
                        CheckError(Api.rtAccelerationSetProperty(InternalPtr, "vertex_buffer_stride", value.ToString()));
                        break;
                    default:
                        break;
                }
            }
        }


        /// <summary>
        /// Sets 'index_buffer_name' property of the acceleration structure. This notifies Sbvh and TriangkeKdTree builders to look at the
        /// index buffer assigned to 'index_buffer_name' in order to build the hierarchy.
        /// This must match the name of the variable the buffer is attached to.
        /// Property only valid for Sbvh or TriangleKdTree built acceleration structures.
        /// </summary>
        /// <remarks>
        /// Available in: Trbvh, Sbvh The name of the buffer variable holding vertex
        /// index data.The entries in this buffer are indices of type int, where each index refers to one entry
        /// in the vertex buffer.A sequence of three indices represents one triangle. If no index buffer is
        /// given, the vertices in the vertex buffer are assumed to be a list of triangles, i.e.every 3 vertices in
        /// a row form a triangle.The default is "index_buffer".
        /// </remarks>
        public string IndexBufferName
        {
            get
            {
                switch (mBuilder)
                {
                    case AccelBuilder.Trbvh:
                    case AccelBuilder.Sbvh:
                        IntPtr str = IntPtr.Zero;
                        CheckError(Api.rtAccelerationGetProperty(InternalPtr, "index_buffer_name", ref str));
                        return Marshal.PtrToStringAnsi(str);
                    default:
                        return "index_buffer";
                }
            }
            set
            {
                switch (mBuilder)
                {
                    case AccelBuilder.Trbvh:
                    case AccelBuilder.Sbvh:
                        CheckError(Api.rtAccelerationSetProperty(InternalPtr, "index_buffer_name", value));
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Sets 'index_buffer_stride' property in bytes of the acceleration structure. This defines the offset between two indices. Default is assumed to be 0.
        /// Property only valid for Sbvh or TriangleKdTree built acceleration structures.
        /// </summary>
        /// <remarks>
        /// Available in: Trbvh, Sbvh The offset between two indices in the index buffer,
        /// given in bytes.The default value is "0", which assumes the indices are tightly packed.
        /// </remarks>
        public int IndexBufferStride
        {
            get
            {
                switch (mBuilder)
                {
                    case AccelBuilder.Trbvh:
                    case AccelBuilder.Sbvh:
                        IntPtr str = IntPtr.Zero;
                        CheckError(Api.rtAccelerationGetProperty(InternalPtr, "index_buffer_stride", ref str));
                        return int.Parse(Marshal.PtrToStringAnsi(str));
                    default:
                        return 0;
                }
            }
            set
            {
                switch (mBuilder)
                {
                    case AccelBuilder.Trbvh:
                    case AccelBuilder.Sbvh:
                        CheckError(Api.rtAccelerationSetProperty(InternalPtr, "index_buffer_stride", value.ToString()));
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Sets 'chunk_size' property of the acceleration structure.
        /// </summary>
        /// <remarks>
        /// Available in: Trbvh Number of bytes to be used for a partitioned acceleration
        /// structure build. If no chunk size is set, or set to "0", the chunk size is chosen automatically. If set
        /// to "-1", the chunk size is unlimited. The minimum chunk size is 64MB. Please note that specifying
        /// a small chunk size reduces the peak-memory footprint of the Trbvh but can result in slower
        /// rendering performance.
        /// </remarks>
        public int ChunkSize
        {

            get
            {
                if (mBuilder == AccelBuilder.Trbvh)
                {
                    var str = IntPtr.Zero;
                    CheckError(Api.rtAccelerationGetProperty(InternalPtr, "chunk_size", ref str));
                    return int.Parse(Marshal.PtrToStringAnsi(str));
                }
                return 0;
            }

            set
            {
                if (mBuilder == AccelBuilder.Trbvh)
                {
                    CheckError(Api.rtAccelerationSetProperty(InternalPtr, "chunk_size", value.ToString()));
                }
            }
        }

        /// <summary>
        /// Sets 'motion_steps' property of the acceleration structure.
        /// </summary>
        /// <remarks>
        ///  Available in: Trbvh Number of motion steps to build into an acceleration structure
        /// that contains motion geometry or motion transforms. Ignored for acceleration structures built over
        /// static nodes. Gives a tradeoff between device memory and time: if the input geometry or
        /// transforms have many motion steps, then increasing the motion steps in the acceleration
        /// structure may result in faster traversal, at the cost of linear increase in memory usage. Default 2,
        /// and clamped >=1.
        /// </remarks>
        public int MotionSteps
        {

            get
            {
                if (mBuilder == AccelBuilder.Trbvh)
                {
                    var str = IntPtr.Zero;
                    CheckError(Api.rtAccelerationGetProperty(InternalPtr, "motion_steps", ref str));
                    return int.Parse(Marshal.PtrToStringAnsi(str));
                }
                return 2;
            }

            set
            {
                if (mBuilder == AccelBuilder.Trbvh)
                {
                    CheckError(Api.rtAccelerationSetProperty(InternalPtr, "motion_steps", value.ToString()));
                }
            }
        }

        public AccelBuilder Builder
        {
            get => mBuilder;
            set
            {
                CheckError(Api.rtAccelerationSetBuilder(InternalPtr, value.ToString()));
                mBuilder = value;
            }
        }
    }
}