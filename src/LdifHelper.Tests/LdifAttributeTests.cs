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
    /// Ensures the constructor rejects an invalid attribute type.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CtorParameterAttributeTypeInvalidThrows(string attributeType) =>
        Assert.Throws<ArgumentException>(() => new LdifAttribute(attributeType, null));

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
        Assert.Equal(1, sutSingleValue.Count);
        Assert.Equal(0, sutEmptyValue.Count);
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