﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions.Attributes;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.TestCommon;
using Moq;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Builder.Conventions.Attributes
{
    public class TimestampAttributeEdmPropertyConventionTests
    {
        [Fact]
        public void TimestampConvention_AppliesWhenTheAttributeIsAppliedToASingleProperty()
        {
            // Arrange
            PropertyInfo property = CreateMockPropertyInfo("TestProperty");
            EntityTypeConfiguration entityType = new EntityTypeConfiguration();
            PrimitivePropertyConfiguration primitiveProperty = new PrimitivePropertyConfiguration(property, entityType);
            entityType.ExplicitProperties.Add(property, primitiveProperty);
            TimestampAttributeEdmPropertyConvention convention = new TimestampAttributeEdmPropertyConvention();

            // Act
            convention.Apply(primitiveProperty, entityType, new ODataConventionModelBuilder());

            // Assert
            Assert.True(primitiveProperty.ConcurrencyToken);
        }

        [Fact]
        public void TimestampConvention_DoesntApplyWhenTheAttributeIsAppliedOnANonEntityType()
        {
            // Arrange
            PropertyInfo property = CreateMockPropertyInfo("TestProperty");
            ComplexTypeConfiguration complexType = new ComplexTypeConfiguration();
            PrimitivePropertyConfiguration primitiveProperty = new PrimitivePropertyConfiguration(property, complexType);
            complexType.ExplicitProperties.Add(property, primitiveProperty);
            TimestampAttributeEdmPropertyConvention convention = new TimestampAttributeEdmPropertyConvention();

            // Act
            convention.Apply(primitiveProperty, complexType, new ODataConventionModelBuilder());

            // Assert
            Assert.False(primitiveProperty.ConcurrencyToken);
        }

        [Fact]
        public void TimestampConvention_DoesntApplyWhenTheAttributeIsAppliedToMultipleProperties()
        {
            // Arrange
            PropertyInfo property = CreateMockPropertyInfo("TestProperty");
            PropertyInfo otherProperty = CreateMockPropertyInfo("OtherTestProperty");
            EntityTypeConfiguration entityType = new EntityTypeConfiguration();
            PrimitivePropertyConfiguration primitiveProperty = new PrimitivePropertyConfiguration(property, entityType);
            entityType.ExplicitProperties.Add(property, primitiveProperty);
            entityType.ExplicitProperties.Add(otherProperty, new PrimitivePropertyConfiguration(otherProperty, entityType));
            TimestampAttributeEdmPropertyConvention convention = new TimestampAttributeEdmPropertyConvention();

            // Act
            convention.Apply(primitiveProperty, entityType, new ODataConventionModelBuilder());

            // Assert
            Assert.False(primitiveProperty.ConcurrencyToken);
        }

        [Fact]
        public void TimestampConvention_DoesntApplyWhenTheAttributeIsAppliedToMultipleProperties_InATypeHierarchy()
        {
            // Arrange
            PropertyInfo property = CreateMockPropertyInfo("TestProperty");
            PropertyInfo otherProperty = CreateMockPropertyInfo("OtherTestProperty");
            ODataModelBuilder modelBuilder = new ODataConventionModelBuilder();
            EntityTypeConfiguration baseEntityType = new EntityTypeConfiguration(modelBuilder, typeof(object));

            //Mock<EntityTypeConfiguration> mockEntityType = new Mock<EntityTypeConfiguration>().SetupAllProperties();
            //mockEntityType.Setup(c => c.BaseType).Returns(baseEntityType);
            //mockEntityType.SetupGet(c => c.Kind).Returns(EdmTypeKind.Entity);

            //EntityTypeConfiguration entityType = mockEntityType.Object;
            EntityTypeConfiguration entityType = new EntityTypeConfiguration(modelBuilder, typeof(Int32));
            entityType.BaseType = baseEntityType;

            PrimitivePropertyConfiguration primitiveProperty = new PrimitivePropertyConfiguration(property, entityType);
            entityType.ExplicitProperties.Add(property, primitiveProperty);
            baseEntityType.ExplicitProperties.Add(otherProperty, new PrimitivePropertyConfiguration(otherProperty, baseEntityType));
            TimestampAttributeEdmPropertyConvention convention = new TimestampAttributeEdmPropertyConvention();

            // Act
            convention.Apply(primitiveProperty, entityType, new ODataConventionModelBuilder());

            // Assert
            Assert.False(primitiveProperty.ConcurrencyToken);
        }

        private static PropertyInfo CreateMockPropertyInfo(string propertyName)
        {
            Mock<PropertyInfo> property = new Mock<PropertyInfo>();
            property.Setup(p => p.Name).Returns(propertyName);
            property.Setup(p => p.PropertyType).Returns(typeof(int));
            property.Setup(p => p.GetCustomAttributes(It.IsAny<bool>())).Returns(new object[] { new TimestampAttribute() });
            property.Setup(p => p.GetCustomAttributes(typeof(TimestampAttribute), It.IsAny<bool>())).Returns(new object[] { new TimestampAttribute() });
            return property.Object;
        }
    }
}