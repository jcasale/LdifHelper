// -----------------------------------------------------------------------
//  <copyright file="ChangeModDnTests.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

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
    /// Ensures the constructor rejects an empty distinguished name.
    /// </summary>
    [Fact]
    public void CtorParameterDistinguishedNameEmptyThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeModDn(string.Empty, NewRdn, false, null));

    /// <summary>
    /// Ensures a distinguished name with an invalid format is rejected.
    /// </summary>
    [Fact]
    public void CtorParameterDistinguishedNameInvalidFormatThrows() =>
        Assert.Throws<ArgumentException>(() => new ChangeModDn("CN=Sophie Germain", null, false, NewSuperior));

    /// <summary>
    /// Ensures the constructor rejects a null distinguished name.
    /// </summary>
    [Fact]
    public void CtorParameterDistinguishedNameNullThrows() =>
        Assert.Throws<ArgumentNullException>(() => new ChangeModDn(null, NewRdn, false, null));

    /// <summary>
    /// Ensures the constructor rejects a white space distinguished name.
    /// </summary>
    [Fact]
    public void CtorParameterDistinguishedNameWhiteSpaceThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeModDn(" ", NewRdn, false, null));

    /// <summary>
    /// Ensures a record with an empty new rdn and a null new superior is rejected.
    /// </summary>
    [Fact]
    public void CtorParametersNewRdnIsEmptyAndNewSuperiorIsNullThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeModDn(DistinguishedName, string.Empty, false, null));

    /// <summary>
    /// Ensures a record with a null new rdn and an empty new superior is rejected.
    /// </summary>
    [Fact]
    public void CtorParametersNewRdnIsNullAndNewSuperiorIsEmptyThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeModDn(DistinguishedName, null, false, string.Empty));

    /// <summary>
    /// Ensures a record with a null new rdn and a null new superior is rejected.
    /// </summary>
    [Fact]
    public void CtorParametersNewRdnIsNullAndNewSuperiorIsNullThrows() =>
        Assert.Throws<ArgumentException>(() => new ChangeModDn(DistinguishedName, null, false, null));

    /// <summary>
    /// Ensures a record with a null new rdn and an empty new superior is rejected.
    /// </summary>
    [Fact]
    public void CtorParametersNewRdnIsNullAndNewSuperiorIsWhiteSpaceThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeModDn(DistinguishedName, null, false, " "));

    /// <summary>
    /// Ensures a record with a null new rdn and a null new superior is rejected.
    /// </summary>
    [Fact]
    public void CtorParametersNewRdnIsUnchangedAndNewSuperiorIsNullThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeModDn(DistinguishedName, Rdn, false, null));

    /// <summary>
    /// Ensures a record with a white space new rdn and a null new superior is rejected.
    /// </summary>
    [Fact]
    public void CtorParametersNewRdnIsWhiteSpaceAndNewSuperiorIsNullThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeModDn(DistinguishedName, " ", false, null));

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
        var sut = new ChangeModDn(DistinguishedName, null, true, NewSuperior);

        // Assert.
        Assert.Equal(Rdn, sut.NewRdn);
        Assert.True(sut.DeleteOldRdn);
        Assert.Equal(NewSuperior, sut.NewSuperior);
        Assert.Equal(DumpNewSuperior, sut.Dump());
    }
}