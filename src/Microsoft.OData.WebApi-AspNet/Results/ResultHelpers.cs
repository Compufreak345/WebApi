﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.WebApi.Adapters;
using Microsoft.OData.WebApi.Builder;
using Microsoft.OData.WebApi.Builder.Conventions;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Extensions;
using Microsoft.OData.WebApi.Formatter;
using Microsoft.OData.WebApi.Formatter.Serialization;
using ODataPath = Microsoft.OData.WebApi.Routing.ODataPath;

namespace Microsoft.OData.WebApi.Results
{
    internal static class ResultHelpers
    {
        public const string EntityIdHeaderName = "OData-EntityId";

        public static Uri GenerateODataLink(HttpRequestMessage request, object entity, bool isEntityId)
        {
            ResourceContext resourceContext = CreateResourceContext(request, entity);
            Contract.Assert(resourceContext != null);

            // Generate location or entityId header from request Uri and key, if Post to a containment.
            // Link builder is not used, since it is also for generating ID, Edit, Read links, etc. scenarios, where
            // request Uri is not used.
            if (resourceContext.NavigationSource.NavigationSourceKind() == EdmNavigationSourceKind.ContainedEntitySet)
            {
                return GenerateContainmentODataPathSegments(resourceContext, isEntityId);
            }

            NavigationSourceLinkBuilderAnnotation linkBuilder =
                resourceContext.EdmModel.GetNavigationSourceLinkBuilder(resourceContext.NavigationSource);
            Contract.Assert(linkBuilder != null);

            Uri idLink = linkBuilder.BuildIdLink(resourceContext);
            if (isEntityId)
            {
                if (idLink == null)
                {
                    throw Error.InvalidOperation(
                        SRResources.IdLinkNullForEntityIdHeader,
                        resourceContext.NavigationSource.Name);
                }

                return idLink;
            }

            Uri editLink = linkBuilder.BuildEditLink(resourceContext);
            if (editLink == null)
            {
                if (idLink != null)
                {
                    return idLink;
                }

                throw Error.InvalidOperation(
                    SRResources.EditLinkNullForLocationHeader,
                    resourceContext.NavigationSource.Name);
            }

            return editLink;
        }

        public static void AddEntityId(HttpResponseMessage response, Func<Uri> entityId)
        {
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                response.Headers.TryAddWithoutValidation(EntityIdHeaderName, entityId().ToString());
            }
        }

        private static Uri GenerateContainmentODataPathSegments(ResourceContext resourceContext, bool isEntityId)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(
                resourceContext.NavigationSource.NavigationSourceKind() == EdmNavigationSourceKind.ContainedEntitySet);
            Contract.Assert(resourceContext.Request != null);

            ODataPath path = resourceContext.Request.Context.Path;
            if (path == null)
            {
                throw Error.InvalidOperation(SRResources.ODataPathMissing);
            }

            path = new ContainmentPathBuilder().TryComputeCanonicalContainingPath(path);

            List<ODataPathSegment> odataPath = path.Segments.ToList();

            // create a template entity set if it's contained entity set
            IEdmEntitySet entitySet = resourceContext.NavigationSource as IEdmEntitySet;
            if (entitySet == null)
            {
                EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
                entitySet = new EdmEntitySet(container, resourceContext.NavigationSource.Name, resourceContext.NavigationSource.EntityType());
            }

            odataPath.Add(new EntitySetSegment(entitySet));
            odataPath.Add(new KeySegment(ConventionsHelpers.GetEntityKey(resourceContext),
                resourceContext.StructuredType as IEdmEntityType, resourceContext.NavigationSource));

            if (!isEntityId)
            {
                bool isSameType = resourceContext.StructuredType == resourceContext.NavigationSource.EntityType();
                if (!isSameType)
                {
                    odataPath.Add(new TypeSegment(resourceContext.StructuredType, resourceContext.NavigationSource));
                }
            }

            string odataLink = resourceContext.Url.CreateODataLink(odataPath);
            return odataLink == null ? null : new Uri(odataLink);
        }

        private static ResourceContext CreateResourceContext(HttpRequestMessage request, object entity)
        {
            IEdmModel model = request.GetModel();
            if (model == null)
            {
                throw new InvalidOperationException(SRResources.RequestMustHaveModel);
            }

            ODataPath path = request.ODataProperties().Path;
            if (path == null)
            {
                throw new InvalidOperationException(SRResources.ODataPathMissing);
            }

            IEdmNavigationSource navigationSource = path.NavigationSource;
            if (navigationSource == null)
            {
                throw new InvalidOperationException(SRResources.NavigationSourceMissingDuringSerialization);
            }

            ODataSerializerContext serializerContext = new ODataSerializerContext
            {
                NavigationSource = navigationSource,
                Model = model,
                Url = new WebApiUrlHelper(request.GetUrlHelper() ?? new UrlHelper(request)),
                MetadataLevel = ODataMetadataLevel.FullMetadata, // Used internally to always calculate the links.
                Request = new WebApiRequestMessage(request),
                Path = path
            };

            IEdmEntityTypeReference entityType = GetEntityType(model, entity);
            return new ResourceContext(serializerContext, entityType, entity);
        }

        private static IEdmEntityTypeReference GetEntityType(IEdmModel model, object entity)
        {
            Type entityType = entity.GetType();
            IEdmTypeReference edmType = model.GetEdmTypeReference(entityType);
            if (edmType == null)
            {
                throw Error.InvalidOperation(SRResources.ResourceTypeNotInModel, entityType.FullName);
            }
            if (!edmType.IsEntity())
            {
                throw Error.InvalidOperation(SRResources.TypeMustBeEntity, edmType.FullName());
            }

            return edmType.AsEntity();
        }
    }
}