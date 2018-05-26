using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmEarlyBound.Utility
{
    public class InterpretConnectionstring
    {
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }

        public static InterpretConnectionstring Interpret(string connectionstring)
        {
            var interpret = new InterpretConnectionstring();

            var Url = GetParameterInStringByName(connectionstring, "url");
            var Username = GetParameterInStringByName(connectionstring, "username");
            var Password = GetParameterInStringByName(connectionstring, "password");
            var Domain = GetParameterInStringByName(connectionstring, "domain");
            var Org = GetParameterInStringByName(connectionstring, "Org");

            if (string.IsNullOrWhiteSpace(Domain))
                interpret.Domain = null;
            else if (string.Compare(Domain, "null", true) == 0)
                interpret.Domain = null;
            else
                interpret.Domain = Domain;


            if (Url.ToLower().EndsWith("organization.svc"))
            {
                interpret.Url = Url;
            }
            else
            {
                var startIndex = Url.IndexOf("//") + 2;
                var disco = Url.Substring(startIndex, Url.IndexOf(".") - startIndex);
                if (interpret.Domain == null)
                {
                    interpret.Url = ReplaceFirstOccurrance(Url, disco, Org).Replace("Discovery.svc", "Organization.svc");
                }

                if (interpret.Domain != null)
                {
                    interpret.Url = Url.Replace("Discovery.svc", "Organization.svc");
                    interpret.Url = interpret.Url.Insert(interpret.Url.IndexOf("/", startIndex) + 1, $"{Org}/");
                }
            }

            
            interpret.Password = Password;
            interpret.UserName = Username;

            return interpret;
        }

        public static string ReplaceFirstOccurrance(string original, string oldValue, string newValue)
        {
            if (String.IsNullOrEmpty(original))
                return String.Empty;
            if (String.IsNullOrEmpty(oldValue))
                return original;
            if (String.IsNullOrEmpty(newValue))
                newValue = String.Empty;
            int loc = original.IndexOf(oldValue);
            return original.Remove(loc, oldValue.Length).Insert(loc, newValue);
        }

        public static string GetParameterInStringByName(string connectionString, string parameter)
        {
            var connectiontionvalues = connectionString.Split(new char[] { ';' });
            if (!connectiontionvalues.Where(x => x.Trim().StartsWith(parameter + "=", StringComparison.InvariantCultureIgnoreCase)).Any())
                return null;
            var value = connectiontionvalues.Where(x => x.Trim().StartsWith(parameter + "=", StringComparison.InvariantCultureIgnoreCase)).First().Trim().Substring(parameter.Length + 1);
            return value;
        }
    }
}
