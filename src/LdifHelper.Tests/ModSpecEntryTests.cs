namespace LdifHelper.Tests;

using System;
using System.Linq;
using Xunit;

/// <summary>
/// Represents mod-spec entry tests.
/// </summary>
public class ModSpecEntryTests
{
    /// <summary>
    /// Ensures the constructor rejects an invalid mod-spec.
    /// </summary>
    [Fact]
    public void CtorParameterModSpecInvalidThrows()
    {
        // Arrange.
        var modSpec = Enum.GetValues(typeof(ModSpecType)).Cast<int>().Max() + 1;

        // Assert.
        Assert.Throws<ArgumentOutOfRangeException>(() => new ModSpec((ModSpecType)modSpec, "description", new[] { "Contractor" }));
    }

    /// <summary>
    /// Ensures the constructor rejects an empty attribute type.
    /// </summary>
    [Fact]
    public void CtorParameterTypeEmptyThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ModSpec(ModSpecType.Add, string.Empty, new[] { "Contractor" }));

    /// <summary>
    /// Ensures the constructor rejects a null attribute type.
    /// </summary>
    [Fact]
    public void CtorParameterTypeNullThrows() =>
        Assert.Throws<ArgumentNullException>(() => new ModSpec(ModSpecType.Add, null, new[] { "Contractor" }));

    /// <summary>
    /// Ensures the constructor rejects a white space attribute type.
    /// </summary>
    [Fact]
    public void CtorParameterTypeWhiteSpaceThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ModSpec(ModSpecType.Add, " ", new[] { "Contractor" }));

    /// <summary>
    /// Ensures the constructor rejects an add mod-spec with an empty collection of attribute values.
    /// </summary>
    [Fact]
    public void CtorParameterValuesEmptyWhenModSpecIsAddThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ModSpec(ModSpecType.Add, "description", Array.Empty<object>()));

    /// <summary>
    /// Ensures the constructor rejects an add mod-spec with a null collection of attribute values.
    /// </summary>
    [Fact]
    public void CtorParameterValuesNullWhenModSpecIsAddThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ModSpec(ModSpecType.Add, "description", null));

    /// <summary>
    /// Ensures the Count property is valid.
    /// </summary>
    [Fact]
    public void PropertyCountIsValid()
    {
        // Arrange.
        const string attributeType = "description";
        const string attributeValueA = "Contractor";
        const string attributeValueB = "Remote";
        object[] values = { attributeValueA, attributeValueB };

        // Act.
        var sut = new ModSpec(ModSpecType.Add, attributeType, values);

        // Assert.
        Assert.Equal(2, sut.Count);
    }

    /// <summary>
    /// Ensures the ModSpec property is valid.
    /// </summary>
    [Fact]
    public void PropertyModSpecIsValid()
    {
        // Arrange.
        const string attributeType = "description";
        const string attributeValue = "Contractor";
        object[] values = { attributeValue };

        // Act.
        var sut = new ModSpec(ModSpecType.Add, attributeType, values);

        // Assert.
        Assert.Equal(ModSpecType.Add, sut.ModSpecType);
    }

    /// <summary>
    /// Ensures the ModSpec property is valid.
    /// </summary>
    [Fact]
    public void PropertyTypeIsValid()
    {
        // Arrange.
        const string attributeType = "description";
        const string attributeValue = "Contractor";
        object[] values = { attributeValue };

        // Act.
        var sut = new ModSpec(ModSpecType.Add, attributeType, values);

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
        string[] values = { attributeValueA, attributeValueB };

        // Act.
        var modSpec = new ModSpec(ModSpecType.Add, attributeType, values);
        var sut = modSpec.AttributeValues.Select(x => (string)x).ToArray();

        // Assert.
        Assert.Equal(values, sut);
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
        string[] values = { attributeValueA, attributeValueB };

        // Act.
        var modSpec = new ModSpec(ModSpecType.Add, attributeType, values);
        var sut = modSpec.AttributeValues.Select(x => (string)x).ToArray();

        // Arrange.
        Assert.Equal(values, sut);
    }
}