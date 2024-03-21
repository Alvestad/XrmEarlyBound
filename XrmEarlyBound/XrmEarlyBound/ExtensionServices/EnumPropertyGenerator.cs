using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using XrmEarlyBound.Utility.Extensions;

namespace XrmEarlyBound.ExtensionServices
{
    class EnumPropertyGenerator : ICustomizeCodeDomService
    {
        public Utility.Config config { get; set; }

      

        public void CustomizeCodeDom(CodeCompileUnit codeUnit, IServiceProvider services)
        {
            config = Utility.Config.LoadSettings();

            var types = codeUnit.Namespaces[0].Types;
            var namespaceName = codeUnit.Namespaces[0].Name;
            foreach (CodeTypeDeclaration type in types)
            {
                if (!type.IsClass || type.IsContextType()) { continue; }
                var logicalName = type.GetFieldInitalizedValue("EntityLogicalName");
                if (logicalName == null)
                    return;

                var propertiesToEdit = new List<Tuple<CodeMemberProperty, string>>();
                foreach (var member in type.Members)
                {
                    var property = member as CodeMemberProperty;
                    if (SkipProperty(property, type))
                    {
                        continue;
                    }

                    if (!IsOptionSetProperty(property))
                        continue;
                   
                    if (property == null || property.Name == null)
                        continue;

                    if (!config.GlobalOptionSetsDepedencies.Where(x => x.Item1 == $"{logicalName}_{property.Name.ToLower()}").Any())
                        continue;

                    propertiesToEdit.Add(new Tuple<CodeMemberProperty, string>(property,
                        config.GlobalOptionSetsDepedencies.Where(x => x.Item1 == $"{logicalName}_{property.Name.ToLower()}").First().Item2));


                }

                foreach (var enumProp in propertiesToEdit.Where(p => p != null && p.Item1 != null))
                {
                    type.Members.Remove(enumProp.Item1);


                    enumProp.Item1.SetStatements.Clear();
                    enumProp.Item1.GetStatements.Clear();


                    enumProp.Item1.Type = new CodeTypeReference($"System.Nullable<{namespaceName}.{enumProp.Item2.ToLower()}>");

                    var codeGetAssigment = new CodeSnippetExpression(
                        $"Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>(\"{enumProp.Item1.Name.ToLower()}\");" + Environment.NewLine +
                        $"                if ((optionSet != null))" + Environment.NewLine +
                        "                {" + Environment.NewLine +
                        $"                    return (({namespaceName}.{enumProp.Item2.ToLower()})(System.Enum.ToObject(typeof({namespaceName}.{enumProp.Item2.ToLower()}), optionSet.Value)));" + Environment.NewLine +
                        "                }" + Environment.NewLine +
                        "                else" + Environment.NewLine +
                        "                {" + Environment.NewLine +
                        "                    return null;" + Environment.NewLine +
                        "                }");

                    var codeSetAssigment = new CodeSnippetExpression(
                        $"this.OnPropertyChanging(\"{enumProp.Item1.Name}\");" + Environment.NewLine +
                        $"                if ((value == null))" + Environment.NewLine +
                        "                {" + Environment.NewLine +
                        $"                    this.SetAttributeValue(\"{enumProp.Item1.Name.ToLower()}\", null);" + Environment.NewLine +
                        "                }" + Environment.NewLine +
                        "                else" + Environment.NewLine +
                        "                {" + Environment.NewLine +
                        $"                    this.SetAttributeValue(\"{enumProp.Item1.Name.ToLower()}\", new Microsoft.Xrm.Sdk.OptionSetValue(((int)(value))));;" + Environment.NewLine +
                        "                }" + Environment.NewLine +
                        $"                this.OnPropertyChanged(\"{enumProp.Item1.Name}\");"
                        );


                    enumProp.Item1.GetStatements.Add(codeGetAssigment);
                    enumProp.Item1.SetStatements.Add(codeSetAssigment);
                    type.Members.Add(enumProp.Item1);
                }
            }
        }

        private bool SkipProperty(CodeMemberProperty property, CodeTypeDeclaration type)
        {
            return property == null || type == null ||
                   !IsOptionSetProperty(property);
        }

        private static bool IsOptionSetProperty(CodeMemberProperty property)
        {
            return property.Type.BaseType == "Microsoft.Xrm.Sdk.OptionSetValue";
        }

        //private static bool IsComponentState(CodeMemberProperty property)
        //{
        //    return property.Name.ToLowerInvariant() == "componentstate";
        //}

    }
}
