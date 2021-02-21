using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Localization.Tables;
using Unity.Jobs;
using System.Threading;

namespace UnityEngine.Localization
{
    public class Locale
    {
        public string name;
    }

    public class LocalizedString
    {
        public string GetLocalizedString()
        {
            throw new NotImplementedException();
        }
    }
}

namespace UnityEngine.Localization.Settings
{
    public static class LocalizationSettings
    {
        public static StringDatabase StringDatabase;

        public static Locale SelectedLocale;
    }
}

namespace Unity.Jobs
{
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
}

namespace UnityEngine.Localization.Tables
{
    public class StringTable
    {
        public LocalizedString GetEntry(string id)
        {
            throw new NotImplementedException();
        }
    }

    public class StringDatabase
    {
        public AsyncJob<StringTable> GetTableAsync(string name)
        {
            return new AsyncJob<StringTable>(() => new StringTable());
        }
    }
}