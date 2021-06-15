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

using Microsoft.Azure.Commands.Network.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using Microsoft.Azure.Management.Network.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Network
{
    [Cmdlet(VerbsCommon.Add, ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "LoadBalancerInboundNatRuleConfig", DefaultParameterSetName = "SetByResource", SupportsShouldProcess = true), OutputType(typeof(PSLoadBalancer))]
    public partial class AddAzureRmLoadBalancerInboundNatRuleConfigCommand : NetworkBaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The reference of the load balancer resource.",
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public PSLoadBalancer LoadBalancer { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "Name of the inbound nat rule.")]
        public string Name { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "The transport protocol for the endpoint.",
            ValueFromPipelineByPropertyName = true)]
        [PSArgumentCompleter(
            "Udp",
            "Tcp",
            "All"
        )]
        public string Protocol { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "The port for the external endpoint. Port numbers for each rule must be unique within the Load Balancer. Acceptable values range from 1 to 65534.",
            ValueFromPipelineByPropertyName = true)]
        public int FrontendPort { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "The port used for the internal endpoint. Acceptable values range from 1 to 65535.",
            ValueFromPipelineByPropertyName = true)]
        public int BackendPort { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "The timeout for the TCP idle connection. The value can be set between 4 and 30 minutes. The default value is 4 minutes. This element is only used when the protocol is set to TCP.",
            ValueFromPipelineByPropertyName = true)]
        public int IdleTimeoutInMinutes { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Configures a virtual machine's endpoint for the floating IP capability required to configure a SQL AlwaysOn Availability Group. This setting is required when using the SQL AlwaysOn Availability Groups in SQL server. This setting can't be changed after you create the endpoint.")]
        public SwitchParameter EnableFloatingIP { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Receive bidirectional TCP Reset on TCP flow idle timeout or unexpected connection termination. This element is only used when the protocol is set to TCP.")]
        public SwitchParameter EnableTcpReset { get; set; }

        [Parameter(
            Mandatory = false,
            ParameterSetName = "SetByResourceId",
            HelpMessage = "A reference to frontend IP addresses.",
            ValueFromPipelineByPropertyName = true)]
        public string FrontendIpConfigurationId { get; set; }

        [Parameter(
            Mandatory = false,
            ParameterSetName = "SetByResource",
            HelpMessage = "A reference to frontend IP addresses.",
            ValueFromPipelineByPropertyName = true)]
        public PSFrontendIPConfiguration FrontendIpConfiguration { get; set; }


        public override void Execute()
        {

            var existingInboundNatRule = this.LoadBalancer.InboundNatRules.SingleOrDefault(resource => string.Equals(resource.Name, this.Name, System.StringComparison.CurrentCultureIgnoreCase));
            if (existingInboundNatRule != null)
            {
                throw new ArgumentException("InboundNatRule with the specified name already exists");
            }

            // InboundNatRules
            if (this.LoadBalancer.InboundNatRules == null)
            {
                this.LoadBalancer.InboundNatRules = new List<PSInboundNatRule>();
            }

            if (string.Equals(ParameterSetName, Microsoft.Azure.Commands.Network.Properties.Resources.SetByResource))
            {
                if (this.FrontendIpConfiguration != null)
                {
                    this.FrontendIpConfigurationId = this.FrontendIpConfiguration.Id;
                }
            }

            var vInboundNatRules = new PSInboundNatRule();

            vInboundNatRules.Protocol = this.Protocol;
            vInboundNatRules.FrontendPort = this.FrontendPort;
            vInboundNatRules.BackendPort = this.BackendPort;
            vInboundNatRules.IdleTimeoutInMinutes = this.MyInvocation.BoundParameters.ContainsKey("IdleTimeoutInMinutes") ? this.IdleTimeoutInMinutes : 4;
            vInboundNatRules.EnableFloatingIP = this.EnableFloatingIP;
            vInboundNatRules.EnableTcpReset = this.EnableTcpReset;
            vInboundNatRules.Name = this.Name;
            if(!string.IsNullOrEmpty(this.FrontendIpConfigurationId))
            {
                // FrontendIPConfiguration
                if (vInboundNatRules.FrontendIPConfiguration == null)
                {
                    vInboundNatRules.FrontendIPConfiguration = new PSResourceId();
                }
                vInboundNatRules.FrontendIPConfiguration.Id = this.FrontendIpConfigurationId;
            }
            var generatedId = string.Format(
                "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Network/loadBalancers/{2}/{3}/{4}",
                this.NetworkClient.NetworkManagementClient.SubscriptionId,
                this.LoadBalancer.ResourceGroupName,
                this.LoadBalancer.Name,
                "InboundNatRules",
                this.Name);
            vInboundNatRules.Id = generatedId;

            this.LoadBalancer.InboundNatRules.Add(vInboundNatRules);
            WriteObject(this.LoadBalancer, true);
        }
    }
}
