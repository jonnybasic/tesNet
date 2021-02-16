using System;
using System.Collections.Generic;
using OptixCore.Library.Native;

namespace OptixCore.Library
{
    public class Group : ContainerNode, IContainerNode<ContainerNode>, INodeCollectionProvider<ContainerNode>
    {
        //NodeCollection<ContainerNode> mCollection;

        public Group(Context context) : base(context)
        {
            //mCollection = new NodeCollection<ContainerNode>(this);
            CheckError(Api.rtGroupCreate(context.InternalPtr, ref InternalPtr));
        }

        internal Group(Context context, IntPtr ptr) : base(context)
        {
            //mCollection = new NodeCollection<ContainerNode>(this);
            InternalPtr = ptr;
        }

        public override void Validate()
        {
            CheckError(Api.rtGroupValidate(InternalPtr));
        }

        public override void Destroy()
        {
            if (InternalPtr != IntPtr.Zero)
            {
                CheckError(Api.rtGroupDestroy(InternalPtr));
                InternalPtr = IntPtr.Zero;
            }
        }

        public override IntPtr ObjectPtr()
        {
            return InternalPtr;
        }

        public int ChildCount
        {
            get
            {
                CheckError(Api.rtGroupGetChildCount(InternalPtr, out var count));
                return (int)count;
            }
            set => CheckError(Api.rtGroupSetChildCount(InternalPtr, (uint)value));
        }

        public ContainerNode this[int index]
        {
            get => throw new NotImplementedException("Cannot get generic child");
            set => SetChild(index, value);
        }

        public Acceleration Acceleration
        {
            get
            {
                CheckError(Api.rtGroupGetAcceleration(InternalPtr, out var accel));

                return new Acceleration(mContext, accel);
            }
            set => CheckError(Api.rtGroupSetAcceleration(InternalPtr, value.InternalPtr));
        }

        public void SetAccel(Acceleration acceleration)
        {
            Api.rtGroupSetAcceleration(InternalPtr, acceleration.InternalPtr);
            acceleration.MarkAsDirty();
        }

        //public NodeCollection<ContainerNode> Collection => mCollection;
        public NodeCollection<ContainerNode> Collection => throw new NotImplementedException("Node collection not supported");

        public void AddChild(ContainerNode child)
        {
            //mCollection[ChildCount] = child;
            SetChild(ChildCount++, child);
        }

        public void RemoveChild(int index)
        {
            int count = ChildCount;
            if (index >= count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            // Replace to-be-removed child with last child. 
            CheckError(Api.rtGroupGetChild(InternalPtr, (uint)(count - 1), out IntPtr temp));
            CheckError(Api.rtGroupSetChild(InternalPtr, (uint)index, temp));
            ChildCount--;
        }

        public void AddChildren(IEnumerable<ContainerNode> children)
        {
            if (children == null)
                return;

            foreach (var child in children)
            {
                SetChild(ChildCount++, child);
            }
        }

        //private ContainerNode GetChild(int index)
        //{
        //    if (index >= ChildCount)
        //    {
        //        throw new ArgumentOutOfRangeException("index");
        //    }
        //
        //    CheckError(Api.rtGroupGetChild(InternalPtr, (uint)index, out var instance));
        //
        //    return new ContainerNode(mContext, instance);
        //}

        private void SetChild(int i, ContainerNode child)
        {
            CheckError(Api.rtGroupSetChild(InternalPtr, (uint)i, child.InternalPtr));
        }
    }
}