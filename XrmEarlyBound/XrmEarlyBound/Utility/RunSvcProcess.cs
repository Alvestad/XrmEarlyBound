using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace XrmEarlyBound.Utility
{
    public class RunSvcProcess
    {
        public static void Run(string url, string username, string domain, string password, string authtype, string ns, string filepath, bool actions, bool globaloptionsets, string servicecontextname, Delegate @delegate)
        {
            string filePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            string exePath = string.Format("{0}\\CrmSvcUtil.exe", System.IO.Path.GetDirectoryName(filePath));

            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.UseShellExecute = false;
            startInfo.FileName = string.Format("{0}{1}{0}", '"', exePath);
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;

            string arguments = string.Empty;

            //var connectionstring = $"AuthType=ClientSecret;ClientId={__userName};ClientSecret={__password};url={__url}";

            AddArguments(ref arguments, "url", url);

            if (!string.IsNullOrEmpty(authtype))
            {
                AddArguments(ref arguments, "authtype", authtype);
                AddArguments(ref arguments, "clientid", username);
                if (authtype.ToLowerInvariant() == "clientsecret")
                    AddArguments(ref arguments, "clientsecret", password);
                else if(authtype.ToLowerInvariant() == "certificate")
                    AddArguments(ref arguments, "thumbprint", password);
                else
                    throw new Exception("Only ClientSecret or Certificate is supported if AuthType is used");
            }
            else
            {
                AddArguments(ref arguments, "username", username);
                AddArguments(ref arguments, "password", password);
                if (domain != null)
                    AddArguments(ref arguments, "domain", domain);
            }

            AddArguments(ref arguments, "namespace", ns);
            AddArguments(ref arguments, "out", string.Format("{0}{1}{0}", '"', filepath));
            if (servicecontextname != null)
                AddArguments(ref arguments, "servicecontextname", servicecontextname);
            AddArguments(ref arguments, "codewriterfilter", "FilteringService,XrmEarlyBound");
            AddArguments(ref arguments, "namingservice", "NamingService,XrmEarlyBound");
            AddArguments(ref arguments, "codecustomization", "CustomizeCodeDomService,XrmEarlyBound");
            if (actions)
            {
                AddArguments(ref arguments, "generateActions");
            }
            if(globaloptionsets)
            {
                AddArguments(ref arguments, "generateGlobalOptionSets");
            }

            startInfo.Arguments = arguments.Trim();

            Console.WriteLine(startInfo.Arguments);

            try
            {
                Process proc = new Process { StartInfo = startInfo };
                proc.Start();
                while (!proc.StandardOutput.EndOfStream)
                {
                    if (@delegate != null)
                    {
                        @delegate.DynamicInvoke(proc.StandardOutput.ReadLine());
                        @delegate.DynamicInvoke(proc.StandardError.ReadLine());
                    }
                    else
                    {
                        proc.StandardOutput.ReadLine();
                        proc.StandardError.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static void AddArguments(ref string value, string par)
        {
            value += @"/" + par + " ";
        }

        private static void AddArguments(ref string value, string par, string val)
        {
            value += @"/" + par + ":" + val + " ";
        }

        private void ForceMissingAssembly()
        {
            var obj = System.Text.Json.JsonValueKind.Object;
        }
    }
}
