﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Extensions;

namespace Microsoft.OData.WebApi.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> to serialize <see cref="ODataError"/>s.
    /// </summary>
    public partial class ODataErrorSerializer
    {
        /// <summary>
        /// Return true of the object is an HttpError.
        /// </summary>
        /// <param name="error">The error to test.</param>
        /// <returns>true of the object is an HttpError</returns>
        public static bool IsHttpError(object error)
        {
            return error is HttpError;
        }

        /// <summary>
        /// Create an ODataError from an HttpError.
        /// </summary>
        /// <param name="error">The error to use.</param>
        /// <returns>an ODataError.</returns>
        public static ODataError CreateODataError(object error)
        {
            HttpError httpError = error as HttpError;
            return httpError.CreateODataError();
        }
    }
}