﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Factories;
using Microsoft.Test.AspNet.OData.Formatter;

namespace Microsoft.Test.AspNet.OData
{
    public class MockContainer : IServiceProvider
    {
        private IServiceProvider _rootContainer;

        public MockContainer(Action<IContainerBuilder> action = null)
        {
            InitializeConfiguration(action);
        }

        public MockContainer(IEdmModel model)
        {
            InitializeConfiguration(b => b.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model));
        }

        public MockContainer(IEdmModel model, IEnumerable<IODataRoutingConvention> routingConventions)
        {
            InitializeConfiguration(builder =>
                builder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model)
                       .AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => routingConventions.ToList().AsEnumerable()));
        }

        public object GetService(Type serviceType)
        {
            return _rootContainer.GetService(serviceType);
        }

        private void InitializeConfiguration(Action<IContainerBuilder> action)
        {
            var configuration = RoutingConfigurationFactory.Create();
            string routeName = HttpRouteCollectionExtensions.RouteName;
#if NETCORE1x
            IPerRouteContainer perRouteContainer = configuration.ServiceProvider.GetRequiredService<IPerRouteContainer>();
            Action<IContainerBuilder> builderAction = ODataRouteBuilderExtensions.ConfigureDefaultServices(action);
            _rootContainer = perRouteContainer.CreateODataRootContainer(routeName, builderAction);

            // Without a proper container, WebApiAssemblyResolved has a problem here. It has no applicationpartManager.
#else
            _rootContainer = configuration.CreateODataRootContainer(routeName, action);
#endif
        }
    }
}