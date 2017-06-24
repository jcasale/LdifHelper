// -----------------------------------------------------------------------
//  <copyright file="ChangeModifyTests.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper.Tests
{
    using System;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Represents change type modify tests.
    /// </summary>
    public class ChangeModifyTests
    {
        private static readonly string DistinguishedName = "CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com";

        private static readonly ModSpec[] ModSpecs = {new ModSpec(ModSpecType.Add, "description", new object[] {"Contractor"})};

        /// <summary>
        /// Ensures the constructor rejects an empty distinguished name.
        /// </summary>
        [Fact]
        public void CtorParameterDistinguishedNameEmptyThrows() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeModify(string.Empty, ModSpecs));

        /// <summary>
        /// Ensures the constructor rejects a null distinguished name.
        /// </summary>
        [Fact]
        public void CtorParameterDistinguishedNameNullThrows() =>
            Assert.Throws<ArgumentNullException>(() => new ChangeModify(null, ModSpecs));

        /// <summary>
        /// Ensures the constructor rejects a white space distinguished name.
        /// </summary>
        [Fact]
        public void CtorParameterDistinguishedNameWhiteSpaceThrows() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeModify(" ", ModSpecs));

        /// <summary>
        /// Ensures the constructor rejects an empty spec collection.
        /// </summary>
        [Fact]
        public void CtorParameterModSpecsEmptyThrows() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeModify(DistinguishedName, new ModSpec[0]));

        /// <summary>
        /// Ensures the constructor rejects a null mod spec collection.
        /// </summary>
        [Fact]
        public void CtorParameterModSpecsNullThrows() =>
            Assert.Throws<ArgumentNullException>(() => new ChangeModify(DistinguishedName, null));

        /// <summary>
        /// Ensures the Count property is valid.
        /// </summary>
        [Fact]
        public void PropertyCountIsValid()
        {
            // Act.
            ChangeModify sut = new ChangeModify(
                DistinguishedName,
                new[]
                {
                    new ModSpec(ModSpecType.Add, "description", new object[] { "Contractor" }),
                    new ModSpec(ModSpecType.Replace, "telephonenumber", new object[] { "+1 (415) 555 1234" })
                });

            // Arrange.
            Assert.Equal(2, sut.Count);
        }

        /// <summary>
        /// Ensures the DistinguishedName property is valid.
        /// </summary>
        [Fact]
        public void PropertyDistinguishedNameIsValid()
        {
            // Act.
            ChangeModify sut = new ChangeModify(DistinguishedName, ModSpecs);

            // Arrange.
            Assert.Equal(DistinguishedName, sut.DistinguishedName);
        }

        /// <summary>
        /// Ensures the Values property is valid.
        /// </summary>
        [Fact]
        public void PropertyValuesIsValid()
        {
            // Arrange.
            const string attributeTypeDescription = "description";
            object[] attributeValuesDescription = { "Contractor" };
            ModSpec modSpecDescription = new ModSpec(ModSpecType.Add, attributeTypeDescription, attributeValuesDescription);

            const string attributeTypeTelephoneNumber = "telephonenumber";
            object[] attributeValuesTelephoneNumber = { "+1 (415) 555 1234" };
            ModSpec modSpecTelephoneNumber = new ModSpec(ModSpecType.Replace, attributeTypeTelephoneNumber, attributeValuesTelephoneNumber);

            // Act.
            ChangeModify sut = new ChangeModify(DistinguishedName, new[] { modSpecDescription, modSpecTelephoneNumber });
            ModSpec sutModSpecDescription = sut.ModSpecs.First(x => x.AttributeType.Equals(attributeTypeDescription));
            ModSpec sutModSpecTelephoneNumber = sut.ModSpecs.First(x => x.AttributeType.Equals(attributeTypeTelephoneNumber));

            // Assert.
            Assert.Equal(2, sut.ModSpecs.ToArray().Length);

            Assert.Equal(attributeTypeDescription, sutModSpecDescription.AttributeType);
            Assert.Equal(attributeValuesDescription, sutModSpecDescription.AttributeValues);

            Assert.Equal(attributeTypeTelephoneNumber, sutModSpecTelephoneNumber.AttributeType);
            Assert.Equal(attributeValuesTelephoneNumber, sutModSpecTelephoneNumber.AttributeValues);
        }

        /// <summary>
        /// Ensures a single value is added to the specified attribute type.
        /// </summary>
        [Fact]
        public void ShouldAddOneValue()
        {
            // Arrange.
            ModSpec[] modSpec = { new ModSpec(ModSpecType.Add, "postaladdress", new object[] {"2400 Fulton St, San Francisco, CA 94118, USA" })};
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
                "changetype: modify",
                "add: postaladdress",
                "postaladdress: 2400 Fulton St, San Francisco, CA 94118, USA",
                "-",
                string.Empty);

            // Act.
            ChangeModify sut = new ChangeModify(DistinguishedName, modSpec);

            // Assert.
            Assert.Equal(dump, sut.Dump());
        }

        /// <summary>
        /// Ensures the object is an enumerable of type <see cref="ModSpec"/>s.
        /// </summary>
        [Fact]
        public void ShouldBeEnumerable()
        {
            // Arrange.
            const string attributeTypeDescription = "description";
            object[] attributeValuesDescription = { "Contractor" };
            ModSpec modSpecDescription = new ModSpec(ModSpecType.Add, attributeTypeDescription, attributeValuesDescription);

            const string attributeTypeTelephoneNumber = "telephonenumber";
            object[] attributeValuesTelephoneNumber = { "+1 (415) 555 1234" };
            ModSpec modSpecTelephoneNumber = new ModSpec(ModSpecType.Replace, attributeTypeTelephoneNumber, attributeValuesTelephoneNumber);

            // Act.
            ChangeModify sut = new ChangeModify(DistinguishedName, new[] { modSpecDescription, modSpecTelephoneNumber });
            ModSpec sutModSpecDescription = sut.First(x => x.AttributeType.Equals(attributeTypeDescription));
            ModSpec sutModSpecTelephoneNumber = sut.First(x => x.AttributeType.Equals(attributeTypeTelephoneNumber));

            // Assert.
            Assert.Equal(2, sut.ToArray().Length);

            Assert.Equal(attributeTypeDescription, sutModSpecDescription.AttributeType);
            Assert.Equal(attributeValuesDescription, sutModSpecDescription.AttributeValues);

            Assert.Equal(attributeTypeTelephoneNumber, sutModSpecTelephoneNumber.AttributeType);
            Assert.Equal(attributeValuesTelephoneNumber, sutModSpecTelephoneNumber.AttributeValues);
        }

        /// <summary>
        /// Ensures an entire attribute type is scheduled to be removed.
        /// </summary>
        [Fact]
        public void ShouldDeleteAllValues()
        {
            // Arrange.
            ModSpec[] modSpec = { new ModSpec(ModSpecType.Delete, "description", null) };
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
                "changetype: modify",
                "delete: description",
                "-",
                string.Empty);

            // Act.
            ChangeModify sut = new ChangeModify(DistinguishedName, modSpec);

            // Assert.
            Assert.Equal(dump, sut.Dump());
        }

        /// <summary>
        /// Ensures a single value for the specified attribute type is scheduled to be removed.
        /// </summary>
        [Fact]
        public void ShouldDeleteSingleValues()
        {
            // Arrange.
            ModSpec[] modSpec = { new ModSpec(ModSpecType.Delete, "description", new object[] { "Contractor" }) };
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
                "changetype: modify",
                "delete: description",
                "description: Contractor",
                "-",
                string.Empty);

            // Act.
            ChangeModify sut = new ChangeModify(DistinguishedName, modSpec);

            // Assert.
            Assert.Equal(dump, sut.Dump());
        }

        /// <summary>
        /// Ensures an entire attribute type is scheduled to be replaced with no new values.
        /// </summary>
        [Fact]
        public void ShouldReplaceAllValues()
        {
            // Arrange.
            ModSpec[] modSpec = { new ModSpec(ModSpecType.Replace, "telephonenumber", null) };
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
                "changetype: modify",
                "replace: telephonenumber",
                "-",
                string.Empty);

            // Act.
            ChangeModify sut = new ChangeModify(DistinguishedName, modSpec);

            // Assert.
            Assert.Equal(dump, sut.Dump());
        }

        /// <summary>
        /// Ensures an attribute type is replaced with a single value.
        /// </summary>
        [Fact]
        public void ShouldReplaceWithSingleValue()
        {
            // Arrange.
            ModSpec[] modSpec = { new ModSpec(ModSpecType.Replace, "telephonenumber", new object[] { "+1 (415) 555 1234" }) };
            string dump = string.Join(
                Environment.NewLine,
                "dn: CN=Leonardo Pisano Bigollo,OU=users,DC=company,DC=com",
                "changetype: modify",
                "replace: telephonenumber",
                "telephonenumber: +1 (415) 555 1234",
                "-",
                string.Empty);

            // Act.
            ChangeModify sut = new ChangeModify(DistinguishedName, modSpec);

            // Assert.
            Assert.Equal(dump, sut.Dump());
        }
    }
}