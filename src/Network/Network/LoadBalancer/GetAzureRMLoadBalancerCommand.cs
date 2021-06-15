// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Commands.ResourceManager.Common.Tags;
using Microsoft.Rest.Azure;
using Microsoft.Azure.Commands.Network.Models;
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Management.Network.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using AutoMapper;
using CNM = Microsoft.Azure.Commands.Network.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

namespace Microsoft.Azure.Commands.Network
{
    [Cmdlet(VerbsCommon.Get, ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "LoadBalancer", DefaultParameterSetName = "NoExpand"), OutputType(typeof(PSLoadBalancer))]
    public partial class GetAzureRmLoadBalancer : NetworkBaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The resource group name of the load balancer.",
            ParameterSetName = "Expand",
            ValueFromPipelineByPropertyName = true)]
        [Parameter(
            Mandatory = false,
            HelpMessage = "The resource group name of the load balancer.",
            ParameterSetName = "NoExpand",
            ValueFromPipelineByPropertyName = true)]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        public string ResourceGroupName { get; set; }

        [Alias("ResourceName")]
        [Parameter(
            Mandatory = true,
            HelpMessage = "The name of the load balancer.",
            ParameterSetName = "Expand",
            ValueFromPipelineByPropertyName = true)]
        [Parameter(
            Mandatory = false,
            HelpMessage = "The name of the load balancer.",
            ParameterSetName = "NoExpand",
            ValueFromPipelineByPropertyName = true)]
        [ResourceNameCompleter("Microsoft.Network/loadBalancers", "ResourceGroupName")]
        [ValidateNotNullOrEmpty]
        [SupportsWildcards]
        public string Name { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The resource reference to be expanded.",
            ParameterSetName = "Expand",
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ExpandResource { get; set; }

        public override void Execute()
        {
            base.Execute();

            if(ShouldGetByName(ResourceGroupName, Name))
            {
                var vLoadBalancer = this.NetworkClient.NetworkManagementClient.LoadBalancers.Get(ResourceGroupName, Name, ExpandResource);
                var vLoadBalancerModel = NetworkResourceManagerProfile.Mapper.Map<CNM.PSLoadBalancer>(vLoadBalancer);
                vLoadBalancerModel.ResourceGroupName = this.ResourceGroupName;
                vLoadBalancerModel.Tag = TagsConversionHelper.CreateTagHashtable(vLoadBalancer.Tags);
                WriteObject(vLoadBalancerModel, true);
            }
            else
            {
                IPage<LoadBalancer> vLoadBalancerPage;
                if(ShouldListByResourceGroup(ResourceGroupName, Name))
                {
                    vLoadBalancerPage = this.NetworkClient.NetworkManagementClient.LoadBalancers.List(this.ResourceGroupName);
                }
                else
                {
                    vLoadBalancerPage = this.NetworkClient.NetworkManagementClient.LoadBalancers.ListAll();
                }

                var vLoadBalancerList = ListNextLink<LoadBalancer>.GetAllResourcesByPollingNextLink(vLoadBalancerPage,
                    this.NetworkClient.NetworkManagementClient.LoadBalancers.ListNext);
                List<PSLoadBalancer> psLoadBalancerList = new List<PSLoadBalancer>();
                foreach (var vLoadBalancer in vLoadBalancerList)
                {
                    var vLoadBalancerModel = NetworkResourceManagerProfile.Mapper.Map<CNM.PSLoadBalancer>(vLoadBalancer);
                    vLoadBalancerModel.ResourceGroupName = NetworkBaseCmdlet.GetResourceGroup(vLoadBalancer.Id);
                    vLoadBalancerModel.Tag = TagsConversionHelper.CreateTagHashtable(vLoadBalancer.Tags);
                    psLoadBalancerList.Add(vLoadBalancerModel);
                }
                WriteObject(TopLevelWildcardFilter(ResourceGroupName, Name, psLoadBalancerList), true);
            }
        }
    }
}
