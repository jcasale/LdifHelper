﻿// -----------------------------------------------------------------------
//  <copyright file="ChangeAddTests.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper.Tests
{
    using System;
    using Xunit;

    /// <summary>
    /// Represents change type add tests.
    /// </summary>
    public class ChangeAddTests
    {
        // Arrange.
        private static readonly string DistinguishedName = "CN=Niklaus Wirth,OU=users,DC=company,DC=com";

        private static readonly string AttributeTypeObjectClass = "objectClass";
        private static readonly object[] AttributeValuesObjectClass = { "top", "person", "organizationalPerson", "user" };

        private static readonly string AttributeTypeDisplayName = "displayName";
        private static readonly object[] AttributeValuesDisplayName = { "Niklaus Wirth" };

        private static readonly string AttributeTypeGivenName = "givenName";
        private static readonly object[] AttributeValuesGivenName = { "Niklaus" };

        private static readonly string AttributeTypeSn = "sn";
        private static readonly object[] AttributeValuesSn = { "Wirth" };

        private static readonly string[] AttributeTypes =
        {
            AttributeTypeObjectClass,
            AttributeTypeDisplayName,
            AttributeTypeGivenName,
            AttributeTypeSn
        };

        private static readonly LdifAttribute[] LdifAttributes =
        {
            new LdifAttribute(AttributeTypeObjectClass, AttributeValuesObjectClass),
            new LdifAttribute(AttributeTypeDisplayName, AttributeValuesDisplayName),
            new LdifAttribute(AttributeTypeGivenName, AttributeValuesGivenName),
            new LdifAttribute(AttributeTypeSn, AttributeValuesSn)
        };

        private static readonly string Dump = string.Join(
            Environment.NewLine,
            "dn: CN=Niklaus Wirth,OU=users,DC=company,DC=com",
            "objectClass: top",
            "objectClass: person",
            "objectClass: organizationalPerson",
            "objectClass: user",
            "displayName: Niklaus Wirth",
            "givenName: Niklaus",
            "sn: Wirth",
            string.Empty);

        /// <summary>
        /// Ensures the collection indexer is valid.
        /// </summary>
        [Fact]
        public void CollectionIndexerIsValid()
        {
            // Act.
            ChangeAdd sut = new ChangeAdd(DistinguishedName, LdifAttributes);

            // Assert.
            Assert.Equal(AttributeValuesObjectClass, sut[AttributeTypeObjectClass]);
            Assert.Equal(AttributeValuesDisplayName, sut[AttributeTypeDisplayName]);
            Assert.Equal(AttributeValuesGivenName, sut[AttributeTypeGivenName]);
            Assert.Equal(AttributeValuesSn, sut[AttributeTypeSn]);
        }

        /// <summary>
        /// Ensures the constructor converts null attribute collection into an empty collection.
        /// </summary>
        [Fact]
        public void CtorParameterAttributesNullCollectionIsValid()
        {
            // Act.
            ChangeAdd sut = new ChangeAdd(DistinguishedName, null);

            // Assert.
            Assert.NotNull(sut.LdifAttributes);
            Assert.Equal(0, sut.Count);
        }

        /// <summary>
        /// Ensures the constructor rejects an empty distinguished name.
        /// </summary>
        [Fact]
        public void CtorParameterDistinguishedNameEmptyThrows() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeAdd(string.Empty, null));

        /// <summary>
        /// Ensures the constructor rejects a null distinguished name.
        /// </summary>
        [Fact]
        public void CtorParameterDistinguishedNameNullThrows() =>
            Assert.Throws<ArgumentNullException>(() => new ChangeAdd(null, null));

        /// <summary>
        /// Ensures the constructor rejects a white space distinguished name.
        /// </summary>
        [Fact]
        public void CtorParameterDistinguishedNameWhiteSpaceThrows() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeAdd(" ", null));

        /// <summary>
        /// Ensures the Contains method is valid.
        /// </summary>
        [Fact]
        public void MethodContainsIsValid()
        {
            // Act.
            ChangeAdd sut = new ChangeAdd(DistinguishedName, LdifAttributes);

            // Assert.
            Assert.True(sut.Contains(AttributeTypeObjectClass));
            Assert.True(sut.Contains(AttributeTypeDisplayName));
            Assert.True(sut.Contains(AttributeTypeGivenName));
            Assert.True(sut.Contains(AttributeTypeSn));
        }

        /// <summary>
        /// Ensures the Dump method is valid.
        /// </summary>
        [Fact]
        public void MethodDumpIsValid()
        {
            // Act.
            ChangeAdd sut = new ChangeAdd(DistinguishedName, LdifAttributes);

            Assert.Equal(Dump, sut.Dump());
        }

        /// <summary>
        /// Ensures the TryGetAttribute method is valid.
        /// </summary>
        [Fact]
        public void MethodTryGetAttributeIsValid()
        {
            // Act.
            ChangeAdd sut = new ChangeAdd(DistinguishedName, LdifAttributes);

            // Assert.
            Assert.True(sut.TryGetAttribute(AttributeTypeObjectClass, out LdifAttribute _));
            Assert.True(sut.TryGetAttribute(AttributeTypeDisplayName, out LdifAttribute _));
            Assert.True(sut.TryGetAttribute(AttributeTypeGivenName, out LdifAttribute _));
            Assert.True(sut.TryGetAttribute(AttributeTypeSn, out LdifAttribute _));
        }

        /// <summary>
        /// Ensures the Attributes property is valid.
        /// </summary>
        [Fact]
        public void PropertyAttributesIsValid()
        {
            // Act.
            ChangeAdd sut = new ChangeAdd(DistinguishedName, LdifAttributes);
            
            Assert.Equal(LdifAttributes, sut);
            Assert.Equal(LdifAttributes, sut.LdifAttributes);
            Assert.Equal(sut, sut.LdifAttributes);
        }

        /// <summary>
        /// Ensures the Count property is valid.
        /// </summary>
        [Fact]
        public void PropertyCountIsValid()
        {
            // Act.
            ChangeAdd sut = new ChangeAdd(DistinguishedName, LdifAttributes);

            // Assert.
            Assert.Equal(LdifAttributes.Length, sut.Count);
        }

        /// <summary>
        /// Ensures the DistinguishedName property is valid.
        /// </summary>
        [Fact]
        public void PropertyDistinguishedNameIsValid()
        {
            // Act.
            ChangeAdd sut = new ChangeAdd(DistinguishedName, LdifAttributes);

            // Assert.
            Assert.Equal(DistinguishedName, sut.DistinguishedName);
        }

        /// <summary>
        /// Ensures the Types property is valid.
        /// </summary>
        [Fact]
        public void PropertyTypesIsValid()
        {
            // Act.
            ChangeAdd sut = new ChangeAdd(DistinguishedName, LdifAttributes);

            // Assert.
            Assert.Equal(AttributeTypes, sut.AttributeTypes);
        }
    }
}