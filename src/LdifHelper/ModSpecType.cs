// -----------------------------------------------------------------------
//  <copyright file="ModSpecType.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper
{
    /// <summary>
    /// Represents the type of mod-spec operation for an RFC2849 change-modify record.
    /// </summary>
    public enum ModSpecType
    {
        /// <summary>
        /// Represents an add mod-spec.
        /// </summary>
        Add,

        /// <summary>
        /// Represents a delete mod-spec.
        /// </summary>
        Delete,

        /// <summary>
        /// Represents a replace mod-spec.
        /// </summary>
        Replace
    }
}