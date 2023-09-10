namespace LdifHelper;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Represents a class for reading RFC 2849 LDIF files.
/// </summary>
public class LdifReader
{
    /// <summary>
    /// Represents data for a change-add record.
    /// </summary>
    private readonly Dictionary<string, List<object>> attributes = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Represents a UTF8 encoder which throws on invalid input.
    /// </summary>
    private readonly Encoding encodingUtf8 = new UTF8Encoding(false, true);

    /// <summary>
    /// Represents data for a change-modify record.
    /// </summary>
    private readonly List<ModSpec> modifyEntries = new();

    /// <summary>
    /// Represents the text reader instance to read from.
    /// </summary>
    private readonly TextReader textReader;

    /// <summary>
    /// Indicates whether URI paths are allowed.
    /// </summary>
    private readonly bool uriEnabled;

    /// <summary>
    /// Indicates the current record type being parsed.
    /// </summary>
    private ChangeType changeType;

    /// <summary>
    /// Indicates the action to be taken with the original distinguished name for a modrdn directive.
    /// </summary>
    private int deleteOldRdn;

    /// <summary>
    /// Represents the distinguished name for all record types.
    /// </summary>
    private string distinguishedName;

    /// <summary>
    /// Indicates the current line number being parsed by the reader.
    /// </summary>
    private int lineNumber;

    /// <summary>
    /// Represents the new relative distinguished name for a modrdn directive.
    /// </summary>
    private string newRdn;

    /// <summary>
    /// Represents the new location to move an object within the DIT for a modrdn directive.
    /// </summary>
    private string newSuperior;

    /// <summary>
    /// Initializes a new instance of the <see cref="LdifReader"/> class.
    /// </summary>
    /// <param name="textReader">The text reader instance to consume.</param>
    /// <param name="uriEnabled"><b>true</b> if URI value-spec entries are allowed, otherwise <b>false</b> to disable and throw if encountered.</param>
    private LdifReader(TextReader textReader, bool uriEnabled)
    {
        this.textReader = textReader ?? throw new ArgumentNullException(nameof(textReader));
        this.uriEnabled = uriEnabled;
    }

    /// <summary>
    /// Parses a text reader stream for LDIF change records.
    /// </summary>
    /// <param name="textReader">The text reader instance to consume.</param>
    /// <param name="uriEnabled"><b>true</b> if URI value-spec entries are allowed, otherwise <b>false</b> to disable and throw if encountered.</param>
    /// <returns>A collection of change records.</returns>
    public static IEnumerable<IChangeRecord> Parse(TextReader textReader, bool uriEnabled = false)
    {
        var ldifReader = new LdifReader(textReader, uriEnabled);

        string line;
        int c;

        // Skip any leading comments or newlines, note that leading newlines violates the RFC2849 spec (see the LDIF-file definition).
        while (true)
        {
            c = ldifReader.textReader.Peek();
            if (c is '#' or '\r' or '\n')
            {
                ldifReader.textReader.ReadLine();
                ldifReader.lineNumber++;

                continue;
            }

            break;
        }

        // Consume a line only if its likely to be a version-spec, otherwise throw as it can't be a distinguished name.
        c = ldifReader.textReader.Peek();
        if (c is 'v' or 'V')
        {
            line = ldifReader.textReader.ReadLine();
            ldifReader.lineNumber++;

            // Ensure the first line is a version-spec or start of record.
            if (line.StartsWith("version:", StringComparison.OrdinalIgnoreCase))
            {
                ldifReader.ParseLine(line, out _, out var attributeValue);

                if (!int.TryParse(attributeValue as string, out var version)
                    || version != 1)
                {
                    throw new LdifReaderException($"Line {ldifReader.lineNumber}: Invalid LDIF version spec: \"{line}\".");
                }
            }
            else
            {
                throw new LdifReaderException($"Line {ldifReader.lineNumber}: LDIF file must begin with version-spec or a record.");
            }
        }

        // Consume the LDIF file.
        while (true)
        {
            line = ldifReader.textReader.ReadLine();
            ldifReader.lineNumber++;

            // Detect end of entry and skip consecutive empty lines.
            if (string.IsNullOrWhiteSpace(line))
            {
                if (ldifReader.changeType != ChangeType.None)
                {
                    yield return ldifReader.CloseEntry();
                }

                if (line is null)
                {
                    yield break;
                }

                continue;
            }

            // Skip comments and control statements.
            if (line.StartsWith("#", StringComparison.Ordinal)
                || line.StartsWith("control:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Process entry.
            switch (ldifReader.changeType)
            {
                case ChangeType.None:
                    // Detect start of new entry and assign distinguished name.
                    if (ldifReader.distinguishedName is null)
                    {
                        if (line.StartsWith("dn:", StringComparison.OrdinalIgnoreCase))
                        {
                            while (ldifReader.textReader.Peek() == ' ')
                            {
                                line += ldifReader.textReader.ReadLine().Substring(1);
                                ldifReader.lineNumber++;
                            }

                            ldifReader.ParseLine(line, out _, out var dnValue);

                            if (dnValue is string dnString)
                            {
                                ldifReader.distinguishedName = dnString;
                            }
                            else if (dnValue is byte[] dnBytes)
                            {
                                try
                                {
                                    // Data for a distinguished name that is encoded is expected to be UTF8.
                                    ldifReader.distinguishedName = ldifReader.encodingUtf8.GetString(dnBytes);
                                }
                                catch (ArgumentException e)
                                {
                                    throw new LdifReaderException($"Line {ldifReader.lineNumber}: Failed to decode distinguished name.", e);
                                }
                            }
                            else
                            {
                                throw new LdifReaderException($"Line {ldifReader.lineNumber}: Unknown distinguished name type, \"{dnValue.GetType()}\".");
                            }

                            if (string.IsNullOrWhiteSpace(ldifReader.distinguishedName))
                            {
                                throw new LdifReaderException($"Line {ldifReader.lineNumber}: Distinguished name is empty.");
                            }

                            break;
                        }

                        throw new LdifReaderException($"Line {ldifReader.lineNumber}: A new entry was not initialized with a distinguished name.");
                    }

                    // Detect change type if specified.
                    if (line.StartsWith("changetype:", StringComparison.OrdinalIgnoreCase))
                    {
                        if (line.Length > 12
                            && Enum.TryParse(line.Substring(12), true, out ldifReader.changeType))
                        {
                            break;
                        }

                        throw new LdifReaderException($"Line {ldifReader.lineNumber}: Invalid changetype specified.");
                    }

                    // Implicit add type.
                    ldifReader.changeType = ChangeType.Add;
                    goto case ChangeType.Add;

                case ChangeType.Add:
                    // Unfold any wrapped value-spec lines.
                    while (ldifReader.textReader.Peek() == ' ')
                    {
                        line += ldifReader.textReader.ReadLine().Substring(1);
                        ldifReader.lineNumber++;
                    }

                    ldifReader.ParseLine(line, out var addAttributeType, out var addAttributeValue);

                    ldifReader.attributes.AddOrAppend(addAttributeType, addAttributeValue);

                    break;

                case ChangeType.Delete:
                    // No additional processing needed for a delete entry.

                    break;

                case ChangeType.ModRdn:
                case ChangeType.ModDn:
                    // Unfold any wrapped moddn lines.
                    while (ldifReader.textReader.Peek() == ' ')
                    {
                        line += ldifReader.textReader.ReadLine().Substring(1);
                        ldifReader.lineNumber++;
                    }

                    string moddnString;
                    ldifReader.ParseLine(line, out var moddnType, out var moddnValue);
                    if (moddnValue is string s)
                    {
                        moddnString = s;
                    }
                    else if (moddnValue is byte[] b)
                    {
                        try
                        {
                            // Data for a newrdn or newsuperior that is encoded is expected to be UTF8.
                            moddnString = ldifReader.encodingUtf8.GetString(b);
                        }
                        catch (ArgumentException e)
                        {
                            throw new LdifReaderException($"Line {ldifReader.lineNumber}: Failed to decode {moddnType}.", e);
                        }
                    }
                    else
                    {
                        throw new LdifReaderException($"Line {ldifReader.lineNumber}: Unknown {moddnType} type, \"{moddnValue.GetType()}\".");
                    }

                    // Apply change-moddn statements.
                    switch (moddnType.ToUpperInvariant())
                    {
                        case "NEWRDN":
                            ldifReader.newRdn = moddnString;

                            break;

                        case "DELETEOLDRDN":
                            if (!int.TryParse(moddnString, out ldifReader.deleteOldRdn)
                                || !(ldifReader.deleteOldRdn == 0 || ldifReader.deleteOldRdn == 1))
                            {
                                throw new LdifReaderException($"Line {ldifReader.lineNumber}: Invalid deleteoldrdn value: \"{moddnString}\".");
                            }

                            break;

                        case "NEWSUPERIOR":
                            ldifReader.newSuperior = moddnString;

                            break;

                        default:
                            throw new LdifReaderException($"Line {ldifReader.lineNumber}: Invalid moddn entry: \"{moddnType}\".");
                    }

                    break;

                case ChangeType.Modify:
                    // Unfold any wrapped value-spec lines.
                    while (ldifReader.textReader.Peek() == ' ')
                    {
                        line += ldifReader.textReader.ReadLine().Substring(1);
                        ldifReader.lineNumber++;
                    }

                    ldifReader.ParseLine(line, out var modSpecString, out var modSpecAttributeType);

                    // Detect mod-spec.
                    if (!Enum.TryParse(modSpecString, true, out ModSpecType modSpec))
                    {
                        throw new LdifReaderException($"Line {ldifReader.lineNumber}: Invalid mod-spec in change-modify entry: \"{modSpecString}\".");
                    }

                    var modSpecAttributeTypeString = modSpecAttributeType as string;
                    if (string.IsNullOrWhiteSpace(modSpecAttributeTypeString))
                    {
                        throw new LdifReaderException($"Line {ldifReader.lineNumber}: Invalid attrval-spec in change-modify entry: \"{modSpecAttributeTypeString}\".");
                    }

                    // Consume all related entries up to SEP.
                    var values = new List<object>();
                    while (ldifReader.textReader.Peek() != '-')
                    {
                        line = ldifReader.textReader.ReadLine();
                        ldifReader.lineNumber++;

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            throw new LdifReaderException($"Line {ldifReader.lineNumber}: Invalid changetype modify entry, unexpected empty line.");
                        }

                        while (ldifReader.textReader.Peek() == ' ')
                        {
                            line += ldifReader.textReader.ReadLine().Substring(1);
                            ldifReader.lineNumber++;
                        }

                        ldifReader.ParseLine(line, out var modAttributeType, out var modAttributeValue);

                        // Validate mod-spec.
                        if (!string.Equals(modSpecAttributeTypeString, modAttributeType, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new LdifReaderException($"Line {ldifReader.lineNumber}: Inconsistent changetype modify entry, expected \"{modSpecAttributeTypeString}\" but found \"{modAttributeType}\".");
                        }

                        values.Add(modAttributeValue);
                    }

                    // Consume SEP.
                    ldifReader.textReader.ReadLine();
                    ldifReader.lineNumber++;

                    // Complete modify entry.
                    ldifReader.modifyEntries.Add(new ModSpec(modSpec, modSpecAttributeTypeString, values));

                    break;

                default:

                    throw new InvalidOperationException($"Line {ldifReader.lineNumber}: Unknown ChangeType: {ldifReader.changeType}.");
            }
        }
    }

    /// <summary>
    /// Closes the active entry being parsed and builds the return record.
    /// </summary>
    /// <returns>The LDIF entries corresponding record object.</returns>
    private IChangeRecord CloseEntry()
    {
        try
        {
            return this.changeType switch
            {
                ChangeType.None => throw new InvalidOperationException($"Line {this.lineNumber}: No open entry to close."),
                ChangeType.Add => new ChangeAdd(this.distinguishedName, this.attributes.ToLdapAttributes()),
                ChangeType.Delete => new ChangeDelete(this.distinguishedName),
                ChangeType.ModDn => new ChangeModDn(this.distinguishedName, this.newRdn, this.deleteOldRdn != 0, this.newSuperior),
                ChangeType.ModRdn => new ChangeModDn(this.distinguishedName, this.newRdn, this.deleteOldRdn != 0, this.newSuperior),
                ChangeType.Modify => new ChangeModify(this.distinguishedName, this.modifyEntries),
                _ => throw new InvalidOperationException($"Line {this.lineNumber}: Unknown ChangeType: {this.changeType}.")
            };
        }
        finally
        {
            this.ResetEntry();
        }
    }

    /// <summary>
    /// Parses a previously unfolded LDIF line for the attribute type and its value. The value will be converted to binary if BASE64 encoded.
    /// </summary>
    /// <param name="line">The unfolded LDIF line.</param>
    /// <param name="attributeType">The resulting attribute type.</param>
    /// <param name="attributeValue">The resulting attribute value.</param>
    private void ParseLine(string line, out string attributeType, out object attributeValue)
    {
        // Abort on empty line.
        if (string.IsNullOrWhiteSpace(line))
        {
            throw new ArgumentException($"Line {this.lineNumber}: Unable to parse null, empty or whitespace line.", nameof(line));
        }

        // Determine if the attrval-spec AttributeDescription contains any options, but ignore the result if the occurance appears in the value.
        var optionsIndex = line.IndexOf(';');

        // Locate the beginning of the value-spec.
        var valueSpecIndex = line.IndexOf(':');

        attributeType = optionsIndex switch
        {
            // The attribute type has been found without any options.
            -1 when valueSpecIndex > 0 => line.Substring(0, valueSpecIndex),

            // The attribute type has been found without any options, but the value contains one or more semicolons.
            > 0 when optionsIndex > valueSpecIndex + 1 => line.Substring(0, valueSpecIndex),

            // The attribute type has been found with one or more options.
            > 0 when valueSpecIndex > optionsIndex + 1 => line.Substring(0, optionsIndex),

            // Any other combination (e.g., an empty attribute type).
            _ => throw new LdifReaderException($"Line {this.lineNumber}: Failed to read attrval-spec.")
        };

        valueSpecIndex++;

        // Check for RFC 2849 note 5 or if continued indexing is possible.
        if (line.Length == valueSpecIndex)
        {
            attributeValue = string.Empty;

            return;
        }

        var nextChar = line[valueSpecIndex];

        // Check for an ASCII value-spec, "attribute: ASCII-value".
        if (nextChar == ' ')
        {
            attributeValue = line.Substring(valueSpecIndex + 1);

            return;
        }

        // The value-spec is either encoded, URI or both, check if continued indexing is possible.
        valueSpecIndex++;
        if (line.Length == valueSpecIndex)
        {
            throw new LdifReaderException($"Line {this.lineNumber}: The value-spec is truncated.");
        }

        // Check for an BASE64 value-spec, "attribute:: BASE64-value"
        var encoded = nextChar == ':';

        // Check for an URI value-spec, "attribute:< ASCII-value" or "attribute::< BASE64-value".
        var fileUri = encoded ? line[valueSpecIndex] == '<' : nextChar == '<';

        // Parse value-spec.
        var value = line.Substring(valueSpecIndex + (encoded && fileUri ? 2 : 1));

        // Decode if required, leaving the binary data in its original format on the wire.
        if (encoded)
        {
            try
            {
                attributeValue = Convert.FromBase64String(value);
            }
            catch (FormatException e)
            {
                throw new LdifReaderException($"Line {this.lineNumber}: Failed to decode BASE64 data.", e);
            }
        }
        else
        {
            attributeValue = value;
        }

        // Read URI if required.
        if (fileUri)
        {
            if (!this.uriEnabled)
            {
                throw new LdifReaderException($"Line {this.lineNumber}: URI value-spec not enabled.");
            }

            string uriString;
            if (attributeValue is string s)
            {
                uriString = s;
            }
            else
            {
                try
                {
                    // Data for a URI resource that is encoded is expected to be UTF8.
                    uriString = this.encodingUtf8.GetString((byte[])attributeValue);
                }
                catch (ArgumentException e)
                {
                    throw new LdifReaderException($"Line {this.lineNumber}: A URI value-spec could not be read as a string, \"{attributeValue.GetType()}\".", e);
                }
            }

            Uri uri;
            try
            {
                uri = new Uri(uriString);
            }
            catch (UriFormatException e)
            {
                throw new LdifReaderException($"Line {this.lineNumber}: Invalid URI format, \"{uriString}\".", e);
            }

            byte[] buffer;
            try
            {
                using var fileStream = new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read);

                var length = (int)fileStream.Length;
                buffer = new byte[length];
                int count;
                var sum = 0;

                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                {
                    sum += count;
                }
            }
            catch (ArgumentException e)
            {
                throw new LdifReaderException($"Line {this.lineNumber}: Failed to load URI based resource.", e);
            }
            catch (IOException e)
            {
                throw new LdifReaderException($"Line {this.lineNumber}: Failed to load URI based resource.", e);
            }
            catch (NotSupportedException e)
            {
                throw new LdifReaderException($"Line {this.lineNumber}: Failed to load URI based resource.", e);
            }
            catch (System.Security.SecurityException e)
            {
                throw new LdifReaderException($"Line {this.lineNumber}: Failed to load URI based resource.", e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new LdifReaderException($"Line {this.lineNumber}: Failed to load URI based resource.", e);
            }

            attributeValue = buffer;
        }
    }

    /// <summary>
    /// Resets the data storage fields after closing an entry.
    /// </summary>
    private void ResetEntry()
    {
        this.changeType = ChangeType.None;
        this.distinguishedName = null;
        this.attributes.Clear();
        this.modifyEntries.Clear();
        this.newRdn = null;
        this.deleteOldRdn = -1;
        this.newSuperior = null;
    }
}