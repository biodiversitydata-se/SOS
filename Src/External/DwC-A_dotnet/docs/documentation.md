# Documentation

## Contents
- Basic Usage
  * [Reading Core File Data](#reading-core-file-data)
  * [Read Archive Meta data](#read-archive-meta-data)
  * [Reading Extension Files](#reading-extension-files)
  * [Using LinQ](#using-linq)

## Reading Core File Data

```
    using DwC_A;
    using DwC_A.Terms;
    using System.Linq;

    class Program
    {
        static void Main()
        {

            string fileName = "./dwca-uta_herps-v8.1.zip";
            using (var archive = new ArchiveReader(fileName))
            {
                foreach(var row in archive.CoreFile.Rows)
                {
                    //Access fields by index
                    Console.WriteLine(row[0]);
                    //Access fields by Term
                    Console.WriteLine(row[Terms.type]);
                    //Iterate over or query fields in a row using LinQ
                    var fields = row.Fields.ToList().Aggregate((current, next) => $"{current}\t{next}");
                    Console.WriteLine(fields);
                }
            }
        }
    }
```

## Read Archive Meta data

Archive meta data can be accessed through the `ArchiveReader.MetaData` property.

## Reading Extension Files

Extension files can be accessed through the `ArchiveReader.Extensions` `FileReaderCollection`.  Extension file readers can be referenced by filename
```
IFileReader fileReaders = archive.Extensions.GetFileReaderByFileName("event.txt");
```
OR they can referenced by the row type associated with the extension file.  Note that there may be multiple extension files of the same row type.
```
IEnumerable<IFileReader> fileReader = archive.Extensions.GetFileReadersByRowType(RowTypes.Event);
```

## Using LINQ

The archive CoreFile and Extension file rows support LINQ queries to search, sort and filter data rows.  For example, a list of all ids and scientific names for a specific genus may be queried from a taxon file as follows.
```
var taxon = from t in archive.CoreFile.DataRows
            where t[Terms.genus] == "Equisetum"
            select new { id = t["id"], ScientificName = t[Terms.scientificName] };
```
OR
```
var taxon = archive.CoreFile.DataRows
            .Where(t => t[Terms.genus] == "Equisetum")
            .Select(t => new { id = t["id"], ScientificName = t[Terms.scientificName] });
```
A one-to-many relationship between a taxon file and a vernacularname extension file may be queried using a group join as follows.
```
var vernacularNamesFile = archive.Extensions.GetFileReaderByFileName("vernacularname.txt");
var taxon = from t in archive.CoreFile.DataRows
            where t[Terms.genus] == "Equisetum"
            join v in vernacularNamesFile.DataRows on t["id"] equals v["id"] into vGroup
            select new
            {
                id = t["id"],
                ScientificName = t[Terms.scientificName],
                VernacularNames = from v1 in vGroup select v1[Terms.vernacularName]
            };
```
OR a lookup could be used to accomplish the same as follows.
```
var vernacularNameLookup = archive.Extensions
                                  .GetFileReaderByFileName("vernacularname.txt")
                                  .DataRows  
                                  .ToLookup(v => v["id"], v => v[Terms.vernacularName]);
var vernacularNames = archive.CoreFile.DataRows
                        .Where(t => t[Terms.genus] == "Equisetum")
                        .Select(t => new
                        {
                            id = t["id"],
                            ScientificName = t[Terms.scientificName],
                            VernacularNames = vernacularNameLookup[t["id"]]
                        });
``` 