// -----------------------------------------------------------------------
//  <copyright file="LdifAttributeTests.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper.Tests;

using System;
using Xunit;

/// <summary>
/// Represents LDIF attribute tests.
/// </summary>
public class LdifAttributeTests
{
    /// <summary>
    /// Ensures the indexer is valid.
    /// </summary>
    [Fact]
    public void CollectionIndexerIsValid()
    {
        // Arrange.
        const string attributeType = "description";
        const string attributeValueA = "Contractor";
        const string attributeValueB = "Remote";
        object[] values = { attributeValueA, attributeValueB };

        // Act.
        var sut = new LdifAttribute(attributeType, values);

        // Assert.
        Assert.Equal(attributeValueA, sut[0]);
        Assert.Equal(attributeValueB, sut[1]);
    }

    /// <summary>
    /// Ensures the constructor rejects an empty attribute type.
    /// </summary>
    [Fact]
    public void CtorParameterTypeEmptyThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new LdifAttribute(string.Empty, null));

    /// <summary>
    /// Ensures the constructor rejects a null attribute type.
    /// </summary>
    [Fact]
    public void CtorParameterTypeNullThrows() =>
        Assert.Throws<ArgumentNullException>(() => new LdifAttribute(null, null));

    /// <summary>
    /// Ensures the constructor rejects a white space attribute type.
    /// </summary>
    [Fact]
    public void CtorParameterTypeWhiteSpaceThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new LdifAttribute(" ", null));

    /// <summary>
    /// Ensures the Count property is valid.
    /// </summary>
    [Fact]
    public void PropertyCountIsValid()
    {
        // Arrange.
        const string attributeType = "description";
        const string attributeValue = "Contractor";
        object[] values = { attributeValue };

        // Act.
        var sutSingleValue = new LdifAttribute(attributeType, values);
        var sutEmptyValue = new LdifAttribute(attributeType, null);

        // Assert.
        Assert.Single(sutSingleValue);
        Assert.Empty(sutEmptyValue);
    }

    /// <summary>
    /// Ensures the Type property is valid.
    /// </summary>
    [Fact]
    public void PropertyTypeIsValid()
    {
        // Arrange.
        const string attributeType = "description";
        const string attributeValue = "Contractor";
        object[] values = { attributeValue };

        // Act.
        var sut = new LdifAttribute(attributeType, values);

        // Assert.
        Assert.Equal(attributeType, sut.AttributeType);
    }

    /// <summary>
    /// Ensures the Values property is valid.
    /// </summary>
    [Fact]
    public void PropertyValuesIsValid()
    {
        // Arrange.
        const string attributeType = "description";
        const string attributeValueA = "Contractor";
        const string attributeValueB = "Remote";
        object[] values = { attributeValueA, attributeValueB };

        // Act.
        var sut = new LdifAttribute(attributeType, values);

        // Assert.
        Assert.Equal(values, sut.AttributeValues);
    }

    /// <summary>
    /// Ensures the object is a collection of type <see cref="object"/>.
    /// </summary>
    [Fact]
    public void ShouldBeEnumerable()
    {
        // Arrange.
        const string attributeType = "description";
        const string attributeValueA = "Contractor";
        const string attributeValueB = "Remote";
        object[] values = { attributeValueA, attributeValueB };

        // Act.
        var sut = new LdifAttribute(attributeType, values);

        // Assert.
        Assert.Equal(values, sut);
    }
}