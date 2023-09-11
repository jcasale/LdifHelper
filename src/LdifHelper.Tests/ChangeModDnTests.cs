namespace LdifHelper.Tests;

using System;
using Xunit;

/// <summary>
/// Represents change type moddn tests.
/// </summary>
public class ChangeModDnTests
{
    private const string DistinguishedName = "CN=Sophie Germain,OU=users,DC=company,DC=com";

    private static readonly string DumpNewRdn = string.Join(
        Environment.NewLine,
        "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
        "changetype: modrdn",
        "newrdn: CN=Emmy Noether",
        "deleteoldrdn: 0",
        string.Empty);

    private static readonly string DumpNewRdnAndNewSuperior = string.Join(
        Environment.NewLine,
        "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
        "changetype: modrdn",
        "newrdn: CN=Emmy Noether",
        "deleteoldrdn: 1",
        "newsuperior: OU=admins,DC=company,DC=com",
        string.Empty);

    private static readonly string DumpNewSuperior = string.Join(
        Environment.NewLine,
        "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
        "changetype: modrdn",
        "newrdn: CN=Sophie Germain",
        "deleteoldrdn: 1",
        "newsuperior: OU=admins,DC=company,DC=com",
        string.Empty);

    private const string NewRdn = "CN=Emmy Noether";

    private const string NewSuperior = "OU=admins,DC=company,DC=com";

    private const string Rdn = "CN=Sophie Germain";

    /// <summary>
    /// Ensures the constructor rejects an invalid distinguished name.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CtorParameterDistinguishedNameInvalidThrows(string distinguishedName) =>
        Assert.Throws<ArgumentException>(() => new ChangeModDn(distinguishedName, NewRdn, false, null));

    /// <summary>
    /// Ensures the constructor rejects an invalid new rdn.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CtorParameterNewRdnInvalidThrows(string newRdn) =>
        Assert.Throws<ArgumentException>(() => new ChangeModDn(DistinguishedName, newRdn, false, null));

    /// <summary>
    /// Ensures the DistinguishedName property is valid.
    /// </summary>
    [Fact]
    public void PropertyDistinguishedNameIsValid()
    {
        // Act.
        var sut = new ChangeModDn(DistinguishedName, NewRdn, false, null);

        // Assert.
        Assert.Equal(DistinguishedName, sut.DistinguishedName);
    }

    /// <summary>
    /// Ensures the NewRdn property is valid.
    /// </summary>
    [Fact]
    public void PropertyNewRdnIsValid()
    {
        // Act.
        var sut = new ChangeModDn(DistinguishedName, NewRdn, false, null);

        // Assert.
        Assert.Equal(NewRdn, sut.NewRdn);
    }

    /// <summary>
    /// Ensures the NewSuperior property is valid.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData(NewSuperior)]
    public void PropertyNewSuperiorIsValid(string newSuperior)
    {
        // Act.
        var sut = new ChangeModDn(DistinguishedName, Rdn, false, newSuperior);

        // Assert.
        Assert.Equal(newSuperior, sut.NewSuperior);
    }

    /// <summary>
    /// Ensures a new relative distinguished name operation is valid.
    /// </summary>
    [Fact]
    public void ShouldProcessNewRdn()
    {
        // Act.
        var sut = new ChangeModDn(DistinguishedName, NewRdn, false, null);

        // Assert.
        Assert.Equal(NewRdn, sut.NewRdn);
        Assert.False(sut.DeleteOldRdn);
        Assert.Null(sut.NewSuperior);
        Assert.Equal(DumpNewRdn, sut.Dump());
    }

    /// <summary>
    /// Ensures a new relative distinguished name operation is valid.
    /// </summary>
    [Fact]
    public void ShouldProcessNewRdnAndNewSuperior()
    {
        // Act.
        var sut = new ChangeModDn(DistinguishedName, NewRdn, true, NewSuperior);

        // Assert.
        Assert.Equal(NewRdn, sut.NewRdn);
        Assert.True(sut.DeleteOldRdn);
        Assert.Equal(NewSuperior, sut.NewSuperior);
        Assert.Equal(DumpNewRdnAndNewSuperior, sut.Dump());
    }

    /// <summary>
    /// Ensures a move within the DIT is valid.
    /// </summary>
    [Fact]
    public void ShouldProcessNewSuperior()
    {
        // Act.
        var sut = new ChangeModDn(DistinguishedName, Rdn, true, NewSuperior);

        // Assert.
        Assert.Equal(Rdn, sut.NewRdn);
        Assert.True(sut.DeleteOldRdn);
        Assert.Equal(NewSuperior, sut.NewSuperior);
        Assert.Equal(DumpNewSuperior, sut.Dump());
    }
}