﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.Test.AspNet.OData.TestCommon
{
    /// <summary>
    /// A generic controller base which derives from a platform-specific type.
    /// </summary>
#if !NETCORE1x
    public class TestControllerBase : System.Web.Http.ApiController
    {
    }
#else
    public class TestControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
    {
    }
#endif
}