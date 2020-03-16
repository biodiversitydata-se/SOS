using DwC_A.Extensions;
using DwC_A.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DwC_A
{
    internal class Tokenizer : ITokenizer
    {
        private readonly IFileMetaData fileMetaData;
        private readonly bool HasQuotes;
        private readonly char Delimiter;
        private readonly char Quotes;

        public Tokenizer(IFileMetaData fileMetaData)
        {
            this.fileMetaData = fileMetaData ?? throw new ArgumentNullException(nameof(fileMetaData));
            this.HasQuotes = fileMetaData.FieldsEnclosedBy.Length > 0;
            this.Delimiter = fileMetaData.FieldsTerminatedBy.FirstOrDefault();
            this.Quotes = fileMetaData.FieldsEnclosedBy.FirstOrDefault();
        }

        public IEnumerable<string> Split(string line)
        {
            StringBuilder token = new StringBuilder();
            bool inQuotes = false;

            foreach (var c in line)
            {
                if (HasQuotes && c == Quotes)
                {
                    inQuotes = inQuotes ^ true;
                }
                else if(!inQuotes && c == Delimiter)
                {
                    yield return token.Flush();
                }
                else
                {
                    token.Append(c);
                }
            }
            yield return token.Flush();
        }
    }
}
