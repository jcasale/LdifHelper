﻿// -----------------------------------------------------------------------
//  <copyright file="LdapAttribute.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an LDIF attribute type and values.
    /// </summary>
    public class LdifAttribute : IReadOnlyList<object>
    {
        /// <summary>
        /// Represents the attribute type.
        /// </summary>
        private readonly string attributeType;

        /// <summary>
        /// Represents the attribute values.
        /// </summary>
        private readonly List<object> attributeValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdifAttribute"/> class.
        /// </summary>
        /// <param name="attributeType">The attribute type.</param>
        /// <param name="attributeValues">The attribute values.</param>
        public LdifAttribute(string attributeType, IEnumerable<object> attributeValues)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException(nameof(attributeType), "The attribute type can not be null.");
            }

            if (string.IsNullOrWhiteSpace(attributeType))
            {
                throw new ArgumentOutOfRangeException(nameof(attributeType), "The attribute type can not be empty or whitespace.");
            }

            this.attributeType = attributeType;
            this.attributeValues = attributeValues == null ? new List<object>() : new List<object>(attributeValues);
        }

        /// <summary>
        /// Gets the attribute type.
        /// </summary>
        /// <value>The attribute type.</value>
        public string AttributeType => this.attributeType;

        /// <summary>
        /// Gets the attribute values.
        /// </summary>
        /// <value>The attribute values.</value>
        public IEnumerable<object> AttributeValues => this.attributeValues;

        /// <summary>
        /// Gets the number of values contained in the <see cref="LdifAttribute"/>.
        /// </summary>
        /// <value>The number of values contained in the <see cref="LdifAttribute"/>.</value>
        public int Count => this.attributeValues.Count;

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The value at the specified index.</returns>
        public object this[int index] => this.attributeValues[index];

        /// <summary>
        /// Gets an enumerable that can be used to iterate through the collection of attribute values.
        /// </summary>
        /// <returns>An enumerable that can be used to iterate through the collection of attribute values.</returns>
        public IEnumerator<object> GetEnumerator() => this.attributeValues.GetEnumerator();

        /// <summary>
        /// Gets an enumerable that can be used to iterate through the collection of attribute values.
        /// </summary>
        /// <returns>An enumerable that can be used to iterate through the collection of attribute values.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{nameof(LdifAttribute)}<{this.attributeType}>";
    }
}