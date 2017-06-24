﻿// -----------------------------------------------------------------------
//  <copyright file="ChangeDeleteTests.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper.Tests
{
    using System;
    using Xunit;

    /// <summary>
    /// Represents change type delete tests.
    /// </summary>
    public class ChangeDeleteTests
    {
        private static readonly string DistinguishedName = "CN=Ada Lovelace,OU=users,DC=company,DC=com";

        private static readonly string Dump = string.Join(
            Environment.NewLine,
            "dn: CN=Ada Lovelace,OU=users,DC=company,DC=com",
            "changetype: delete",
            string.Empty);

        /// <summary>
        /// Ensures the constructor rejects an empty distinguished name.
        /// </summary>
        [Fact]
        public void CtorParameterDistinguishedNameEmptyThrows() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeDelete(string.Empty));

        /// <summary>
        /// Ensures the constructor rejects a null distinguished name.
        /// </summary>
        [Fact]
        public void CtorParameterDistinguishedNameNullThrows() =>
            Assert.Throws<ArgumentNullException>(() => new ChangeDelete(null));

        /// <summary>
        /// Ensures the constructor rejects a white space distinguished name.
        /// </summary>
        [Fact]
        public void CtorParameterDistinguishedNameWhiteSpaceThrows() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeDelete(" "));

        /// <summary>
        /// Ensures the Dump method is valid.
        /// </summary>
        [Fact]
        public void MethodDumpIsValid()
        {
            // Act.
            ChangeDelete sut = new ChangeDelete(DistinguishedName);

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
            ChangeDelete sut = new ChangeDelete(DistinguishedName);

            // Assert.
            Assert.Equal(DistinguishedName, sut.DistinguishedName);
        }
    }
}