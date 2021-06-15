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
using Microsoft.Azure.Management.Network.Models;
using Microsoft.WindowsAzure.Commands.Common.CustomAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using MNM = Microsoft.Azure.Management.Network.Models;

namespace Microsoft.Azure.Commands.Network
{
    [GenericBreakingChange("Default behaviour of Zone will be changed", OldWay = "Sku = Standard means the Standard FrontendIpConfig is zone-redundant.",
        NewWay = "Sku = Standard and Zone = {} means the Standard FrontendIpConfig has no zones. If you want to create a zone-redundant FrontendIpConfig, please specify all the zones in the region. For example, Zone = ['1', '2', '3'].")]
    [Cmdlet(VerbsCommon.New, ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "LoadBalancerFrontendIpConfig", DefaultParameterSetName = "SetByResourceSubnet", SupportsShouldProcess = true), OutputType(typeof(PSFrontendIPConfiguration))]
    public partial class NewAzureRmLoadBalancerFrontendIpConfigCommand : NetworkBaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "Name of the frontend ip configuration.")]
        public string Name { get; set; }

        [Parameter(
            Mandatory = false,
            ParameterSetName = "SetByResourceSubnet",
            HelpMessage = "The private IP address of the IP configuration.",
            ValueFromPipelineByPropertyName = true)]
        [Parameter(
            Mandatory = false,
            ParameterSetName = "SetByResourceIdSubnet",
            HelpMessage = "The private IP address of the IP configuration.",
            ValueFromPipelineByPropertyName = true)]
        public string PrivateIpAddress { get; set; }

        [Parameter(
            Mandatory = false,
            ParameterSetName = "SetByResourceSubnet",
            HelpMessage = "The private IP address version of the IP configuration.",
            ValueFromPipelineByPropertyName = true)]
        [Parameter(
            Mandatory = false,
            ParameterSetName = "SetByResourceIdSubnet",
            HelpMessage = "The private IP address version of the IP configuration.",
            ValueFromPipelineByPropertyName = true)]
        [ValidateSet(
            MNM.IPVersion.IPv4,
            MNM.IPVersion.IPv6,
            IgnoreCase = true)]
        public string PrivateIpAddressVersion { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "A list of availability zones denoting the IP allocated for the resource needs to come from.",
            ValueFromPipelineByPropertyName = true)]
        public string[] Zone { get; set; }

        [Parameter(
            Mandatory = true,
            ParameterSetName = "SetByResourceIdSubnet",
            HelpMessage = "The reference of the subnet resource.",
            ValueFromPipelineByPropertyName = true)]
        public string SubnetId { get; set; }

        [Parameter(
            Mandatory = true,
            ParameterSetName = "SetByResourceSubnet",
            HelpMessage = "The reference of the subnet resource.",
            ValueFromPipelineByPropertyName = true)]
        public PSSubnet Subnet { get; set; }

        [Parameter(
            Mandatory = true,
            ParameterSetName = "SetByResourceIdPublicIpAddress",
            HelpMessage = "The reference of the Public IP resource.",
            ValueFromPipelineByPropertyName = true)]
        public string PublicIpAddressId { get; set; }

        [Parameter(
            Mandatory = true,
            ParameterSetName = "SetByResourcePublicIpAddress",
            HelpMessage = "The reference of the Public IP resource.",
            ValueFromPipelineByPropertyName = true)]
        public PSPublicIpAddress PublicIpAddress { get; set; }

        [Parameter(
    Mandatory = true,
    ParameterSetName = "SetByResourceIdPublicIpAddressPrefix",
    HelpMessage = "The reference of the Public IP Prefix resource.",
    ValueFromPipelineByPropertyName = true)]
        public string PublicIpAddressPrefixId { get; set; }

        [Parameter(
            Mandatory = true,
            ParameterSetName = "SetByResourcePublicIpAddressPrefix",
            HelpMessage = "The reference of the Public IP Prefix resource.",
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public PSPublicIpPrefix PublicIpAddressPrefix { get; set; }

        public override void Execute()
        {

            if (string.Equals(ParameterSetName, "SetByResourceSubnet"))
            {
                if (this.Subnet != null)
                {
                    this.SubnetId = this.Subnet.Id;
                }
            }

            if (string.Equals(ParameterSetName, "SetByResourcePublicIpAddress"))
            {
                if (this.PublicIpAddress != null)
                {
                    this.PublicIpAddressId = this.PublicIpAddress.Id;
                }
            }

            if (string.Equals(ParameterSetName, "SetByResourcePublicIpAddressPrefix"))
            {
                if (this.PublicIpAddressPrefix != null)
                {
                    this.PublicIpAddressPrefixId = this.PublicIpAddressPrefix.Id;
                }
            }

            var vFrontendIpConfigurations = new PSFrontendIPConfiguration();

            vFrontendIpConfigurations.PrivateIpAddress = this.PrivateIpAddress;
            vFrontendIpConfigurations.PrivateIpAddressVersion = this.PrivateIpAddressVersion;
            if(!string.IsNullOrEmpty(vFrontendIpConfigurations.PrivateIpAddress))
            {
                vFrontendIpConfigurations.PrivateIpAllocationMethod = "Static";
            }
            else
            {
                vFrontendIpConfigurations.PrivateIpAllocationMethod = "Dynamic";
            }

            vFrontendIpConfigurations.Name = this.Name;
            vFrontendIpConfigurations.Zones = this.Zone?.ToList();
            if(!string.IsNullOrEmpty(this.SubnetId))
            {
                // Subnet
                if (vFrontendIpConfigurations.Subnet == null)
                {
                    vFrontendIpConfigurations.Subnet = new PSSubnet();
                }
                vFrontendIpConfigurations.Subnet.Id = this.SubnetId;
            }
            if(!string.IsNullOrEmpty(this.PublicIpAddressId))
            {
                // PublicIpAddress
                if (vFrontendIpConfigurations.PublicIpAddress == null)
                {
                    vFrontendIpConfigurations.PublicIpAddress = new PSPublicIpAddress();
                }
                vFrontendIpConfigurations.PublicIpAddress.Id = this.PublicIpAddressId;
            }
            if (!string.IsNullOrEmpty(this.PublicIpAddressPrefixId))
            {
                // PublicIpAddressPrefix
                if (vFrontendIpConfigurations.PublicIPPrefix == null)
                {
                    vFrontendIpConfigurations.PublicIPPrefix = new PSPublicIpPrefix();
                }
                vFrontendIpConfigurations.PublicIPPrefix.Id = this.PublicIpAddressPrefixId;
            }
            var generatedId = string.Format(
                "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Network/loadBalancers/{2}/{3}/{4}",
                this.NetworkClient.NetworkManagementClient.SubscriptionId,
                Microsoft.Azure.Commands.Network.Properties.Resources.ResourceGroupNotSet,
                Microsoft.Azure.Commands.Network.Properties.Resources.LoadBalancerNameNotSet,
                "FrontendIpConfigurations",
                this.Name);
            vFrontendIpConfigurations.Id = generatedId;

            WriteObject(vFrontendIpConfigurations, true);
        }
    }
}
