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
        public string AuthType { get; set; }

        public static InterpretConnectionstring Interpret(string connectionstring)
        {
            var interpret = new InterpretConnectionstring();

            var Url = GetParameterInStringByName(connectionstring, "url");
            var Username = GetParameterInStringByName(connectionstring, "username");
            var Password = GetParameterInStringByName(connectionstring, "password");
            var Domain = GetParameterInStringByName(connectionstring, "domain");
            var AuthType = GetParameterInStringByName(connectionstring, "authtype");


            interpret.Url = Url;
            interpret.Domain = Domain;
            interpret.Password = Password;
            interpret.UserName = Username;
            interpret.AuthType = AuthType;

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
