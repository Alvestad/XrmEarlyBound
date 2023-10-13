//Some methods copied from https://github.com/daryllabar/DLaB.Xrm.XrmToolBoxTools/blob/master/DLaB.CrmSvcUtilExtensions/Extensions.cs
//Copyright(c) 2015 Daryl LaBar
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

using Microsoft.Xrm.Sdk;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace XrmEarlyBound.Utility.Extensions
{
    public static class Extensions
    {
        //LaBar
        public static string GetLocalOrDefaultText(this Label label, string defaultIfNull = null)
        {
            var local = label.UserLocalizedLabel ?? label.LocalizedLabels.FirstOrDefault();

            if (local == null)
            {
                return defaultIfNull;
            }
            else
            {
                return local.Label ?? defaultIfNull;
            }
        }

        public static string ToJson(this object value)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            return json;
        }

        public static void ToJsonFile(this object value, string filePath)
        {
            System.IO.File.WriteAllText(filePath, value.ToJson());
        }

        //LaBar
        public static string GetFieldInitalizedValue(this CodeTypeDeclaration type, string fieldName)
        {
            var field = type.Members.OfType<CodeMemberField>().FirstOrDefault(f => f.Name == fieldName);
            if (field != null)
            {
                return ((CodePrimitiveExpression)field.InitExpression).Value.ToString();
            }

            return null;
        }

        //Labar
        /// <summary>
        /// Determines if the type inherits from one of the known Xrm OrganizationServiceContext types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsContextType(this CodeTypeDeclaration type)
        {
            if(type.BaseTypes.Count == 0)
                return false;
            
            var baseType = type.BaseTypes[0].BaseType;
            return baseType == "Microsoft.Xrm.Client.CrmOrganizationServiceContext"
                   || baseType == "Microsoft.Xrm.Sdk.Client.OrganizationServiceContext";
        }

        public static string RemoveDiacritics(this string text)
        {
            byte[] tempBytes;
            tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(text);
            string asciiStr = System.Text.Encoding.UTF8.GetString(tempBytes);
            return asciiStr;
        }

        public static string GetParameter(this string[] values, string paramenterName)
        {
            var value = values.FirstOrDefault(x => x.Trim().StartsWith($"{paramenterName}=", StringComparison.InvariantCultureIgnoreCase))?.Trim().Substring(paramenterName.Length + 1);
            if (value == null)
                return null;
            if (value.StartsWith("'") && value.EndsWith("'"))
                value = value.Substring(1, value.Length - 2);
            return value;
        }

    }
}
