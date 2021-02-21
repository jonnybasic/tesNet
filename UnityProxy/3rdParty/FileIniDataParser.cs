#if false
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IniParser
{

    public class FileIniDataParser
    {
        public IniData ReadData(System.IO.StreamReader stream)
        {
            throw new NotImplementedException();
        }

        public IniData ReadFile(string path)
        {
            throw new NotImplementedException();
        }

        public void WriteFile(string path, IniData data)
        {
            throw new NotImplementedException();
        }
    }
    
}

namespace IniParser.Model
{
    public class IniData : Dictionary<string, Dictionary<string, string>>
    {
        public SectionData Sections
        {
            get => throw new NotImplementedException();
        }
    }

    public struct SectionKey
    {
        public string KeyName;
    }

    public class SectionKeyDictionary : IEnumerable<SectionKey>
    {
        public void AddKey(SectionKey key)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public void RemoveKey(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<SectionKey> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class SectionData : IEnumerable<SectionData>
    {
        public string SectionName
        {
            get => throw new NotImplementedException();
        }

        public SectionKeyDictionary Keys
        {
            get => throw new NotImplementedException();
        }

        public bool ContainsSection(string name)
        {
            throw new NotImplementedException();
        }

        public void AddSection(string name)
        {
            throw new NotImplementedException();
        }

        public void RemoveSection(string name)
        {
            throw new NotImplementedException();
        }

        public SectionData GetSectionData(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<SectionData> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
#endif
