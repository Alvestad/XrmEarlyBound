using System;
using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;
using XrmEarlyBound.ExtensionServices;

public sealed class FilteringService : ICodeWriterFilterService
{
    private Dictionary<String, bool> GeneratedOptionSets { get; set; }

    public static Dictionary<Guid, OptionSetMetadataBase> OptionSetMetadata { get; set; }

    private string currentEntity = null;

    private XrmEarlyBound.Utility.Config config = null;


    bool GenereateForEntity(string entityLogicalName)
    {
        if (config.Entites.Contains(entityLogicalName))
        {
            return true;
        }
        return false;
    }

    public FilteringService(ICodeWriterFilterService defaultService)
    {
        this.DefaultService = defaultService;
        config = XrmEarlyBound.Utility.Config.LoadSettings();
        GeneratedOptionSets = new Dictionary<String, bool>();

    }

    static FilteringService()
    {
        OptionSetMetadata = new Dictionary<Guid, OptionSetMetadataBase>();
    }

    private ICodeWriterFilterService DefaultService { get; set; }

    bool ICodeWriterFilterService.GenerateAttribute(AttributeMetadata attributeMetadata, IServiceProvider services)
    {
        if (attributeMetadata.LogicalName.ToLowerInvariant() == "componentstate")
            return false;

        return this.DefaultService.GenerateAttribute(attributeMetadata, services);
    }

    bool ICodeWriterFilterService.GenerateEntity(EntityMetadata entityMetadata, IServiceProvider services)
    {
        if (!GenereateForEntity(entityMetadata.LogicalName)) { currentEntity = null; return false; }

        currentEntity = entityMetadata.LogicalName;

        return this.DefaultService.GenerateEntity(entityMetadata, services);
    }

    bool ICodeWriterFilterService.GenerateOption(OptionMetadata optionMetadata, IServiceProvider services)
    {
        return this.DefaultService.GenerateOption(optionMetadata, services);
    }

    bool ICodeWriterFilterService.GenerateOptionSet(OptionSetMetadataBase optionSetMetadata, IServiceProvider services)
    {
        if (optionSetMetadata.Name.ToLowerInvariant() == "componentstate")
            return false;

        if (optionSetMetadata.IsGlobal.HasValue && optionSetMetadata.IsGlobal.Value)
        {
            if (!GeneratedOptionSets.ContainsKey(optionSetMetadata.Name))
            {
                if (!config.GlobalOptionSets.Contains(optionSetMetadata.Name))
                {
                    return false;
                }
                GeneratedOptionSets[optionSetMetadata.Name] = true;
                return true;
            }
        }
        else
        {
            if (currentEntity == null)
                return false;

            return true;
        }
        return false;
    }

    bool ICodeWriterFilterService.GenerateRelationship(RelationshipMetadataBase relationshipMetadata, EntityMetadata otherEntityMetadata,
    IServiceProvider services)
    {
        return this.DefaultService.GenerateRelationship(relationshipMetadata, otherEntityMetadata, services);
    }

    bool ICodeWriterFilterService.GenerateServiceContext(IServiceProvider services)
    {
        return this.DefaultService.GenerateServiceContext(services);
    }
}

