using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unity.Jobs
{
    public interface IJob
    { }

    public interface IJobParallelFor
    { }

    public class AsyncJob<TResult> : IJob
    {
        internal Task<TResult> task;
        internal AsyncJob(Task<TResult> t)
        {
            task = t;
        }
        internal AsyncJob(Func<TResult> f)
        {
            task = Task.Run(f);
        }

        public TResult Result
        { get => task.Result; }

        public bool IsDone
        { get => task.IsCompleted; }

        public Func<Task<TResult>, TResult> Completed
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }

    public static class JobA
    {
        public static int Idx(int row, int col, int colWidth)
        {
            return row * colWidth + col;
        }
    }

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
