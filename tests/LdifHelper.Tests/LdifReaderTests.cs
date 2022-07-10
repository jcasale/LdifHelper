// -----------------------------------------------------------------------
//  <copyright file="LdifReaderTests.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LdifHelper;
using Xunit;

/// <summary>
/// Represents LDIF reader tests.
/// </summary>
public class LdifReaderTests
{
    /// <summary>
    /// Ensures a change-add record is read correctly.
    /// </summary>
    [Fact]
    public void ChangeAddRecordIsValid()
    {
        // Arrange.
        const string distinguishedName = "CN=Émilie du Châtelet,OU=users,DC=company,DC=com";
        const string typeDisplayName = "displayName";
        object[] valuesDisplayName = { "Émilie du Châtelet" };
        const string typeSn = "sn";
        object[] valuesSn = { "du Châtelet" };
        const string typeGivenName = "givenName";
        object[] valuesGivenName = { "Émilie" };
        const string typeObjectClass = "objectClass";
        object[] valuesObjectClass = { "top", "person", "organizationalPerson", "user" };
        var dump = string.Join(
            Environment.NewLine,
            "dn:: Q049w4ltaWxpZSBkdSBDaMOidGVsZXQsT1U9dXNlcnMsREM9Y29tcGFueSxEQz1jb20=",
            "objectClass: top",
            "objectClass: person",
            "objectClass: organizationalPerson",
            "objectClass: user",
            "displayName:: w4ltaWxpZSBkdSBDaMOidGVsZXQ=",
            "givenName:: w4ltaWxpZQ==",
            "sn:: ZHUgQ2jDonRlbGV0",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeAdd)records[0];

        // Assert.
        Assert.Equal(distinguishedName, sut.DistinguishedName);
        Assert.Equal(valuesObjectClass, sut[typeObjectClass]);
        Assert.Equal(valuesDisplayName, sut[typeDisplayName].Select(s => (object)Encoding.UTF8.GetString((byte[])s)).ToArray());
        Assert.Equal(valuesGivenName, sut[typeGivenName].Select(s => (object)Encoding.UTF8.GetString((byte[])s)).ToArray());
        Assert.Equal(valuesSn, sut[typeSn].Select(s => (object)Encoding.UTF8.GetString((byte[])s)).ToArray());
        Assert.Equal(dump, sut.Dump());
    }

    /// <summary>
    /// Ensures the reader correctly unfolds a change-add record value-spec.
    /// </summary>
    [Fact]
    public void ChangeAddValueSpecIsUnFolded()
    {
        // Arrange.
        const string typeDisplayName = "displayName";
        string[] valuesDisplayName = { "Ada Lovelace" };
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "objectClass: top",
            "objectClass: person",
            "objectClass: organizationalPerson",
            "objectClass: user",
            "displayName: Ada",
            "  Lovelace",
            "givenName: Ada",
            "sn: Lovelace",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeAdd)records[0];

        // Assert.
        Assert.Equal(valuesDisplayName, sut[typeDisplayName].Select(s => (string)s).ToArray());
    }

    /// <summary>
    /// Ensures a change-delete record is read correctly.
    /// </summary>
    [Fact]
    public void ChangeDeleteRecordIsValid()
    {
        // Arrange.
        const string distinguishedName = "CN=Ada Lovelace,OU=users,DC=company,DC=com";
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: delete",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeDelete)records[0];

        // Assert.
        Assert.Equal(distinguishedName, sut.DistinguishedName);
        Assert.Equal(dump, sut.Dump());
    }

    /// <summary>
    /// Ensures the reader rejects a change-moddn record with an invalid deleteoldrdn value.
    /// </summary>
    [Fact]
    public void ChangeModDnDeleteOldRdnInvalidValueThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
            "changetype: modrdn",
            "newrdn: CN=Emmy Noether",
            "deleteoldrdn: x",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader rejects a change-moddn record with an invalid field.
    /// </summary>
    [Fact]
    public void ChangeModDnInvalidRecordThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
            "changetype: modrdn",
            "newrdn: CN=Emmy Noether",
            "invalid: line",
            "deleteoldrdn: 0",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader rejects a change-moddn record with an invalid UTF8 encoded newrdn.
    /// </summary>
    [Fact]
    public void ChangeModDnNewRdnInvalidUtf8Throws()
    {
        // Arrange.
        const string distinguishedName = "CN=Émilie du Châtelet,OU=users,DC=company,DC=com";

        const string newRdn = "CN=Irmgard Flügge-Lotz";
        var newRdnUtf16Bytes = Encoding.Unicode.GetBytes(newRdn);
        var newRdnUtf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, newRdnUtf16Bytes);

        // Invalidate byte array.
        newRdnUtf8Bytes[3] = 0xFF;

        var dump = string.Join(
            Environment.NewLine,
            $"dn:: {distinguishedName.ToBase64()}",
            "changetype: modrdn",
            $"newrdn:: {Convert.ToBase64String(newRdnUtf8Bytes)}",
            "deleteoldrdn: 0",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Exception exception = Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
        Assert.IsType<DecoderFallbackException>(exception.InnerException);
    }

    /// <summary>
    /// Ensures a change-moddn record with a folded newrdn is read correctly.
    /// </summary>
    [Fact]
    public void ChangeModDnNewRdnUnFolds()
    {
        // Arrange.
        const string newRdn = "CN=Maria Gaetana Agnesi";
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: modrdn",
            "newrdn: CN=Maria Gaetana",
            "  Agnesi",
            "deleteoldrdn: 0",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeModDn)records[0];

        // Assert.
        Assert.Equal(newRdn, sut.NewRdn);
    }

    /// <summary>
    /// Ensures the reader rejects a change-moddn record with an invalid UTF8 encoded newsuperior.
    /// </summary>
    [Fact]
    public void ChangeModDnNewSuperiorInvalidUtf8Throws()
    {
        // Arrange.
        const string distinguishedName = "CN=Émilie du Châtelet,OU=users,DC=company,DC=com";

        const string newRdn = "CN=Émilie du Châtelet";

        const string newSuperior = "OU=admins,DC=cȯmpany,DC=com";
        var newSuperiorUtf16Bytes = Encoding.Unicode.GetBytes(newSuperior);
        var newSuperiorUtf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, newSuperiorUtf16Bytes);

        // Invalidate byte array.
        newSuperiorUtf8Bytes[3] = 0xFF;

        var dump = string.Join(
            Environment.NewLine,
            $"dn:: {distinguishedName.ToBase64()}",
            "changetype: modrdn",
            $"newrdn:: {newRdn.ToBase64()}",
            "deleteoldrdn: 0",
            $"newsuperior:: {Convert.ToBase64String(newSuperiorUtf8Bytes)}",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Exception exception = Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
        Assert.IsType<DecoderFallbackException>(exception.InnerException);
    }

    /// <summary>
    /// Ensures a change-moddn record with a folded newsuperior is read correctly.
    /// </summary>
    [Fact]
    public void ChangeModDnNewSuperiorUnFolds()
    {
        // Arrange.
        const string newSuperior = "OU=admins,DC=company,DC=com";
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
            "changetype: modrdn",
            "newrdn: CN=Sophie Germain",
            "deleteoldrdn: 1",
            "newsuperior: OU=admins",
            " ,DC=company,DC=com",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeModDn)records[0];

        // Assert.
        Assert.Equal(newSuperior, sut.NewSuperior);
    }

    /// <summary>
    /// Ensures a change-moddn record with a single newrdn operation is read correctly.
    /// </summary>
    [Fact]
    public void ChangeModDnRecordNewRdnIsValid()
    {
        // Arrange.
        const string distinguishedName = "CN=Sophie Germain,OU=users,DC=company,DC=com";
        const string newRdn = "CN=Emmy Noether";
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
            "changetype: modrdn",
            "newrdn: CN=Emmy Noether",
            "deleteoldrdn: 0",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeModDn)records[0];

        // Assert.
        Assert.Equal(distinguishedName, sut.DistinguishedName);
        Assert.Equal(newRdn, sut.NewRdn);
        Assert.False(sut.DeleteOldRdn);
        Assert.Equal(dump, sut.Dump());
    }

    /// <summary>
    /// Ensures a change-moddn record with a single newsuperior operation is read correctly.
    /// </summary>
    [Fact]
    public void ChangeModDnRecordNewSuperiorIsValid()
    {
        // Arrange.
        const string distinguishedName = "CN=Sophie Germain,OU=users,DC=company,DC=com";
        const string newRdn = "CN=Sophie Germain";
        const string newSuperior = "OU=admins,DC=company,DC=com";
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
            "changetype: modrdn",
            "newrdn: CN=Sophie Germain",
            "deleteoldrdn: 1",
            "newsuperior: OU=admins,DC=company,DC=com",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeModDn)records[0];

        // Assert.
        Assert.Equal(distinguishedName, sut.DistinguishedName);
        Assert.Equal(newRdn, sut.NewRdn);
        Assert.True(sut.DeleteOldRdn);
        Assert.Equal(newSuperior, sut.NewSuperior);
        Assert.Equal(dump, sut.Dump());
    }

    /// <summary>
    /// Ensures a change-modify add record with multiple attribute type entries are read correctly.
    /// </summary>
    [Fact]
    public void ChangeModifyAddDistinctRecordIsValid()
    {
        // Arrange.
        const string distinguishedName = "CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com";
        const string typePostalAddress = "postaladdress";
        string[] valuesPostalAddress = { "2400 Fulton St, San Francisco, CA 94118, USA" };
        const string typeTelephoneNumber = "telephonenumber";
        string[] valuesTelephoneNumber = { "+1 415 555 1234", "+1 628 555 0000" };
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
            "changetype: modify",
            "add: postaladdress",
            "postaladdress: 2400 Fulton St, San Francisco, CA 94118, USA",
            "-",
            "add: telephonenumber",
            "telephonenumber: +1 415 555 1234",
            "telephonenumber: +1 628 555 0000",
            "-",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeModify)records[0];

        // Assert.
        Assert.Equal(distinguishedName, sut.DistinguishedName);
        Assert.Equal(2, sut.Count);

        var modSpecsPostalAddress = sut.ModSpecs.Where(x => x.AttributeType.Equals(typePostalAddress, StringComparison.Ordinal)).ToArray();
        Assert.Single(modSpecsPostalAddress);
        var modSpecPostalAddress = modSpecsPostalAddress[0];
        Assert.Equal(ModSpecType.Add, modSpecPostalAddress.ModSpecType);
        Assert.Equal(valuesPostalAddress, modSpecPostalAddress.AttributeValues.Select(x => (string)x));

        var modSpecsTelephoneNumber = sut.ModSpecs.Where(x => x.AttributeType.Equals(typeTelephoneNumber, StringComparison.Ordinal)).ToArray();
        Assert.Single(modSpecsTelephoneNumber);
        var modSpecTelephoneNumber = modSpecsTelephoneNumber[0];
        Assert.Equal(ModSpecType.Add, modSpecTelephoneNumber.ModSpecType);
        Assert.Equal(valuesTelephoneNumber, modSpecTelephoneNumber.AttributeValues.Select(x => (string)x));

        Assert.Equal(dump, sut.Dump());
    }

    /// <summary>
    /// Ensures a change-modify add record with a single entry containing multiple values is read correctly.
    /// </summary>
    [Fact]
    public void ChangeModifyAddMultipleRecordIsValid()
    {
        // Arrange.
        const string distinguishedName = "CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com";
        const string typeTelephoneNumber = "telephonenumber";
        string[] valuesTelephoneNumber = { "+1 415 555 1234", "+1 628 555 0000" };
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
            "changetype: modify",
            "add: telephonenumber",
            "telephonenumber: +1 415 555 1234",
            "telephonenumber: +1 628 555 0000",
            "-",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeModify)records[0];

        // Assert.
        Assert.Equal(distinguishedName, sut.DistinguishedName);

        Assert.Single(sut);
        var modSpec = sut.ModSpecs.First();
        Assert.Equal(ModSpecType.Add, modSpec.ModSpecType);
        Assert.Equal(typeTelephoneNumber, modSpec.AttributeType);
        Assert.Equal(valuesTelephoneNumber, modSpec.AttributeValues.Select(x => (string)x));

        Assert.Equal(dump, sut.Dump());
    }

    /// <summary>
    /// Ensures a change-modify add record with a single entry is read correctly.
    /// </summary>
    [Fact]
    public void ChangeModifyAddSingleRecordIsValid()
    {
        // Arrange.
        const string distinguishedName = "CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com";
        const string typePostalAddress = "postaladdress";
        string[] valuesPostalAddress = { "2400 Fulton St, San Francisco, CA 94118, USA" };
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
            "changetype: modify",
            "add: postaladdress",
            "postaladdress: 2400 Fulton St, San Francisco, CA 94118, USA",
            "-",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeModify)records[0];

        // Assert.
        Assert.Equal(distinguishedName, sut.DistinguishedName);

        Assert.Single(sut);
        var modSpec = sut.ModSpecs.First();
        Assert.Equal(ModSpecType.Add, modSpec.ModSpecType);
        Assert.Equal(typePostalAddress, modSpec.AttributeType);
        Assert.Equal(valuesPostalAddress, modSpec.AttributeValues.Select(x => (string)x));

        Assert.Equal(dump, sut.Dump());
    }

    /// <summary>
    /// Ensures a change-modify delete record which removes all values is read correctly.
    /// </summary>
    [Fact]
    public void ChangeModifyDeleteAllValuesRecordIsValid()
    {
        // Arrange.
        const string distinguishedName = "CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com";
        const string typeTelephoneNumber = "telephonenumber";
        string[] valuesTelephoneNumber = { "+1 415 555 1234" };
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
            "changetype: modify",
            "delete: telephonenumber",
            "telephonenumber: +1 415 555 1234",
            "-",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeModify)records[0];

        // Assert.
        Assert.Equal(distinguishedName, sut.DistinguishedName);

        Assert.Single(sut);
        var modSpec = sut.ModSpecs.First();
        Assert.Equal(ModSpecType.Delete, modSpec.ModSpecType);
        Assert.Equal(typeTelephoneNumber, modSpec.AttributeType);
        Assert.Equal(valuesTelephoneNumber, modSpec.AttributeValues.Select(x => (string)x));

        Assert.Equal(dump, sut.Dump());
    }

    /// <summary>
    /// Ensures a change-modify delete record which removes a single value is read correctly.
    /// </summary>
    [Fact]
    public void ChangeModifyDeleteSingleValueRecordIsValid()
    {
        // Arrange.
        const string distinguishedName = "CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com";
        const string typePostalAddress = "postaladdress";
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
            "changetype: modify",
            "delete: postaladdress",
            "-",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeModify)records[0];

        // Assert.
        Assert.Equal(distinguishedName, sut.DistinguishedName);

        Assert.Single(sut);
        var modSpec = sut.ModSpecs.First();
        Assert.Equal(ModSpecType.Delete, modSpec.ModSpecType);
        Assert.Equal(typePostalAddress, modSpec.AttributeType);
        Assert.Empty(modSpec.AttributeValues);

        Assert.Equal(dump, sut.Dump());
    }

    /// <summary>
    /// Ensures the reader rejects a change-modify with an unexpected empty line.
    /// </summary>
    [Fact]
    public void ChangeModifyEmptyLineThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: modify",
            "add: postaladdress",
            "postaladdress: 2400 Fulton St, San Francisco, CA 94118, USA",
            string.Empty,
            "-",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader rejects an invalid change-modify attribute type.
    /// </summary>
    [Fact]
    public void ChangeModifyInvalidAttributeTypeThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: modify",
            "add:",
            "postaladdress: 2400 Fulton St, San Francisco, CA 94118, USA",
            "-",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader rejects an invalid change-modify mod-spec.
    /// </summary>
    [Fact]
    public void ChangeModifyInvalidModSpecThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: modify",
            "invalid: postaladdress",
            "postaladdress: 2400 Fulton St, San Francisco, CA 94118, USA",
            "-",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader rejects a change-modify with mixed attribute types.
    /// </summary>
    [Fact]
    public void ChangeModifyMixedAttributeTypeThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: modify",
            "add: postaladdress",
            "postaladdress: 2400 Fulton St, San Francisco, CA 94118, USA",
            "streetaddress: 3127 Fillmore St, San Francisco, CA 94123, USA",
            "-",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader correctly unfolds a change-modify record value-spec.
    /// </summary>
    [Fact]
    public void ChangeModifyValueSpecIsUnFolded()
    {
        // Arrange.
        const string typePostalAddress = "postaladdress";
        string[] valuesPostalAddress = { "2400 Fulton St, San Francisco, CA 94118, USA" };
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: modify",
            "add: postal",
            " address",
            "postaladdress: 2400 Fulton St",
            " , San Francisco",
            " , CA 94118",
            " , USA",
            "-",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeModify)records[0];

        // Assert.
        var modSpec = sut.ModSpecs.First();
        Assert.Equal(ModSpecType.Add, modSpec.ModSpecType);
        Assert.Equal(typePostalAddress, modSpec.AttributeType);
        Assert.Equal(valuesPostalAddress, modSpec.AttributeValues.Select(x => (string)x));
    }

    /// <summary>
    /// Ensures the reader rejects a value-spec with a missing separator.
    /// </summary>
    [Fact]
    public void ChangeModifyValueSpecMissingSeparatorThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "objectclass person",
            "objectclass: organizationalPerson",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader rejects a truncated value-spec.
    /// </summary>
    [Fact]
    public void ChangeModifyValueSpecTruncatedThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            "description::",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader rejects an invalid change type.
    /// </summary>
    [Fact]
    public void ChangeTypeInvalidThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: invalid",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the LineNumber property is valid.
    /// </summary>
    [Fact]
    public void PropertyLineNumberIsValid()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "version: 1",
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act.
        var sut = LdifReader.Parse(streamReader).ToArray();

        // Assert.
        Assert.Single(sut);
    }

    /// <summary>
    /// Ensures the reader rejects an empty distinguished name.
    /// </summary>
    [Fact]
    public void RecordDistinguishedNameEmptyThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn: ",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader rejects a distinguished name with an invalid base64 encoding.
    /// </summary>
    [Fact]
    public void RecordDistinguishedNameInvalidBase64Throws()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn:: invalid",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader rejects a distinguished name with an invalid UTF8 encoding.
    /// </summary>
    [Fact]
    public void RecordDistinguishedNameInvalidUtf8Throws()
    {
        // Arrange.
        const string distinguishedName = "CN=Émilie du Châtelet,OU=users,DC=company,DC=com";
        var utf16Bytes = Encoding.Unicode.GetBytes(distinguishedName);
        var utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);

        // Invalidate byte array.
        utf8Bytes[3] = 0xFF;

        var dump = string.Join(
            Environment.NewLine,
            $"dn:: {Convert.ToBase64String(utf8Bytes)}",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        var exception = Record.Exception(() => LdifReader.Parse(streamReader).ToArray());

        Assert.NotNull(exception);
        Assert.IsType<LdifReaderException>(exception);
        Assert.IsType<DecoderFallbackException>(exception.InnerException);
    }

    /// <summary>
    /// Ensures the reader correctly unfolds a distinguished name.
    /// </summary>
    [Fact]
    public void RecordDistinguishedNameIsUnFolded()
    {
        // Arrange.
        const string distinguishedName = "CN=Pablo Diego José Francisco de Paula Juan Nepomuceno María de los Remedios Cipriano de la Santísima Trinidad Ruiz y Picasso,OU=users,DC=company,DC=com";
        var dump = string.Join(
            Environment.NewLine,
            "dn:: Q049UGFibG8gRGllZ28gSm9zw6kgRnJhbmNpc2NvIGRlIFBhdWxhIEp1YW4gTmVwb211Y2V",
            " ubyBNYXLDrWEgZGUgbG9zIFJlbWVkaW9zIENpcHJpYW5vIGRlIGxhIFNhbnTDrXNpbWEgVHJpbm",
            " lkYWQgUnVpeiB5IFBpY2Fzc28sT1U9dXNlcnMsREM9Y29tcGFueSxEQz1jb20=",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeAdd)records[0];

        // Assert.
        Assert.Equal(distinguishedName, sut.DistinguishedName);
    }

    /// <summary>
    /// Ensures the reader rejects an invalid distinguished name with a missing separator.
    /// </summary>
    [Fact]
    public void RecordDistinguishedNameMissingSeparatorThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader rejects an LDIF record with a missing distinguished name.
    /// </summary>
    [Fact]
    public void RecordDistinguishedNameMissingThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader rejects a white space distinguished name.
    /// </summary>
    [Fact]
    public void RecordDistinguishedNameWhiteSpaceThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn:  ",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader rejects a file URI with an invalid path.
    /// </summary>
    [Fact]
    public void RecordFileUriInvalidPathThrows()
    {
        // Arrange.
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        Assert.False(new FileInfo(path).Exists);

        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            $"description:< {new Uri(path)}",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        var exception = Record.Exception(() => LdifReader.Parse(streamReader, true).ToArray());

        Assert.NotNull(exception);
        Assert.IsType<LdifReaderException>(exception);
        Assert.IsType<FileNotFoundException>(exception.InnerException);
    }

    /// <summary>
    /// Ensures the reader rejects a file URI with an invalid path.
    /// </summary>
    [Fact]
    public void RecordFileUriInvalidUriThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            "jpegphoto:< invalid",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        var exception = Record.Exception(() => LdifReader.Parse(streamReader, true).ToArray());

        Assert.NotNull(exception);
        Assert.IsType<LdifReaderException>(exception);
        Assert.IsType<UriFormatException>(exception.InnerException);
    }

    /// <summary>
    /// Ensures the reader rejects a file URI with an invalid encoding.
    /// </summary>
    [Fact]
    public void RecordFileUriInvalidUtf8Throws()
    {
        // Arrange.
        const string path = @"c:\path\picture.jpeg";
        var utf16Bytes = Encoding.Unicode.GetBytes(new Uri(path).ToString());
        var utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);

        // Invalidate byte array.
        utf8Bytes[utf8Bytes.Length - 1] = 0xFF;

        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            $"jpegphoto::< {Convert.ToBase64String(utf8Bytes)}",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        var exception = Record.Exception(() => LdifReader.Parse(streamReader, true).ToArray());

        Assert.NotNull(exception);
        Assert.IsType<LdifReaderException>(exception);
        Assert.IsType<DecoderFallbackException>(exception.InnerException);
    }

    /// <summary>
    /// Ensures the reader prohibits file URIs if not enabled.
    /// </summary>
    [Fact]
    public void RecordFileUriNotEnabledThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            "jpegphoto:< file://picture.jpg",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures the reader accepts base64 encoded file URIs if enabled.
    /// </summary>
    [Fact]
    public void ShouldAcceptRecordFileUriBase64IfEnabled()
    {
        // Arrange.
        const string typeDescription = "description";
        var valueDescription = string.Join(
            Environment.NewLine,
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus convallis",
            "et erat at mollis. Nullam in risus laoreet, pharetra leo a, volutpat massa. Cras",
            "quis sodales velit. In sit amet augue gravida, sagittis dui a, placerat nunc.",
            "Fusce non nisi vel orci sagittis elementum. Praesent elit nulla, elementum sed",
            "sem a, semper dictum arcu. Duis luctus arcu id arcu scelerisque pharetra. Nunc",
            "a elementum felis, quis auctor diam.");
        string path = null;

        try
        {
            path = Path.GetTempFileName();

            using (var fileStream = new FileStream(path, FileMode.Truncate, FileAccess.Write))
            {
                var bytes = Encoding.UTF8.GetBytes(valueDescription);
                fileStream.Write(bytes, 0, bytes.Length);
            }

            var dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                $"{typeDescription}::< {new Uri(path).ToString().ToBase64()}",
                string.Empty);

            // Act.
            var records = new List<IChangeRecord>();

            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
            using var streamReader = new StreamReader(memoryStream);

            records.AddRange(LdifReader.Parse(streamReader, true));

            Assert.Single(records);

            var changeAdd = (ChangeAdd)records[0];

            var sut = changeAdd[typeDescription].Select(s => Encoding.UTF8.GetString((byte[])s)).ToArray();

            // Assert.
            Assert.Single(sut);
            Assert.Equal(valueDescription, sut[0]);
        }
        finally
        {
            if (path != null)
            {
                new FileInfo(path).Delete();
            }
        }
    }

    /// <summary>
    /// Ensures the reader accepts file URIs if enabled.
    /// </summary>
    [Fact]
    public void ShouldAcceptRecordFileUriIfEnabled()
    {
        // Arrange.
        const string typeDescription = "description";
        var valueDescription = string.Join(
            Environment.NewLine,
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus convallis",
            "et erat at mollis. Nullam in risus laoreet, pharetra leo a, volutpat massa. Cras",
            "quis sodales velit. In sit amet augue gravida, sagittis dui a, placerat nunc.",
            "Fusce non nisi vel orci sagittis elementum. Praesent elit nulla, elementum sed",
            "sem a, semper dictum arcu. Duis luctus arcu id arcu scelerisque pharetra. Nunc",
            "a elementum felis, quis auctor diam.");
        string path = null;

        try
        {
            path = Path.GetTempFileName();

            using (var fileStream = new FileStream(path, FileMode.Truncate, FileAccess.Write))
            {
                var bytes = Encoding.UTF8.GetBytes(valueDescription);
                fileStream.Write(bytes, 0, bytes.Length);
            }

            var dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                $"{typeDescription}:< {new Uri(path)}",
                string.Empty);

            // Act.
            var records = new List<IChangeRecord>();

            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
            using var streamReader = new StreamReader(memoryStream);

            records.AddRange(LdifReader.Parse(streamReader, true));

            Assert.Single(records);

            var changeAdd = (ChangeAdd)records[0];
            var sut = changeAdd[typeDescription].Select(s => Encoding.UTF8.GetString((byte[])s)).ToArray();

            // Assert.
            Assert.Single(sut);
            Assert.Equal(valueDescription, sut[0]);
        }
        finally
        {
            if (path != null)
            {
                new FileInfo(path).Delete();
            }
        }
    }

    /// <summary>
    /// Ensures the reader consumes a file with multiple records and irregular formatting.
    /// </summary>
    [Fact]
    public void ShouldConsumeFile()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "version: 1",
            string.Empty,
            "# Comment",
            string.Empty,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty,
            "# Comment",
            "dn: CN=Niklaus Wirth,OU=users,DC=company,DC=com",
            "changetype: delete",
            string.Empty,
            "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
            "changetype: modify",
            "add: postaladdress",
            "postaladdress: 2400 Fulton St, San Francisco, CA 94118, USA",
            "-",
            "add: telephonenumber",
            "telephonenumber: +1 415 555 1234",
            "telephonenumber: +1 628 555 0000",
            "-",
            string.Empty,
            "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
            "changetype: modrdn",
            "newrdn: CN=Sophie Germain",
            "deleteoldrdn: 1",
            "newsuperior: OU=admins,DC=company,DC=com",
            string.Empty,
            string.Empty,
            string.Empty);

        // Act.
        var sut = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        sut.AddRange(LdifReader.Parse(streamReader));

        // Assert.
        Assert.Equal(4, sut.Count);
    }

    /// <summary>
    /// Ensures the reader accepts a valid version declaration.
    /// </summary>
    [Fact]
    public void ShouldProcessValidVersionNumber()
    {
        // Arrange.
        var input = string.Join(
            Environment.NewLine,
            "version: 1",
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);
        var expected = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeAdd)records[0];

        // Assert.
        Assert.Equal(expected, sut.Dump());
    }

    /// <summary>
    /// Ensures the reader skips control statements.
    /// </summary>
    [Fact]
    public void ShouldSkipControlStatements()
    {
        // Arrange.
        var input = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "control: 1.2.840.113556.1.4.805 true",
            "changetype: delete",
            string.Empty);
        var expected = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: delete",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeDelete)records[0];

        // Assert.
        Assert.Equal(expected, sut.Dump());
    }

    /// <summary>
    /// Ensures the reader skips leading comments and empty lines.
    /// </summary>
    [Fact]
    public void ShouldSkipLeadingComments()
    {
        // Arrange.
        var input = string.Join(
            Environment.NewLine,
            "# Comment",
            string.Empty,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);
        var expected = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeAdd)records[0];

        // Assert.
        Assert.Equal(expected, sut.Dump());
    }

    /// <summary>
    /// Ensures the reader skips nested comments.
    /// </summary>
    [Fact]
    public void ShouldSkipNestedComments()
    {
        // Arrange.
        var input = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "# Comment",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);
        var expected = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        // Act.
        var records = new List<IChangeRecord>();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var streamReader = new StreamReader(memoryStream);

        records.AddRange(LdifReader.Parse(streamReader));

        Assert.Single(records);

        var sut = (ChangeAdd)records[0];

        // Assert.
        Assert.Equal(expected, sut.Dump());
    }

    /// <summary>
    /// Ensures the reader rejects an LDIF record with an invalid starting declaration.
    /// </summary>
    [Fact]
    public void StartOfFileInvalidThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "vabcd: 1",
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }

    /// <summary>
    /// Ensures a null text reader instance is rejected.
    /// </summary>
    [Fact]
    public void TextReaderInvalidThrows()
        => Assert.Throws<ArgumentNullException>(() => LdifReader.Parse(null).ToArray());

    /// <summary>
    /// Ensures the reader rejects an invalid version declaration.
    /// </summary>
    [Fact]
    public void VersionNumberInvalidThrows()
    {
        // Arrange.
        var dump = string.Join(
            Environment.NewLine,
            "version: 2",
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: add",
            "objectclass: top",
            "objectclass: person",
            "objectclass: organizationalPerson",
            string.Empty);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump));
        using var streamReader = new StreamReader(memoryStream);

        // Act, Assert.
        Assert.Throws<LdifReaderException>(() => LdifReader.Parse(streamReader).ToArray());
    }
}