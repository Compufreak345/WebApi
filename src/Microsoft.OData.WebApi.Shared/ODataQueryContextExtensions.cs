﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.WebApi.Query;

namespace Microsoft.OData.WebApi
{
    internal static class ODataQueryContextExtensions
    {
        public static IAssembliesResolver GetAssembliesResolver(this ODataQueryContext context)
        {
            return context.RequestContainer.GetRequiredService<IAssembliesResolver>();
        }

        public static ODataQuerySettings UpdateQuerySettings(this ODataQueryContext context, ODataQuerySettings querySettings, IQueryable query)
        {
            ODataQuerySettings updatedSettings = context.RequestContainer.GetRequiredService<ODataQuerySettings>();
            updatedSettings.CopyFrom(querySettings);

            if (updatedSettings.HandleNullPropagation == HandleNullPropagationOption.Default)
            {
                updatedSettings.HandleNullPropagation = query != null
                    ? HandleNullPropagationOptionHelper.GetDefaultHandleNullPropagationOption(query)
                    : HandleNullPropagationOption.True;
            }

            return updatedSettings;
        }
    }
}