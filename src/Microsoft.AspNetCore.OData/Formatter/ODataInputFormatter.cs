﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using ODataPath = Microsoft.OData.WebApi.Routing.ODataPath;

namespace Microsoft.AspNetCore.OData.Formatter
{
    public class ODataInputFormatter : TextInputFormatter
    {
        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            try
            {
                object value = ReadRequestBody(context);
                return InputFormatterResult.SuccessAsync(value);
            }
            catch (Exception)
            {
                return InputFormatterResult.FailureAsync();
            }
        }

        private object ReadRequestBody(InputFormatterContext contex)
        {
            return null;
        }
    }
}