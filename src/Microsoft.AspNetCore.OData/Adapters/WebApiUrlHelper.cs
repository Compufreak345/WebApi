﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNetCore.OData.Adapters
{
    /// <summary>
    /// Adapter class to abstract the Asp.Net Url helper.
    /// </summary>
    internal class WebApiUrlHelper : IWebApiUrlHelper
    {
        /// <summary>
        /// The inner request wrapped by this instance.
        /// </summary>
        internal HttpRequest innerRequest;

        /// <summary>
        /// Initializes a new instance of the WebApiUrlHelper class.
        /// </summary>
        /// <param name="helper">The inner helper.</param>
        public WebApiUrlHelper(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            this.innerRequest = request;
        }

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public string CreateODataLink(params ODataPathSegment[] segments)
        {
            return this.CreateODataLink(segments as IList<ODataPathSegment>);
        }

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public string CreateODataLink(IList<ODataPathSegment> segments)
        {
            string routeName = this.innerRequest.ODataFeature().RouteName;
            if (String.IsNullOrEmpty(routeName))
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveODataRouteName);
            }

            IODataPathHandler pathHandler = this.innerRequest.GetPathHandler();
            return CreateODataLink(routeName, pathHandler, segments);
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
            if (String.IsNullOrEmpty(routeName))
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveODataRouteName);
            }

            if (pathHandler == null)
            {
                throw Error.ArgumentNull("pathHandler");
            }

            string odataPath = pathHandler.Link(new ODataPath(segments));
            return Url.RouteUrl(new UrlRouteContext()
            {
                RouteName = routeName,
                Values = new RouteValueDictionary() { { ODataRouteConstants.ODataPath, odataPath } },
                Protocol = this.innerRequest.Scheme,
                Host = this.innerRequest.Host.ToUriComponent()
            });
        }
    }
}