// -----------------------------------------------------------------------
//  <copyright file="LdifReaderTests.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper.Tests
{
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
            string dump = string.Join(
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
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeAdd sut = (ChangeAdd)records[0];

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
            string dump = string.Join(
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
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeAdd sut = (ChangeAdd)records[0];

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
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: delete",
                string.Empty);

            // Act.
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeDelete sut = (ChangeDelete)records[0];

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
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
                "changetype: modrdn",
                "newrdn: CN=Emmy Noether",
                "deleteoldrdn: x",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the reader rejects a change-moddn record with an invalid field.
        /// </summary>
        [Fact]
        public void ChangeModDnInvalidRecordThrows()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
                "changetype: modrdn",
                "newrdn: CN=Emmy Noether",
                "invalid: line",
                "deleteoldrdn: 0",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
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
            byte[] newRdnUtf16Bytes = Encoding.Unicode.GetBytes(newRdn);
            byte[] newRdnUtf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, newRdnUtf16Bytes);

            // Invalidate byte array.
            newRdnUtf8Bytes[3] = 0xFF;

            string dump = string.Join(
                Environment.NewLine,
                $"dn:: {distinguishedName.ToBase64()}",
                "changetype: modrdn",
                $"newrdn:: {Convert.ToBase64String(newRdnUtf8Bytes)}",
                "deleteoldrdn: 0",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Exception exception = Assert.Throws<LdifReaderException>(() => sut.ToArray());
                Assert.IsType<DecoderFallbackException>(exception.InnerException);
            }
        }

        /// <summary>
        /// Ensures a change-moddn record with a folded newrdn is read correctly.
        /// </summary>
        [Fact]
        public void ChangeModDnNewRdnUnFolds()
        {
            // Arrange.
            const string newRdn = "CN=Maria Gaetana Agnesi";
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: modrdn",
                "newrdn: CN=Maria Gaetana",
                "  Agnesi",
                "deleteoldrdn: 0",
                string.Empty);

            // Act.
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeModDn sut = (ChangeModDn)records[0];

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
            byte[] newSuperiorUtf16Bytes = Encoding.Unicode.GetBytes(newSuperior);
            byte[] newSuperiorUtf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, newSuperiorUtf16Bytes);

            // Invalidate byte array.
            newSuperiorUtf8Bytes[3] = 0xFF;

            string dump = string.Join(
                Environment.NewLine,
                $"dn:: {distinguishedName.ToBase64()}",
                "changetype: modrdn",
                $"newrdn:: {newRdn.ToBase64()}",
                "deleteoldrdn: 0",
                $"newsuperior:: {Convert.ToBase64String(newSuperiorUtf8Bytes)}",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Exception exception = Assert.Throws<LdifReaderException>(() => sut.ToArray());
                Assert.IsType<DecoderFallbackException>(exception.InnerException);
            }
        }

        /// <summary>
        /// Ensures a change-moddn record with a folded newsuperior is read correctly.
        /// </summary>
        [Fact]
        public void ChangeModDnNewSuperiorUnFolds()
        {
            // Arrange.
            const string newSuperior = "OU=admins,DC=company,DC=com";
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
                "changetype: modrdn",
                "newrdn: CN=Sophie Germain",
                "deleteoldrdn: 1",
                "newsuperior: OU=admins",
                " ,DC=company,DC=com",
                string.Empty);

            // Act.
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeModDn sut = (ChangeModDn)records[0];

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
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
                "changetype: modrdn",
                "newrdn: CN=Emmy Noether",
                "deleteoldrdn: 0",
                string.Empty);

            // Act.
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeModDn sut = (ChangeModDn)records[0];

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
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Sophie Germain,OU=users,DC=company,DC=com",
                "changetype: modrdn",
                "newrdn: CN=Sophie Germain",
                "deleteoldrdn: 1",
                "newsuperior: OU=admins,DC=company,DC=com",
                string.Empty);

            // Act.
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeModDn sut = (ChangeModDn)records[0];

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
            string dump = string.Join(
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
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeModify sut = (ChangeModify)records[0];

            // Assert.
            Assert.Equal(distinguishedName, sut.DistinguishedName);
            Assert.Equal(2, sut.Count);

            ModSpec[] modSpecsPostalAddress = sut.ModSpecs.Where(x => x.AttributeType.Equals(typePostalAddress)).ToArray();
            Assert.Single(modSpecsPostalAddress);
            ModSpec modSpecPostalAddress = modSpecsPostalAddress[0];
            Assert.Equal(ModSpecType.Add, modSpecPostalAddress.ModSpecType);
            Assert.Equal(valuesPostalAddress, modSpecPostalAddress.AttributeValues.Select(x => (string)x));

            ModSpec[] modSpecsTelephoneNumber = sut.ModSpecs.Where(x => x.AttributeType.Equals(typeTelephoneNumber)).ToArray();
            Assert.Single(modSpecsTelephoneNumber);
            ModSpec modSpecTelephoneNumber = modSpecsTelephoneNumber[0];
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
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
                "changetype: modify",
                "add: telephonenumber",
                "telephonenumber: +1 415 555 1234",
                "telephonenumber: +1 628 555 0000",
                "-",
                string.Empty);

            // Act.
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeModify sut = (ChangeModify)records[0];

            // Assert.
            Assert.Equal(distinguishedName, sut.DistinguishedName);

            Assert.Single(sut);
            ModSpec modSpec = sut.ModSpecs.First();
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
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
                "changetype: modify",
                "add: postaladdress",
                "postaladdress: 2400 Fulton St, San Francisco, CA 94118, USA",
                "-",
                string.Empty);

            // Act.
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeModify sut = (ChangeModify)records[0];

            // Assert.
            Assert.Equal(distinguishedName, sut.DistinguishedName);

            Assert.Single(sut);
            ModSpec modSpec = sut.ModSpecs.First();
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
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
                "changetype: modify",
                "delete: telephonenumber",
                "telephonenumber: +1 415 555 1234",
                "-",
                string.Empty);

            // Act.
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeModify sut = (ChangeModify)records[0];

            // Assert.
            Assert.Equal(distinguishedName, sut.DistinguishedName);

            Assert.Single(sut);
            ModSpec modSpec = sut.ModSpecs.First();
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
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
                "changetype: modify",
                "delete: postaladdress",
                "-",
                string.Empty);

            // Act.
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeModify sut = (ChangeModify)records[0];

            // Assert.
            Assert.Equal(distinguishedName, sut.DistinguishedName);

            Assert.Single(sut);
            ModSpec modSpec = sut.ModSpecs.First();
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
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: modify",
                "add: postaladdress",
                "postaladdress: 2400 Fulton St, San Francisco, CA 94118, USA",
                string.Empty,
                "-",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the reader rejects an invalid change-modify attribute type.
        /// </summary>
        [Fact]
        public void ChangeModifyInvalidAttributeTypeThrows()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: modify",
                "add:",
                "postaladdress: 2400 Fulton St, San Francisco, CA 94118, USA",
                "-",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the reader rejects an invalid change-modify mod-spec.
        /// </summary>
        [Fact]
        public void ChangeModifyInvalidModSpecThrows()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: modify",
                "invalid: postaladdress",
                "postaladdress: 2400 Fulton St, San Francisco, CA 94118, USA",
                "-",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the reader rejects a change-modify with mixed attribute types.
        /// </summary>
        [Fact]
        public void ChangeModifyMixedAttributeTypeThrows()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: modify",
                "add: postaladdress",
                "postaladdress: 2400 Fulton St, San Francisco, CA 94118, USA",
                "streetaddress: 3127 Fillmore St, San Francisco, CA 94123, USA",
                "-",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
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
            string dump = string.Join(
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
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeModify sut = (ChangeModify)records[0];

            // Assert.
            ModSpec modSpec = sut.ModSpecs.First();
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
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass person",
                "objectclass: organizationalPerson",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the reader rejects a truncated value-spec.
        /// </summary>
        [Fact]
        public void ChangeModifyValueSpecTruncatedThrows()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                "description::",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the reader rejects an invalid change type.
        /// </summary>
        [Fact]
        public void ChangeTypeInvalidThrows()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: invalid",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the LineNumber property is valid.
        /// </summary>
        [Fact]
        public void PropertyLineNumberIsValid()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "version: 1",
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);


            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Single(sut);
                Assert.Equal(7, sut.LineNumber);
            }
        }

        /// <summary>
        /// Ensures the reader rejects a distinguished name with an invalid base64 encoding.
        /// </summary>
        [Fact]
        public void RecordDistinguishedNameInvalidBase64Throws()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "dn:: invalid",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the reader rejects an empty distinguished name.
        /// </summary>
        [Fact]
        public void RecordDistinguishedNameEmptyThrows()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "dn: ",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the reader rejects a white space distinguished name.
        /// </summary>
        [Fact]
        public void RecordDistinguishedNameWhiteSpaceThrows()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "dn:  ",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the reader rejects a distinguished name with an invalid UTF8 encoding.
        /// </summary>
        [Fact]
        public void RecordDistinguishedNameInvalidUtf8Throws()
        {
            // Arrange.
            const string distinguishedName = "CN=Émilie du Châtelet,OU=users,DC=company,DC=com";
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(distinguishedName);
            byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);

            // Invalidate byte array.
            utf8Bytes[3] = 0xFF;

            string dump = string.Join(
                Environment.NewLine,
                $"dn:: {Convert.ToBase64String(utf8Bytes)}",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Exception exception = Assert.Throws<LdifReaderException>(() => sut.ToArray());
                Assert.IsType<DecoderFallbackException>(exception.InnerException);
            }
        }

        /// <summary>
        /// Ensures the reader correctly unfolds a distinguished name.
        /// </summary>
        [Fact]
        public void RecordDistinguishedNameIsUnFolded()
        {
            // Arrange.
            const string distinguishedName = "CN=Pablo Diego José Francisco de Paula Juan Nepomuceno María de los Remedios Cipriano de la Santísima Trinidad Ruiz y Picasso,OU=users,DC=company,DC=com";
            string dump = string.Join(
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
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeAdd sut = (ChangeAdd)records[0];

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
            string dump = string.Join(
                Environment.NewLine,
                "dn CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the reader rejects an LDIF record with a missing distinguished name.
        /// </summary>
        [Fact]
        public void RecordDistinguishedNameMissingThrows()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the reader rejects a file URI with an invalid path.
        /// </summary>
        [Fact]
        public void RecordFileUriInvalidPathThrows()
        {
            // Arrange.
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Assert.False(new FileInfo(path).Exists);

            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                $"description:< {new Uri(path)}",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader, true);

                // Assert.
                Exception exception = Assert.Throws<LdifReaderException>(() => sut.ToArray());
                Assert.IsType<FileNotFoundException>(exception.InnerException);
            }
        }

        /// <summary>
        /// Ensures the reader rejects a file URI with an invalid path.
        /// </summary>
        [Fact]
        public void RecordFileUriInvalidUriThrows()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                "jpegphoto:< invalid",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader, true);

                // Assert.
                Exception exception = Assert.Throws<LdifReaderException>(() => sut.ToArray());
                Assert.IsType<UriFormatException>(exception.InnerException);
            }
        }

        /// <summary>
        /// Ensures the reader rejects a file URI with an invalid encoding.
        /// </summary>
        [Fact]
        public void RecordFileUriInvalidUtf8Throws()
        {
            // Arrange.
            const string path = @"c:\path\picture.jpeg";
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(new Uri(path).ToString());
            byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);

            // Invalidate byte array.
            utf8Bytes[utf8Bytes.Length - 1] = 0xFF;

            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                $"jpegphoto::< {Convert.ToBase64String(utf8Bytes)}",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader, true);

                // Assert.
                Exception exception = Assert.Throws<LdifReaderException>(() => sut.ToArray());
                Assert.IsType<DecoderFallbackException>(exception.InnerException);
            }
        }

        /// <summary>
        /// Ensures the reader prohibits file URIs if not enabled.
        /// </summary>
        [Fact]
        public void RecordFileUriNotEnabledThrows()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                "jpegphoto:< file://picture.jpg",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the reader prohibits re-enumeration.
        /// </summary>
        [Fact]
        public void ReEnumerationThrows()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Single(sut);

                Assert.Throws<InvalidOperationException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures the reader accepts base64 encoded file URIs if enabled.
        /// </summary>
        [Fact]
        public void ShouldAcceptRecordFileUriBase64IfEnabled()
        {
            // Arrange.
            const string typeDescription = "description";
            string valueDescription = string.Join(
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

                using (FileStream fileStream = new FileStream(path, FileMode.Truncate, FileAccess.Write))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(valueDescription);
                    fileStream.Write(bytes, 0, bytes.Length);
                }

                string dump = string.Join(
                    Environment.NewLine,
                    "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                    "changetype: add",
                    "objectclass: top",
                    "objectclass: person",
                    "objectclass: organizationalPerson",
                    $"{typeDescription}::< {new Uri(path).ToString().ToBase64()}",
                    string.Empty);

                // Act.
                List<IChangeRecord> records = new List<IChangeRecord>();

                using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
                using (TextReader textReader = new StreamReader(memoryStream))
                {
                    records.AddRange(new LdifReader(textReader, true));
                }

                Assert.Single(records);

                ChangeAdd changeAdd = (ChangeAdd)records[0];

                string[] sut = changeAdd[typeDescription].Select(s => Encoding.UTF8.GetString((byte[])s)).ToArray();

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
            string valueDescription = string.Join(
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

                using (FileStream fileStream = new FileStream(path, FileMode.Truncate, FileAccess.Write))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(valueDescription);
                    fileStream.Write(bytes, 0, bytes.Length);
                }

                string dump = string.Join(
                    Environment.NewLine,
                    "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                    "changetype: add",
                    "objectclass: top",
                    "objectclass: person",
                    "objectclass: organizationalPerson",
                    $"{typeDescription}:< {new Uri(path)}",
                    string.Empty);

                // Act.
                List<IChangeRecord> records = new List<IChangeRecord>();

                using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
                using (TextReader textReader = new StreamReader(memoryStream))
                {
                    records.AddRange(new LdifReader(textReader, true));
                }

                Assert.Single(records);

                ChangeAdd changeAdd = (ChangeAdd)records[0];
                string[] sut = changeAdd[typeDescription].Select(s => Encoding.UTF8.GetString((byte[])s)).ToArray();

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
            string dump = string.Join(
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
            List<IChangeRecord> sut = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                sut.AddRange(new LdifReader(textReader));
            }

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
            string input = string.Join(
                Environment.NewLine,
                "version: 1",
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);
            string expected = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);

            // Act.
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeAdd sut = (ChangeAdd)records[0];

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
            string input = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "control: 1.2.840.113556.1.4.805 true",
                "changetype: delete",
                string.Empty);
            string expected = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: delete",
                string.Empty);

            // Act.
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeDelete sut = (ChangeDelete)records[0];

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
            string input = string.Join(
                Environment.NewLine,
                "# Comment",
                string.Empty,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);
            string expected = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);

            // Act.
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeAdd sut = (ChangeAdd)records[0];

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
            string input = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "# Comment",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);
            string expected = string.Join(
                Environment.NewLine,
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);

            // Act.
            List<IChangeRecord> records = new List<IChangeRecord>();

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                records.AddRange(new LdifReader(textReader));
            }

            Assert.Single(records);

            ChangeAdd sut = (ChangeAdd)records[0];

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
            string dump = string.Join(
                Environment.NewLine,
                "vabcd: 1",
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }

        /// <summary>
        /// Ensures a null text reader instance is rejected.
        /// </summary>
        [Fact]
        public void TextReaderInvalidThrows()
            => Assert.Throws<ArgumentNullException>(() => new LdifReader(null));

        /// <summary>
        /// Ensures the reader rejects an invalid version declaration.
        /// </summary>
        [Fact]
        public void VersionNumberInvalidThrows()
        {
            // Arrange.
            string dump = string.Join(
                Environment.NewLine,
                "version: 2",
                "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
                "changetype: add",
                "objectclass: top",
                "objectclass: person",
                "objectclass: organizationalPerson",
                string.Empty);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dump)))
            using (TextReader textReader = new StreamReader(memoryStream))
            {
                // Act.
                LdifReader sut = new LdifReader(textReader);

                // Assert.
                Assert.Throws<LdifReaderException>(() => sut.ToArray());
            }
        }
    }
}