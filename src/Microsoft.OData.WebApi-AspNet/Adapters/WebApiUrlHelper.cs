﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Routing;
using Microsoft.OData.UriParser;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Extensions;
using Microsoft.OData.WebApi.Interfaces;
using Microsoft.OData.WebApi.Routing;

namespace Microsoft.OData.WebApi.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi Url helper to OData WebApi.
    /// </summary>
    public class WebApiUrlHelper : IWebApiUrlHelper
    {
        /// <summary>
        /// The inner helper wrapped by this instance.
        /// </summary>
        private UrlHelper innerHelper;

        /// <summary>
        /// Initializes a new instance of the WebApiUrlHelper class.
        /// </summary>
        /// <param name="helper">The inner helper.</param>
        public WebApiUrlHelper(UrlHelper helper)
        {
            if (helper == null)
            {
                throw Error.ArgumentNull("helper");
            }

            this.innerHelper = helper;
        }

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public string CreateODataLink(IList<ODataPathSegment> segments)
        {
            return this.innerHelper.CreateODataLink(segments);
        }

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public string CreateODataLink(params ODataPathSegment[] segments)
        {
            return this.innerHelper.CreateODataLink(segments);
        }

        /// <summary>
        /// Generates an OData link using the given OData route name, path handler, and segments.
        /// </summary>
        /// <param name="routeName">The name of the OData route.</param>
        /// <param name="pathHandler">The path handler to use for generating the link.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public string CreateODataLink(string routeName, IODataPathHandler pathHandler, IList<ODataPathSegment> segments)
        {
            return this.innerHelper.CreateODataLink(routeName, pathHandler, segments);
        }
    }
}