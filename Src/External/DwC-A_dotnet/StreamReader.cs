﻿using DwC_A.Factories;
using DwC_A.Meta;
using System.Collections.Generic;
using System.IO;

namespace DwC_A
{
    internal class StreamReader
    {
        private readonly IFileMetaData fileMetaData;
        private readonly IRowFactory rowFactory;
        private readonly ITokenizer tokenizer;

        public StreamReader(
            IRowFactory rowFactory,
            ITokenizer tokenizer,
            IFileMetaData fileMetaData)
        {
            this.fileMetaData = fileMetaData;
            this.rowFactory = rowFactory;
            this.tokenizer = tokenizer;
        }

        public IEnumerable<IRow> ReadRows(Stream stream)
        {
            using var reader = new System.IO.StreamReader(stream, fileMetaData.Encoding);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var row = rowFactory.CreateRow(tokenizer.Split(line), fileMetaData.Fields);
                if (row.IsValid) yield return row;
            }
        }

        public async IAsyncEnumerable<IRow> ReadRowsAsync(Stream stream)
        {
            using var reader = new System.IO.StreamReader(stream, fileMetaData.Encoding);
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var row = rowFactory.CreateRow(tokenizer.Split(line), fileMetaData.Fields);
                if (row.IsValid) yield return row;
            }
        }
    }
}