using System;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SolutionDevTestLab
{
    class Program
    {

        static void Main(string[] args)
        {
            try {
                
                // Authenticate

                var credentials = SdkContext.AzureCredentialsFactory
                    .FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

                
                var azure = Azure.Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();

                var labSolution = new Solution(azure);

                string name = "tyler-devtestlab";
                string rgName = $"rg-{name}";
                Region region = Region.USWest2;

                labSolution.RunDeploy(name, rgName, region);

            } catch (Exception ex) {
                Utilities.Log(ex);
            }
        }
    }
}
