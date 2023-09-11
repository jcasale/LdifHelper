namespace LdifHelper.Tests;

using System;
using Xunit;

/// <summary>
/// Represents comparer tests.
/// </summary>
public class ComparerTests
{
    /// <summary>
    /// Ensures the constructor rejects an invalid argument.
    /// </summary>
    [Theory]
    [InlineData(null, "valid")]
    [InlineData("", "valid")]
    [InlineData(" ", "valid")]
    [InlineData("valid", null)]
    [InlineData("valid", "")]
    [InlineData("valid", " ")]
    public void CtorParameterInvalidThrows(string x, string y) =>
        Assert.Throws<ArgumentException>(() => AttributeTypeComparer.GetComparer.Compare(x, y));

    /// <summary>
    /// Ensures the comparer orders objectClass correctly.
    /// </summary>
    [Fact]
    public void ObjectClassOrderIsValid()
    {
        Assert.Equal(0, AttributeTypeComparer.GetComparer.Compare("objectClass", "ObjectClass"));
        Assert.Equal(-1, AttributeTypeComparer.GetComparer.Compare("objectClass", "description"));
        Assert.Equal(1, AttributeTypeComparer.GetComparer.Compare("description", "objectClass"));
    }

    /// <summary>
    /// Ensures the comparer orders correctly.
    /// </summary>
    [Fact]
    public void ShouldBeOrdered()
    {
        // Arrange.
        string[] typesOrdered =
        {
            "objectClass",
            "displayName",
            "givenName",
            "sn"
        };

        string[] types =
        {
            "sn",
            "displayName",
            "objectClass",
            "givenName"
        };

        // Act.
        Array.Sort(types, AttributeTypeComparer.GetComparer);

        // Assert.
        Assert.Equal(typesOrdered[0], types[0]);
        Assert.Equal(typesOrdered[1], types[1]);
        Assert.Equal(typesOrdered[2], types[2]);
        Assert.Equal(typesOrdered[3], types[3]);
    }
}