LdifHelper
==========
[![NuGet](https://img.shields.io/nuget/v/LdifHelper.svg)](https://www.nuget.org/packages/LdifHelper/) [![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

A library for reading and writing RFC 2849 LDIF files.


## Usage

### Reading
```csharp
using System;
using System.IO;
using System.Text;
using LdifHelper;

internal class Program
{
    private static void Main()
    {
        // Open and parse an ASCII encoded file. Use 1252 when reading Microsoft's ldifde.exe output.
        using (TextReader textReader = new StreamReader("input.ldif", Encoding.GetEncoding(20127), false))
        {
            foreach (IChangeRecord changeRecord in new LdifReader(textReader))
            {
                if (changeRecord is ChangeAdd changeAdd)
                {
                    Console.WriteLine($"Adding {changeAdd.DistinguishedName}");
                    foreach (LdifAttribute attribute in changeAdd)
                    {
                        Console.WriteLine($"  * {attribute.AttributeType}");
                        foreach (object value in attribute)
                        {
                            Console.WriteLine($"    - {value}");
                        }
                    }
                }
                else if (changeRecord is ChangeDelete changeDelete)
                {
                    Console.WriteLine($"Deleting {changeDelete.DistinguishedName}");
                }
                else if (changeRecord is ChangeModDn changeModDn)
                {
                    string[] components = Constants.DistinguishedNameRegex.Split(changeModDn.DistinguishedName);
                    if (components.Length < 2)
                    {
                        throw new InvalidOperationException(
                            $"Invalid distinguished name found for {changeModDn.DistinguishedName}.");
                    }

                    // Only the newsuperior field is optional.
                    string newDistinguishedName =
                        changeModDn.NewSuperior == null
                            ? $"{changeModDn.NewRdn},{changeModDn.DistinguishedName.Substring(components[0].Length + 1)}"
                            : $"{changeModDn.NewRdn},{changeModDn.NewSuperior}";

                    Console.WriteLine($"Renaming {changeModDn.DistinguishedName}");
                    Console.WriteLine($"  * New DN: {newDistinguishedName}.");
                    Console.WriteLine($"  * Delete old rdn: {changeModDn.DeleteOldRdn}.");
                }
                else if (changeRecord is ChangeModify changeModify)
                {
                    Console.WriteLine($"Modifying {changeModify.DistinguishedName}");
                    foreach (ModSpec modSpec in changeModify.ModSpecs)
                    {
                        switch (modSpec.ModSpecType)
                        {
                            case ModSpecType.Add:
                                Console.WriteLine($"  * Adding the following values to {modSpec.AttributeType}:");
                                foreach (object value in modSpec)
                                {
                                    Console.WriteLine($"    - {value}");
                                }

                                break;
                            case ModSpecType.Delete:
                                if (modSpec.Count == 0)
                                {
                                    Console.WriteLine($"  * Deleting all values from {modSpec.AttributeType}.");
                                }
                                else
                                {
                                    Console.WriteLine($"  * Deleting the following values from {modSpec.AttributeType}:");
                                    foreach (object value in modSpec)
                                    {
                                        Console.WriteLine($"    - {value}");
                                    }
                                }

                                break;
                            case ModSpecType.Replace:
                                Console.WriteLine($"  * Replacing all values from {modSpec.AttributeType}:");
                                foreach (object value in modSpec)
                                {
                                    Console.WriteLine($"    - {value}");
                                }

                                break;
                            default:
                                throw new InvalidOperationException(
                                    $"Unknown mod-spec type: {modSpec.ModSpecType}.");
                        }

                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Unknown change record type: {changeRecord.GetType().BaseType}.");
                }
            }
        }
    }
}
```

### Writing
```csharp
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Text;
using LdifHelper;

internal class Program
{
    private static void Main()
    {
        using (FileStream fileStream = new FileStream("output.ldif", FileMode.Create))
        using (TextWriter textWriter = new StreamWriter(fileStream, Encoding.ASCII))
        using (DirectorySearcher directorySearcher = new DirectorySearcher())
        {
            foreach (SearchResult searchResult in directorySearcher.FindAll())
            {
                List<LdifAttribute> ldifAttributes = new List<LdifAttribute>();
                foreach (DictionaryEntry dictionaryEntry in searchResult.Properties)
                {
                    List<object> values = new List<object>();
                    foreach (object value in (ResultPropertyValueCollection)dictionaryEntry.Value)
                    {
                        /* 
                         * The library does not make assumptions on what the string representation of an object should be.
                         * All types must be converted to either a string or byte[] before being boxed.
                         */
                        values.Add(value is byte[] ? value : value.ToString());
                    }

                    ldifAttributes.Add(new LdifAttribute((string)dictionaryEntry.Key, values));
                }

                ChangeAdd changeAdd = new ChangeAdd(searchResult.Path.Substring(7), ldifAttributes);
                textWriter.WriteLine(changeAdd.Dump());
            }
        }
    }
}
```
