//Some metods inspired by https://github.com/daryllabar/DLaB.Xrm.XrmToolBoxTools/blob/master/DLaB.CrmSvcUtilExtensions/NamingService.cs
//Copyright(c) 2015 Daryl LaBar
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json;
using XrmEarlyBound.Utility.Extensions;

public sealed class NamingService : INamingService
{
    static List<Tuple<string, string>> languageReplacement = null;
    private INamingService DefaultNamingService { get; set; }

    private Dictionary<OptionSetMetadataBase, Dictionary<String, int>> OptionNames;

    public NamingService(INamingService namingService)
    {
        DefaultNamingService = namingService;
        OptionNames = new Dictionary<OptionSetMetadataBase,
            Dictionary<String, int>>();
    }

    public String GetNameForOptionSet(
        EntityMetadata entityMetadata, OptionSetMetadataBase optionSetMetadata,
        IServiceProvider services)
    {
        if (optionSetMetadata.IsGlobal.HasValue && !optionSetMetadata.IsGlobal.Value)
        {
            var attribute =
                (from a in entityMetadata.Attributes
                 where a.AttributeType == AttributeTypeCode.Picklist
                 && ((EnumAttributeMetadata)a).OptionSet.MetadataId
                     == optionSetMetadata.MetadataId
                 select a).FirstOrDefault();

            if (attribute != null)
            {
                return String.Format("{0}_{1}",
                    DefaultNamingService.GetNameForEntity(entityMetadata, services),
                    DefaultNamingService.GetNameForAttribute(
                        entityMetadata, attribute, services));
            }
        }

        return DefaultNamingService.GetNameForOptionSet(
            entityMetadata, optionSetMetadata, services);
    }


    private static string FindLableIfNotEnglish(OptionMetadata optionMetadata, string defaultName)
    {
        
        string value = null;
        var defaultNameIsInEnglish = IsLabelOK(defaultName);
        if (defaultNameIsInEnglish)
        {
            value = defaultName;
        }
        else
        {
            var localizedLabels = optionMetadata.Label.LocalizedLabels.FirstOrDefault();
           
            if (localizedLabels != null)
            {
                if (!string.IsNullOrWhiteSpace(localizedLabels.Label))
                    value = localizedLabels.Label.RemoveDiacritics(); //ReplaceLanguageChar(localizedLabels.Label, localizedLabels.LanguageCode);
                else
                    value = defaultName;
            }
        }

        return GetValidCSharpName(value);
    }

    public static string ReplaceLanguageChar(string value, int languageCode)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (languageReplacement == null)
        {
            var languageFilePath = $@"LangFiles\{languageCode}.json";
            if (System.IO.File.Exists(languageFilePath))
            {
                using (var sr = new StreamReader(languageFilePath))
                {
                    string json = sr.ReadToEnd();
                    languageReplacement = JsonConvert.DeserializeObject<List<Tuple<string, string>>>(json);
                }
            }
        }
        if (languageReplacement != null)
            foreach (var t in languageReplacement)
                value = value.Replace(t.Item1, t.Item2);

        return value;
    }

    public static string GetValidCSharpName(string name)
    {
        name = Regex.Replace(name, @"[^a-zA-Z0-9_]", string.Empty);
        if (name.Length > 0 && !char.IsLetter(name, 0))
        {
            name = "_" + name;
        }
        else if (name.Length == 0)
        {
            name = "_";
        }

        return name;
    }

    private static bool IsLabelOK(string label)
    {
        return !string.IsNullOrEmpty(label) && !label.Contains("UnknownLabel");
    }


    #region other INamingService Methods

    public String GetNameForAttribute(
        EntityMetadata entityMetadata, AttributeMetadata attributeMetadata,
        IServiceProvider services)
    {
        return DefaultNamingService.GetNameForAttribute(
            entityMetadata, attributeMetadata, services);
    }

    public String GetNameForEntity(
        EntityMetadata entityMetadata, IServiceProvider services)
    {
        return DefaultNamingService.GetNameForEntity(entityMetadata, services);
    }

    public String GetNameForEntitySet(
        EntityMetadata entityMetadata, IServiceProvider services)
    {
        return DefaultNamingService.GetNameForEntitySet(entityMetadata, services);
    }

    public String GetNameForMessagePair(
        SdkMessagePair messagePair, IServiceProvider services)
    {
        return DefaultNamingService.GetNameForMessagePair(messagePair, services);
    }


    public string GetNameForOption(OptionSetMetadataBase optionSetMetadata,
        OptionMetadata optionMetadata, IServiceProvider services)
    {

        var name = optionMetadata.Label.GetLocalOrDefaultText();

        if (string.IsNullOrWhiteSpace(name))
            name = DefaultNamingService.GetNameForOption(optionSetMetadata, optionMetadata, services);

        name = name.RemoveDiacritics();

        //var name = DefaultNamingService.GetNameForOption(optionSetMetadata,
        //    optionMetadata, services);

        name = FindLableIfNotEnglish(optionMetadata, name);

        Trace.TraceInformation(String.Format("The name of this option is {0}",
            name));
        name = EnsureValidIdentifier(name);
        name = EnsureUniqueOptionName(optionSetMetadata, name);
        return name;
    }

    private static String EnsureValidIdentifier(String name)
    {
        var pattern = @"^[A-Za-z_][A-Za-z0-9_]*$";
        if (!Regex.IsMatch(name, pattern))
        {
            name = String.Format("_{0}", name);
            Trace.TraceInformation(String.Format("Name of the option changed to {0}",
                name));
        }
        return name;
    }

    private String EnsureUniqueOptionName(OptionSetMetadataBase metadata, String name)
    {
        if (OptionNames.ContainsKey(metadata))
        {
            if (OptionNames[metadata].ContainsKey(name))
            {
                ++OptionNames[metadata][name];

                var newName = String.Format("{0}_{1}",
                    name, OptionNames[metadata][name]);

                Trace.TraceInformation(String.Format(
                    "The {0} OptionSet already contained a definition for {1}. Changed to {2}",
                    metadata.Name, name, newName));

                return EnsureUniqueOptionName(metadata, newName);
            }
        }
        else
        {
            OptionNames[metadata] = new Dictionary<string, int>();
        }

        OptionNames[metadata][name] = 1;

        return name;
    }

    public String GetNameForRelationship(
        EntityMetadata entityMetadata, RelationshipMetadataBase relationshipMetadata,
        EntityRole? reflexiveRole, IServiceProvider services)
    {
        return DefaultNamingService.GetNameForRelationship(
            entityMetadata, relationshipMetadata, reflexiveRole, services);
    }

    public String GetNameForRequestField(
        SdkMessageRequest request, SdkMessageRequestField requestField,
        IServiceProvider services)
    {
        return DefaultNamingService.GetNameForRequestField(
            request, requestField, services);
    }

    public String GetNameForResponseField(
        SdkMessageResponse response, SdkMessageResponseField responseField,
        IServiceProvider services)
    {
        return DefaultNamingService.GetNameForResponseField(
            response, responseField, services);
    }

    public String GetNameForServiceContext(IServiceProvider services)
    {
        return DefaultNamingService.GetNameForServiceContext(services);
    }
    #endregion
}


