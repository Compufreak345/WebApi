﻿using System;
using System.Web.Http;
using Microsoft.OData.WebApi;
using Microsoft.OData.WebApi.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Newtonsoft.Json.Linq;

namespace WebStack.QA.Test.OData.Swagger
{
    public class SwaggerController : ApiController
    {
        private static readonly Version _defaultEdmxVersion = new Version(4, 0);

        [EnableQuery]
        public JObject GetSwagger()
        {
            IEdmModel model = Request.GetModel();
            model.SetEdmxVersion(_defaultEdmxVersion);
            ODataSwaggerConverter converter = new ODataSwaggerConverter(model);
            return converter.GetSwaggerModel();
        }
    }
}
