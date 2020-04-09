using System.Linq;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Newtonsoft.Json.Linq;

namespace SolutionDevTestLab 
{
    public class Solution
    {
        private IAzure _azure;

        public Solution(IAzure azure) => _azure = azure;

        public void RunDeploy(string name, string resourceGroupName, Region region)
        {

            var deploymentName = SdkContext.RandomResourceName($"{name}-deploy-", 24);

            var templateJson = Utilities.GetArmTemplate("azuredeploy.json");

            Utilities.Log("Starting a deployment for DevTest Lab: " + deploymentName);

            var paramObj = JObject.FromObject(new {name = new { value = name}});

            var deployment = _azure.Deployments.Define(deploymentName)
                .WithNewResourceGroup(resourceGroupName, region)
                .WithTemplate(templateJson)
                .WithParameters(paramObj)
                .WithMode(DeploymentMode.Incremental)
                
                .Create();
            
            Utilities.Log("--- Deployment Operations ---");
            var ops = deployment.DeploymentOperations.List()
                .OrderBy(o => o.Timestamp);

            foreach (var operation in ops)
            {
                Utilities.PrintDeploymentOperation(operation);
            }            
            
        }        
    }
}