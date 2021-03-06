﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.TestCommon
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PropertyDataAttribute : Xunit.Extensions.PropertyDataAttribute
    {
        public PropertyDataAttribute(string propertyName)
            : base(propertyName)
        {
        }
    }
}
