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
        static void RunDeploy(IAzure azure)
        {
            var name = "tyler-devtestlab";
            var rgName = $"rg-{name}";

            var deploymentName = SdkContext.RandomResourceName("tyler-deploy", 24);

            var templateJson = Utilities.GetArmTemplate("azuredeploy.json");

            Utilities.Log($"Creating a resource group with name: {rgName}");

            azure.ResourceGroups.Define(rgName)
                .WithRegion(Region.USWest2)
                .Create();

            Utilities.Log($"Created a resource group with name: {rgName}");

            Utilities.Log("Starting a deployment for DevTest Lab: " + deploymentName);

            var paramObj = JObject.FromObject(new {name = new { value = name}});

            azure.Deployments.Define(deploymentName)
                .WithExistingResourceGroup(rgName)
                .WithTemplate(templateJson)
                .WithParameters(paramObj)
                .WithMode(DeploymentMode.Incremental)
                
                .Create();
            
        }

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



                RunDeploy(azure);

            } catch (Exception ex) {
                Utilities.Log(ex);
            }
        }
    }
}
