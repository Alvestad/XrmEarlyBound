using System;

using Microsoft.Crm.Services.Utility;
using System.Diagnostics;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;



public sealed class CustomizeCodeDomService : ICustomizeCodeDomService
{
    XrmEarlyBound.Utility.Config config = XrmEarlyBound.Utility.Config.LoadSettings();

    public void CustomizeCodeDom(CodeCompileUnit codeUnit, IServiceProvider services)
    {
        if (config.GlobalOptionSets.Count > 0)
            new XrmEarlyBound.ExtensionServices.EnumPropertyGenerator().CustomizeCodeDom(codeUnit, services);
        if (config.Actions.Count > 0)
            new XrmEarlyBound.ExtensionServices.ActionRemover().CustomizeCodeDom(codeUnit, services);
    }
}
