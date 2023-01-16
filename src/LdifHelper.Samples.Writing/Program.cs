namespace LdifHelper.Samples.Writing;

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
        using var fileStream = new FileStream("output.ldif", FileMode.Create);
        using var streamWriter = new StreamWriter(fileStream, Encoding.ASCII);
        using var directorySearcher = new DirectorySearcher();

        foreach (SearchResult searchResult in directorySearcher.FindAll())
        {
            var ldifAttributes = new List<LdifAttribute>();
            foreach (DictionaryEntry dictionaryEntry in searchResult.Properties)
            {
                var values = new List<object>();
                foreach (var value in (ResultPropertyValueCollection)dictionaryEntry.Value)
                {
                    /*
                     * The library does not make assumptions on what the string representation of an object should be.
                     * All types must be converted to either a string or byte[] before being boxed.
                     */
                    values.Add(value is byte[] ? value : value.ToString());
                }

                ldifAttributes.Add(new LdifAttribute((string)dictionaryEntry.Key, values));
            }

            var changeAdd = new ChangeAdd(searchResult.Path.Substring(7), ldifAttributes);
            streamWriter.WriteLine(changeAdd.Dump());
        }
    }
}