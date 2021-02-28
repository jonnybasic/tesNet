using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Localization.Tables;
using Unity.Jobs;
using System.Threading;

namespace UnityEngine
{
    public static class Parser
    {
        public static int ParseInt(string value)
        {
            throw new NotImplementedException();
        }
    }

}

namespace UnityEngine.Localization
{

    public class Locale
    {
        internal CultureInfo cultureInfo;

        public string name
        { get => cultureInfo.Name; }

        internal Locale(CultureInfo ci)
        {
            cultureInfo = ci;
        }

        public static implicit operator Locale(CultureInfo ci)
            => new Locale(ci);
    }

    public class LocalizedString
    {
        internal StringInfo stringInfo;

        internal LocalizedString(StringInfo si)
        {
            stringInfo = si;
        }

        public static implicit operator LocalizedString(StringInfo si)
            => new LocalizedString(si);

        public string GetLocalizedString()
        {
            return stringInfo.String;
        }
    }
}

namespace UnityEngine.Localization.Tables
{
    public class Table
    {
        private readonly Dictionary<String, int> columns = new Dictionary<string, int>();
        private readonly List<String> tableRows;

        public readonly int RowCount;

        public Table(string[] rows)
        {
            throw new NotImplementedException();
            // first row is header?
            if (rows.Length > 0)
            {
                var headers = rows[0].Split(' ');
                for (int i = 0; i < headers.Length; ++i)
                {
                    columns[headers[i]] = i;
                }
            }
            if (rows.Length > 1)
            {
                tableRows = new List<string>(rows.Skip(1));
            }
        }

        public string[] GetRow(int i)
        {
            throw new NotImplementedException();
        }

        public bool HasValue(string key)
        {
            throw new NotImplementedException();
        }

        public string GetValue(string textColumn, string key)
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

        public static class AvailableLocales
        {
            public static IList<Locale> Locales
            {
                get
                {
                    return new Locale[] { CultureInfo.CurrentCulture };
                }
            }
        }

        public static AsyncJob<Locale> SelectedLocaleAsync
        {
            get => new AsyncJob<Locale>(() => CultureInfo.CurrentCulture);
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