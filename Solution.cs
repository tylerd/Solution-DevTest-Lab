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
        private string _name;
        private string _resourceGroupName;
        private Region _region;

        public Solution(IAzure azure) => _azure = azure;

        public Solution(IAzure azure, string name, string resourceGroupName, Region region) 
        {
            _azure = azure;
            _name = name;
            _resourceGroupName = resourceGroupName;
            _region = region;
        }

        public void RunDeploy(string name, string resourceGroupName, Region region)
        {

            var deploymentName = SdkContext.RandomResourceName("tyler-deploy", 24);

            var templateJson = Utilities.GetArmTemplate("azuredeploy.json");

            Utilities.Log("Starting a deployment for DevTest Lab: " + deploymentName);

            var paramObj = JObject.FromObject(new {name = new { value = name}});

            _azure.Deployments.Define(deploymentName)
                .WithNewResourceGroup(resourceGroupName, region)
                .WithTemplate(templateJson)
                .WithParameters(paramObj)
                .WithMode(DeploymentMode.Incremental)
                
                .Create();
            
        }        
    }
}