// -----------------------------------------------------------------------
//  <copyright file="ComparerTests.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper.Tests
{
    using System;
    using Xunit;

    /// <summary>
    /// Represents comparer tests.
    /// </summary>
    public class ComparerTests
    {
        /// <summary>
        /// Ensures the comparer rejects an empty value.
        /// </summary>
        [Fact]
        public void EmptyValueThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => AttributeTypeComparer.GetComparer.Compare(string.Empty, "valid"));
            Assert.Throws<ArgumentOutOfRangeException>(() => AttributeTypeComparer.GetComparer.Compare("valid", string.Empty));
        }

        /// <summary>
        /// Ensures the comparer rejects a null value.
        /// </summary>
        [Fact]
        public void NullValueThrows()
        {
            Assert.Throws<ArgumentNullException>(() => AttributeTypeComparer.GetComparer.Compare(null, "valid"));
            Assert.Throws<ArgumentNullException>(() => AttributeTypeComparer.GetComparer.Compare("valid", null));
        }

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

        /// <summary>
        /// Ensures the comparer rejects a white space value.
        /// </summary>
        [Fact]
        public void WhiteSpaceThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => AttributeTypeComparer.GetComparer.Compare(" ", "valid"));
            Assert.Throws<ArgumentOutOfRangeException>(() => AttributeTypeComparer.GetComparer.Compare("valid", " "));
        }
    }
}