﻿// ----------------------------------------------------------------------------------
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

using Microsoft.Azure.Management.Internal.Resources.Utilities;
using Microsoft.Azure.Management.Network.Models;
using Microsoft.WindowsAzure.Commands.Common.Attributes;
using System.Collections;

namespace Microsoft.Azure.Commands.Network.Models
{
    public class PSTopLevelResource : PSChildResource
    {
        [Ps1Xml(Label = "ResourceGroupName", Target = ViewControl.Table, Position = 0)]
        public string ResourceGroupName { get; set; }

        [Ps1Xml(Label = "Location", Target = ViewControl.Table, Position = 2)]
        public string Location { get; set; }

        public ExtendedLocation ExtendedLocation { get; set; }

        // Not sure that is right
        public string ExtendedLocationType { get; set; }

        public string ResourceGuid { get; set; }

        public string Type { get; set; }

        public Hashtable Tag { get; set; }

        public string TagsTable
        {
            get { return ResourcesExtensions.ConstructTagsTable(Tag); }
        }
    }
}
