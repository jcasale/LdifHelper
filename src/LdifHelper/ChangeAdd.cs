﻿// -----------------------------------------------------------------------
//  <copyright file="ChangeAdd.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents an RFC2849 change-add record.
    /// </summary>
    public class ChangeAdd : IChangeRecord, IEnumerable<LdifAttribute>
    {
        /// <summary>
        /// Represents the distinguished name of the record.
        /// </summary>
        private readonly string distinguishedName;

        /// <summary>
        /// Represents the attribute type and value data in the record.
        /// </summary>
        private readonly SortedDictionary<string, LdifAttribute> ldifAttributes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeAdd"/> class.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the record.</param>
        /// <param name="ldifAttributes">The attribute type and value data in the record.</param>
        public ChangeAdd(string distinguishedName, IEnumerable<LdifAttribute> ldifAttributes)
        {
            if (distinguishedName == null)
            {
                throw new ArgumentNullException(nameof(distinguishedName), "The distinguished name can not be null.");
            }

            if (string.IsNullOrWhiteSpace(distinguishedName))
            {
                throw new ArgumentOutOfRangeException(nameof(distinguishedName), "The distinguished name can not be empty or whitespace.");
            }

            this.distinguishedName = distinguishedName;

            this.ldifAttributes = new SortedDictionary<string, LdifAttribute>(AttributeTypeComparer.GetComparer);

            if (ldifAttributes != null)
            {
                foreach (LdifAttribute ldapAttribute in ldifAttributes)
                {
                    this.ldifAttributes.Add(ldapAttribute.AttributeType, ldapAttribute);
                }
            }
        }

        /// <summary>
        /// Gets a collection that contains the attribute types in the record.
        /// </summary>
        /// <value>The collection of attribute types in the record.</value>
        public IEnumerable<string> AttributeTypes => this.ldifAttributes.Keys;

        /// <summary>
        /// Gets the number of <see cref="LdifAttribute"/> objects in the record.
        /// </summary>
        /// <value>The number of <see cref="LdifAttribute"/> objects in the record.</value>
        public int Count => this.ldifAttributes.Count;

        /// <summary>
        /// Gets the distinguished name of the record.
        /// </summary>
        /// <value>The distinguished name of the record.</value>
        public string DistinguishedName => this.distinguishedName;

        /// <summary>
        /// Gets a collection that contains the attributes in the record.
        /// </summary>
        public IEnumerable<LdifAttribute> LdifAttributes => this.ldifAttributes.Values;

        /// <summary>
        /// Gets the <see cref="LdifAttribute"/> for the specified attribute type.
        /// </summary>
        /// <param name="attributeType">The attribute type whose value will be retrieved.</param>
        /// <returns>The LDIF attribute that has the specified attribute type.</returns>
        public LdifAttribute this[string attributeType] => this.ldifAttributes[attributeType];

        /// <summary>
        /// Determines whether the record contains the attribute type.
        /// </summary>
        /// <param name="attributeType">The attribute type to check.</param>
        /// <returns><b>true</b> if the record contains the attribute type, otherwise <b>false</b>.</returns>
        public bool Contains(string attributeType) => this.ldifAttributes.ContainsKey(attributeType);

        /// <summary>
        /// Generates an RFC2849 LDIF string representation for the record.
        /// </summary>
        /// <returns>The RFC2849 LDIF string representation for the record.</returns>
        public string Dump()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(Extensions.GetValueSpec("dn", this.distinguishedName).Wrap());

            foreach (LdifAttribute ldapAttribute in this)
            {
                foreach (object value in ldapAttribute)
                {
                    stringBuilder.AppendLine(Extensions.GetValueSpec(ldapAttribute.AttributeType, value).Wrap());
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="LdifAttribute"/> objects in the record.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<LdifAttribute> GetEnumerator() => this.ldifAttributes.Values.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="LdifAttribute"/> objects in the record.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{nameof(ChangeAdd)}<{this.distinguishedName}>";

        /// <summary>
        /// Retrieves the <see cref="LdifAttribute"/> that is associated with the specified attribute type.
        /// </summary>
        /// <param name="attributeType">The attribute type whose value will be retrieved.</param>
        /// <param name="ldapAttribute">When this method returns, the value associated with the specified attribute type if found; otherwise null. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if the record contains attribute type; otherwise, <b>false</b>.</returns>
        public bool TryGetAttribute(string attributeType, out LdifAttribute ldapAttribute) => this.ldifAttributes.TryGetValue(attributeType, out ldapAttribute);
    }
}