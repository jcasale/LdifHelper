namespace LdifHelper;

using System;
using System.Text;

/// <summary>
/// Represents an RFC2849 change-delete record.
/// </summary>
public class ChangeDelete : IChangeRecord
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeDelete"/> class.
    /// </summary>
    /// <param name="distinguishedName">The distinguished name of the record.</param>
    public ChangeDelete(string distinguishedName)
    {
        if (string.IsNullOrWhiteSpace(distinguishedName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(distinguishedName));
        }

        this.DistinguishedName = distinguishedName;
    }

    /// <summary>
    /// Gets the distinguished name of the record.
    /// </summary>
    /// <value>The distinguished name of the record.</value>
    public string DistinguishedName { get; }

    /// <summary>
    /// Generates an RFC2849 LDIF string representation for the record.
    /// </summary>
    /// <returns>The RFC2849 LDIF string representation for the record.</returns>
    public string Dump()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(Extensions.GetValueSpec("dn", this.DistinguishedName).Wrap());
        stringBuilder.AppendLine("changetype: delete");

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{nameof(ChangeDelete)}<{this.DistinguishedName}>";
}