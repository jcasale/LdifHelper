// -----------------------------------------------------------------------
//  <copyright file="IChangeRecord.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper
{
    /// <summary>
    /// Represents the common interface between RFC2849 LDIF change types.
    /// </summary>
    public interface IChangeRecord
    {
        /// <summary>
        /// Gets the distinguished name of the record.
        /// </summary>
        /// <value>The distinguished name of the record.</value>
        string DistinguishedName { get; }

        /// <summary>
        /// Generates an RFC2849 LDIF string representation for the record.
        /// </summary>
        /// <returns>The RFC2849 LDIF string representation for the record.</returns>
        string Dump();
    }
}