// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace SolutionDevTestLab
{
    public static class Utilities
    {
        public static bool IsRunningMocked { get; set; }
        public static Action<string> LoggerMethod { get; set; }
        public static Func<string> PauseMethod { get; set; }

        public static string ProjectPath { get; set; }

        static Utilities()
        {
            LoggerMethod = Console.WriteLine;
            PauseMethod = Console.ReadLine;
            ProjectPath = ".";
        }

        public static void Log(string message)
        {
            LoggerMethod.Invoke(message);
        }

        public static void Log(object obj)
        {
            if (obj != null)
            {
                LoggerMethod.Invoke(obj.ToString());
            }
            else
            {
                LoggerMethod.Invoke("(null)");
            }
        }

        public static void Log()
        {
            Utilities.Log("");
        }

        public static string ReadLine()
        {
            return PauseMethod.Invoke();
        }

        // Print resource group info.
        public static void PrintResourceGroup(IResourceGroup resource)
        {
            StringBuilder info = new StringBuilder();
            info.Append("Resource Group: ").Append(resource.Id)
                    .Append("\n\tName: ").Append(resource.Name)
                    .Append("\n\tRegion: ").Append(resource.Region)
                    .Append("\n\tTags: ").Append(resource.Tags.ToString());
            Log(info.ToString());
        }

        public static string CreateRandomName(string namePrefix)
        {
            return SdkContext.RandomResourceName(namePrefix, 30);
        }

        /**
         * Retrieve the secondary service principal client ID.
         * @param envSecondaryServicePrincipal an Azure Container Registry
         * @return a service principal client ID
         */
        public static string GetSecondaryServicePrincipalClientID(string envSecondaryServicePrincipal)
        {
            string clientId = "";
            File.ReadAllLines(envSecondaryServicePrincipal).All(line =>
            {
                var keyVal = line.Trim().Split(new char[] { '=' }, 2);
                if (keyVal.Length < 2)
                    return true; // Ignore lines that don't look like $$$=$$$
                if (keyVal[0].Equals("client"))
                    clientId = keyVal[1];
                return true;
            });

            return clientId;
        }

        /**
         * Retrieve the secondary service principal secret.
         * @param envSecondaryServicePrincipal an Azure Container Registry
         * @return a service principal secret
         */
        public static string GetSecondaryServicePrincipalSecret(string envSecondaryServicePrincipal)
        {
            string secret = "";
            File.ReadAllLines(envSecondaryServicePrincipal).All(line =>
            {
                var keyVal = line.Trim().Split(new char[] { '=' }, 2);
                if (keyVal.Length < 2)
                    return true; // Ignore lines that don't look like $$$=$$$
                if (keyVal[0].Equals("key"))
                    secret = keyVal[1];
                return true;
            });

            return secret;
        }

        public static void CreateCertificate(string domainName, string pfxPath, string password)
        {
            if (!IsRunningMocked)
            {
                string args = string.Format(
                    @".\createCert.ps1 -pfxFileName {0} -pfxPassword ""{1}"" -domainName ""{2}""",
                    pfxPath,
                    password,
                    domainName);
                ProcessStartInfo info = new ProcessStartInfo("powershell", args);
                string assetPath = Path.Combine(ProjectPath, "Asset");
                info.WorkingDirectory = assetPath;
                Process process = Process.Start(info);
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    // call "Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Bypass" in powershell if you fail here

                    Utilities.Log("powershell createCert.ps1 script failed");
                }
            }
            else
            {
                //File.Copy(
                //    Path.Combine(Utilities.ProjectPath, "Asset", "SampleTestCertificate.pfx"),
                //    Path.Combine(Utilities.ProjectPath, "Asset", pfxPath),
                //    overwrite: true);
            }
        }

        public static void CreateCertificate(string domainName, string pfxName, string cerName, string password)
        {
            if (!IsRunningMocked)
            {
                string args = string.Format(
                    @".\createCert1.ps1 -pfxFileName {0} -cerFileName {1} -pfxPassword ""{2}"" -domainName ""{3}""",
                    pfxName,
                    cerName,
                    password,
                    domainName);
                ProcessStartInfo info = new ProcessStartInfo("powershell", args);
                string assetPath = Path.Combine(ProjectPath, "Asset");
                info.WorkingDirectory = assetPath;
                Process.Start(info).WaitForExit();
            }
            else
            {
                //File.Copy(
                //    Path.Combine(Utilities.ProjectPath, "Asset", "SampleTestCertificate.pfx"),
                //    Path.Combine(Utilities.ProjectPath, "Asset", pfxName),
                //    overwrite: true);
            }
        }

        

        public static string CheckAddress(string url, IDictionary<string, string> headers = null)
        {
            if (!IsRunningMocked)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(300);
                        if (headers != null)
                        {
                            foreach (var header in headers)
                            {
                                client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            }
                        }
                        return client.GetAsync(url).Result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    }
                }
                catch (Exception ex)
                {
                    Utilities.Log(ex);
                }
            }

            return "[Running in PlaybackMode]";
        }

        public static string PostAddress(string url, string body, IDictionary<string, string> headers = null)
        {
            if (!IsRunningMocked)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        if (headers != null)
                        {
                            foreach (var header in headers)
                            {
                                client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            }
                        }
                        return client.PostAsync(url, new StringContent(body)).Result.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Utilities.Log(ex);
                }
            }

            return "[Running in PlaybackMode]";
        }

        public static string GetArmTemplate(string templateFileName)
        {
            return File.ReadAllText(Path.Combine(Utilities.ProjectPath, templateFileName));
        }

        public static string GetCertificatePath(string certificateName)
        {
            return Path.Combine(Utilities.ProjectPath, "Asset", certificateName);
        }
    }
}