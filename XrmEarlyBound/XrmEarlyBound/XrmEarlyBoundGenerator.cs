using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmEarlyBound
{
    public class XrmEarlyBoundGenerator
    {
        public event Message ShowMessage;
        public delegate void Message(string message);
        private void Log(string message)
        {
            if (ShowMessage != null)
                ShowMessage.Invoke(message);
        }

        public List<string> GetActionsList(string connectionstring)
        {
            var client = Connection.CrmConnection.GetClientByConnectionString(connectionstring);
            var metadata = new Utility.XrmMetaData(client);
            return metadata.GetActions();
        }

        public void GenerateActionsMetaDataStruct(string connectionstring, string filepath)
        {
            var client = Connection.CrmConnection.GetClientByConnectionString(connectionstring);
            var metadata = new Utility.XrmMetaData(client);
            var actions = metadata.GetActions();

            //string filePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            //var outpath = Path.GetFullPath($"{System.IO.Path.GetDirectoryName(filePath)}\\{filepath}");
            var outpath = GetPath(filepath);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outpath))
            {
                file.WriteLine("public struct XrmActions");
                file.WriteLine("{");
                foreach (var x in actions)
                {
                    file.WriteLine($"\tpublic static readonly string {x} = \"{x}\";");
                }
                file.WriteLine("}");
            }

            Log($"Struct generated to {outpath}");
        }

        public List<string> GetEntitiesList(string connectionstring)
        {
            var client = Connection.CrmConnection.GetClientByConnectionString(connectionstring);
            var metadata = new Utility.XrmMetaData(client);
            return metadata.GetEntities();
        }

        public void GenerateEntitiesMetaDataStruct(string connectionstring, string filepath)
        {
            var client = Connection.CrmConnection.GetClientByConnectionString(connectionstring);
            var metadata = new Utility.XrmMetaData(client);
            var entites = metadata.GetEntities();


            //string filePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            //var outpath = Path.GetFullPath($"{System.IO.Path.GetDirectoryName(filePath)}\\{filepath}");
            var outpath = GetPath(filepath);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outpath))
            {
                file.WriteLine("public struct XrmEntities");
                file.WriteLine("{");
                foreach (var x in entites)
                {
                    file.WriteLine($"\tpublic static readonly string {x} = \"{x}\";");
                }
                file.WriteLine("}");
            }
            Log($"Struct generated to {outpath}");
        }

        public List<string> GetOptionSetList(string connectionstring)
        {
            var client = Connection.CrmConnection.GetClientByConnectionString(connectionstring);
            var metadata = new Utility.XrmMetaData(client);
            return metadata.GetGlobalOptionSets();
        }

        public void GenerateOptionSetMetaDataStruct(string connectionstring, string filepath)
        {
            var client = Connection.CrmConnection.GetClientByConnectionString(connectionstring);
            var metadata = new Utility.XrmMetaData(client);
            var optionsets = metadata.GetGlobalOptionSets();

            //string filePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            //var outpath = Path.GetFullPath($"{System.IO.Path.GetDirectoryName(filePath)}\\{filepath}");
            var outpath = GetPath(filepath);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outpath))
            {
                file.WriteLine("public struct XrmOptionSets");
                file.WriteLine("{");
                foreach (var x in optionsets)
                {
                    file.WriteLine($"\tpublic static readonly string {x} = \"{x}\";");
                }
                file.WriteLine("}");
            }
            Log($"Struct generated to {outpath}");
        }

        public void GenerateEarlyBoundEntities(string connectionstring, List<string> entities, List<string> globalOptionSets, List<string> actions, string @namespace, string filepath)
        {
            GenerateEarlyBoundEntities(connectionstring, entities, globalOptionSets, actions, @namespace, filepath, null);
        }
        public void GenerateEarlyBoundEntities(string connectionstring, List<string> entities, List<string> globalOptionSets, List<string> actions, string @namespace, string filepath, string servicecontextname)
        {
            if (string.IsNullOrWhiteSpace(servicecontextname))
                servicecontextname = null;

            var intepret = Utility.InterpretConnectionstring.Interpret(connectionstring);

            if (actions != null)
                actions = actions.Select(x => x.ToLower()).ToList();

            var service = Connection.CrmConnection.GetClientByConnectionString(connectionstring);
            var globalOptionSetDepedencies = new List<XrmEarlyBound.Utility.ListItem>(); //new Dictionary<string, string>();

            foreach (var gos in globalOptionSets)
            {
                var response = (RetrieveOptionSetResponse)service.Execute(new RetrieveOptionSetRequest { Name = gos });
                RetrieveDependentComponentsRequest dependencyRequest = new RetrieveDependentComponentsRequest
                {
                    ObjectId = response.OptionSetMetadata.MetadataId.Value,
                    ComponentType = 9
                };

                var result = (RetrieveDependentComponentsResponse)service.Execute(dependencyRequest);

                foreach(var dep in result.EntityCollection.Entities)
                {
                    var dependentcomponentobjectid = (Guid)dep.Attributes["dependentcomponentobjectid"];
                    var _result = (RetrieveAttributeResponse)service.Execute(new RetrieveAttributeRequest { MetadataId = dependentcomponentobjectid });

                    if (_result.AttributeMetadata != null && _result.AttributeMetadata.EntityLogicalName != null && _result.AttributeMetadata.LogicalName != null)
                    {
                        globalOptionSetDepedencies.Add(new Utility.ListItem
                        {
                            Item1 = $"{_result.AttributeMetadata.EntityLogicalName.ToLower()}_{_result.AttributeMetadata.LogicalName.ToLower()}",
                            Item2 = gos
                        });
                    }
                }
            }

            var config = new Utility.Config
            {
                Entites = entities,
                Actions = actions,
                GlobalOptionSets = globalOptionSets,
                GlobalOptionSetsDepedencies = globalOptionSetDepedencies
            };

            config.Save();

            var hasActions = actions != null ? actions.Count > 0 : false;

            var outpath = GetPath(filepath);

            Utility.RunSvcProcess.Run(intepret.Url, intepret.UserName, intepret.Domain, intepret.Password, @namespace, filepath, hasActions, servicecontextname, ShowMessage);

            config.Delete();
        }

        private string GetPath(string path)
        {
            if (System.IO.Path.IsPathRooted(path))
                return path;
            else
            {
                string filePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
                var outpath = Path.GetFullPath($"{System.IO.Path.GetDirectoryName(filePath)}\\{path}");
                return outpath;
            }
        }
    }
}
