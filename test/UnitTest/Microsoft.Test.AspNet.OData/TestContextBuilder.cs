﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;


namespace Microsoft.Test.AspNet.OData
{
    /// <summary>
    /// A context builder used with <see cref="TestAspNetCoreServer"/>.
    /// </summary>
    internal class TestContextBuilder
    {
        private readonly IHttpApplication<TestContext> _application;
        private readonly HttpContext _httpContext;

        private TaskCompletionSource<HttpContext> _responseTcs = new TaskCompletionSource<HttpContext>(TaskCreationOptions.RunContinuationsAsynchronously);
        private TestResponseStream _responseStream;
        private TestResponseFeature _responseFeature = new TestResponseFeature();
        private CancellationTokenSource _requestAbortedSource = new CancellationTokenSource();
        private bool _pipelineFinished;
        private TestContext _testContext;

        internal TestContextBuilder(IHttpApplication<TestContext> application)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _httpContext = new DefaultHttpContext();

            var request = _httpContext.Request;
            request.Protocol = "HTTP/1.1";
            request.Method = HttpMethods.Get;

            _httpContext.Features.Set<IHttpResponseFeature>(_responseFeature);
            var requestLifetimeFeature = new HttpRequestLifetimeFeature();
            requestLifetimeFeature.RequestAborted = _requestAbortedSource.Token;
            _httpContext.Features.Set<IHttpRequestLifetimeFeature>(requestLifetimeFeature);

            _responseStream = new TestResponseStream(ReturnResponseMessageAsync, AbortRequest);
            _responseFeature.Body = _responseStream;
        }

        internal void Configure(Action<HttpContext> configureContext)
        {
            if (configureContext == null)
            {
                throw new ArgumentNullException(nameof(configureContext));
            }

            configureContext(_httpContext);
        }

        /// <summary>
        /// Start processing the request.
        /// </summary>
        /// <returns></returns>
        internal Task<HttpContext> SendAsync(CancellationToken cancellationToken)
        {
            var registration = cancellationToken.Register(AbortRequest);

            _testContext = _application.CreateContext(_httpContext.Features);

            // Async offload, don't let the test code block the caller.
            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    await _application.ProcessRequestAsync(_testContext);
                    await CompleteResponseAsync();
                    _application.DisposeContext(_testContext, exception: null);
                }
                catch (Exception ex)
                {
                    Abort(ex);
                    _application.DisposeContext(_testContext, ex);
                }
                finally
                {
                    registration.Dispose();
                }
            });

            return _responseTcs.Task;
        }

        internal void AbortRequest()
        {
            if (!_pipelineFinished)
            {
                _requestAbortedSource.Cancel();
            }
            _responseStream.Complete();
        }

        internal async Task CompleteResponseAsync()
        {
            _pipelineFinished = true;
            await ReturnResponseMessageAsync();
            _responseStream.Complete();
            await _responseFeature.FireOnResponseCompletedAsync();
        }

        internal async Task ReturnResponseMessageAsync()
        {
            // Check if the response has already started because the TrySetResult below could happen a bit late
            // (as it happens on a different thread) by which point the CompleteResponseAsync could run and calls this
            // method again.
            if (!_responseFeature.HasStarted)
            {
                // Sets HasStarted
                await _responseFeature.FireOnSendingHeadersAsync();
                // Copy the feature collection so we're not multi-threading on the same collection.
                var newFeatures = new FeatureCollection();
                foreach (var pair in _httpContext.Features)
                {
                    newFeatures[pair.Key] = pair.Value;
                }
                _responseTcs.TrySetResult(new DefaultHttpContext(newFeatures));
            }
        }

        internal void Abort(Exception exception)
        {
            _pipelineFinished = true;
            _responseStream.Abort(exception);
            _responseTcs.TrySetException(exception);
        }
    }
}
#endif