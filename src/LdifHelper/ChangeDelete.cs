// -----------------------------------------------------------------------
//  <copyright file="ChangeDelete.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper
{
    using System;
    using System.Text;

    /// <summary>
    /// Represents an RFC2849 change-delete record.
    /// </summary>
    public class ChangeDelete : IChangeRecord
    {
        /// <summary>
        /// Represents the distinguished name of the record.
        /// </summary>
        private readonly string distinguishedName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeDelete"/> class.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the record.</param>
        public ChangeDelete(string distinguishedName)
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
        }

        /// <summary>
        /// Gets the distinguished name of the record.
        /// </summary>
        /// <value>The distinguished name of the record.</value>
        public string DistinguishedName => this.distinguishedName;

        /// <summary>
        /// Generates an RFC2849 LDIF string representation for the record.
        /// </summary>
        /// <returns>The RFC2849 LDIF string representation for the record.</returns>
        public string Dump()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(Extensions.GetValueSpec("dn", this.distinguishedName).Wrap());
            stringBuilder.AppendLine("changetype: delete");

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{nameof(ChangeDelete)}<{this.distinguishedName}>";
    }
}