﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.Test.AspNet.OData.TestCommon
{
    public class ForceGCAttribute : Xunit.Sdk.BeforeAfterTestAttribute
    {
        public override void After(MethodInfo methodUnderTest)
        {
            GC.Collect(99);
            GC.Collect(99);
            GC.Collect(99);
        }
    }
}