using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmEarlyBound.Utility
{
    public class XrmMetaData
    {
        Dictionary<string, string[]> attributeStore = new Dictionary<string, string[]>();

        private readonly IOrganizationService _client;
        public XrmMetaData(IOrganizationService client)
        {
            _client = client;
        }
        public List<string> GetEntities()
        {
            RetrieveAllEntitiesRequest raer = new RetrieveAllEntitiesRequest
            {
                EntityFilters = Microsoft.Xrm.Sdk.Metadata.EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveAllEntitiesResponse)_client.Execute(raer);

            var result = new List<string>();

            foreach (var r in response.EntityMetadata)
            {
                result.Add(r.LogicalName);
            }

            return result;
        }

        public List<string> GetGlobalOptionSets()
        {
            var result = new List<string>();

            RetrieveAllOptionSetsRequest raor = new RetrieveAllOptionSetsRequest
            {
                RetrieveAsIfPublished = true,
            };

            var response = (RetrieveAllOptionSetsResponse)_client.Execute(raor);

            foreach (var r in response.OptionSetMetadata)
            {
                if (r.IsGlobal.HasValue && r.IsGlobal.Value)
                    result.Add(r.Name);
            }

            return result;
        }

        public List<string> GetActions()
        {
            var result = new List<string>();

            var context = new Microsoft.Xrm.Sdk.Client.OrganizationServiceContext(_client);

            var actions = (from w in context.CreateQuery("workflow")
                           where ((OptionSetValue)w["category"]).Value == 3
                           select new
                           {
                               Name = (string)w["uniquename"],
                           }).Distinct().ToList();

            foreach (var a in actions)
            {
                result.Add(a.Name);
            }

            return result;
        }

    }
}
