// -----------------------------------------------------------------------
//  <copyright file="Program.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper.Samples.Writing
{
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
}