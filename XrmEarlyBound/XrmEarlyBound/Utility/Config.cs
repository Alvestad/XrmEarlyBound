using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XrmEarlyBound.Utility
{
    public class Config
    {
        public List<string> Entites { get; set; }
        public List<string> GlobalOptionSets { get; set; }
        public List<string> Actions { get; set; }
        public List<ListItem> GlobalOptionSetsDepedencies { get; set; }


        public void Save()
        {
            string filePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            string path = string.Format("{0}\\config.xml", System.IO.Path.GetDirectoryName(filePath));

            XmlSerializer SerializerObj = new XmlSerializer(typeof(Config));
            TextWriter WriteFileStream = new StreamWriter(path);
            SerializerObj.Serialize(WriteFileStream, this);
            WriteFileStream.Close();
        }

        public static Config LoadSettings()
        {
            string filePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            string path = string.Format("{0}\\config.xml", System.IO.Path.GetDirectoryName(filePath));

            XmlSerializer SerializerObj = new XmlSerializer(typeof(Config));
            if (System.IO.File.Exists(path))
            {
                FileStream ReadFileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                Config LoadedObj = (Config)SerializerObj.Deserialize(ReadFileStream);
                ReadFileStream.Close();

                if (LoadedObj.Actions == null)
                    LoadedObj.Actions = new List<string>();
                if (LoadedObj.Entites == null)
                    LoadedObj.Entites = new List<string>();
                if (LoadedObj.GlobalOptionSets == null)
                    LoadedObj.GlobalOptionSets = new List<string>();

                return LoadedObj;
            }
            else
            {
                throw new Exception("Could not find config file!");
            }
        }

        public void Delete()
        {
            string filePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            string path = string.Format("{0}\\config.xml", System.IO.Path.GetDirectoryName(filePath));
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }

    public class ListItem
    {
        public string Item1 { get; set; }
        public string Item2 { get; set; }
    }

}
