﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.AspNet.OData.Builder.TestModels;
using Microsoft.Test.AspNet.OData.Factories;
using Microsoft.Test.AspNet.OData.TestCommon;
using Newtonsoft.Json.Linq;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.AspNet.OData.Builder.TestModels;
using Microsoft.Test.AspNet.OData.Factories;
using Microsoft.Test.AspNet.OData.TestCommon;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.Test.AspNet.OData.Routing
{
    public class ODataCountTest
    {
        private static HttpClient _client;

        public ODataCountTest()
        {
            IEdmModel model = GetEdmModel();
            var controllers = new[] { typeof(DollarCountEntitiesController) };
            var server = TestServerFactory.Create("odata", "odata", controllers, (routingConfig) => model);
            _client = TestServerFactory.CreateClient(server);
        }

        public static TheoryDataSet<string, int> DollarCountData
        {
            get
            {
                var data = new TheoryDataSet<string, int>();

                // $count follows entity set, structural collection property or navigation collection property.
                data.Add("DollarCountEntities/$count", 10);
                data.Add("DollarCountEntities/$count?$filter=Id gt 5", 5);
                data.Add("DollarCountEntities/Microsoft.Test.AspNet.OData.Routing.DerivedDollarCountEntity/$count", 5);
                data.Add("DollarCountEntities(5)/StringCollectionProp/$count", 2);
                data.Add("DollarCountEntities(5)/StringCollectionProp/$count?$filter=$it eq '2'", 1);
                data.Add("DollarCountEntities(5)/EnumCollectionProp/$count", 3);
                data.Add("DollarCountEntities(5)/EnumCollectionProp/$count?$filter=$it has Microsoft.Test.AspNet.OData.Builder.TestModels.Color'Green'", 2);
                data.Add("DollarCountEntities(5)/TimeSpanCollectionProp/$count", 4);
                data.Add("DollarCountEntities(5)/ComplexCollectionProp/$count", 5);
                data.Add("DollarCountEntities(5)/EntityCollectionProp/$count", 4);

                // $count follows unbound function that returns collection.
                data.Add("UnboundFunctionReturnsPrimitveCollection()/$count", 6);
                data.Add("UnboundFunctionReturnsEnumCollection()/$count", 7);
                data.Add("UnboundFunctionReturnsDateTimeOffsetCollection()/$count", 8);
                data.Add("UnboundFunctionReturnsDateCollection()/$count", 18);
                data.Add("UnboundFunctionReturnsComplexCollection()/$count", 9);
                data.Add("UnboundFunctionReturnsEntityCollection()/$count", 10);
                data.Add("UnboundFunctionReturnsEntityCollection()/Microsoft.Test.AspNet.OData.Routing.DerivedDollarCountEntity/$count", 11);

                // $count follows bound function that returns collection.
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsPrimitveCollection()/$count", 12);
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsEnumCollection()/$count", 13);
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsDateTimeOffsetCollection()/$count", 14);
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsComplexCollection()/$count", 15);
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsComplexCollection()/$count?$filter=contains(StringProp,'1')", 7);
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsEntityCollection()/$count", 10);
                data.Add("DollarCountEntities/Default.BoundFunctionReturnsEntityCollection()/Microsoft.Test.AspNet.OData.Routing.DerivedDollarCountEntity/$count", 5);

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(DollarCountData))]
        public void DollarCount_Works(string uri, int expectedCount)
        {
            // Arrange & Act
            HttpResponseMessage response = _client.GetAsync("http://localhost/odata/" + uri).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            int actualCount = Int32.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public void GetCollection_Works_WithoutDollarCount()
        {
            // Arrange
            var uri = "DollarCountEntities(5)/StringCollectionProp";

            // Act
            HttpResponseMessage response = _client.GetAsync("http://localhost/odata/" + uri).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = response.Content.ReadAsAsync<JObject>().Result;
            Assert.Equal(2, result["value"].Count());
            Assert.Equal("1", result["value"][0]);
            Assert.Equal("2", result["value"][1]);
        }

        [Fact]
        public void Function_Works_WithDollarCountInQueryOption()
        {
            // Arrange
            var uri = "DollarCountEntities/Default.BoundFunctionReturnsComplexCollection()?$count=true";

            // Act
            HttpResponseMessage response = _client.GetAsync("http://localhost/odata/" + uri).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = response.Content.ReadAsAsync<JObject>().Result;
            Assert.Equal(15, result["@odata.count"]);
        }

        [Fact]
        public void GetCount_Throws_DollarCountNotAllowed()
        {
            // Arrange
            var uri = "DollarCountEntities(5)/DollarCountNotAllowedCollectionProp/$count";

            // Act
            HttpResponseMessage response = _client.GetAsync("http://localhost/odata/" + uri).Result;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = response.Content.ReadAsStringAsync().Result;
            Assert.Contains(
                "The query specified in the URI is not valid. Query option 'Count' is not allowed. " +
                "To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
                result);
        }

        public static IEdmModel GetEdmModel()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            var entityCollection = builder.EntitySet<DollarCountEntity>("DollarCountEntities").EntityType.Collection;

            // Add unbound functions that return collection.
            FunctionConfiguration function = builder.Function("UnboundFunctionReturnsPrimitveCollection");
            function.IsComposable = true;
            function.ReturnsCollection<int>();

            function = builder.Function("UnboundFunctionReturnsEnumCollection");
            function.IsComposable = true;
            function.ReturnsCollection<Color>();

            function = builder.Function("UnboundFunctionReturnsDateTimeOffsetCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DateTimeOffset>();

            function = builder.Function("UnboundFunctionReturnsDateCollection");
            function.IsComposable = true;
            function.ReturnsCollection<Date>();

            function = builder.Function("UnboundFunctionReturnsComplexCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DollarCountComplex>();

            function = builder.Function("UnboundFunctionReturnsEntityCollection");
            function.IsComposable = true;
            function.ReturnsCollectionFromEntitySet<DollarCountEntity>("DollarCountEntities");

            // Add bound functions that return collection.
            function = entityCollection.Function("BoundFunctionReturnsPrimitveCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DateTimeOffset>();

            function = entityCollection.Function("BoundFunctionReturnsEnumCollection");
            function.IsComposable = true;
            function.ReturnsCollection<Color>();

            function = entityCollection.Function("BoundFunctionReturnsDateTimeOffsetCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DateTimeOffset>();

            function = entityCollection.Function("BoundFunctionReturnsComplexCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DollarCountComplex>();

            function = entityCollection.Function("BoundFunctionReturnsEntityCollection");
            function.IsComposable = true;
            function.ReturnsCollectionFromEntitySet<DollarCountEntity>("DollarCountEntities");

            return builder.GetEdmModel();
        }

        public class DollarCountEntitiesController : TestController
        {
            public IList<DollarCountEntity> Entities;

            public DollarCountEntitiesController()
            {
                Entities = new List<DollarCountEntity>();
                for (int i = 1; i <= 10; i++)
                {
                    if (i % 2 == 0)
                    {
                        var newEntity = new DollarCountEntity
                        {
                            Id = i,
                            StringCollectionProp = Enumerable.Range(1, 2).Select(index => index.ToString()).ToArray(),
                            EnumCollectionProp = new[] { Color.Red, Color.Blue | Color.Green, Color.Green },
                            TimeSpanCollectionProp = Enumerable.Range(1, 4).Select(_ => TimeSpan.Zero).ToArray(),
                            ComplexCollectionProp =
                                Enumerable.Range(1, 5).Select(_ => new DollarCountComplex()).ToArray(),
                            EntityCollectionProp = Entities.ToArray(),
                            DollarCountNotAllowedCollectionProp = new[] { 1, 2, 3, 4 }
                        };
                        Entities.Add(newEntity);
                    }
                    else
                    {
                        var newEntity = new DerivedDollarCountEntity
                        {
                            Id = i,
                            StringCollectionProp = Enumerable.Range(1, 2).Select(index => index.ToString()).ToArray(),
                            EnumCollectionProp = new[] { Color.Red, Color.Blue | Color.Green, Color.Green },
                            TimeSpanCollectionProp = Enumerable.Range(1, 4).Select(_ => TimeSpan.Zero).ToArray(),
                            ComplexCollectionProp =
                                Enumerable.Range(1, 5).Select(_ => new DollarCountComplex()).ToArray(),
                            EntityCollectionProp = Entities.ToArray(),
                            DollarCountNotAllowedCollectionProp = new[] { 1, 2, 3, 4 },
                            DerivedProp = "DerivedProp"
                        };
                        Entities.Add(newEntity);
                    }
                }
            }

            [EnableQuery(PageSize = 3)]
            public ITestActionResult Get()
            {
                return Ok(Entities);
            }

            [EnableQuery]
            public ITestActionResult GetDollarCountEntitiesFromDerivedDollarCountEntity()
            {
                return Ok(Entities.OfType<DerivedDollarCountEntity>());
            }

            [EnableQuery]
            public ITestActionResult Get(int key)
            {
                return Ok(Entities.Single(e => e.Id == key));
            }

            public ITestActionResult GetStringCollectionProp(int key, ODataQueryOptions<string> options)
            {
                IQueryable<string> result = Entities.Single(e => e.Id == key).StringCollectionProp.AsQueryable();

                if (options.Filter != null)
                {
                    result = options.Filter.ApplyTo(result, new ODataQuerySettings()).Cast<string>();
                }

#if NETCORE
                if (Request.ODataFeature().Path.Segments.OfType<CountSegment>().Any())
#else
                if (Request.ODataProperties().Path.Segments.OfType<CountSegment>().Any())
#endif
                {
                    return Ok(result.Count());
                }

                return Ok(result);
            }

            [HttpGet]
            [ODataRoute("DollarCountEntities({key})/EnumCollectionProp/$count")]
            public ITestActionResult GetCountForEnumCollectionProp(int key, ODataQueryOptions<Color> options)
            {
                IQueryable<Color> result = Entities.Single(e => e.Id == key).EnumCollectionProp.AsQueryable();

                if (options.Filter != null)
                {
                    result = options.Filter.ApplyTo(result, new ODataQuerySettings()).Cast<Color>();
                }

                return Ok(result.Count());
            }

            [EnableQuery]
            public ITestActionResult GetTimeSpanCollectionProp(int key)
            {
                return Ok(Entities.Single(e => e.Id == key).TimeSpanCollectionProp);
            }

            [EnableQuery]
            public ITestActionResult GetComplexCollectionProp(int key)
            {
                return Ok(Entities.Single(e => e.Id == key).ComplexCollectionProp);
            }

            [EnableQuery]
            public ITestActionResult GetEntityCollectionProp(int key)
            {
                return Ok(Entities.Single(e => e.Id == key).EntityCollectionProp);
            }

            [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All ^ AllowedQueryOptions.Count)]
            public ITestActionResult GetDollarCountNotAllowedCollectionProp(int key)
            {
                return Ok(Entities.Single(e => e.Id == key).EntityCollectionProp);
            }

            [HttpGet]
            [ODataRoute("UnboundFunctionReturnsPrimitveCollection()/$count")]
            public ITestActionResult UnboundFunctionReturnsPrimitveCollectionWithDollarCount()
            {
                return Ok(6);
            }

            [HttpGet]
            [ODataRoute("UnboundFunctionReturnsEnumCollection()/$count")]
            public ITestActionResult UnboundFunctionReturnsEnumCollectionWithDollarCount()
            {
                return Ok(7);
            }

            [HttpGet]
            [ODataRoute("UnboundFunctionReturnsDateTimeOffsetCollection()/$count")]
            public ITestActionResult UnboundFunctionReturnsDateTimeOffsetCollectionWithDollarCount()
            {
                return Ok(8);
            }

            [HttpGet]
            [ODataRoute("UnboundFunctionReturnsDateCollection()/$count")]
            public ITestActionResult UnboundFunctionReturnsDateCollectionWithDollarCount()
            {
                return Ok(18);
            }

            [HttpGet]
            [ODataRoute("UnboundFunctionReturnsComplexCollection()/$count")]
            public ITestActionResult UnboundFunctionReturnsComplexCollectionWithDollarCount()
            {
                return Ok(9);
            }

            [HttpGet]
            [ODataRoute("UnboundFunctionReturnsEntityCollection()/$count")]
            public ITestActionResult UnboundFunctionReturnsEntityCollectionWithDollarCount()
            {
                return Ok(10);
            }

            [HttpGet]
            [ODataRoute("UnboundFunctionReturnsEntityCollection()/Microsoft.Test.AspNet.OData.Routing.DerivedDollarCountEntity/$count")]
            public ITestActionResult UnboundFunctionReturnsDerivedEntityCollectionWithDollarCount()
            {
                return Ok(11);
            }

            [HttpGet]
            [EnableQuery]
            public ITestActionResult BoundFunctionReturnsPrimitveCollectionOnCollectionOfDollarCountEntity()
            {
                return Ok(Enumerable.Range(1, 12).Select(_ => DateTimeOffset.Now));
            }

            [HttpGet]
            [EnableQuery]
            public ITestActionResult BoundFunctionReturnsEnumCollectionOnCollectionOfDollarCountEntity()
            {
                return Ok(Enumerable.Range(1, 13).Select(_ => Color.Green));
            }

            [HttpGet]
            [EnableQuery]
            public ITestActionResult BoundFunctionReturnsDateTimeOffsetCollectionOnCollectionOfDollarCountEntity()
            {
                return Ok(Enumerable.Range(1, 14).Select(_ => DateTimeOffset.Now));
            }

            [HttpGet]
            [EnableQuery]
            public ITestActionResult BoundFunctionReturnsComplexCollectionOnCollectionOfDollarCountEntity()
            {
                return Ok(Enumerable.Range(1, 15).Select(i => new DollarCountComplex { StringProp = i.ToString() }));
            }

            [HttpGet]
            [EnableQuery]
            public ITestActionResult BoundFunctionReturnsEntityCollectionOnCollectionOfDollarCountEntity()
            {
                return Ok(Entities);
            }

            [HttpGet]
            [EnableQuery]
            [ODataRoute("DollarCountEntities/Default.BoundFunctionReturnsEntityCollection()/Microsoft.Test.AspNet.OData.Routing.DerivedDollarCountEntity/$count")]
            public ITestActionResult BoundFunctionReturnsDerivedEntityCollectionOnCollectionOfDollarCountEntity()
            {
                return Ok(Entities.OfType<DerivedDollarCountEntity>());
            }
        }

        public class DollarCountEntity
        {
            public int Id { get; set; }
            public string[] StringCollectionProp { get; set; }
            public Color[] EnumCollectionProp { get; set; }
            public TimeSpan[] TimeSpanCollectionProp { get; set; }
            public DollarCountComplex[] ComplexCollectionProp { get; set; }
            public DollarCountEntity[] EntityCollectionProp { get; set; }
            public int[] DollarCountNotAllowedCollectionProp { get; set; }
        }

        public class DerivedDollarCountEntity : DollarCountEntity
        {
            public string DerivedProp { get; set; }
        }

        public class DollarCountComplex
        {
            public string StringProp { get; set; }
        }
    }
}