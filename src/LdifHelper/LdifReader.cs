// -----------------------------------------------------------------------
//  <copyright file="LdifReader.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a class for reading RFC 2849 LDIF files.
    /// </summary>
    public class LdifReader : IEnumerable<IChangeRecord>
    {
        /// <summary>
        /// Represents data for a change-add record.
        /// </summary>
        private readonly Dictionary<string, List<object>> attributes = new Dictionary<string, List<object>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Represents a UTF8 encoder which throws on invalid input.
        /// </summary>
        private readonly Encoding encodingUtf8 = new UTF8Encoding(false, true);

        /// <summary>
        /// Represents data for a change-modify record.
        /// </summary>
        private readonly List<ModSpec> modifyEntries = new List<ModSpec>();

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
        /// Indicates whether the instance has read from the text reader already.
        /// </summary>
        private bool enumerated;

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
        public LdifReader(TextReader textReader, bool uriEnabled = false)
        {
            this.textReader = textReader ?? throw new ArgumentNullException(nameof(textReader));
            this.uriEnabled = uriEnabled;
        }

        /// <summary>
        /// Gets the current line number being parsed by the reader.
        /// </summary>
        /// <value>The current line number being parsed by the reader.</value>
        public int LineNumber => this.lineNumber;

        /// <summary>
        /// Returns an enumerator that reads through an RFC 2849 LDIF representation from a <see cref="TextReader"/> instance.
        /// </summary>
        /// <returns>An IEnumerator{IChangeRecord} that can be used to iterate through the collection.</returns>
        public IEnumerator<IChangeRecord> GetEnumerator() => this.ParseFile();

        /// <summary>
        /// Returns an enumerator that reads through the LDIF file and generates LDIF records for each entry.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Closes the active entry being parsed and builds the return record.
        /// </summary>
        /// <returns>The LDIF entries corresponding record object.</returns>
        private IChangeRecord CloseEntry()
        {
            try
            {
                switch (this.changeType)
                {
                    case ChangeType.None:

                        throw new InvalidOperationException($"Line {this.lineNumber}: No open entry to close.");

                    case ChangeType.Add:

                        return new ChangeAdd(this.distinguishedName, this.attributes.ToLdapAttributes());

                    case ChangeType.Delete:

                        return new ChangeDelete(this.distinguishedName);

                    case ChangeType.ModDn:
                    case ChangeType.ModRdn:

                        return new ChangeModDn(this.distinguishedName, this.newRdn, this.deleteOldRdn != 0, this.newSuperior);

                    case ChangeType.Modify:

                        return new ChangeModify(this.distinguishedName, this.modifyEntries);

                    default:

                        throw new InvalidOperationException($"Line {this.lineNumber}: Unknown ChangeType: {this.changeType}.");
                }
            }
            finally
            {
                this.ResetEntry();
            }
        }

        /// <summary>
        /// Parses the LDIF data generating change records.
        /// </summary>
        /// <returns>An IEnumerator of IChangeRecords.</returns>
        private IEnumerator<IChangeRecord> ParseFile()
        {
            // Ensure a used text reader is not reread if the instance is misused.
            if (this.enumerated)
            {
                throw new InvalidOperationException("Method has already been invoked.");
            }

            this.enumerated = true;

            string line;
            int c;

            // Skip any leading comments or newlines, note that leading newlines violates the RFC2849 spec (see the LDIF-file definition).
            while (true)
            {
                c = this.textReader.Peek();
                if (c == '#' || c == '\r' || c == '\n')
                {
                    this.textReader.ReadLine();
                    this.lineNumber++;

                    continue;
                }

                break;
            }

            // Consume a line only if its likely to be a version-spec, otherwise throw as it can't be a distinguished name.
            c = this.textReader.Peek();
            if (c == 'v' || c == 'V')
            {
                line = this.textReader.ReadLine();
                this.lineNumber++;

                // Ensure the first line is a version-spec or start of record.
                if (line.StartsWith("version:", StringComparison.OrdinalIgnoreCase))
                {
                    this.ParseLine(line, out string _, out object attributeValue);

                    if (!int.TryParse(attributeValue as string, out int version)
                        || version != 1)
                    {
                        throw new LdifReaderException($"Line {this.lineNumber}: Invalid LDIF version spec: \"{line}\".");
                    }
                }
                else
                {
                    throw new LdifReaderException($"Line {this.lineNumber}: LDIF file must begin with version-spec or a record.");
                }
            }

            // Consume the LDIF file.
            while (true)
            {
                line = this.textReader.ReadLine();
                this.lineNumber++;

                // Detect end of entry and skip consecutive empty lines.
                if (line == null
                    || line.Equals(string.Empty))
                {
                    if (this.changeType != ChangeType.None)
                    {
                        yield return this.CloseEntry();
                    }

                    if (line == null)
                    {
                        yield break;
                    }

                    continue;
                }

                // Skip comments and control statements.
                if (line.StartsWith("#")
                    || line.StartsWith("control:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Process entry.
                switch (this.changeType)
                {
                    case ChangeType.None:
                        // Detect start of new entry and assign distinguished name.
                        if (this.distinguishedName == null)
                        {
                            if (line.StartsWith("dn:", StringComparison.OrdinalIgnoreCase))
                            {
                                while (this.textReader.Peek() == ' ')
                                {
                                    line += this.textReader.ReadLine().Substring(1);
                                    this.lineNumber++;
                                }

                                this.ParseLine(line, out string _, out object dnValue);

                                if (dnValue is string dnString)
                                {
                                    this.distinguishedName = dnString;
                                }
                                else if (dnValue is byte[] dnBytes)
                                {
                                    try
                                    {
                                        // Data for a distinguished name that is encoded is expected to be UTF8.
                                        this.distinguishedName = this.encodingUtf8.GetString(dnBytes);
                                    }
                                    catch (ArgumentException e)
                                    {
                                        throw new LdifReaderException($"Line {this.lineNumber}: Failed to decode distinguished name.", e);
                                    }
                                }
                                else
                                {
                                    throw new LdifReaderException($"Line {this.lineNumber}: Unknown distinguished name type, \"{dnValue.GetType()}\".");
                                }

                                if (string.IsNullOrWhiteSpace(this.distinguishedName))
                                {
                                    throw new LdifReaderException($"Line {this.lineNumber}: Distinguished name is empty.");
                                }

                                break;
                            }

                            throw new LdifReaderException($"Line {this.lineNumber}: A new entry was not initialized with a distinguished name.");
                        }

                        // Detect change type if specified.
                        if (line.StartsWith("changetype:", StringComparison.OrdinalIgnoreCase))
                        {
                            if (line.Length > 12
                                && Enum.TryParse(line.Substring(12), true, out this.changeType))
                            {
                                break;
                            }

                            throw new LdifReaderException($"Line {this.lineNumber}: Invalid changetype specified.");
                        }

                        // Implicit add type.
                        this.changeType = ChangeType.Add;
                        goto case ChangeType.Add;

                    case ChangeType.Add:
                        // Unfold any wrapped value-spec lines.
                        while (this.textReader.Peek() == ' ')
                        {
                            line += this.textReader.ReadLine().Substring(1);
                            this.lineNumber++;
                        }

                        this.ParseLine(line, out string addAttributeType, out object addAttributeValue);

                        this.attributes.AddOrAppend(addAttributeType, addAttributeValue);

                        break;

                    case ChangeType.Delete:
                        // No additional processing needed for a delete entry.

                        break;

                    case ChangeType.ModRdn:
                    case ChangeType.ModDn:
                        // Unfold any wrapped moddn lines.
                        while (this.textReader.Peek() == ' ')
                        {
                            line += this.textReader.ReadLine().Substring(1);
                            this.lineNumber++;
                        }

                        string moddnType;
                        string moddnString;
                        this.ParseLine(line, out moddnType, out object moddnValue);
                        if (moddnValue is string s)
                        {
                            moddnString = s;
                        }
                        else if (moddnValue is byte[] b)
                        {
                            try
                            {
                                // Data for a newrdn or newsuperior that is encoded is expected to be UTF8.
                                moddnString = this.encodingUtf8.GetString(b);
                            }
                            catch (ArgumentException e)
                            {
                                throw new LdifReaderException($"Line {this.lineNumber}: Failed to decode {moddnType}.", e);
                            }
                        }
                        else
                        {
                            throw new LdifReaderException($"Line {this.lineNumber}: Unknown {moddnType} type, \"{moddnValue.GetType()}\".");
                        }

                        // Apply change-moddn statements.
                        switch (moddnType.ToUpperInvariant())
                        {
                            case "NEWRDN":
                                this.newRdn = moddnString;

                                break;

                            case "DELETEOLDRDN":
                                if (!int.TryParse(moddnString, out this.deleteOldRdn)
                                    || !(this.deleteOldRdn == 0 || this.deleteOldRdn == 1))
                                {
                                    throw new LdifReaderException($"Line {this.lineNumber}: Invalid deleteoldrdn value: \"{moddnString}\".");
                                }

                                break;

                            case "NEWSUPERIOR":
                                this.newSuperior = moddnString;

                                break;

                            default:
                                throw new LdifReaderException($"Line {this.lineNumber}: Invalid moddn entry: \"{moddnType}\".");
                        }

                        break;

                    case ChangeType.Modify:
                        // Unfold any wrapped value-spec lines.
                        while (this.textReader.Peek() == ' ')
                        {
                            line += this.textReader.ReadLine().Substring(1);
                            this.lineNumber++;
                        }

                        this.ParseLine(line, out string modSpecString, out object modSpecAttributeType);

                        // Detect mod-spec.
                        if (!Enum.TryParse(modSpecString, true, out ModSpecType modSpec))
                        {
                            throw new LdifReaderException($"Line {this.lineNumber}: Invalid mod-spec in change-modify entry: \"{modSpecString}\".");
                        }

                        string modSpecAttributeTypeString = modSpecAttributeType as string;
                        if (string.IsNullOrWhiteSpace(modSpecAttributeTypeString))
                        {
                            throw new LdifReaderException($"Line {this.lineNumber}: Invalid attrval-spec in change-modify entry: \"{modSpecAttributeTypeString}\".");
                        }

                        // Consume all related entries up to SEP.
                        List<object> values = new List<object>();
                        while (this.textReader.Peek() != '-')
                        {
                            line = this.textReader.ReadLine();
                            this.lineNumber++;

                            if (string.IsNullOrWhiteSpace(line))
                            {
                                throw new LdifReaderException($"Line {this.lineNumber}: Invalid changetype modify entry, unexpected empty line.");
                            }

                            while (this.textReader.Peek() == ' ')
                            {
                                line += this.textReader.ReadLine().Substring(1);
                                this.lineNumber++;
                            }

                            this.ParseLine(line, out string modAttributeType, out object modAttributeValue);

                            // Validate mod-spec.
                            if (!modSpecAttributeTypeString.Equals(modAttributeType))
                            {
                                throw new LdifReaderException($"Line {this.lineNumber}: Inconsistent changetype modify entry, expected \"{modSpecAttributeTypeString}\" but found \"{modAttributeType}\".");
                            }

                            values.Add(modAttributeValue);
                        }

                        // Consume SEP.
                        this.textReader.ReadLine();
                        this.lineNumber++;

                        // Complete modify entry.
                        this.modifyEntries.Add(new ModSpec(modSpec, modSpecAttributeTypeString, values));

                        break;

                    default:

                        throw new InvalidOperationException($"Line {this.lineNumber}: Unknown ChangeType: {this.changeType}.");
                }
            }
        }

        /// <summary>
        /// Parses a previously unfolded LDIF line for the an attribute type and its value. The value will be converted to binary if BASE64 encoded.
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

            // Locate first occurrence of separator (empty string case previously validated).
            int index = line.IndexOf(':');

            // Abort on missing separator.
            if (index == -1)
            {
                throw new LdifReaderException($"Line {this.lineNumber}: Attribute type and value separator not found.");
            }

            // The attribute type has been found.
            attributeType = line.Substring(0, index);

            index++;

            // Check for RFC 2849 note 5 or if continued indexing is possible.
            if (line.Length == index)
            {
                attributeValue = string.Empty;

                return;
            }

            char nextChar = line[index];

            // Check for an ASCII value-spec, "attribute: ASCII-value".
            if (nextChar == ' ')
            {
                attributeValue = line.Substring(index + 1);

                return;
            }

            // The value-spec is either encoded, URI or both, check if continued indexing is possible.
            index++;
            if (line.Length == index)
            {
                throw new LdifReaderException($"Line {this.lineNumber}: The value-spec is truncated.");
            }

            // Check for an BASE64 value-spec, "attribute:: BASE64-value"
            bool encoded = nextChar == ':';

            // Check for an URI value-spec, "attribute:< ASCII-value" or "attribute::< BASE64-value".
            bool fileUri = encoded ? line[index] == '<' : nextChar == '<';

            // Parse value-spec.
            string value = line.Substring(index + (encoded && fileUri ? 2 : 1));

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
                    using (FileStream fileStream = new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read))
                    {
                        int length = (int)fileStream.Length;
                        buffer = new byte[length];
                        int count;
                        int sum = 0;

                        while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                        {
                            sum += count;
                        }
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
}