namespace LdifHelper.Tests;

using System;
using Xunit;

/// <summary>
/// Represents change type delete tests.
/// </summary>
public class ChangeDeleteTests
{
    private const string DistinguishedName = "CN=Ada Lovelace,OU=users,DC=company,DC=com";

    private static readonly string Dump = string.Join(
        Environment.NewLine,
        "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
        "changetype: delete",
        string.Empty);

    /// <summary>
    /// Ensures the constructor rejects an invalid distinguished name.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CtorParameterDistinguishedNameInvalidThrows(string distinguishedName) =>
        Assert.Throws<ArgumentException>(() => new ChangeDelete(distinguishedName));

    /// <summary>
    /// Ensures the Dump method is valid.
    /// </summary>
    [Fact]
    public void MethodDumpIsValid()
    {
        // Act.
        var sut = new ChangeDelete(DistinguishedName);

        // Assert.
        Assert.Equal(Dump, sut.Dump());
    }

    /// <summary>
    /// Ensures the DistinguishedName property is valid.
    /// </summary>
    [Fact]
    public void PropertyDistinguishedNameIsValid()
    {
        // Act.
        var sut = new ChangeDelete(DistinguishedName);

        // Assert.
        Assert.Equal(DistinguishedName, sut.DistinguishedName);
    }
}