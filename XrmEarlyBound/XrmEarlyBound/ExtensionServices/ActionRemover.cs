using Microsoft.Crm.Services.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom;

namespace XrmEarlyBound.ExtensionServices
{
    public class ActionRemover : ICustomizeCodeDomService
    {
        public Utility.Config config { get; set; }

        public void CustomizeCodeDom(CodeCompileUnit codeUnit, IServiceProvider services)
        {
            config = Utility.Config.LoadSettings();

            var actionsToRemove = new List<CodeTypeDeclaration>();
            var types = codeUnit.Namespaces[0].Types;
            var namespaceName = codeUnit.Namespaces[0].Name;
            foreach (CodeTypeDeclaration type in types)
            {
                var _type = type.BaseTypes.Count > 0 ? type.BaseTypes[0].BaseType : null;
                if (Skip(type.Name, _type))
                {
                    actionsToRemove.Add(type);
                }
            }

            foreach(var actionToRemove in actionsToRemove)
            {
                types.Remove(actionToRemove);
            }
        }

        private bool Skip(string name, string type)
        {
            var isAction = false;
            name = name.Replace(" ", string.Empty);
            if (type == "Microsoft.Xrm.Sdk.OrganizationRequest")
            {
                name = name.Remove(name.Length - "Request".Length);
                isAction = true;
            }
            else if (type == "Microsoft.Xrm.Sdk.OrganizationResponse")
            {
                name = name.Remove(name.Length - "Response".Length);
                isAction = true;
            }

            if (!isAction)
                return false;

            var index = name.IndexOf('_');
            if (index >= 0)
            {
                name = name.Substring(index + 1, name.Length - index - 1);
            }

            name = name.ToLower();
            return !config.Actions.Contains(name);
        }
    }
}
