using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Extra
{
    public static class DataManager
    {
        static BinaryFormatter formatter = new BinaryFormatter();

        public static Dictionary<string, List<KnowledgeBase>> Data = new Dictionary<string, List<KnowledgeBase>>();

        public static void Save(string filename)
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(filename));
            using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                formatter.Serialize(stream, Data);
        }

        public static void Load(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                Data = (Dictionary<string, List<KnowledgeBase>>)formatter.Deserialize(stream);

            Sort();
        }

        public static void Sort()
        {
            foreach (var key in Data.Keys)
                Data[key].Sort((a, b) => -a.InformationSum.CompareTo(b.InformationSum));
        }
    }
}
