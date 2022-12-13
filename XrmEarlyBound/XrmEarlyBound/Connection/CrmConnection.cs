using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Text.RegularExpressions;
using System.Web;
using XrmEarlyBound.Utility.Extensions;

namespace XrmEarlyBound.Connection
{
    public static class CrmConnection
    {
        public static IOrganizationService GetClientByConnectionString(string connectionString)
        {
            var connectiontionvalues = Regex.Split(connectionString, @";(?=(?:[^']*'[^']*')*[^']*$)");

            var _url = connectiontionvalues.GetParameter("url"); //GetParameterInStringByName(connectionString, "url");
            var _userName = connectiontionvalues.GetParameter("username"); //GetParameterInStringByName(connectionString, "username");
            var _password = connectiontionvalues.GetParameter("password"); //GetParameterInStringByName(connectionString, "password");
            var _domain = connectiontionvalues.GetParameter("domain"); //GetParameterInStringByName(connectionString, "domain");
            var _authtype = connectiontionvalues.GetParameter("authtype");

            var federation = false;
            if ((string.IsNullOrWhiteSpace(_domain) || _domain.ToLower() == "null") && !string.IsNullOrWhiteSpace(_userName) && !string.IsNullOrWhiteSpace(_password))
                federation = true;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            OrganizationServiceProxy organizationProxy;
            var clientCredentials = new ClientCredentials();
            if (federation)
            {
                if(!string.IsNullOrEmpty(_authtype))
                {
                    var connectionstring = "";
                    if (_authtype.ToLowerInvariant() == "clientsecret")
                        connectionstring = $"AuthType=ClientSecret;ClientId={_userName};ClientSecret={_password};url={_url}";
                    else if (_authtype.ToLowerInvariant() == "certificate")
                        connectionString = $"AuthType=Certificate;ClientId={_userName};thumbprint={_password};url={_url}";
                    else
                        throw new Exception("Only ClientSecret or Certificate is supported if AuthType is used");

                    var crmclient = new Microsoft.Xrm.Tooling.Connector.CrmServiceClient(connectionstring);
                    return crmclient;
                }

                clientCredentials.UserName.UserName = _userName;
                clientCredentials.UserName.Password = _password;
            }
            else
            {
                clientCredentials = new ClientCredentials();
                if (string.IsNullOrWhiteSpace(_userName) || string.IsNullOrWhiteSpace(_password))
                {
                    clientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
                }
                else
                {
                    clientCredentials.Windows.ClientCredential.UserName = _userName;
                    clientCredentials.Windows.ClientCredential.Password = _password;
                    clientCredentials.Windows.ClientCredential.Domain = _domain;
                }
            }
            organizationProxy = new OrganizationServiceProxy(new Uri(_url), null, clientCredentials, null);
            organizationProxy.EnableProxyTypes();
            return organizationProxy;
        }

        private static string GetParameterInStringByName(string connectionString, string parameter)
        {
            var connectiontionvalues = connectionString.Split(new char[] { ';' });

            var exits = connectiontionvalues.Where(x => x.Trim().StartsWith(parameter + "=", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (exits == null)
                return null;

            var value = connectiontionvalues.Where(x => x.Trim().StartsWith(parameter + "=", StringComparison.InvariantCultureIgnoreCase)).First().Trim().Substring(parameter.Length + 1);
            return value;
        }
    }
}