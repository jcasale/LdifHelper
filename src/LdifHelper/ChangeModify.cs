// -----------------------------------------------------------------------
//  <copyright file="ChangeModify.cs" company="Active Netwerx">
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
    /// Represents an RFC2849 change-modify record.
    /// </summary>
    public class ChangeModify : IChangeRecord, IEnumerable<ModSpec>
    {
        /// <summary>
        /// Represents the distinguished name of the record.
        /// </summary>
        private readonly string distinguishedName;

        /// <summary>
        /// Represents the mod-spec entries in the record.
        /// </summary>
        private readonly List<ModSpec> modSpecs;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeAdd"/> class.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the record.</param>
        /// <param name="modSpecs">The mod-spec entries for the record.</param>
        public ChangeModify(string distinguishedName, IEnumerable<ModSpec> modSpecs)
        {
            if (distinguishedName == null)
            {
                throw new ArgumentNullException(nameof(distinguishedName), "The distinguished name can not be null.");
            }

            if (string.IsNullOrWhiteSpace(distinguishedName))
            {
                throw new ArgumentOutOfRangeException(nameof(distinguishedName), "The distinguished name can not be empty or whitespace.");
            }

            if (modSpecs == null)
            {
                throw new ArgumentNullException(nameof(modSpecs), "The modify specification entries cannot be null.");
            }

            this.distinguishedName = distinguishedName;
            this.modSpecs = new List<ModSpec>(modSpecs);

            if (this.modSpecs.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(modSpecs), "At least one modify specification must be present.");
            }
        }

        /// <summary>
        /// Gets the number of <see cref="ModSpec"/> objects in the record.
        /// </summary>
        /// <value>The number of <see cref="ModSpec"/> objects in the record.</value>
        public int Count => this.modSpecs.Count;

        /// <summary>
        /// Gets the distinguished name of the record.
        /// </summary>
        /// <value>The distinguished name of the record.</value>
        public string DistinguishedName => this.distinguishedName;

        /// <summary>
        /// Gets the <see cref="ModSpec"/> objects in the record.
        /// </summary>
        /// <value>The <see cref="ModSpec"/> objects in the record.</value>
        public IEnumerable<ModSpec> ModSpecs => this.modSpecs;

        /// <summary>
        /// Generates an RFC2849 LDIF string representation for the record.
        /// </summary>
        /// <returns>The RFC2849 LDIF string representation for the record.</returns>
        public string Dump()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(Extensions.GetValueSpec("dn", this.distinguishedName).Wrap());
            stringBuilder.AppendLine("changetype: modify");

            foreach (ModSpec modSpecEntry in this.modSpecs)
            {
                string spec;
                switch (modSpecEntry.ModSpecType)
                {
                    case ModSpecType.Add:
                        spec = "add";

                        break;

                    case ModSpecType.Delete:
                        spec = "delete";

                        break;

                    case ModSpecType.Replace:
                        spec = "replace";

                        break;

                    default:
                        throw new InvalidOperationException($"Unknown mod-spec \"{modSpecEntry.ModSpecType}\".");
                }

                stringBuilder.AppendLine(Extensions.GetValueSpec(spec, modSpecEntry.AttributeType).Wrap());

                foreach (object value in modSpecEntry.AttributeValues)
                {
                    stringBuilder.AppendLine(Extensions.GetValueSpec(modSpecEntry.AttributeType, value).Wrap());
                }

                stringBuilder.AppendLine("-");
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ModSpec"/> objects in the record.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ModSpec"/> objects in the record.</returns>
        public IEnumerator<ModSpec> GetEnumerator() => this.modSpecs.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ModSpec"/> objects in the record.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ModSpec"/> objects in the record.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{nameof(ChangeModify)}<{this.distinguishedName}>";
    }
}