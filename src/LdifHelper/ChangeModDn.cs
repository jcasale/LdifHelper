namespace LdifHelper;

using System;
using System.Text;

/// <summary>
/// Represents an RFC2849 change-moddn record.
/// </summary>
public class ChangeModDn : IChangeRecord
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeModDn"/> class.
    /// </summary>
    /// <param name="distinguishedName">The distinguished name of the record.</param>
    /// <param name="newRdn">The new relative distinguished name. If null, the existing value will be calculated and used, additionally the <b>newSuperior</b> must then be specified.</param>
    /// <param name="deleteOldRdn"><b>true</b> to delete the old RDN of the record, otherwise <b>false</b> to retain it.</param>
    /// <param name="newSuperior">The new location in the directory tree to move the object. Not required if the <b>newRdn</b> was specified.</param>
    public ChangeModDn(string distinguishedName, string newRdn, bool deleteOldRdn, string newSuperior)
    {
        // The distinguished name is mandatory.
        if (distinguishedName is null)
        {
            throw new ArgumentNullException(nameof(distinguishedName), "The distinguished name can not be null.");
        }

        if (string.IsNullOrWhiteSpace(distinguishedName))
        {
            throw new ArgumentOutOfRangeException(nameof(distinguishedName), "The distinguished name can not be empty or whitespace.");
        }

        // The new rdn can not be empty or whitespace.
        if (newRdn is not null
            && string.IsNullOrWhiteSpace(newRdn))
        {
            throw new ArgumentOutOfRangeException(nameof(newRdn), "The new rdn can not be empty or whitespace.");
        }

        // The new superior can not be empty or whitespace.
        if (newSuperior is not null
            && string.IsNullOrWhiteSpace(newSuperior))
        {
            throw new ArgumentOutOfRangeException(nameof(newSuperior), "The new superior can not be empty or whitespace.");
        }

        // Validate record produces a change.
        if (newRdn is null
            && newSuperior is null)
        {
            throw new ArgumentException($"At least one of {nameof(newRdn)} or {nameof(newSuperior)} must be specified.");
        }

        // Calculate the existing rdn.
        var components = Constants.DistinguishedNameRegex.Split(distinguishedName);
        if (components.Length < 2)
        {
            throw new ArgumentException("The distinguished name is invalid.", nameof(distinguishedName));
        }

        var rdn = components[0];

        // Validate potential invalid change scenarios.
        if (string.IsNullOrWhiteSpace(newRdn))
        {
            // If the new rdn is not specified, assume the current value.
            newRdn = rdn;
        }
        else if (string.IsNullOrWhiteSpace(newSuperior)
                 && string.Equals(newRdn, rdn, StringComparison.OrdinalIgnoreCase))
        {
            // If the new superior is unchanged, enforce a change in the newRdn.
            throw new ArgumentOutOfRangeException(nameof(newRdn), "No changes detected.");
        }

        this.DistinguishedName = distinguishedName;
        this.NewRdn = newRdn;
        this.DeleteOldRdn = deleteOldRdn;
        this.NewSuperior = newSuperior;
    }

    /// <summary>
    /// Gets a value indicating whether or not the original distinguished name will be kept.
    /// </summary>
    /// <value><b>true</b> if the original distinguished name should be kept, otherwise <b>false</b>.</value>
    public bool DeleteOldRdn { get; }

    /// <summary>
    /// Gets the distinguished name of the record.
    /// </summary>
    /// <value>The distinguished name of the record.</value>
    public string DistinguishedName { get; }

    /// <summary>
    /// Get the new relative distinguished name.
    /// </summary>
    /// <value>The new relative distinguished name.</value>
    public string NewRdn { get; }

    /// <summary>
    /// Gets the new location in the directory tree to move the object.
    /// </summary>
    /// <value>The new location in the directory tree to move the object.</value>
    public string NewSuperior { get; }

    /// <summary>
    /// Generates an RFC2849 LDIF string representation for the record.
    /// </summary>
    /// <returns>The RFC2849 LDIF string representation for the record.</returns>
    public string Dump()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(Extensions.GetValueSpec("dn", this.DistinguishedName).Wrap());
        stringBuilder.AppendLine("changetype: modrdn");
        stringBuilder.AppendLine(Extensions.GetValueSpec("newrdn", this.NewRdn).Wrap());
        stringBuilder.AppendLine($"deleteoldrdn: {(this.DeleteOldRdn ? 1 : 0)}");

        if (!string.IsNullOrWhiteSpace(this.NewSuperior))
        {
            stringBuilder.AppendLine(Extensions.GetValueSpec("newsuperior", this.NewSuperior).Wrap());
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{nameof(ChangeModDn)}<{this.DistinguishedName}>";
}